using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.SplashScreen
{
    public class FoeViewModel : ObservableObject, ITabViewModel, IFoeViewModel
    {
        private int retry = 0;
        private string foe_sPathName;
        private bool Init_Status = false;
        private bool Foe_Status = false;
        private bool Op_Status = false;
        private string foe_dwPass;



        private bool ValidateMyString(string filpath)
        {
            // Define your regex pattern
            string pattern = @"\.(bin|hex)$";

            if (string.IsNullOrEmpty(filpath))
            {
                // Handle the case when filePath is null or empty
                return false;
            }

            // Perform the regex match
            bool isMatch = Regex.IsMatch(filpath, pattern);

            // Handle the validation result
            if (isMatch)
            {
                return true;
            }
            else
            {
                log.Log(LogLevel.Info, "Please Select File Path");
                return false;
            }
        }

        private bool? _isCheckedAll;

        public bool? IsCheckedAll
        {
            get => _isCheckedAll;
            set
            {
                if (value == _isCheckedAll) return;
                _isCheckedAll = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<FoeModel> MyDevicesAndBoxes { get; set; }
        //public   
        //{
        //    get { return _MyDevicesAndBoxes; }
        //    set
        //    {
        //        _MyDevicesAndBoxes = value;
        //        OnPropertyChanged();
        //    }
        //}


        private FoeModel _SelectedDeviceAndBox;
        public FoeModel SelectedDeviceAndBox
        {
            get { return _SelectedDeviceAndBox; }
            set
            {
                _SelectedDeviceAndBox = value;
                OnPropertyChanged();
            }
        }
        private uint _OverAllProgress;

        public uint OverAllProgress
        {
            get { return _OverAllProgress; }
            set
            {
                _OverAllProgress = value;
                OnPropertyChanged();
            }
        }
        int runningIndex = 0;
        private Logger log;
        public RelayCommand UpdateFirmwareCommand { get; set; }
        public RelayCommand CheckAllCommand { get; set; }
        public RelayCommand BrowseFirmwareCommand { get; set; }
        public RelayCommand RefreshDataGridCommand { get; set; }
        private bool _BT_FirmwareEnable;

        public bool BT_FirmwareEnable
        {
            get { return _BT_FirmwareEnable; }
            set { _BT_FirmwareEnable = value; OnPropertyChanged(); }
        }
        private void BrowseFirmwareFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "bin files (*.bin)|*.bin|hex files (*.hex)|*.hex";
                log.Log(LogLevel.Info, $"Select Project Folder");
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Foe_sPathName = new FileInfo(dialog.FileName).FullName;
                }
            }
        }
        private string _DeviceStatusIndex;
        public string DeviceStatusIndex
        {
            get { return _DeviceStatusIndex; }
            set { _DeviceStatusIndex = value; OnPropertyChanged(); }
        }

        public string Foe_sPathName { get => foe_sPathName; set { foe_sPathName = value; OnPropertyChanged(); } }
        public string Foe_dwPass { get => foe_dwPass; 
            set 
            {
                foe_dwPass = value; 
                OnPropertyChanged();
            } 
        }

        public string Foe_sNetId { get; private set; }
        public string Foe_Remarks { get;  set; }

        private List<int> NotificatioHandles;
        public event EventHandler<bool> TUpdate_Status;

        private void RegisterNotification()
        {

            NotificatioHandles = new List<int>();
            _TwinCathelper.Client.AdsNotification += Client_AdsNotificationFoe;
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.foe_nErrID}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.foe_bBusy}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.foe_bError}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.foe_nPercent}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.foe_nBytesWritten}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.ecs_bBusy}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.ecs_bError}"));
            NotificatioHandles.Add(_TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.ecs_nErrId}"));
        }
        List<FoeModel> checkedSlaves;
        int cur;
        string curStr;
        private void UpdateFirmwareMeth()
        {
            try
            {
                ResetDeviceCollection();
                OverAllProgress = 0;
                cur = 0;
                checkedSlaves = MyDevicesAndBoxes.Where(a => a.IsChecked == true).ToList();
                RegisterNotification();
                _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_dwPass}").FirstOrDefault().WriteValue(Convert.ToInt32(Foe_dwPass, 16));
                _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sPathName}").FirstOrDefault().WriteValue(Foe_sPathName);
                Foe_sNetId = _TwinCathelper.SAmsNetId;
                for (UInt16 i = 0; i < checkedSlaves.Count; i++)
                {
                    cur = i;
                    curStr = $"{i + 1}/{checkedSlaves.Count}";
                    runningIndex = MyDevicesAndBoxes.IndexOf(MyDevicesAndBoxes.Where(a => a.Foe_nSlaveAddr == checkedSlaves[i].Foe_nSlaveAddr).FirstOrDefault());
                    ResetSetContols();
                    DeviceStatusIndex = $"Updating {i + 1}/{checkedSlaves.Count}";
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);

                    Thread.Sleep(1000);
                    DeviceStatusIndex = $"Initializing {i + 1}/{checkedSlaves.Count}";
                    if (ChangeBoxState(ESlaveStateMachine.EC_DEVICE_STATE_INIT))
                    {
                        Init_Status = true;
                        _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
                        DeviceStatusIndex = $"Setted into : {ESlaveStateMachine.EC_DEVICE_STATE_INIT} {curStr}";
                        log.Log(LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} State Updated");
                        MyDevicesAndBoxes[runningIndex].Foe_Remarks = "State Updated";
                        _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bLoad}").FirstOrDefault().WriteValue(1);
                        DeviceStatusIndex = $"Downloading {curStr}";
                        if (FoeWait())
                        {
                            Op_Status = true;
                            log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} Update Completed");
                            MyDevicesAndBoxes[runningIndex].Foe_Remarks = "Update Completed";
                        }
                        else
                        {
                            Op_Status = false;
                            log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} Update Failed");
                            MyDevicesAndBoxes[runningIndex].Foe_Remarks = "Update Failed";
                        }
                    }
                    else
                    {
                        Init_Status = false;
                        log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} Device State not set");
                        MyDevicesAndBoxes[runningIndex].Foe_Remarks = "Device State not set";
                    }
                    MyDevicesAndBoxes[runningIndex].Init_Status = Init_Status;
                    MyDevicesAndBoxes[runningIndex].Foe_Status = Foe_Status;
                    MyDevicesAndBoxes[runningIndex].Op_Status = Op_Status;
                }
                var updatedDeviceCount = checkedSlaves.Where(a => (a.Init_Status == true) & (a.Foe_Status == true) & (a.Op_Status == true)).Count();

                foreach (var item in MyDevicesAndBoxes)
                {
                    log.Log(LogLevel.Info, $"\tSlave Addr : {item.Foe_nSlaveAddr} : \n" +
                        $" \t Init Status = {item.Init_Status}\n" +
                        $" \t Foe Status = {item.Foe_Status}\n" +
                        $" \t OP Status = {item.Init_Status}\n" +
                        $" \t Foe_nErrID = {(AdsErrorCode)item.Foe_nErrID}"
                        );
                }
                log.Log(LogLevel.Info, $"FOE Update Summary \n " +
                   $"Total Devices : {checkedSlaves.Count} ." +
                   $"\n Sucessful Update Operations : {updatedDeviceCount} out of {checkedSlaves.Count}");

                DeviceStatusIndex = $"{updatedDeviceCount}/{checkedSlaves.Count} Updated";
            }
            catch (Exception ex)
            {
                DeviceStatusIndex = ex.Message + $" {curStr}";
                Foe_Remarks = ex.Message;
                log.Log(LogLevel.Error, ex.Message + $" {curStr}");
            }
            finally
            {
                BT_FirmwareEnable = true;
                TUpdate_Status?.Invoke(null, true);
                DeleteNotifications();
            }
        }
        private void ResetSetContols()
        {
            MyDevicesAndBoxes[runningIndex].Foe_nPercent = 0;
            MyDevicesAndBoxes[runningIndex].Foe_bError = false;
            MyDevicesAndBoxes[runningIndex].Foe_bBusy = false;
            MyDevicesAndBoxes[runningIndex].Foe_nErrID = 0;
            MyDevicesAndBoxes[runningIndex].Ecs_bError = false;
            MyDevicesAndBoxes[runningIndex].Ecs_bBusy = false;
            MyDevicesAndBoxes[runningIndex].Ecs_nErrId = 0;
            MyDevicesAndBoxes[runningIndex].SlaveState = "";
            MyDevicesAndBoxes[runningIndex].DateTimeLU = DateTime.Now.ToString("t");
        }
        private void DeleteNotifications()
        {
            if (_TwinCathelper.Client.State == ConnectionState.Connected)
            {
                foreach (var item in NotificatioHandles)
                {
                    if (item != -1)
                    {
                        _TwinCathelper.Client.DeleteDeviceNotification((uint)item);
                    }
                }
                _TwinCathelper.Client.AdsNotification -= Client_AdsNotificationFoe;
            }
        }
        private bool GetSlaveState(ESlaveStateMachine _EreqState)
        {
            DeviceStatusIndex = $"Requesting Slave State {curStr}";
            log.Log(LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} Requesting Slave State....");
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(1);
            Thread.Sleep(500);
            var state = BitConverter.GetBytes((UInt16)_TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.state}").FirstOrDefault().ReadAnyValue(typeof(UInt16)));
            var aa = (ESlaveStateMachine)state[0];
            switch ((ESlaveStateMachine)state[0])
            {
                case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                    MyDevicesAndBoxes[runningIndex].SlaveState = ((ESlaveStateMachine)state[0]).ToString();
                    MyDevicesAndBoxes[runningIndex].SlaveStateBackground = Brushes.Green;
                    break;
                case ESlaveStateMachine.EC_DEVICE_STATE_INIT:
                case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                case ESlaveStateMachine.RESET_FIRMWARE_REQUIRED:
                    MyDevicesAndBoxes[runningIndex].SlaveState = ((ESlaveStateMachine)state[0]).ToString();
                    MyDevicesAndBoxes[runningIndex].SlaveStateBackground = Brushes.Red;
                    break;
                case ESlaveStateMachine.BAD_FIRMWARE_FILE:
                    MyDevicesAndBoxes[runningIndex].SlaveState = ((ESlaveStateMachine)state[0]).ToString();
                    MyDevicesAndBoxes[runningIndex].SlaveStateBackground = Brushes.Red;
                    break;
                case ESlaveStateMachine.UnAvailable:
                    break;
                default:
                    MyDevicesAndBoxes[runningIndex].SlaveState = ((ESlaveStateMachine)state[0]).ToString()+ " ! Undocumented State !";
                    MyDevicesAndBoxes[runningIndex].SlaveStateBackground = Brushes.Red;
                    break;
            }

            log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} State {state}");
            if (retry < 5)
            {
                log.Log(LogLevel.Warn, $"{(retry != 0 ? ("Retrying : " + retry) : "")} Requesting Slave State Please Wait ....");
                DeviceStatusIndex = $"Retrying[{retry}] Requesting Slave State {curStr}";
                retry++;
                if ((ESlaveStateMachine)state[0] != _EreqState)
                {
                    Thread.Sleep(2000);
                    return GetSlaveState(_EreqState);
                }
                else
                {
                    _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(0);
                    return true;
                }
            }
            else
            {
                if (_EreqState == ESlaveStateMachine.EC_DEVICE_STATE_OP)
                {
                    if (ChangeBoxState(ESlaveStateMachine.EC_DEVICE_STATE_OP))
                    {
                        DeviceStatusIndex = $"Slave State {ESlaveStateMachine.EC_DEVICE_STATE_OP} {curStr}";
                        log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr}" +
                            $" Configuring Slave State To {ESlaveStateMachine.EC_DEVICE_STATE_OP}");
                        return true;
                    }
                    else
                    {
                        Op_Status = false;
                        return false;
                    }
                }
                return false;
            }
        }
        private bool ChangeBoxState(ESlaveStateMachine _EreqState)
        {
            DeviceStatusIndex = $"Changing Slave State to {_EreqState} {curStr}";
            log.Log(LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} Changing Box State : {_EreqState}");
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecs_reqState}").FirstOrDefault().WriteValue((int)_EreqState);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(1);
            return EcsWait(_EreqState);
        }

        private bool FoeWait()
        {
            DeviceStatusIndex = $"Foe Wait {curStr}";
            Thread.Sleep(5000);
            if (!MyDevicesAndBoxes[runningIndex].Foe_bError)
            {
                if (MyDevicesAndBoxes[runningIndex].Foe_bBusy | MyDevicesAndBoxes[runningIndex].Foe_nPercent != 100)
                {
                    DeviceStatusIndex = $"Foe Busy {curStr}";
                    log.Log(LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} FOE System is busy please wait.");
                    return FoeWait();
                }
                else
                {
                    Foe_Status = true;
                    retry = 0;
                    return GetSlaveState(ESlaveStateMachine.EC_DEVICE_STATE_OP);
                }
            }
            else
            {
                Foe_Status = false;
                return false;
            }
        }

        private bool EcsWait(ESlaveStateMachine _EreqState)
        {
            DeviceStatusIndex = $"Ecs Wait {curStr}";
            Thread.Sleep(2000);
            if (!MyDevicesAndBoxes[runningIndex].Ecs_bError)
            {
                if (MyDevicesAndBoxes[runningIndex].Ecs_bBusy)
                {
                    DeviceStatusIndex = $"Ecs Busy {curStr}";
                    log.Log(LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr}  Ecs System is busy please wait.");
                    Thread.Sleep(1000);
                    return EcsWait(_EreqState);
                }
                retry = 0;
                return GetSlaveState(_EreqState);
            }
            else
            {
                return false;
            }
        }

        private void Client_AdsNotificationFoe(object sender, AdsNotificationEventArgs e)
        {
            var pdd = (UInt16)(_TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nSlaveAddr}").FirstOrDefault().ReadAnyValue(typeof(UInt16)));

            if (pdd != 0)
            {
                var _mysymbol = e.UserData as Symbol;
                TCSymbols aa;
                Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                switch (aa)
                {
                    case TCSymbols.foe_dwPass:
                        break;

                    case TCSymbols.foe_nSlaveAddr:
                        break;

                    case TCSymbols.foe_eMode:
                        break;

                    case TCSymbols.foe_sPathName:
                        break;

                    case TCSymbols.foe_bLoad:
                        MyDevicesAndBoxes[runningIndex].Foe_bLoad = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(LogLevel.Info, $"{TCSymbols.foe_bLoad} : {MyDevicesAndBoxes[runningIndex].Foe_bLoad}");
                        break;

                    case TCSymbols.foe_bBusy:
                        MyDevicesAndBoxes[runningIndex].Foe_bBusy = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyDevicesAndBoxes[runningIndex].Foe_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.foe_bBusy} : {MyDevicesAndBoxes[runningIndex].Foe_bBusy}");
                        break;

                    case TCSymbols.foe_bError:
                        MyDevicesAndBoxes[runningIndex].Foe_bError = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyDevicesAndBoxes[runningIndex].Foe_bError == false ? LogLevel.Info : LogLevel.Error, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.foe_bError} : {MyDevicesAndBoxes[runningIndex].Foe_bError}");
                        break;

                    case TCSymbols.foe_nErrID:
                        MyDevicesAndBoxes[runningIndex].Foe_nErrID = BitConverter.ToUInt32(e.Data.ToArray());
                        DeviceStatusIndex = ((AdsErrorCode)MyDevicesAndBoxes[runningIndex].Foe_nErrID).ToString() + $" {curStr}";
                        log.Log(MyDevicesAndBoxes[runningIndex].Foe_nErrID == 0 ? LogLevel.Info : LogLevel.Error, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.foe_nErrID} : {(AdsErrorCode)MyDevicesAndBoxes[runningIndex].Foe_nErrID}");
                        break;

                    case TCSymbols.foe_nBytesWritten:
                        MyDevicesAndBoxes[runningIndex].Foe_nBytesWritten = BitConverter.ToUInt32(e.Data.ToArray());
                        log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.foe_nBytesWritten} : {MyDevicesAndBoxes[runningIndex].Foe_nBytesWritten}");
                        break;

                    case TCSymbols.foe_nPercent:
                        MyDevicesAndBoxes[runningIndex].Foe_nPercent = BitConverter.ToUInt32(e.Data.ToArray());
                        OverAllProgress = (uint)((double)MyDevicesAndBoxes[runningIndex].Foe_nPercent / (double)checkedSlaves.Count + (100 / (double)checkedSlaves.Count) * (cur));
                        log.Log(LogLevel.Info, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.foe_nPercent} : {MyDevicesAndBoxes[runningIndex].Foe_nPercent}");
                        break;

                    case TCSymbols.ecs_bExecute:
                        break;

                    case TCSymbols.ecs_reqState:
                        break;

                    case TCSymbols.ecs_bBusy:
                        MyDevicesAndBoxes[runningIndex].Ecs_bBusy = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyDevicesAndBoxes[runningIndex].Ecs_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.ecs_bBusy} : {MyDevicesAndBoxes[runningIndex].Ecs_bBusy}");
                        break;

                    case TCSymbols.ecs_bError:
                        MyDevicesAndBoxes[runningIndex].Ecs_bError = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyDevicesAndBoxes[runningIndex].Ecs_bError == false ? LogLevel.Info : LogLevel.Error, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.ecs_bError} : {MyDevicesAndBoxes[runningIndex].Ecs_bError}");
                        break;

                    case TCSymbols.ecs_nErrId:
                        MyDevicesAndBoxes[runningIndex].Ecs_nErrId = BitConverter.ToUInt32(e.Data.ToArray());
                        DeviceStatusIndex = ((AdsErrorCode)MyDevicesAndBoxes[runningIndex].Ecs_nErrId).ToString();
                        log.Log(MyDevicesAndBoxes[runningIndex].Ecs_nErrId == 0 ? LogLevel.Info : LogLevel.Error, $"{TCSymbols.foe_nSlaveAddr} : {MyDevicesAndBoxes[runningIndex].Foe_nSlaveAddr} ==> {TCSymbols.ecs_nErrId} : {(AdsErrorCode)MyDevicesAndBoxes[runningIndex].Ecs_nErrId}");
                        break;

                    case TCSymbols.ecs_currState:
                        break;

                    default:
                        break;
                }
            }
        }
        private void ResetTwinCatVariables()
        {
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);
            _TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(0);
        }
        private void ResetFoeCollection()
        {
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                MyDevicesAndBoxes.Add(new FoeModel()
                {
                    Foe_nSlaveAddr = _TwinCathelper.SAmsPort[i],
                    BusyForeground = Brushes.White,
                    ErrorForeground = Brushes.White,
                    IsChecked = true
                });
        }
        private void ResetDeviceCollection()
        {
            for (int i = 0; i < MyDevicesAndBoxes.Count; i++)
            {
                MyDevicesAndBoxes[i].DateTimeLU = "";
                MyDevicesAndBoxes[i].Foe_nPercent = 0;
                MyDevicesAndBoxes[i].Foe_nErrID = 0;
                MyDevicesAndBoxes[i].Foe_bError = false;
                MyDevicesAndBoxes[i].Foe_nBytesWritten = 0;
                MyDevicesAndBoxes[i].Foe_bBusy = false;
                MyDevicesAndBoxes[i].Foe_Status = false;
                MyDevicesAndBoxes[i].SlaveState = "";
                MyDevicesAndBoxes[i].SlaveStateBackground = Brushes.Transparent;
            }
        }
        private readonly ITwinCathelper _TwinCathelper; /*{ get { return App.AppHost.Services.GetRequiredService<ITwinCathelper>(); } }*/
        public FoeViewModel(ITwinCathelper twinCathelper )
        {
            _TwinCathelper = twinCathelper;
            if (this._TwinCathelper.Client==null)
            {
                return;
            }
            MyDevicesAndBoxes = new();
            IsCheckedAll = true;
            ResetFoeCollection();

            BT_FirmwareEnable = true;
            //Foe_dwPass = "287454020";
            Foe_dwPass = "11223344";
            log = LogManager.GetLogger("Motor Firmaware Update");
            UpdateFirmwareCommand = new RelayCommand(o =>
            {
                if (ValidateMyString(Foe_sPathName))
                {
                    BT_FirmwareEnable = false;
                    TUpdate_Status?.Invoke(null, false);
                    var FoeWroker = new Thread(UpdateFirmwareMeth) { IsBackground = true };
                    FoeWroker.Start();
                }
                else
                    return;
            });
            BrowseFirmwareCommand = new RelayCommand(o => { BrowseFirmwareFile(); });
            RefreshDataGridCommand = new RelayCommand(o =>
            {

            });
            CheckAllCommand = new RelayCommand(o =>
            {
                for (int i = 0; i < MyDevicesAndBoxes.Count; i++)
                    MyDevicesAndBoxes[i].IsChecked = (bool)IsCheckedAll;
            });

            ResetTwinCatVariables();
        }


    }
}
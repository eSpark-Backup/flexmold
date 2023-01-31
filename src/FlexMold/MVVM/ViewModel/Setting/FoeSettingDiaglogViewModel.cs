using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using NLog.Fluent;
using SMBLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace FlexMold.MVVM.ViewModel.Setting
{
    class FoeSettingDiaglogViewModel : ObservableObject
    {
        Logger log;
        public Foe MyFoe { get; set; }
        public RelayCommand UpdateFirmwareCommand { get; set; }
        public RelayCommand BrowseFirmwareCommand { get; set; }
        private bool _BT_FirmwareEnable;

        public bool BT_FirmwareEnable
        {
            get { return _BT_FirmwareEnable; }
            set { _BT_FirmwareEnable = value; OnPropertyChanged(); }
        }

        public int MyProperty { get; set; }
        string sAmsNetId;
        UInt16[] sAmsPort;

        public FoeSettingDiaglogViewModel()
        {
            BT_FirmwareEnable = true;
            MyFoe = new Foe() { BusyForeground = Brushes.White, ErrorForeground = Brushes.White };
            MyFoe.Foe_dwPass = 287454020;
            log = LogManager.GetLogger("Motor Firmaware Update");
            UpdateFirmwareCommand = new RelayCommand(o =>
            {
                BT_FirmwareEnable = false;
                var FoeWroker = new Thread(UpdateFirmwareMeth) { IsBackground = true };
                FoeWroker.Start();
            });
            BrowseFirmwareCommand = new RelayCommand(o => { BrowseFirmwareFile(); });
            sAmsNetId = (string)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sAmsNetId}").FirstOrDefault().ReadAnyValue(typeof(string));
            sAmsPort = (UInt16[])TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sAmsPort}").FirstOrDefault().ReadAnyValue(typeof(UInt16[]));

            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(0);
        }
        private string _DeviceStatusIndex;

        public string DeviceStatusIndex
        {
            get { return _DeviceStatusIndex; }
            set { _DeviceStatusIndex = value; OnPropertyChanged(); }
        }

        void BrowseFirmwareFile()
        {

            using (var dialog = new OpenFileDialog())
            {
                log.Log(LogLevel.Info, $"Select Project Folder");
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    MyFoe.Foe_sPathName = new FileInfo(dialog.FileName).FullName;
                }
            }
        }
        void ResetSetContols()
        {
            MyFoe.Foe_nPercent = 0;
            MyFoe.Foe_bError = false;
            MyFoe.Foe_bBusy = false;
            MyFoe.Foe_nErrID = 0;
            MyFoe.Ecs_bError = false;
            MyFoe.Ecs_bBusy = false;
            MyFoe.Ecs_nErrId = 0;
        }
        uint NSymbolfoe_nErrID = 0;
        uint NSymbolfoe_bBusy = 0;
        uint NSymbolfoe_bError = 0;
        uint NSymbolfoe_nPercent = 0;
        uint NSymbolfoe_nBytesWritten = 0;
        uint NSymbolecs_bBusy = 0;
        uint NSymbolecs_bError = 0;
        uint NSymbolecs_nErrId = 0;
        List<uint> NotificatioHandles;
        void RegisterNotification()
        {
            NotificatioHandles = new List<uint>();
            TwinCathelper.Client.AdsNotification += Client_AdsNotificationFoe;
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.foe_nErrID}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.foe_bBusy}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.foe_bError}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.foe_nPercent}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.foe_nBytesWritten}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.ecs_bBusy}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.ecs_bError}"));
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{EFoeSymbols.ecs_nErrId}"));
        }
        bool Init_Status = false;
        bool Foe_Status = false;
        bool Op_Status = false;
        void UpdateFirmwareMeth()
        {
            try
            {
                FCAModel.FoeStatuses = new List<Foe>();
                RegisterNotification();
                TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_dwPass}").FirstOrDefault().WriteValue(MyFoe.Foe_dwPass);
                TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sPathName}").FirstOrDefault().WriteValue(MyFoe.Foe_sPathName);
                MyFoe.Foe_sNetId = sAmsNetId;
                for (UInt16 i = 0; i < sAmsPort.Length; i++)
                {
                    ResetSetContols();
                    DeviceStatusIndex = $"Updating {i + 1}/{sAmsPort.Length}";
                    MyFoe.Foe_nSlaveAddr = sAmsPort[i];
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sNetId}").FirstOrDefault().WriteValue(MyFoe.Foe_sNetId);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(MyFoe.Foe_nSlaveAddr);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);
                    Thread.Sleep(1000);
                    DeviceStatusIndex = $"Initializing {i + 1}/{sAmsPort.Length}";
                    if (ChengeBoxState(EreqState.EC_DEVICE_STATE_INIT))
                    {
                        Init_Status = true;
                        TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
                        DeviceStatusIndex = $"Setted into : {EreqState.EC_DEVICE_STATE_INIT} {i + 1}/{sAmsPort.Length}";
                        log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} State Updated");
                        TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(1);
                        DeviceStatusIndex = $"Downloading {i + 1}/{sAmsPort.Length}";
                        if (FoeWait())
                        {
                            Op_Status = true;
                            log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Update Completed");
                        }
                        else
                        {
                            Op_Status = false;
                            log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Update Failed");
                        }
                    }
                    else
                    {
                        Init_Status = false;
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Deveice State not set");
                    }
                    MyFoe.Init_Status = Init_Status;
                    MyFoe.Foe_Status = Foe_Status;
                    MyFoe.Op_Status = Op_Status;
                    FCAModel.FoeStatuses.Add(MyFoe.ShallowCopy());
                }
                var updatedDeviceCount = FCAModel.FoeStatuses.Where(a => (a.Init_Status == true) & (a.Foe_Status == true) & (a.Op_Status == true)).Count();

                foreach (var item in FCAModel.FoeStatuses)
                {
                    log.Log(LogLevel.Info, $"\tSlave Addr : {item.Foe_nSlaveAddr} : \n" +
                        $" \t Init Status = {item.Init_Status}\n" +
                        $" \t Foe Status = {item.Foe_Status}\n" +
                        $" \t OP Status = {item.Init_Status}\n" +
                        $" \t Foe_nErrID = {(AdsErrorCode)item.Foe_nErrID}"
                        );
                }
                log.Log(LogLevel.Info, $"FOE Update Summary \n " +
                   $"Total Devices : {TwinCathelper.DeviceCount} ." +
                   $"\n Sucessful Update Operations : {updatedDeviceCount} out of {FCAModel.FoeStatuses.Count}");

                DeviceStatusIndex = $"{updatedDeviceCount}/{FCAModel.FoeStatuses.Count} Updated";
            }
            catch (Exception ex)
            {
                DeviceStatusIndex = ex.Message;
                log.Log(LogLevel.Error, $"{ex.Message}");
            }
            finally
            {
                BT_FirmwareEnable = true;
                DeleteNotifications();
            }
        }
        void DeleteNotifications()
        {
            foreach (var item in NotificatioHandles)
            {
                TwinCathelper.Client.DeleteDeviceNotification(item);
            }
            TwinCathelper.Client.AdsNotification -= Client_AdsNotificationFoe;
        }
        int retry = 0;
        bool GetSlaveState(EreqState _EreqState)
        {
            log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Requesting Slave Box State....");
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(1);
            Thread.Sleep(500);
            var state = BitConverter.GetBytes((UInt16)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.state}").FirstOrDefault().ReadAnyValue(typeof(UInt16)));
            log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} State {state}");
            if (retry < 5)
            {
                log.Log(LogLevel.Warn, $"{(retry != 0 ? ("Retrying : " + retry) : "")} Requesting Slave State Please Wait ....");
                retry++;
                if ((EreqState)state[0] != _EreqState)
                {
                    Thread.Sleep(2000);
                    return GetSlaveState(_EreqState);
                }
                else
                {
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(0);
                    return true;
                }
            }
            else
            {
                if (_EreqState == EreqState.EC_DEVICE_STATE_OP)
                {
                    if (ChengeBoxState(EreqState.EC_DEVICE_STATE_OP))
                    {
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr}" +
                            $" Configuraing Device State To {EreqState.EC_DEVICE_STATE_OP}");
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
        bool ChengeBoxState(EreqState _EreqState)
        {
            log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Changing Box State : {_EreqState}");
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_reqState}").FirstOrDefault().WriteValue((int)_EreqState);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(1);
            return EcsWait(_EreqState);
        }
        bool FoeWait()
        {
            Thread.Sleep(5000);
            if (!MyFoe.Foe_bError)
            {
                if (MyFoe.Foe_bBusy | MyFoe.Foe_nPercent != 100)
                {
                    log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} FOE System is busy please wait.");
                    return FoeWait();
                }
                else
                {
                    Foe_Status = true;
                    retry = 0;
                    return GetSlaveState(EreqState.EC_DEVICE_STATE_OP);
                }
            }
            else
            {
                Foe_Status = false;
                return false;
            }
        }
        bool EcsWait(EreqState _EreqState)
        {
            Thread.Sleep(2000);
            if (!MyFoe.Ecs_bError)
            {
                if (MyFoe.Ecs_bBusy)
                {
                    log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr}  Ecs System is busy please wait.");
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
            var pdd = (UInt16)(TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nSlaveAddr}").FirstOrDefault().ReadAnyValue(typeof(UInt16)));

            if (pdd != 0)
            {
                var _mysymbol = e.UserData as Symbol;
                EFoeSymbols aa;
                Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                switch (aa)
                {
                    case EFoeSymbols.foe_dwPass:
                        break;
                    case EFoeSymbols.foe_sNetId:
                        break;
                    case EFoeSymbols.foe_nSlaveAddr:
                        break;
                    case EFoeSymbols.foe_eMode:
                        break;
                    case EFoeSymbols.foe_sPathName:
                        break;
                    case EFoeSymbols.foe_bLoad:
                        MyFoe.Foe_bLoad = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_bLoad} : {MyFoe.Foe_bLoad}");
                        break;
                    case EFoeSymbols.foe_bBusy:
                        MyFoe.Foe_bBusy = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyFoe.Foe_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bBusy} : {MyFoe.Foe_bBusy}");
                        break;
                    case EFoeSymbols.foe_bError:
                        MyFoe.Foe_bError = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyFoe.Foe_bError == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bError} : {MyFoe.Foe_bError}");
                        break;
                    case EFoeSymbols.foe_nErrID:
                        MyFoe.Foe_nErrID = BitConverter.ToUInt32(e.Data.ToArray());
                        DeviceStatusIndex = ((AdsErrorCode)MyFoe.Foe_nErrID).ToString();
                        log.Log(MyFoe.Foe_nErrID == 0 ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nErrID} : {(AdsErrorCode)MyFoe.Foe_nErrID}");
                        break;
                    case EFoeSymbols.foe_nBytesWritten:
                        MyFoe.Foe_nBytesWritten = BitConverter.ToUInt32(e.Data.ToArray());
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nBytesWritten} : {MyFoe.Foe_nBytesWritten}");
                        break;
                    case EFoeSymbols.foe_nPercent:
                        MyFoe.Foe_nPercent = BitConverter.ToUInt32(e.Data.ToArray());
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nPercent} : {MyFoe.Foe_nPercent}");
                        break;
                    case EFoeSymbols.ecs_bExecute:
                        break;
                    case EFoeSymbols.ecs_reqState:
                        break;
                    case EFoeSymbols.ecs_bBusy:
                        MyFoe.Ecs_bBusy = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyFoe.Ecs_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.ecs_bBusy} : {MyFoe.Ecs_bBusy}");
                        break;
                    case EFoeSymbols.ecs_bError:
                        MyFoe.Ecs_bError = BitConverter.ToBoolean(e.Data.ToArray());
                        log.Log(MyFoe.Ecs_bError == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.ecs_bError} : {MyFoe.Ecs_bError}");
                        break;
                    case EFoeSymbols.ecs_nErrId:
                        MyFoe.Ecs_nErrId = BitConverter.ToUInt32(e.Data.ToArray());
                        DeviceStatusIndex = ((AdsErrorCode)MyFoe.Ecs_nErrId).ToString();
                        log.Log(MyFoe.Ecs_nErrId == 0 ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.ecs_nErrId} : {(AdsErrorCode)MyFoe.Ecs_nErrId}");
                        break;
                    case EFoeSymbols.ecs_currState:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

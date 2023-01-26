using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using NLog.Fluent;
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
        void RegisterNotification()
        {
            TwinCathelper.Client.AdsNotification += Client_AdsNotification2;
            var Symbolfoe_nErrID = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault();
            NSymbolfoe_nErrID = TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nErrID.IndexGroup, Symbolfoe_nErrID.IndexOffset,
            (int)(Symbolfoe_nErrID.Size), NotificationSettings.Default, Symbolfoe_nErrID);

            var Symbolfoe_bBusy = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault();
            NSymbolfoe_bBusy = TwinCathelper.Client.AddDeviceNotification(Symbolfoe_bBusy.IndexGroup, Symbolfoe_bBusy.IndexOffset,
               (int)(Symbolfoe_bBusy.Size), NotificationSettings.Default, Symbolfoe_bBusy);

            var Symbolfoe_bError = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault();
            NSymbolfoe_bError = TwinCathelper.Client.AddDeviceNotification(Symbolfoe_bError.IndexGroup, Symbolfoe_bError.IndexOffset,
               (int)(Symbolfoe_bError.Size), NotificationSettings.Default, Symbolfoe_bError);

            var Symbolfoe_nPercent = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault();
            NSymbolfoe_nPercent = TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nPercent.IndexGroup, Symbolfoe_nPercent.IndexOffset,
               (int)(Symbolfoe_nPercent.Size), NotificationSettings.Default, Symbolfoe_nPercent);

            var Symbolfoe_nBytesWritten = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault();
            NSymbolfoe_nBytesWritten = TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nBytesWritten.IndexGroup, Symbolfoe_nBytesWritten.IndexOffset,
               (int)(Symbolfoe_nBytesWritten.Size), NotificationSettings.Default, Symbolfoe_nBytesWritten);

            var Symbolecs_bBusy = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bBusy}").FirstOrDefault();
            NSymbolecs_bBusy = TwinCathelper.Client.AddDeviceNotification(Symbolecs_bBusy.IndexGroup, Symbolecs_bBusy.IndexOffset,
               (int)(Symbolecs_bBusy.Size), NotificationSettings.Default, Symbolecs_bBusy);

            var Symbolecs_bError = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bError}").FirstOrDefault();
            NSymbolecs_bError = TwinCathelper.Client.AddDeviceNotification(Symbolecs_bError.IndexGroup, Symbolecs_bError.IndexOffset,
               (int)(Symbolecs_bError.Size), NotificationSettings.Default, Symbolecs_bError);

            var Symbolecs_nErrId = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_nErrId}").FirstOrDefault();
            NSymbolecs_nErrId = TwinCathelper.Client.AddDeviceNotification(Symbolecs_nErrId.IndexGroup, Symbolecs_nErrId.IndexOffset,
               (int)(Symbolecs_nErrId.Size), NotificationSettings.Default, Symbolecs_nErrId);
        }
        void UpdateFirmwareMeth()
        {

            try
            {
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
                        TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
                        DeviceStatusIndex = $"Setted into : {EreqState.EC_DEVICE_STATE_INIT} {i + 1}/{sAmsPort.Length}";
                        log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} State Updated");
                        TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(1);
                        DeviceStatusIndex = $"Downloading {i + 1}/{sAmsPort.Length}";
                        if (FoeWait())
                        {
                            DeviceStatusIndex = $"Configuring {i + 1}/{sAmsPort.Length}";
                            log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Update Completed");
                        }
                        else
                        {
                            log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Update Failed");
                        }
                    }
                    else
                    {
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Deveice State not set");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, $"{ex.Message}");
            }
            finally
            {
                DeviceStatusIndex = $"Completed";
                BT_FirmwareEnable = true;
                DeleteNotifications();
            }
        }
        void DeleteNotifications()
        {
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolfoe_nErrID);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolfoe_bBusy);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolfoe_bError);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolfoe_nPercent);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolfoe_nBytesWritten);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolecs_bBusy);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolecs_bError);
            TwinCathelper.Client.DeleteDeviceNotification(NSymbolecs_nErrId);
            TwinCathelper.Client.AdsNotification -= Client_AdsNotification2;
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
            if ((EreqState)state[0] != _EreqState)
            {
                log.Log(LogLevel.Warn, "Sleep GetSlaveState");
                Thread.Sleep(2000);
                return GetSlaveState(_EreqState);
            }
            else
            {
                TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecg_bExecute}").FirstOrDefault().WriteValue(0);
                return true;
            }
        }
        bool ChengeBoxState(EreqState _EreqState)
        {
            log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} Changing Box State : {_EreqState}");
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_reqState}").FirstOrDefault().WriteValue((int)_EreqState);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.ecs_bExecute}").FirstOrDefault().WriteValue(1);
            return EcsWait();
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
                    return GetSlaveState(EreqState.EC_DEVICE_STATE_OP);
                }
            }
            else
            {
                return false;
            }
        }
        bool EcsWait()
        {
            Thread.Sleep(2000);
            if (!MyFoe.Ecs_bError)
            {
                if (MyFoe.Ecs_bBusy)
                {
                    log.Log(LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr}  Ecs System is busy please wait.");
                    Thread.Sleep(1000);
                    return EcsWait();
                }
                return GetSlaveState(EreqState.EC_DEVICE_STATE_INIT);
            }
            else
            {
                return false;
            }
        }
        public Foe MyFoe { get; set; }
        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
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
                        MyFoe.Foe_bLoad = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_bLoad} : {MyFoe.Foe_bLoad}");
                        break;
                    case EFoeSymbols.foe_bBusy:
                        MyFoe.Foe_bBusy = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(MyFoe.Foe_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bBusy} : {MyFoe.Foe_bBusy}");
                        break;
                    case EFoeSymbols.foe_bError:
                        MyFoe.Foe_bError = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(MyFoe.Foe_bError == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bError} : {MyFoe.Foe_bError}");
                        break;
                    case EFoeSymbols.foe_nErrID:
                        MyFoe.Foe_nErrID = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(MyFoe.Foe_nErrID == 0 ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nErrID} : {(AdsErrorCode)MyFoe.Foe_nErrID}");
                        break;
                    case EFoeSymbols.foe_nBytesWritten:
                        MyFoe.Foe_nBytesWritten = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nBytesWritten} : {MyFoe.Foe_nBytesWritten}");
                        break;
                    case EFoeSymbols.foe_nPercent:
                        MyFoe.Foe_nPercent = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nPercent} : {MyFoe.Foe_nPercent}");
                        break;
                    case EFoeSymbols.ecs_bExecute:
                        break;
                    case EFoeSymbols.ecs_reqState:
                        break;
                    case EFoeSymbols.ecs_bBusy:
                        MyFoe.Ecs_bBusy = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(MyFoe.Ecs_bBusy == false ? LogLevel.Info : LogLevel.Warn, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.ecs_bBusy} : {MyFoe.Ecs_bBusy}");
                        break;
                    case EFoeSymbols.ecs_bError:
                        MyFoe.Ecs_bError = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(MyFoe.Ecs_bError == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.ecs_bError} : {MyFoe.Ecs_bError}");
                        break;
                    case EFoeSymbols.ecs_nErrId:
                        MyFoe.Ecs_nErrId = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
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

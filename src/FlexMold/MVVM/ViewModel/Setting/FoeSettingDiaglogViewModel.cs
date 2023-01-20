using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
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
        public int MyProperty { get; set; }
        public FoeSettingDiaglogViewModel(uint _MotorNumber)
        {
            MyFoe = new Foe();
            log = LogManager.GetLogger("Motor Firmaware Update");
            UpdateFirmwareCommand = new RelayCommand(o => { UpdateFirmware(); });
            BrowseFirmwareCommand = new RelayCommand(o => { BrowseFirmwareFile(); });

            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(0);


            TwinCathelper.Client.AdsNotification += Client_AdsNotification2;

            var Symbolfoe_nErrID = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault();
            TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nErrID.IndexGroup, Symbolfoe_nErrID.IndexOffset,
            (int)(Symbolfoe_nErrID.Size), NotificationSettings.Default, Symbolfoe_nErrID);

            var Symbolfoe_bBusy = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault();
            TwinCathelper.Client.AddDeviceNotification(Symbolfoe_bBusy.IndexGroup, Symbolfoe_bBusy.IndexOffset,
               (int)(Symbolfoe_bBusy.Size), NotificationSettings.Default, Symbolfoe_bBusy);

            var Symbolfoe_bError = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault();
            TwinCathelper.Client.AddDeviceNotification(Symbolfoe_bError.IndexGroup, Symbolfoe_bError.IndexOffset,
               (int)(Symbolfoe_bError.Size), NotificationSettings.Default, Symbolfoe_bError);

            var Symbolfoe_nPercent = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault();
            TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nPercent.IndexGroup, Symbolfoe_nPercent.IndexOffset,
               (int)(Symbolfoe_nPercent.Size), NotificationSettings.Default, Symbolfoe_nPercent);

            var Symbolfoe_nBytesWritten = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault();
            TwinCathelper.Client.AddDeviceNotification(Symbolfoe_nBytesWritten.IndexGroup, Symbolfoe_nBytesWritten.IndexOffset,
               (int)(Symbolfoe_nBytesWritten.Size), NotificationSettings.Default, Symbolfoe_nBytesWritten);
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
        void UpdateFirmware()
        {
            try
            {
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    MyFoe.Foe_nSlaveAddr = (ushort)(1001 + i);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_dwPass}").FirstOrDefault().WriteValue(MyFoe.Foe_dwPass);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sNetId}").FirstOrDefault().WriteValue(TwinCathelper.Client.Address.NetId.ToString());
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nSlaveAddr}").FirstOrDefault().WriteValue(MyFoe.Foe_nSlaveAddr);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_sPathName}").FirstOrDefault().WriteValue(MyFoe.Foe_sPathName);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bBusy}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bError}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nErrID}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nBytesWritten}").FirstOrDefault().WriteValue(0);
                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_nPercent}").FirstOrDefault().WriteValue(0);

                    TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{EFoeSymbols.foe_bLoad}").FirstOrDefault().WriteValue(1);
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, $"{ex.Message}");
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
                        log.Log(MyFoe.Foe_bBusy == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bBusy} : {MyFoe.Foe_bBusy}");
                        break;
                    case EFoeSymbols.foe_bError:
                        MyFoe.Foe_bError = (bool)_mysymbol.ReadAnyValue(typeof(bool));
                        log.Log(MyFoe.Foe_bError == false ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_bError} : {MyFoe.Foe_bError}");
                        break;
                    case EFoeSymbols.foe_nErrID:
                        MyFoe.Foe_nErrID = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(MyFoe.Foe_nErrID == 0 ? LogLevel.Info : LogLevel.Error, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nErrID} : {MyFoe.Foe_nErrID}");
                        break;
                    case EFoeSymbols.foe_nBytesWritten:
                        MyFoe.Foe_nBytesWritten = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nBytesWritten} : {MyFoe.Foe_nBytesWritten}");
                        break;
                    case EFoeSymbols.foe_nPercent:
                        MyFoe.Foe_nPercent = (UInt32)_mysymbol.ReadAnyValue(typeof(UInt32));
                        log.Log(LogLevel.Info, $"{EFoeSymbols.foe_nSlaveAddr} : {MyFoe.Foe_nSlaveAddr} ==> {EFoeSymbols.foe_nPercent} : {MyFoe.Foe_nPercent}");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

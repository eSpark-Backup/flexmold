using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel.Home;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FlexMold.MVVM.ViewModel.Setting
{
    internal class LaserProjectorSettingViewModel //: TelnetHelper
    {
        public RelayCommand TelnetConnectCommand { get; set; }
        public RelayCommand TelnetMessageSendCommand { get; set; }
        public RelayCommand TelnetDisConnectCommand { get; set; }
        public RelayCommand ProjectionOnCommand { get; set; }
        public RelayCommand ProjectionOffCommand { get; set; }
        public RelayCommand CallibrateProjectorCommand { get; set; }
        public RelayCommand LaunchLMPCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        private Logger log = LogManager.GetLogger("Setting");

        private LaserProjector _LP;

        public LaserProjector LP
        {
            get {
                return _LP; }
            set
            {
                try { CSVHelper.IpAddres = LP.IpAddres;
                CSVHelper.TelnetPort = LP.TelnetPort;
            }
                catch { }
            _LP = value;
                //OnPropertyChanged();
            }
        }

        public LaserProjectorSettingViewModel()
        {
            string ZlaserIP = "";
            int ZlaserPort = 23;
            String[] ipcollection = null;
            //try
            //{
            //    ZlaserIP = ConfigurationManager.AppSettings["ZlaserIP"].ToString();
            //    ZlaserPort = int.Parse(ConfigurationManager.AppSettings["ZlaserPort"].ToString());
            //}
            //catch (System.Exception ex)
            //{
            //    //log.Log(LogLevel.Error, ex, ex.Message);
            //    TNF.ShowError(ex.Message);
            //}


            DataSet dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", null);

            if (dsMachSet != null)
            {
                foreach (DataTable dt in dsMachSet.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ZlaserIP = dr["LP_IPAddress"].ToString();
                        try { ZlaserPort = int.Parse(dr["LP_Port"].ToString()); } catch { }
                        break;
                    }
                    break;
                }
            }
            //editingEnable = FlexMoldDB.IsEditingEnabled;

            //LP_SelectProj.ItemsSource = 





            LP = new LaserProjector();
            LP.IpAddres = ZlaserIP;
            LP.TelnetPort = ZlaserPort;
            log.Log(LogLevel.Info, $"LaserProj>>>>>>"+ LP.IpAddres+">>>>"+LP.TelnetPort + ">>>>" + ZlaserIP + ">>>>" + ZlaserPort);


            TelnetConnectCommand = new RelayCommand(o => { _ = TelnetHelper.Instance.ConnectTelnet(CSVHelper.IpAddres, CSVHelper.TelnetPort); 
                //try
                //{
                //    //CSVHelper.IpAddres = LP.IpAddres;
                //    //CSVHelper.TelnetPort = LP.TelnetPort;
                //}
                //catch { }
            });
            TelnetMessageSendCommand = new RelayCommand(o => {
                CSVHelper.IpAddres = LP.IpAddres.ToString();
                CSVHelper.TelnetPort = LP.TelnetPort;
                _ = TelnetHelper.Instance.SendTelnetMessgae(); });
            TelnetDisConnectCommand = new RelayCommand(o => { TelnetHelper.Instance.DisconnectTelnet(); });

            ProjectionOnCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZN; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();

                CSVHelper._statusBaseViewModel.HomeLaserStatus = true;
                
            });
            ProjectionOffCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZF; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();
                CSVHelper._statusBaseViewModel.HomeLaserStatus = false;
            });
            CallibrateProjectorCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZD; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();
            });
            LaunchLMPCommand = new RelayCommand(o => {
                TelnetHelper.Instance.DisconnectTelnet(); try { System.Diagnostics.Process.Start("LPM.exe"); } catch { }
            });

            SaveCommand = new RelayCommand(o => { if (FlexMoldDB.IsEditingEnabled) SaveSettings(); else MessageBox.Show("Please login as admin to save changes!"); });
        }

        private void SaveSettings()
        {
            MessageBoxResult  msr = MessageBox.Show("Do you want to save these values to database?","Confirm save",MessageBoxButton.YesNo);
            if (msr == MessageBoxResult.Yes)
            {                
                UpdateSettingsDB();
            }

        }

        private void UpdateSettingsDB()
        {
            //CREATE TABLE "FM_LaserProjectors"(
            //    "LP_IPAddress"  TEXT,
            //    "LP_Port"   TEXT,
            //    "LP_ProjectorName"  INTEGER,
            //    PRIMARY KEY("LP_IPAddress")
            //)
            Dictionary<string, string> dct = new Dictionary<string, string>();
            dct.Add("LP_IPAddress", LP.IpAddres.ToString());
            dct.Add("LP_Port", LP.TelnetPort.ToString());
            dct.Add("LP_ProjectorName", "Zlaser1");
            FlexMoldDB.InsertOrUpdateData("FM_LaserProjectors", "LP_IPAddress='"+LP.IpAddres.ToString()+"' ", dct);
            CSVHelper.IpAddres = LP.IpAddres.ToString();
            CSVHelper.TelnetPort = LP.TelnetPort;
            log.Log(LogLevel.Info, $"Saved projector settings to database successfully!");
        }
    }
}

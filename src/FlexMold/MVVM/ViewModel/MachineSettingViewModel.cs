using FlexMold.Core;
using FlexMold.MVVM.View;
using FlexMold.MVVM.View.MachineTest;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlexMold.MVVM.ViewModel
{
    public class MachineSettingViewModel : ObservableObject, IMachineSettingViewModel
    {
        private Logger log = LogManager.GetLogger("MachineSettings");

        private bool _editingEnable;
        public bool editingEnable
        {
            get { return _editingEnable; }
            set { _editingEnable = value; OnPropertyChanged(); }
        }

        private string _machinePartsProduced;
        public string machinePartsProduced
        {
            get { return _machinePartsProduced; }
            set { _machinePartsProduced = value; OnPropertyChanged(); }
        }

        private string _machineOperatedTime;
        public string machineOperatedTime
        {
            get { return _machineOperatedTime; }
            set { _machineOperatedTime = value; OnPropertyChanged(); }
        }

        private string _calcDays;
        public string calcDays
        {
            get { return _calcDays; }
            set { _calcDays = value; OnPropertyChanged(); }
        }

        private string _machineRadius;
        public string machineRadius
        {
            get { return _machineRadius; }
            set { _machineRadius = value; OnPropertyChanged(); }
        }

        private string _machineSize;
        public string machineSize
        {
            get { return _machineSize; }
            set { _machineSize = value; OnPropertyChanged(); }
        }

        private string _motorCount;

        public string motorCount
        {
            get { return _motorCount; }
            set { _motorCount = value; OnPropertyChanged(); }
        }

        private string _prjFolderLoc;

        public string prjFolderLoc
        {
            get { return _prjFolderLoc; }
            set { _prjFolderLoc = value; OnPropertyChanged(); }
        }

        private string _AdsStateTimeout;

        public string AdsStateTimeout
        {
            get { return _AdsStateTimeout; }
            set { _AdsStateTimeout = value; OnPropertyChanged(); }
        }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand UpdateMachineSettingCommand { get; set; }
        public RelayCommand AdminMachineSettingCommand { get; set; }
        public RelayCommand BrowseProjectFolderCommand { get; set; }

        //private bool _UpdateEnable;
        //public bool UpdateEnable
        //{
        //    get { return _UpdateEnable; }
        //    set { _UpdateEnable = value; OnPropertyChanged(); }
        //}

        private string ConvertSectoDay(uint n)
        {
            try
            {
                uint day = n / (24 * 3600);

                n = n % (24 * 3600);
                uint hour = n / 3600;

                n %= 3600;
                uint minutes = n / 60;

                n %= 60;
                uint seconds = n;

                return (day + ":" + "d " + hour + ":" + "h " + minutes + ":" + "m " + seconds + ":" + "s");
            }
            catch { return ("0:d 0:h 0:m 0s"); }
        }

        public MachineSettingViewModel()
        {
            motorCount = TwinCathelper.DeviceCount.ToString();
            machinePartsProduced = "10000";
            machineRadius = "10";
            machineSize = "1000";
            machineOperatedTime = "3Hr 24Min 33Sec 1121";

            UpdateMachineSettingCommand = new RelayCommand(async => {
                UpdateSettingsDB();
            });

            AdminMachineSettingCommand = new RelayCommand(async o => {
                                await PriveledgeUpView();
                            });

            BrowseProjectFolderCommand = new RelayCommand(async => { FolderLocBrowse(); });

            try { GetSettingsDB(); calcDays = ConvertSectoDay(uint.Parse(machineOperatedTime)); } catch (Exception ex) { string st = ex.Message; calcDays = ConvertSectoDay(1); }
        }

        private void FolderLocBrowse()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Please select Project CSV folder path";
                log.Log(LogLevel.Info, $"Select Project Folder");
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                    prjFolderLoc = new FileInfo(dialog.SelectedPath).FullName;

            }
        }

        private void UpdateSettingsDB()
        {

            //            CREATE TABLE "FM_MachineSetting"(
            //    "Size"  TEXT,
            //    "MinRadius" TEXT,
            //    "MotorCount"    INTEGER,
            //    "PartsProduced" INTEGER,
            //    "TimeInOperation"   TEXT,
            //    "id"    INTEGER,
            //    PRIMARY KEY("id")
            //)

            Dictionary<string, string> dct = new Dictionary<string, string>();
            dct.Add("Size", machineSize.ToString());
            dct.Add("MinRadius", machineRadius.ToString());
            dct.Add("MotorCount", motorCount.ToString());
            dct.Add("PartsProduced", machinePartsProduced.ToString());
            dct.Add("TimeInOperation", machineOperatedTime.ToString());
            dct.Add("AdsStateTimeout", AdsStateTimeout.ToString());
            dct.Add("ProjectFolder", prjFolderLoc.ToString());

            FlexMoldDB.UpdateData("FM_MachineSetting", null, dct);
            try{CSVHelper.TotalNoHrs = uint.Parse(machineOperatedTime);}catch { }
            try{CSVHelper.PartsProduced = int.Parse(machinePartsProduced); } catch { }
            try { CSVHelper.AdsStateTimeout = int.Parse(AdsStateTimeout); } catch { CSVHelper.AdsStateTimeout = 6; }
            try{CSVHelper.prjFolderLoc = prjFolderLoc; } catch { }

            log.Log(LogLevel.Info, $"Updated Machine Settings!");
        }

        private void GetSettingsDB()
        {
            editingEnable = FlexMoldDB.IsEditingEnabled;

            //            CREATE TABLE "FM_MachineSetting"(
            //    "Size"  TEXT,
            //    "MinRadius" TEXT,
            //    "MotorCount"    INTEGER,
            //    "PartsProduced" INTEGER,
            //    "TimeInOperation"   TEXT,
            //    "id"    INTEGER,
            //    PRIMARY KEY("id")
            //)
            //Dictionary<string, string> dct = new Dictionary<string, string>();
            //dct.Add("Size", machineSize.ToString());
            //dct.Add("MinRadius", machineRadius.ToString());
            //dct.Add("MotorCount", motorCount.ToString());
            //dct.Add("PartsProduced", machinePartsProduced.ToString());
            //dct.Add("TimeInOperation", machineOperatedTime.ToString());

            DataSet dsMachSet  = FlexMoldDB.ReadData("FM_MachineSetting", null);

            if (dsMachSet != null)
            {
                foreach (DataTable dt in dsMachSet.Tables)
                {
                    //if (dt.TableName == "FM_MachineSetting")
                        foreach (DataRow dr in dt.Rows)
                        {
                            machineSize = dr["Size"].ToString();
                            machineRadius = dr["MinRadius"].ToString();
                            motorCount = dr["MotorCount"].ToString();
                            machinePartsProduced = dr["PartsProduced"].ToString();
                            machineOperatedTime = dr["TimeInOperation"].ToString();
                        AdsStateTimeout = dr["AdsStateTimeout"].ToString();
                        prjFolderLoc = dr["ProjectFolder"].ToString();
                        break;
                        }
                    break;
                }
                try { CSVHelper.AdsStateTimeout = int.Parse(AdsStateTimeout); }catch{ CSVHelper.AdsStateTimeout = 6; }
                try { if(prjFolderLoc!=null) CSVHelper.prjFolderLoc = prjFolderLoc; else CSVHelper.prjFolderLoc = Environment.CurrentDirectory; } catch { CSVHelper.prjFolderLoc = Environment.CurrentDirectory; }
            }
            editingEnable = FlexMoldDB.IsEditingEnabled;

            //GetSettings ends here!
        }


        private PriveledgeUpView _PriveledgeUpView { get { return App.AppHost.Services.GetRequiredService<PriveledgeUpView>(); } }

        private async Task PriveledgeUpView()
        {
            Logger log = LogManager.GetLogger("Please enter admin password!");
            try
            {
                //string SelectedZlaserIP = "";
                //log.Log(LogLevel.Info, $"Current port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
                //log.Log(LogLevel.Info, $"Please Select Port params from available list ");
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    DialogHost.Show(_PriveledgeUpView, "RootDialog", OpenEventHandler, ClosingEventHandler);
                }));

                //log.Log(LogLevel.Info, $"Selected laser projector {_PriveledgeUpView.LP_ZLaserIPSelect}");
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
                TNF.ShowError(ex.Message);
            }
        }
        private void OpenEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            var src = eventArgs.Source;
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            

            GetPriviledgeUp();
        }

        private void GetPriviledgeUp()
        {
            if (FlexMoldDB.checkAdminPriv(_PriveledgeUpView.LP_ZLaserIPSelect.Text))
            {
                MessageBox.Show("Now you are in Admin mode \r\n Be Carefull when Updating values as it may effect on performance!", "Priveledge Mode Enabled!", MessageBoxButtons.OK);
                FlexMoldDB.IsEditingEnabled = true;
                OnPropertyChanged();
                //UpdateSettingsDB();
            }
            else {
                MessageBox.Show("Please try again!", "Incorrect Password", MessageBoxButtons.OK);
                FlexMoldDB.IsEditingEnabled = false;
            }

        }
    }
}
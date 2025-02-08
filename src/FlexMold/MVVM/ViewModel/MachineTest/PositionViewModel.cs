using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using static FlexMold.Utility.TwinCathelper;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using FlexMold.MVVM.View;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using FlexMold.MVVM.ViewModel.Home;
using Newtonsoft.Json;
using Sylvan.Data.Csv;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    internal class PositionViewModel : ObservableObject, IPositionViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        /// 
        private Logger log = LogManager.GetLogger("MachineTest");
        private CultureInfo vCulture = System.Globalization.CultureInfo.CurrentCulture;
        private ITwinCathelper _TwinCathelper;
        //private IResetViewModel _ResetViewModel { get { return App.AppHost.Services.GetRequiredService<IResetViewModel>(); } }
        public bool DoMachineUserCanceled = false;
        public IList<float> CalibrationData { get; set; }
        public string selectedCSV = "";
        public static bool blnOptionBrowse = true;
        uint[] validSeq01Val = new uint[DeviceCount];
        uint[] validSeq02Val = new uint[DeviceCount];
        uint[] healthyMotorDiag = new uint[DeviceCount];
        uint[] validForHome = new uint[DeviceCount];

        //private bool _editingEnable;
        //public bool editingEnable
        //{
        //    get { return _editingEnable; }
        //    set { _editingEnable = value; OnPropertyChanged(); }
        //}

        //private uint[] _DeviceStatus;
        //public uint[] DeviceStatus
        //{
        //    get { return _DeviceStatus; }
        //    set
        //    {
        //        //SetDeviceStatus(value);
        //        _DeviceStatus = value;
        //    }
        //}

        private int _NumMotorFromProp;

        public int NumMotorFromProp
        {
            get { return _NumMotorFromProp; }
            set
            {
                //if (value <= TwinCathelper.DeviceCount)
                //    _NumMotorFromProp = value;
                //else
                //    MessageBox.Show("Invalid value selected");

                if (value < TwinCathelper.DeviceCount)
                    _NumMotorFromProp = value;
                else
                    MessageBox.Show("Invalid value selected");

                OnPropertyChanged();
            }
        }

        private int _NumMotorToProp;

        public int NumMotorToProp
        {
            get { return _NumMotorToProp; }
            set
            {
                //if (value <= TwinCathelper.DeviceCount)
                //    _NumMotorToProp = value;
                //else
                //    MessageBox.Show("Invalid value selected");

                if (value < TwinCathelper.DeviceCount)
                    _NumMotorToProp = value;
                else
                    MessageBox.Show("Invalid value selected");

                OnPropertyChanged();
            }
        }

        bool bln_MoveUp = true;
        
        //public RelayCommand Moveinmm { get; set; }

        private double _Moveinmm;

        public double Moveinmm
        {
            get { return _Moveinmm; }
            set
            {
                _Moveinmm = Math.Round(value, 2);

                OnPropertyChanged();
            }
        }


        public RelayCommand GetFileBrowse { get; set; }
        public RelayCommand doMoveUp { get; set; }

        public RelayCommand doMoveDown { get; set; }
        public RelayCommand doMoveManually { get; set; }

        public RelayCommand NumMotorFrom { get; set; }
        public RelayCommand NumMotorTo { get; set; }
        public RelayCommand NumMotorManual { get; set; }


        private string _HomingOffsetDataCommand;

        public string HomingOffsetDataCommand
        {
            get { return _HomingOffsetDataCommand; }
            set
            {
                _HomingOffsetDataCommand = value;
                OnPropertyChanged();
            }
        }

        //private Logger log;

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

        //public PositionViewModel() { 
        //    GetHomingProcess();
        //    //editingEnable = FlexMoldDB.IsEditingEnabled;
        //}
        public PositionViewModel(ITwinCathelper twinCathelper)
        {
            //ReadMotorVM = new HeaterView();
            //CurrentView = HeaterStatusVM;
            //editingEnable = FlexMoldDB.IsEditingEnabled;

            //DoMachineUserCanceled = false;
            GetFileBrowse = new RelayCommand(o => { _TwinCathelper = twinCathelper; BrowseProjectFiles();
            });

            doMoveUp = new RelayCommand(o => { _TwinCathelper = twinCathelper; bln_MoveUp = true;
            });
            NumMotorFrom = new RelayCommand(async o => {  _TwinCathelper = twinCathelper;

                //if (!FlexMoldDB.IsEditingEnabled)
                //    await PriveledgeUpView();
                //else
                //    UpdateHomingProcess();
            });
            NumMotorTo = new RelayCommand(o => { });
            NumMotorManual = new RelayCommand(o => { });

            doMoveDown = new RelayCommand(o => { _TwinCathelper = twinCathelper; bln_MoveUp = false;
            });

            //Moveinmm = new RelayCommand(o => {
            //    _TwinCathelper = twinCathelper;
            //});

            doMoveManually = new RelayCommand(o =>
            {
                _TwinCathelper = twinCathelper;

                if (NumMotorFromProp > TwinCathelper.DeviceCount)
                { MessageBox.Show("Invalid value selected Motor From"); return; }
                else if (NumMotorToProp > TwinCathelper.DeviceCount)
                { MessageBox.Show("Invalid value selected Motor To"); return; }
                else if (NumMotorFromProp > NumMotorToProp)
                { MessageBox.Show("Invalid value selected.Outside of bounds"); return; }

                
                if (!blnOptionBrowse && (Moveinmm == null || Moveinmm == 0 || Moveinmm > 1000 || Moveinmm < -1000))
                {
                    //if (selectedCSV == null || selectedCSV.Length < 6 || !File.Exists(selectedCSV))
                    //{ selectedCSV = "";  MessageBox.Show("Please select at least one option from Browsefiles OR MovePosition"); return; }
                    //else { blnOptionBrowse = true; }
                        MessageBox.Show("Please select correct value for MovePosition!");
                        return;
                }
                else
                {
                    //if (selectedCSV == null || selectedCSV.Length < 6 || !File.Exists(selectedCSV))
                    //{ selectedCSV = ""; Moveinmm = 1001; MessageBox.Show("Please select at least one option from Browsefiles OR MovePosition"); return; }
                    //else { 
                    
                    
                    //}
                }
                Project.ProjectViewModel.LastEventPressed = 'P';// H=Home R=Resume S=Stop W= WriteCSV O=Initial F=ForceResume C=Calibrate P=Position
                
                CSVSend(blnOptionBrowse);
                //selectedCSV = ""; blnOptionBrowse = false;

            });

            //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
            //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
            //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

            try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }
            //try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            {
                try
                {

                    //for (int j = 0; j < DeviceCount; j++)
                    //{
                    //    try
                    //    {
                    //        NegativeoffsetResumeProj[j] = 0.00F - _TwinCathelper.PLCMotorsStatuses.mPostion[j];
                    //    }
                    //    catch { }
                    //}

                    //if (System.Globalization.CultureInfo.CurrentCulture.Name.Contains("en-DK"))
                    //{
                    //    vCulture.NumberFormat.NumberDecimalSeparator = ",";
                    //    vCulture.NumberFormat.CurrencyDecimalSeparator = ",";
                    //}
                    //else
                    //{
                    //    vCulture.NumberFormat.NumberDecimalSeparator = ".";
                    //    vCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                    //}

                    String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                    String[] offset_resume = CSVOffset_contents.Split('~');
                    int i = 1;
                    string errmsg = "";
                    //foreach (string negHomin in offset_resume)
                    for (int j = 0; j < DeviceCount; j++)//-10,849304
                    {                        
                        HomingOffsetDataCommand += "Motor "+j +":="+float.Parse(offset_resume[j])+Environment.NewLine;

                        //if (Math.Abs(NegativeoffsetResumeProj[i]) >= CSVHelper.Max_mm)
                        //    errmsg += " Motor" + i + "-" + NegativeoffsetResumeProj[i].ToString() + Environment.NewLine;
                        i++;
                    }

                    i = 0;

                    //if (errmsg.Length > 1)
                    //{
                    //    MessageBox.Show("Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButtons.OK);
                    //    log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                    //    errmsg = "";
                    //}

                    //if (NegativeoffsetResumeProj.Count > 0)
                    //{
                    //    CSVHelper.NegativeoffsetResume = "";
                    //    //foreach (float negHomin in NegativeoffsetResume)
                    //    //{
                    //    //    NegativeoffsetResume[i] = negHomin + float.Parse(offset_resume[i], vCulture);
                    //    //}
                    //    String newHomingOffset = "";
                    //    foreach (float flt in NegativeoffsetResumeProj)
                    //        newHomingOffset += flt.ToString(CultureInfo.InvariantCulture) + ",";

                    //    CSVHelper.NegativeoffsetResume = newHomingOffset.Substring(0, newHomingOffset.Length - 1);

                    //    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }
                    //}
                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");
                }
            }
            
        }

        private async Task CSVSend(bool blnOptionBrowse)
        {

            //if (NumMotorFromProp < 1)
            //    NumMotorFromProp = 1;
            //if (NumMotorToProp < 1)
            //    NumMotorToProp = 1;

            if (!ConfirmMotorsAreFine())
                return;

            if (CSVHelper.ST_timer.IsRunning)
            {
                MessageBox.Show("Operation Still In Progress, Please Try Later!","Machine still busy!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (selectedCSV == null || selectedCSV.Length < 6 || !File.Exists(selectedCSV))
            //{
            //    MessageBox.Show("Please select proper CSV file before starting the process!","Warning! No CSV selected!",MessageBoxButton.OK, MessageBoxImage.Stop);
            //    return;
            //}

            if (!CSVHelper.MachineEnabled)
            {
                MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            log.Log(LogLevel.Info, "blnOptionBrowse>>"+ blnOptionBrowse+ "  NumMotorToProp - NumMotorFromProp>>>"+ NumMotorToProp+" - "+ NumMotorFromProp + " ");


            if (blnOptionBrowse)//if browse button selected
            {
                FileInfo SelectedFile_finf = new FileInfo(selectedCSV);
                List<float> csvdata = ReadCSV_DoubleData(SelectedFile_finf);

                if (csvdata.Count == 0)
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                    TNF.ShowError($"CSV Failed");
                    return;
                }

                if (csvdata.Count != (NumMotorToProp - NumMotorFromProp + 1))
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV," + " Current motors count is: " + csvdata.Count + " But expected is: " + _TwinCathelper.PLCMotors.Length+ Environment.NewLine+
                    $" File doesn't conatain desired data: "+ selectedCSV);
                    TNF.ShowError($"CSV Failed");
                    return;
                }

                if (_TwinCathelper.PLCMotors.Length != csvdata.Count)
                {

                    log.Log(LogLevel.Error, $"Please select correct CSV");
                    log.Log(LogLevel.Error, $"Data and Box counts are not equal");
                    log.Log(LogLevel.Error, $"Data Count : {csvdata.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
                    TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{csvdata.Count}");
                    return;
                }

                //if(!MotorsCheckTimer.IsEnabled)
                //    MotorsCheckTimer.Start();

                //CSVHelper.MaxDisplacement = (float)csvdata.Max();
                RecommendedMaxDisplacement(csvdata);

                //_CSVDiaglogViewModel.MaxRPM = _TwinCathelper.AppSettings.MaxRPM;
                //CSVHelper.MaxRPM = float.Parse(ConfigurationManager.AppSettings["CSV_MaxRPM"].ToString());
                int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;//int.Parse(ConfigurationManager.AppSettings["CSV_RecommendedRPM"].ToString());
                RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);


                string dir = bln_MoveUp ? "Clockwise" : "Anti-Clockwise";


                log.Log(LogLevel.Info, $"Selected Direction : {dir}");
                WriteCSVData(csvdata);
            }
            else //
            {
                try
                {
                    List<float> csvdata = new List<float>(((NumMotorToProp) + 1));// - NumMotorFromProp
                                                                                  //csvdata.AddRange(Enumerable.Repeat((float)Moveinmm, (NumMotorToProp - NumMotorFromProp) + 1));


                    for (int k = 0; k <= ((NumMotorToProp)); k++)//- NumMotorFromProp
                        csvdata.Add((float)Moveinmm);

                    log.Log(LogLevel.Info, $"Added devices {csvdata.Count}" + "  >>>range>>>" + ((NumMotorToProp - NumMotorFromProp) + 1));

                    //if (csvdata.Count == 0)
                    //{
                    //    log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    //    $" File doesn't conatain desired data");
                    //    TNF.ShowError($"CSV Failed");
                    //    return;
                    //}

                    if (csvdata.Count > _TwinCathelper.PLCMotors.Length)
                    {
                        log.Log(LogLevel.Error, $"Data Count : {csvdata.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
                        TNF.ShowError($"MotorCount Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{csvdata.Count}");
                        return;
                    }

                    RecommendedMaxDisplacement(csvdata);

                    int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;
                    RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);
                    string dir = bln_MoveUp ? "Clockwise" : "Anti-Clockwise";


                    log.Log(LogLevel.Info, $"Selected Direction : {dir}");
                    WriteCSVData(csvdata);

                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Info, $"Position Manual err" + ee.Message);
                }
                MotorStatus.IsMotorsIdle = false;
            }
        }

        public List<float> ReadCSV_DoubleData(FileInfo filename)
        {
            Logger log = LogManager.GetLogger("CSVHelper");

            List<float> NegativeoffsetResumeProj = new List<float>(TwinCathelper.DeviceCount);
            CultureInfo vCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB();
            }
            catch { }


            var data = new List<float>();
            if (filename != null)
            {
                if (filename.FullName.EndsWith(".csv"))
                {
                    log.Log(LogLevel.Info, $"Reading CSV File {filename.Name}");
                    try
                    {
                        using (var reader = new StreamReader(filename.FullName))
                        {
                            var options = new CsvDataReaderOptions
                            {
                                HasHeaders = false,
                                BufferSize = 0x10000,
                            };
                            var csvr = CsvDataReader.Create(reader, options);

                            //if old Homing data exist then add to it ie -5 + 10 = 5
                            //if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
                            //{
                            //    try
                            //    {
                            //        String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                            //        String[] offset_resume = CSVOffset_contents.Split('~');
                            //        int i = 0;

                            //        foreach (string negHomin in offset_resume)
                            //        {
                            //            try { NegativeoffsetResumeProj[i] = float.Parse(negHomin, vCulture); } catch { }
                            //            i++;
                            //        }
                            //    }
                            //    catch { }


                            //}


                            float Homingadd = 0.00F;
                            String errmsg = "";
                            while (csvr.Read())
                            {
                                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                                {
                                    try
                                    {
                                        Homingadd = csvr.GetFloat(i+ NumMotorFromProp);

                                        if (Math.Abs(Homingadd) >= CSVHelper.Max_mm)
                                            errmsg += " Motor" + (i + NumMotorFromProp) + "-" + Homingadd.ToString() + Environment.NewLine;
                                    }
                                    catch { Homingadd = csvr.GetFloat(i + NumMotorFromProp); }

                                    data.Add(Homingadd);
                                }
                                break;
                            }

                            if (errmsg.Length > 1)
                            {
                                MessageBox.Show("CSV Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButton.OK);
                                log.Log(LogLevel.Error, "Alert! Max homing exceed " + "CSV Homing for motors exceeds at " + errmsg);
                                errmsg = "";
                                return new List<float>();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex.Message);
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .csv File {filename.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select a valid File");
            log.Info($"CSV data : {JsonConvert.SerializeObject(data)}");
            return data;
        }


        private void WriteCSVData(List<float> data)
        {

            if (!ConfirmMotorsAreFine())
                return;

            checkFirmware();           
            
            //String selectedListRootDir = Directory.GetParent(CSVHelper.LastSelectedCSV).FullName;
            DirectoryInfo selectedListRootDir = new DirectoryInfo(CSVHelper.LastSelectedCSV);

            FileInfo SelectedFile_finf = new FileInfo(CSVHelper.LastSelectedCSV);                        
            
            if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume\\" + selectedListRootDir.Parent.Parent.Name + "-" + SelectedFile_finf.Name + ".Resume"))
            {
                log.Log(LogLevel.Warn, $"Deleting Resume File > "+ CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume\\" + selectedListRootDir.Parent.Parent.Name + "-" + SelectedFile_finf.Name + ".Resume");

                try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume\\" + selectedListRootDir.Parent.Parent.Name + "-" + SelectedFile_finf.Name + ".Resume"); } catch { }
            }

            RecommendedMaxDisplacement(data);
            float calcTime = GetRecommendedRPM();


            for (int i = NumMotorFromProp; i <= NumMotorToProp; i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
                _TwinCathelper.PLCMotors[i].mDirection = bln_MoveUp ? false : true;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = calcTime;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                log.Log(LogLevel.Warn, "Position > WriteCSVData Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + (_TwinCathelper.PLCMotors[i].mDirection ? "Down":"Up")
                + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                + "  >> mRPM: " + CSVHelper.RecommendedRPM
                + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                + "  >> mMode: " + EMode.Project.ToString());
            }

            //GetRecommendedRPM(0);

            //CSVHelper.CompletingTime = _TwinCathelper.PLCMotors.Max().mTime + 3.00F;
            if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                CSVHelper.ST_timer.Restart();

            //if (data.Count != null)
            //    LastmaxVal = data.Max();
            //else
            //    LastmaxVal = _TwinCathelper.PLCMotors.Max().mDisplacement;

            try
            {
                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = false;

                for (int i = NumMotorFromProp; i <= NumMotorToProp; i++)
                    _TwinCathelper.WriteEthercatDeviceData((uint)i);
                log.Log(LogLevel.Info, $"{DeviceCount} csv value(s) has been written sucessfully");

                Thread.Sleep(200);
                uint zero = 0;
                for (uint i = 0; i < DeviceCount; i++)
                    _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
                log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

                //IsPsitionDataProcessing = true;
                //try { LastPositionData = MotorStatus.mPosition[0]; } catch { LastPositionData = 0; }
                //try { progressbar_FinalVal = Math.Abs((int)(_TwinCathelper.PLCMotors[0].mDisplacement)); } catch { }// + Math.Abs((int)CSVHelper.NegativeoffsetResumeFlt.Min())
                //progressbar_timespent = 0;
                //TravelTimeUpdate = false;
                //IsRampCompleted = false;

                //LastEventPressed = 'W';// H=Home R=Resume S=Stop W= WriteCSV O=Initial F=ForceResume C=Calibrate
            }
            catch (Exception ee) { log.Log(LogLevel.Info, "WriteCSVData>>>>" + ee.Message + "---- " + ee.StackTrace.ToLower()); }
        }

        private void checkFirmware()
        {

            try
            {

                String FirmwareNotMatched = "";
                for (uint i = 0; i < DeviceCount; i++)
                {
                    //if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713969130 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713968323 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)//0x662917EA (1713969130=Wed Apr 24 2024 14:32:10 GMT+0000) CSVHelper.MotorsStatusGlobal[0] == "Healthy" && //1713968323=Wed Apr 24 2024 14:18:43 GMT+0000
                    if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1737720238 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)
                        continue;
                    else
                        FirmwareNotMatched += "Motor" + (i + 1) + ",";
                }

                if (!CSVHelper.MachineEnabled)
                {
                    MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (FirmwareNotMatched.Length > 0)
                {
                    MessageBox.Show("Please update the firmware/machine versions of the following motor(s)!" + Environment.NewLine + FirmwareNotMatched, "Wrong firmware/Machine found!", MessageBoxButton.OK, MessageBoxImage.Error);
                    //MessageBox.Show("Please close the application and update the firmware!" + Environment.NewLine + FirmwareNotMatched, "Wrong firmware found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CSVHelper.wrongFirmwareFound = true;

                    return;
                }

            }
            catch { }

        }




        public IMotorStatusViewModel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewModel>(); } }

        public void RecommendedMaxDisplacement(List<float> negdis)
        {
            float chkBiggest = Math.Abs(negdis.Max()) > Math.Abs(negdis.Min()) ? Math.Abs(negdis.Max()) : Math.Abs(negdis.Min());
            CSVHelper.MaxDisplacement = chkBiggest;

        }

        public float GetRecommendedRPM()
        {

            //Every motor must reach exactly same time ie some will move faster and some will slower --- Client requirement
            float MotorTime = (CSVHelper.MaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);
            MotorTime = Math.Abs(MotorTime);

            float fltDelayval = ((CSVHelper.MotorRampDelayValue * 2) + 2);

            //Never will reach
            if (MotorTime < 2.0F) log.Log(LogLevel.Warn, "Shaping: Invalid MotorTime/too Small > " + MotorTime);

            if (MotorTime < fltDelayval)
            {
                MessageBoxResult msgres = MessageBox.Show("Please adjust MotorRampDelay, Current: " + MotorTime + " But required:" + fltDelayval, "Warning! Incorrect Ramp delay value!", MessageBoxButton.YesNo);
                if (MessageBoxResult.No == msgres)
                {
                    log.Log(LogLevel.Error, "Please adjust MotorRampDelay, Current: " + fltDelayval + " But required: " + MotorTime);
                    CSVHelper.CompletingTime = MotorTime + 3.00F;
                    CSVHelper.MotorTime = MotorTime;
                    return MotorTime;
                }
                else MotorTime = fltDelayval;

            }

            if (MotorTime > 900.0F)//Time cant be set more than 15 min.
            {
                MessageBox.Show("Time is out of bound! Max is 15Mins. But counted is " + (MotorTime / 60) + " Mins", "Warning! Time is out of bound!", MessageBoxButton.OK);
                CSVHelper.CompletingTime = 0;
                CSVHelper.MotorTime = 0;
                return 0;
            }

            CSVHelper.CompletingTime = MotorTime + 3.00F;
            CSVHelper.MotorTime = MotorTime;
            return MotorTime;
        }

        public void RecommendedRPM(int CSV_RecommendedRPM, float maxRPM)
        {
            CSVHelper.MotorTime = (CSVHelper.MaxDisplacement * 60) / (CSV_RecommendedRPM * 2);
            if (CSVHelper.MotorTime > 900F)//Time cant be set more than 15 min.
                MessageBox.Show("Time is out of bound! Max is 15Mins. But counted is " + (CSVHelper.MotorTime / 60) + " Mins", "Warning! Time is out of bound!", MessageBoxButton.OK);
        }


        private bool ConfirmMotorsAreFine()
        {
            try
            {
                string Max_mm = CSVHelper.Max_mm.ToString();//ConfigurationManager.AppSettings["Max_mm"].ToString();

                for (int i = 0; i < DeviceCount; i++)
                {
                    if (MotorStatus.mPosition != null)
                    {
                        if (((int)MotorStatus.mPosition[i] > int.Parse(Max_mm)) || ((int)MotorStatus.mPosition[i] < int.Parse("-" + Max_mm)) || Moveinmm >= int.Parse(Max_mm))
                        {
                            String st = "";
                            log.Log(LogLevel.Error, $"Motor no:" + i + " has invalid position!");
                            
                            var data = (int)EMode.Abort;
                            for (uint j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                            { _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray()); st += j + ":" + MotorStatus.mPosition[j] + ","; }

                            log.Log(LogLevel.Warn, $"Position, Stopping Motors > " + st);

                            //_TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray());


                            return false;
                        }
                    }
                    else if (((int)_TwinCathelper.PLCMotorsStatuses.mPostion[i] > int.Parse(Max_mm)) || ((int)_TwinCathelper.PLCMotorsStatuses.mPostion[i] < int.Parse("-" + Max_mm)) || Moveinmm >= int.Parse(Max_mm))
                    {
                        log.Log(LogLevel.Error, $"Motor no:" + i + " has invalid position!");
                        
                        log.Log(LogLevel.Warn, $"Stopping Motors");
                        var data = (int)EMode.Abort;
                        for (uint j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                            _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray());


                        return false;
                    }

                }

                if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                {
                    log.Log(LogLevel.Error, $"Machine status disconnected! ");

                    MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.YesNo);

                    if (dr == MessageBoxResult.No)
                    {
                        DoMachineUserCanceled = true;
                        return true;
                    }

                    return false;
                }
            }
            catch(Exception ee) { log.Log(LogLevel.Error, $"MotorCount exception= "+ee.Message); return false; }
            return true;
        }

        private void BrowseProjectFiles()
        {
            Logger log = LogManager.GetLogger("Browse Project Files");
            try
            {
                try
                {
                    if (!ConfirmMotorsAreFine())
                        return;
                }
                catch { }

                log.Log(LogLevel.Info, $"Loading Project Files Folder");
                //string projectfolder = ConfigurationManager.AppSettings["ProjectFolder"].ToString();
                string projectfolder = "";
                try
                {
                    projectfolder = "C:\\";//CSVHelper.prjFolderLoc+ 

                    try { if (CSVHelper.prjFolderLoc != null) projectfolder = CSVHelper.prjFolderLoc; } catch { }

                }
                catch { }
                System.Windows.Forms.DialogResult result;

                if (!CSVHelper.MachineEnabled)
                {
                    MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var dialog = new System.Windows.Forms.OpenFileDialog())
                {
                    dialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    dialog.InitialDirectory = projectfolder;
                    dialog.Title = "Please select an motors position file in csv format!";

                    result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        FileInfo SelectedFile_finf = new FileInfo(dialog.FileName);
                        selectedCSV = SelectedFile_finf.FullName;
                        CSVHelper.LastSelectedCSV = selectedCSV;
                    }
                    else
                        log.Log(LogLevel.Warn, $"Project folder path not found!");



                }
                //}
                //}
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }            
        }


        //public void HardZeroProcess() {

        //    log.Log(LogLevel.Info, "HardZeroProcess Started successfully!");
        //    try{
        //        if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
        //        {
        //            log.Log(LogLevel.Error, $"Machine status disconnected! ");
        //            MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

        //            if (dr == MessageBoxResult.Cancel)
        //            {
        //                DoMachineUserCanceled = true;
        //                return;
        //            }

        //            return;
        //        }

        //        CalibrateSeq01(); } catch (Exception ee) { log.Log(LogLevel.Warn, "HardZeroProcess Ended with error! " + ee.Message); }
        //    log.Log(LogLevel.Info, "HardZeroProcess Completed successfully!");
        //}


        public void GetHomingProcess()
        {
            log.Log(LogLevel.Info, "Getting Homming data started!");

            //try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

            //if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            //{
            //    try
            //    {
            //        String[] newHomingArr = CSVHelper.NegativeoffsetResume.Split("~");
            //        //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(negHomin, vCulture) + Environment.NewLine;
            //        String newCsvHomeData = "";
            //        int i = 1;
            //        HomingOffsetDataCommand = "";

            //        for (int j = 0; j < DeviceCount; j++)
            //        {
            //            try
            //            {
            //                //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(st, vCulture) + Environment.NewLine;
            //                if ((newHomingArr[j]).Contains('"'))
            //                    HomingOffsetDataCommand += "Motor " + j + ":=" + float.Parse(newHomingArr[j]) + Environment.NewLine;
            //                else
            //                    HomingOffsetDataCommand += "Motor " + j + ":=" + float.Parse(newHomingArr[j], vCulture) + Environment.NewLine;
            //                i++;
            //            }
            //            catch { }
            //        }
            //    }
            //    catch { }
            //}
            //else
            //{
            //    int i = 1;
            //    for (int j = 0; j < DeviceCount; j++)
            //    {
            //        try
            //        {
            //            //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(st, vCulture) + Environment.NewLine;
            //                HomingOffsetDataCommand += "Motor " + j + ":=-0" + Environment.NewLine;
            //            CSVHelper.NegativeoffsetResume += "0~";
            //            i++;
            //        }
            //        catch { }

            //    }
            //    CSVHelper.NegativeoffsetResume = CSVHelper.NegativeoffsetResume.Remove(CSVHelper.NegativeoffsetResume.Length - 1);
            //    CSVHelper.SetNegativeoffsetResumeFlt();
            //}

                log.Log(LogLevel.Info, "Getting Homming data ended!");
        }


        public void UpdateHomingProcess()
        {
            log.Log(LogLevel.Info, "Update Homing started!");
            String errmsg = "";
            //if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            //{
            //    try
            //    {
            //        String[] newHomingArr = HomingOffsetDataCommand.Split(Environment.NewLine);
            //        //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(negHomin, vCulture) + Environment.NewLine;
            //        String newCsvHomeData = "";
            //        int i = 1;
            //        //log.Log(LogLevel.Info, "chkMotorInput11 chkMotorInput>>>>" + DeviceCount.ToString()+"---"+ newHomingArr[0]);
            //        //foreach (String st in newHomingArr)
            //        for (int j = 0; j < DeviceCount; j++)
            //        {
            //            try
            //            {
            //                if (newHomingArr[j].Length > 0)
            //                {
            //                    float chkMotorInput = float.Parse((newHomingArr[j].Substring(newHomingArr[j].IndexOf('=') + 1)).Trim(), vCulture);

            //                    //log.Log(LogLevel.Info, "chkMotorInput chkMotorInput>>>>" + chkMotorInput);

            //                    if (Math.Abs(chkMotorInput) >= CSVHelper.Max_mm)
            //                        errmsg += " Motor" + i + "-" + chkMotorInput.ToString() + Environment.NewLine;

            //                    newCsvHomeData += chkMotorInput.ToString() + "~";//vCulture
            //                }
            //                i++;                            
            //            }
            //            catch { errmsg += " Motor" + i + "-" + newHomingArr[j] + Environment.NewLine; }
            //        }

            //        if (errmsg.Length > 0)
            //        {
            //            MessageBox.Show(errmsg, "Alert! Max homing exceed ", MessageBoxButton.OK);
            //            log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
            //            errmsg = "";
            //            return;                    
            //        }


            //        CSVHelper.NegativeoffsetResume = newCsvHomeData.Substring(0, newCsvHomeData.Length - 1);
            //        CSVHelper.SetNegativeoffsetResumeFlt();


            //        try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

            //        log.Log(LogLevel.Info, "Successfully updated homing data!" + errmsg);

            //        //MessageBox.Show("Homing Status, Data Successfully Updated!", "Records Updated!", MessageBoxButton.OK);

            //    }
            //    catch (Exception ee)
            //    {
            //        log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");

            //        if (errmsg.Length > 0)
            //        {
            //            MessageBox.Show(errmsg, "Alert! Max homing exceed ", MessageBoxButton.OK);
            //            log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
            //            errmsg = "";
            //            return;
            //        }
            //    }
            //}

        }


        //public void HardCalibrateProcess()
        //{
        //    log.Log(LogLevel.Info, "HardCalibrateProcess Started successfully!");
        //    try {
        //        if (!CSVHelper.MachineEnabled)//&& !DoMachineUserCanceled
        //        {
        //            log.Log(LogLevel.Error, $"Machine status disconnected! ");
        //            MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

        //            if (dr == MessageBoxResult.Cancel)
        //            {
        //                //DoMachineUserCanceled = true;
        //                return;
        //            }

        //            return;
        //        }

        //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = false;
        //        //CalibrateSeq02(); 
        //        CalibrateSeq02_HomeUp();

        //    //Line1:
        //    //    Thread.Sleep(1000);
        //    //    if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted)
        //    //        goto Line1;

        //    //    CalibrateSeq02_HomeDown();

        //    } catch(Exception ee) { log.Log(LogLevel.Warn, "CalibrateSeq02 Ended with error! "+ee.Message); }
        //    log.Log(LogLevel.Info, "HardCalibrateProcess Completed successfully!");
        //}

        //void CalibrateSeq02_HomeUp() {

        //    for (int i = 0; i < DeviceCount; i++)
        //    {
        //        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Resume;

        //        log.Log(LogLevel.Info, "CalibrateHard-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + (_TwinCathelper.PLCMotors[i].mDirection ? "Down":"Up")
        //        + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
        //        + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
        //        + "  >> mMode: " + EMode.Project.ToString());
        //    }

        //    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
        //    log.Log(LogLevel.Info, $"{DeviceCount} CalibrateHard value(s) has been written sucessfully");

        //    Thread.Sleep(200);
        //    uint zero = 0;
        //    for (uint i = 0; i < DeviceCount; i++)
        //        _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
        //    log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

        //}

        //private bool ValidateCSVData()
        //{
        //    var log = LogManager.GetLogger("CSV Validator");
        //    if (CalibrationData.Count != 0)
        //    {

        //        try
        //        {
        //            if (_TwinCathelper!=null && _TwinCathelper.PLCMotors != null && _TwinCathelper.PLCMotors.Length == CalibrationData.Count)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                log.Log(LogLevel.Error, $"Please select correct CSV");
        //                log.Log(LogLevel.Error, $"Data and Box count are not equal");
        //                //log.Log(LogLevel.Error, $"Data Count : {CalibrationData.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
        //                //TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{CalibrationData.Count}");
        //            }
        //        }
        //        catch { log.Log(LogLevel.Info, $"motorsCountError>>>>"); }
        //    }
        //    else
        //    {
        //        log.Log(LogLevel.Error, $"Please select correct CSV\n" +
        //            $" File doesn't conatain desired data");
        //        TNF.ShowError($"CSV Failed");
        //    }
        //    return false;
        //}

        //private void WriteDistpacementCSVData(IList<float> data)
        //{
        //    var log = LogManager.GetLogger("Calibration");
        //    for (int i = 0; i < data.Count; i++)
        //    {
        //        _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
        //    }
        //    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
        //    log.Log(LogLevel.Info, $"{data.Count} csv value(s) for Calibration has been written sucessfully");
        //    log.Log(LogLevel.Info, $"{JsonSerializer.Serialize(data)}");
        //}



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
                MessageBox.Show("Now you are in Admin mode \r\n Be Carefull when Updating values as it may effect on performance!", "Priveledge Mode Enabled!", MessageBoxButton.OK);
                FlexMoldDB.IsEditingEnabled = true;
                OnPropertyChanged();
                //UpdateSettingsDB();
            }
            else
            {
                MessageBox.Show("Please try again!", "Incorrect Password", MessageBoxButton.OK);
                FlexMoldDB.IsEditingEnabled = false;
            }

        }

        private PriveledgeUpView _PriveledgeUpView { get { return App.AppHost.Services.GetRequiredService<PriveledgeUpView>(); } }
        private async Task PriveledgeUpView()
        {
            Logger log = LogManager.GetLogger("Please enter admin password!");
            MessageBox.Show("After Successful login, Please update again!", "Warning!", MessageBoxButton.OK);
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

    }
}

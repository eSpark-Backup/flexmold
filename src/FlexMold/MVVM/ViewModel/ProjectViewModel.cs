using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.MVVM.ViewModel.Home;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using static FlexMold.Utility.TwinCathelper;
using FlexMold.MVVM.ViewModel.MachineTest;
using FlexMold.MVVM.View.MachineTest;
using System.Globalization;
using System.Text;
using FlexMold.Contols;
using System.Data;
using System.Diagnostics;

namespace FlexMold.MVVM.ViewModel.Project
{
    public class ProjectViewModel : ObservableObject, IProjectViewModel
    {
        private MachineTestViewModel MyMachineTestViewModel { get { return App.AppHost.Services.GetRequiredService<MachineTestViewModel>(); } }
        private String ZlaserIP = "";//ConfigurationManager.AppSettings["ZlaserIP"].ToString();
        private int ZlaserPort = 0;//int.Parse(ConfigurationManager.AppSettings["ZlaserPort"].ToString());
        private Stopwatch MachineTotalTimeSW = new Stopwatch();
        private string _toggleStopResumeButtonText="Stop";
        private string _toggleShapeHomeButtonText = "Home";
        private int LastProgressval = 0;
        private int progressActonCounter = 0;
        //private int UpdateProgressStatusSeconds = 5;
        private int intSpecifiedSecs = 60;
        private float LastmaxVal = 0.00F;
        private bool Timer_ElapsedBusy = false;
        public static DispatcherTimer MotorsCheckTimer = new DispatcherTimer();
        public static DispatcherTimer StallErrMotorsCheckTimer = new DispatcherTimer();
        public string selectedCSV = "";        
        public static bool IsRampCompleted = true;

        public static DispatcherTimer positionProcessCompleted_Timer = new DispatcherTimer();
        //private bool IsPsitionDataProcessing = false;
        private bool DoMachineUserCanceled = false;
        private float LastPositionData = 0.00F;
        private int positionCounterTimer = 0;
        private int resumeCounterTimer = 0;
        //private int progressbar_InitVal = 0;
        private int progressbar_FinalVal = 100;
        private int progressbar_timespent = 0;
        private bool TravelTimeUpdate = true;
        public LaserProjector MyPorjector { get; set; }
        public string toggleStopResumeButtonText
        {
            get { 
                return _toggleStopResumeButtonText; 
            }
            set { 
                    _toggleStopResumeButtonText = value;
                //OnPropertyChanged(_toggleStopResumeButtonText);
                OnPropertyChanged();
            }
        }

        private string _selectedListRootDir;
        public string selectedListRootDir
        {
            get {return _selectedListRootDir; }
            set
            {
                _selectedListRootDir = value;
                OnPropertyChanged();
            }
        }


        public string toggleShapeHomeButtonText
        {
            get { return _toggleShapeHomeButtonText; }
            set
            {
                _toggleShapeHomeButtonText = value;                
                OnPropertyChanged();
            }
        }

        public static char LastEventPressed = 'O';// H=Home R=Resume S=Stop W= WriteCSV O=Initial F=ForceResume
        public static bool UserStop = false;
        //public static bool EventStopToggle = false;
        public RelayCommand SendLasFileCommand { get; set; }

        private Logger log = LogManager.GetLogger("Project");
        public RelayCommand SendCSVCommand { get; set; }
        public RelayCommand ShapeHomCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        public RelayCommand StopMotorsCommand { get; set; }
        //public RelayCommand StopResumeButtonText { get; set; }
        public RelayCommand HomeMotorsCommand { get; set; }
        public RelayCommand ForceResumeCommand { get; set; }

        //public int MotorDirValue;
        //public float MotorRampDelayValue;
        //public float MotorTime;
        private float intProgressbar = 0.00F;
        private List<float> csvdata;        
        //private List<bool> NegativeoffsetResumeDirection;//false=0 , true=1
        private bool Isintimer = false;
        string selectedPath = "";
        private CultureInfo vCulture = System.Globalization.CultureInfo.CurrentCulture;
        public IMotorStatusViewModel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewModel>(); } }

        private Model.Project _rootSelectedMyDataModel;
        public Model.Project rootSelectedMyDataModel
        {
            get { return _rootSelectedMyDataModel; }
            set
            {
                _rootSelectedMyDataModel = value;
                SelectedFile = null;
                OnPropertyChanged();
            }
        }

        private List<DirectoryInfo> _projectsList;
        public List<DirectoryInfo> projectsList
        {
            get { return _projectsList; }
            set { _projectsList = value; OnPropertyChanged(); }
        }

        private Model.Project _selectedMyDataModel;
        public Model.Project selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                SelectedFile = null;
                OnPropertyChanged();
                if (value != null)
                {
                    ProjectFilePanel myFile = new ProjectFilePanel();
                    myFile.FileName = value.ProjectName;
                    myFile.FileFullName = selectedListRootDir + "\\" + value.Panel + "\\" + value.ProjectName;
                    myFile.ID = 1;
                    SelectedFile = myFile;
                }
            }
        }
        //private int _motorCount;

        //public int motorCount
        //{
        //    get { return _motorCount; }
        //    set { _motorCount = value; OnPropertyChanged(); }
        //}

        private string _motorMaxTravel;
        public string motorMaxTravel
        {
            get { return _motorMaxTravel; }
            set { _motorMaxTravel = value; OnPropertyChanged(); }
        }

        private string _travelTime;
        public string travelTime
        {
            get { return _travelTime; }
            set { _travelTime = value; OnPropertyChanged(); }
        }

        //private int _machinePartsProduced;
        //public int machinePartsProduced
        //{
        //    get { return _machinePartsProduced; }
        //    set { _machinePartsProduced = value; OnPropertyChanged(); }
        //}

        //private string _machineOperatedTime;
        //public string machineOperatedTime
        //{
        //    get { return _machineOperatedTime; }
        //    set { _machineOperatedTime = value; OnPropertyChanged(); }
        //}

        //private int _machineRadius;
        //public int machineRadius
        //{
        //    get { return _machineRadius; }
        //    set { _machineRadius = value; OnPropertyChanged(); }
        //}

        //private int _machineSize;
        //public int machineSize
        //{
        //    get { return _machineSize; }
        //    set { _machineSize = value; OnPropertyChanged(); }
        //}


        private int _pgProcesStatusVal;

        public int pgProcesStatusVal
        {
            get { return _pgProcesStatusVal; }
            set { _pgProcesStatusVal = value; OnPropertyChanged(); }
        }

        private Porjects _myDataModels;

        public Porjects myDataModels
        {
            get { return _myDataModels; }
            set
            {
                _myDataModels = value;
                OnPropertyChanged();
            }
        }
        private bool _CSVWriteButton;

        public bool CSVWriteButton
        {
            get { return _CSVWriteButton; }
            set { _CSVWriteButton = value; OnPropertyChanged(); }
        }
        
        private bool _IsStopEnable;

        public bool IsStopEnable
        {
            get { return _IsStopEnable; }
            set { _IsStopEnable = value; OnPropertyChanged(); }
        }

        private bool _IsForceResumeValid;

        public bool IsForceResumeValid
        {
            get { return _IsForceResumeValid; }
            set { _IsForceResumeValid = value; OnPropertyChanged(); }
        }

        private bool _IsHomeEnabled;

        public bool IsHomeEnabled
        {
            get { return _IsHomeEnabled; }
            set { _IsHomeEnabled = value; OnPropertyChanged(); }
        }

        private bool _LaserSendButton;

        public bool LaserSendButton
        {
            get { return _LaserSendButton; }
            set { _LaserSendButton = value; OnPropertyChanged(); }
        }
        private float[] _Position;
        public float[] Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
            }
        }
        private ProjectFilePanel _selectedFile;
        private readonly ITwinCathelper _TwinCathelper;
        private readonly LaserProjectorViewModel _laserProjectorViewModel;
        //private ICSVDiaglogViewModel _CSVDiaglogViewModel { get { return App.AppHost.Services.GetRequiredService<ICSVDiaglogViewModel>(); } }
        //private CSVDiaglogView _CSVDiaglogView { get { return App.AppHost.Services.GetRequiredService<CSVDiaglogView>(); } }

        public ProjectFilePanel SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (value == null)
                    return;

                _selectedFile = value;
                CSVWriteButton = false;
                //toggleShapeHomeButtonText = "Home";
                IsHomeEnabled = false;
                IsForceResumeValid = false;
                LaserSendButton = false;

                updateHomeButtonStatus();

                if (value != null)
                {
                    //var se = value.FileName.Substring(value.FileName.Length - 4);
                    //switch (se)
                    //{
                    //    case ".csv":
                    //        {
                    //            CSVWriteButton = true;
                    //            try
                    //            {
                    //                if (MotorStatus.mPosition[0] > 1)
                    //                    toggleShapeHomeButtonText = "Home";
                    //                else
                    //                    toggleShapeHomeButtonText = "Shape";
                    //            }
                    //            catch { }
                    //            OnPropertyChanged();

                    //            if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + value.FileName + ".Resume"))
                    //            {
                    //                toggleStopResumeButtonText = "Resume";
                    //                OnPropertyChanged();
                    //                log.Log(LogLevel.Info, "Resume file found>>>"+ value.FileName + ".Resume");
                    //            }

                    //            //IsForceResumeValid = true;
                    //            //else
                    //            LaserSendButton = false;
                    //            IsForceResumeValid = false;
                    //            MotorStatus.IsMotorsIdle = true;
                    //            IsStopEnable = true;
                    //            IsHomeEnabled = true;
                    //        }
                    //        break;
                    //    case ".las":
                    //        try
                    //        {
                    //            if (MotorStatus.mPosition[0] > 1)
                    //                toggleShapeHomeButtonText = "Home";
                    //            else
                    //                toggleShapeHomeButtonText = "Shape";
                    //        }
                    //        catch { }

                    //        LaserSendButton = true;
                    //        IsForceResumeValid = false;
                    //        MotorStatus.IsMotorsIdle = false;
                    //        IsStopEnable = false;
                    //        IsHomeEnabled = false;
                    //        OnPropertyChanged();

                    //        break;
                    //    default:
                    //        break;
                    //}

                    //FileInfo SelectedFile_finf = new FileInfo(SelectedFile.FileFullName);
                    Regex reg = new Regex(@".csv$");
                    DirectoryInfo di = new DirectoryInfo(_selectedFile.FileFullName.Remove(_selectedFile.FileFullName.LastIndexOf("\\")));
                    if (!Directory.Exists(di.FullName))
                    {
                        MessageBox.Show("Please select project first.", "No project/panel selected!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }

                    var laserFilesPath = di.GetFiles("*.csv", SearchOption.AllDirectories)
                             .Where(path => reg.IsMatch(path.FullName))
                             .ToList();


                    //If there is CSV found
                    if (laserFilesPath.Count() == 1)
                    {
                        if (selectedCSV != laserFilesPath[0].FullName)
                            CSVHelper.IsNewPanelSelected = true;

                        selectedCSV = laserFilesPath[0].FullName;
                        CSVWriteButton = true;
                        OnPropertyChanged();



                        FileInfo SelectedFile_finf = new FileInfo(selectedCSV);
                        log.Log(LogLevel.Info, CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume");

                        if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"))
                        {
                            toggleStopResumeButtonText = "Resume";
                            OnPropertyChanged();
                            //log.Log(LogLevel.Info, "Resume file found>>>" + CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume");
                        }
                        else toggleStopResumeButtonText = "Stop";

                        LaserSendButton = false;
                        IsForceResumeValid = false;
                        MotorStatus.IsMotorsIdle = true;
                        IsStopEnable = true;
                        IsHomeEnabled = true;


                        LaserSendButton = true;
                        //MotorStatus.IsMotorsIdle = false;
                        //IsStopEnable = false;
                        //IsHomeEnabled = false;
                        OnPropertyChanged();




                    }
                    else
                    {
                        CSVWriteButton = false;
                        IsHomeEnabled = false;
                        IsForceResumeValid = false;
                        LaserSendButton = false;
                        //MessageBox.Show("No Shape file found Or More than one Shape file present! @"+di, "Shapefile Missing!", MessageBoxButtons.OK); }
                        log.Log(LogLevel.Info, "No Shape file found Or More than one Shape file present! @" + di, "Shapefile Missing!");

                        OnPropertyChanged();
                    }
                }
            }
        }

        //public string Base64Encode(string plainText)
        //{
        //    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        //    return System.Convert.ToBase64String(plainTextBytes);
        //}
        //public string Base64Decode(string base64EncodedData)
        //{
        //    var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        //    return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        //}
        private void WriteCSVData(IList<float> data)
        {

            if (!ConfirmMotorsAreFine())
                return;

            checkFirmware();


            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
                _TwinCathelper.PLCMotors[i].mDirection = CSVHelper.MotorDirValue == 0 ? false : true;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = GetRecommendedRPM(data[i]);//CSVHelper.MotorTime;
            _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                //log.Log(LogLevel.Info, "WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                //+ "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                //+ "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                //+ "  >> mMode: " + EMode.Project.ToString());
                log.Log(LogLevel.Warn, "ProjectView > WriteCSVData  Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + (_TwinCathelper.PLCMotors[i].mDirection ? "Down" : "Up")
                    + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                    + "  >> mRPM: " + _TwinCathelper.PLCMotors[i].mRPM
                    + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                    + "  >> mMode: " + EMode.Project.ToString());
            }


            if (data.Count != null)
                LastmaxVal = data.Max();
            else
                LastmaxVal = _TwinCathelper.PLCMotors.Max().mDisplacement;

            try
            {
                CSVHelper.CompletingTime = _TwinCathelper.PLCMotors.Max().mTime + 5.00F;
                if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                CSVHelper.ST_timer.Restart();

                _TwinCathelper.WriteEthercatDeviceData();
                log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} csv value(s) has been written sucessfully");

                Thread.Sleep(200);
                uint zero = 0;
                for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                    _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
                log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} zero value(s) has been written sucessfully");


                string RemoveResumeAfterProcess = ConfigurationManager.AppSettings["RemoveResumeAfterProcess"].ToString();

                FileInfo SelectedFile_finf = new FileInfo(selectedCSV);

                if (RemoveResumeAfterProcess == "1" && File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"))
                {
                    log.Log(LogLevel.Warn, $"Deleting Resume File > "+ SelectedFile_finf);

                    try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"); } catch { }
                    //try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset"); } catch { }
                    MotorStatus.IsMotorsIdle = false;
                    toggleStopResumeButtonText = "Stop";
                }                

                //IsPsitionDataProcessing = true;
                try { LastPositionData = MotorStatus.mPosition[0]; } catch { LastPositionData = 0; }                
                try { progressbar_FinalVal = Math.Abs((int)(_TwinCathelper.PLCMotors[0].mDisplacement)); } catch { }// + Math.Abs((int)CSVHelper.NegativeoffsetResumeFlt.Min())
                progressbar_timespent = 0;
                TravelTimeUpdate = false;
                IsRampCompleted = false;

                LastEventPressed = 'W';// H=Home R=Resume S=Stop W= WriteCSV O=Initial F=ForceResume C=Calibrate
            }
            catch(Exception ee)  { log.Log(LogLevel.Info, "WriteCSVData>>>>" + ee.Message +"---- "+ee.StackTrace.ToLower()); }
        }

        private bool StallStopProcess_WriteCSVData()
        {
            List<float> NegativeoffsetResume = new List<float>();
            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                NegativeoffsetResume.Add(0.00f);


            if (!ConfirmMotorsAreFine())
                return false;

            if (selectedCSV == null || selectedCSV.Length<1)
            {
                log.Log(LogLevel.Error, "CSV is not selected!");
                return false;
            }

            FileInfo SelectedFile_finf = new FileInfo(selectedCSV);
            if (SelectedFile == null)
            {
                log.Log(LogLevel.Error, "Selected CSV file is incorrect!");
                return false;
            }

            csvdata = CSVHelper.ReadCSV_DoubleData(SelectedFile_finf);
            if (csvdata == null || csvdata.Count < 1)
            {
                log.Log(LogLevel.Error, "CSV data or Position is Null!");
                return false;
            }

            log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process");

            IList<float> data = csvdata;
            IList<float> data_resume = csvdata;
            FileInfo finf = SelectedFile_finf;

            //string Max_mm = CSVHelper.MaxDisplacement.ToString();//ConfigurationManager.AppSettings["Max_mm"].ToString();

            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                double round3digs = (data[i] - MotorStatus.mPosition[i]);//+ float.Parse(NegativeoffsetResume[i].ToString())
                data_resume[i] = (float)(Math.Round(round3digs, 3));

                try
                {
                    //Max_mm
                    if (round3digs > float.Parse(CSVHelper.Max_mm.ToString(), vCulture))
                    {
                        log.Log(LogLevel.Error, "StallStop>>Calculated Value mm Exceeded! at " + i + " " + round3digs.ToString());
                        return false;
                    }
                }
                catch
                { log.Log(LogLevel.Error, "STOP Calculation Error! Value Exceeded!"); }
            }

            //String CSVMotors_contents = string.Join(",", data_resume);
            String CSVMotors_contents = "";
            foreach (float flt in data_resume)
                CSVMotors_contents += flt.ToString(CultureInfo.InvariantCulture) + "~";

            CSVMotors_contents = CSVMotors_contents.Substring(0, CSVMotors_contents.Length - 1);

            //CSVMotors_contents += MotorDirValue + Environment.NewLine;
            //CSVMotors_contents += MotorRampDelayValue + Environment.NewLine;
            //CSVMotors_contents += MotorTime + Environment.NewLine;
            //CSVMotors_contents += (int)EMode.Project + Environment.NewLine;

            try
            {
                //if (LastEventPressed != 'H')//On Resuming After PowerCutoff it will Home to CSV location of HOME and Position will be 0s
                //{
                using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume", false, Encoding.UTF8))
                {
                    writer.Write(CSVMotors_contents);
                    writer.WriteLine();
                    writer.Flush();
                }
                log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process, Completed!");
                //}
            }
            catch (Exception ee) { log.Log(LogLevel.Error, $"Error Saving CSV/Motors data! " + ee.Message); return false; }

            if (LastEventPressed != 'R' && LastEventPressed != 'D')
                LastEventPressed = 'S';// H=Home R=Resume S=Stop W= WriteCSV O=Initial
            MotorStatus.IsMotorsIdle = true;

            toggleStopResumeButtonText = "Resume";
            OnPropertyChanged();

            updateHomeButtonStatus();

            return true;
        }

        private bool RestoreLastState()
        {

            try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

            //log.Log(LogLevel.Info, $"11111");
            //if (!ConfirmMotorsAreFine())
            //    return false;


            String LastLocationsFile = "";
            String[] LastLocationsValues = new String[_TwinCathelper.PLCMotors.Count()];
            //try { LastLocationsFile = File.ReadAllText(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\LastPositions" + DeviceCount + ".Backup", Encoding.UTF8).Trim(); } catch { LastLocationsFile = ""; }
            try { LastLocationsFile = CSVHelper.LastPositionValues; 
                if (CSVHelper.LastPositionValues.Split('~').Count() > _TwinCathelper.PLCMotors.Count()) 
                    { 
                    DialogResult drclient = MessageBox.Show("Lastlocation is for "+ CSVHelper.LastPositionValues.Split('~').Count()+" Motors and "+ _TwinCathelper.PLCMotors.Count() + " motors are connected now online, Do you still want to continue?", "Motors count not matched!",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning);
                    if (drclient == DialogResult.Cancel)
                        return false;

                } 
            
            } catch { }

            //log.Log(LogLevel.Info, $"44444" + CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\LastPositions" + DeviceCount + ".Backup");
            //log.Log(LogLevel.Info, $"44444" + LastLocationsFile);
            try { LastLocationsValues = LastLocationsFile.Split('~'); } catch { LastLocationsValues[0] = LastLocationsFile; }
            //log.Log(LogLevel.Info, $"LastLocationsFile>>>>"+ LastLocationsFile);

            if (LastLocationsValues.Length < 1)
            {
                MessageBox.Show("No Last Locations data found, Please calibrate the machine again!", "No Last Locations found!", MessageBoxButtons.OK);
                return false;
            }

                String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                String[] offset_resume = CSVOffset_contents.Split('~');
            //log.Log(LogLevel.Info, $"6666666");
            string errmsg = "";
            
            //log.Log(LogLevel.Info, $"CSVOffset_contents>>>>" + CSVOffset_contents);

            if (offset_resume.Length < LastLocationsValues.Length)
            { 
                offset_resume = new String[_TwinCathelper.PLCMotors.Count()];

                for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                    offset_resume[i] = "0";

            }

            //log.Log(LogLevel.Info, $"7777777777");
            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                {
                    try
                    {

                        if (CSVHelper.NegativeoffsetResume.Length < 1)//when no offset exist
                        {
                        offset_resume[i] = (0.00F - float.Parse(LastLocationsValues[i],vCulture)).ToString();
                        //log.Log(LogLevel.Info, $"offset_resume111111>>>>" + offset_resume[i]);
                    }
                        else//if (CSVHelper.NegativeoffsetResume.Length > 2)//0.0F
                        {
                            try
                            {

                            float OffsetResumeParsed = 0.00F;
                            try { OffsetResumeParsed = float.Parse(offset_resume[i], vCulture); } catch { OffsetResumeParsed = 0.00F; }
                                offset_resume[i] = ((OffsetResumeParsed + (float.Parse(LastLocationsValues[i], vCulture)) * -1)).ToString();

                            //log.Log(LogLevel.Info, $"offset_resume22222>>>>" + offset_resume[i]);

                            if (Math.Abs(float.Parse(offset_resume[i],vCulture)) >= CSVHelper.Max_mm)
                                    errmsg += " Motor" + i + "-" + offset_resume[i].ToString() + Environment.NewLine;
                            }
                            catch (Exception ee)
                            {
                                log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");
                            }
                        }



                    }
                    catch { 
                    //log.Log(LogLevel.Info, $"9999999999999911111"); 
                }
                }

                if (errmsg.Length > 1)
                {
                    MessageBox.Show("Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButtons.OK);
                    log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                    errmsg = "";
                }
            //log.Log(LogLevel.Info, $"91919191919");

            if (offset_resume.Count() > 0)
                {
                    CSVHelper.NegativeoffsetResume = "";
                    String newHomingOffset = "";
                    foreach (string flt in offset_resume)
                        newHomingOffset += flt.ToString(CultureInfo.InvariantCulture).Trim() + "~";

                    CSVHelper.NegativeoffsetResume = newHomingOffset.Substring(0, newHomingOffset.Length - 1);
                    CSVHelper.SetNegativeoffsetResumeFlt();

                try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

                
            }

            //log.Log(LogLevel.Info, $"CSVHelper.NegativeoffsetResume222222>>>>" + CSVHelper.NegativeoffsetResume);

            return true;
        }



        private bool StopProcess_WriteCSVData()
        {

            List<float> NegativeoffsetResumeProj = new List<float>();
            String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
            String[] offset_resume = CSVOffset_contents.Split('~');
            CSVHelper.LastStopTime = DateTime.Now;

            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                NegativeoffsetResumeProj.Add(float.Parse(offset_resume[i]));


            if (!ConfirmMotorsAreFine())
            {
                updateHomeButtonStatus(); return false;
            }

            if (CSVHelper.LastSelectedCSV != selectedCSV && toggleShapeHomeButtonText == "Home")  //negative offset exist ++(Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) > 1)  && 
            {
                if(selectedCSV != null && selectedCSV.Length > 10)
                    MessageBox.Show("You can not select other Resume while Shaping for other process, Please calibrate first to coutinue!" + Environment.NewLine + " Last selected CSV = " + CSVHelper.LastSelectedCSV, "Please calibrate first to continue!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            if (SelectedFile == null)
            {
                log.Log(LogLevel.Error, "CSV data or Position is Null!");
                updateHomeButtonStatus();
                return false;
            }

            if (selectedCSV == null || selectedCSV.Length < 1)
            {
                log.Log(LogLevel.Error, "CSV data or Position is Null!");
                updateHomeButtonStatus();
                return false;
            }


            FileInfo SelectedFile_finf = new FileInfo(selectedCSV);

            csvdata = CSVHelper.ReadCSV_DoubleData(SelectedFile_finf);
            if (csvdata == null || csvdata.Count < 1)
            {
                log.Log(LogLevel.Error, "CSV is not Readable!");
                updateHomeButtonStatus();
                return false;
            }

            log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process");

            // Process for creating a RESUME FILE
            IList<float> data = csvdata;
            IList<float> data_resume = csvdata;
            FileInfo finf = SelectedFile_finf;
            //int motTime = MotorDirValue;
            //float MotRampDTime = MotorRampDelayValue;
            //float MTime = MotorTime;
            //int mMode1 = (int)EMode.Project;

            if (CSVHelper.LastSelectedCSV != selectedCSV && toggleShapeHomeButtonText == "Home")  //negative offset exist ++(Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) > 1)  && 
            {
                if (selectedCSV != null && selectedCSV.Length > 10)
                    MessageBox.Show("You can not select other Resume while Shaping for other process, Please calibrate first to coutinue!" + Environment.NewLine + " Last selected CSV = " + CSVHelper.LastSelectedCSV, "Please calibrate first to continue!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            //string Max_mm = CSVHelper.MaxDisplacement.ToString();//ConfigurationManager.AppSettings["Max_mm"].ToString();

            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                //double round3digs = (data[i] - MotorStatus.mPosition[i] + NegativeoffsetResumeProj[i] );

                //Resume location preparing
                //double round3digs = MotorStatus.mPosition[i] + data[i]; //1: -20 + 25=5 ----- 20 +25=
                double round3digs = 0;
                if (NegativeoffsetResumeProj[i]!=null && NegativeoffsetResumeProj[i] < 0)
                    round3digs = (NegativeoffsetResumeProj[i] - MotorStatus.mPosition[i]) + data[i];//1: -20 -- 25 + 25=5 ----- -25 +5=-20
                else
                    round3digs = data[i] - MotorStatus.mPosition[i];

                data_resume[i] = (float)(Math.Round(round3digs, 3));

                try
                {
                    //Max_mm
                    if (round3digs > float.Parse(CSVHelper.Max_mm.ToString(), vCulture))
                    {
                        log.Log(LogLevel.Error, "StopProc>>Calculated Value mm Exceeded! at " + i + " " + round3digs.ToString());
                        updateHomeButtonStatus();
                        return false;
                    }
                }
                catch
                { log.Log(LogLevel.Error, "STOP Calculation Error! Value Exceeded!"); }
            }

            //String CSVMotors_contents = string.Join(",", data_resume);
            String CSVMotors_contents = "";
            foreach (float flt in data_resume)
                CSVMotors_contents += flt.ToString(vCulture) + "~";

            CSVMotors_contents = CSVMotors_contents.Substring(0, CSVMotors_contents.Length - 1);

            //CSVMotors_contents += MotorDirValue + Environment.NewLine;
            //CSVMotors_contents += MotorRampDelayValue + Environment.NewLine;
            //CSVMotors_contents += MotorTime + Environment.NewLine;
            //CSVMotors_contents += (int)EMode.Project + Environment.NewLine;

            try
            {
                //if (LastEventPressed != 'H')//On Resuming After PowerCutoff it will Home to CSV location of HOME and Position will be 0s
                //{
                    using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume", false, Encoding.UTF8))
                    {
                        writer.Write(CSVMotors_contents);
                        writer.WriteLine();
                        writer.Flush();
                    }
                    log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process, Completed!");
                //}
            }
            catch(Exception ee) { log.Log(LogLevel.Error, $"Error Saving CSV/Motors data! "+ee.Message); updateHomeButtonStatus(); return false; }

            //if (LastEventPressed == 'N')//-ivePositionData     On Resuming After PowerCutoff it will Home to CSV location of HOME and Position will be 0s
            //{ //File.Delete(SelectedFile + ".Resume"); //Added for On Power Restart it automatically reads resume
            //    for (int i = 0; i < DeviceCount; i++)
            //    {
            //        try
            //        {
            //            NegativeoffsetResume[i] = _TwinCathelper.PLCMotorsStatuses.mPostion[i];
            //            NegativeoffsetResumeDirection[i] = false;//direction will be the min -ive value as previous was min is 0
            //        }
            //        catch { }
            //    }
            //    String CSVMotors_contents2 = "";// string.Join(",", NegativeoffsetResume);
            //    String CSVMotors_contentsDirection = "";// string.Join(",", NegativeoffsetResumeDirection);
            //    try
            //    {
            //        //line 1 has the resume data
            //        //line 2 has the offset data
            //        //line 3 has the direction data

            //        //File.AppendAllText(finf.FullName + ".Resume", Base64Encode(CSVMotors_contents2 + Environment.NewLine));
            //        //File.AppendAllText(finf.FullName + ".Resume", Base64Encode(CSVMotors_contentsDirection + Environment.NewLine));

            //        foreach (float flt in NegativeoffsetResume)
            //            CSVMotors_contents2 += flt.ToString(CultureInfo.InvariantCulture) + ",";

            //        CSVMotors_contents2 = CSVMotors_contents2.Substring(0, CSVMotors_contents2.Length - 1);

            //        using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + SelectedFile.Name + ".Resume", true, Encoding.UTF8))
            //        {
            //            writer.Write(CSVMotors_contents2);
            //            writer.WriteLine();
            //            writer.Flush();
            //        }

            //        foreach (bool bln1 in NegativeoffsetResumeDirection)
            //            CSVMotors_contentsDirection += bln1.ToString(CultureInfo.InvariantCulture) + ",";

            //        CSVMotors_contentsDirection = CSVMotors_contentsDirection.Substring(0, CSVMotors_contentsDirection.Length - 1);

            //        using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + SelectedFile.Name + ".Resume", true, Encoding.UTF8))
            //        {
            //            writer.Write(CSVMotors_contentsDirection);
            //            writer.WriteLine();
            //            writer.Flush();
            //        }
            //        log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process for HOME Adjustment, Completed!");
            //    }
            //    catch (Exception ee) { log.Log(LogLevel.Error, $"Error Saving CSV/Motors data for HOME Adjustment! " + ee.Message); return false; }
            //}

            //-ivePositionData     On Resuming After PowerCutoff it will Home to CSV location of HOME and Position will be 0s
            
            if (LastEventPressed == 'N')//LastEventPressed == 'N' Only for external stop
            { //File.Delete(SelectedFile + ".Resume"); //Added for On Power Restart it automatically reads resume

                try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

                //String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                //String[] offset_resume = CSVOffset_contents.Split('~');
                string errmsg = "";

                for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                {
                    try
                    {

                        if (CSVHelper.NegativeoffsetResume.Length < 1)//when no offset exist
                        {
                            NegativeoffsetResumeProj[i] = 0.00F - _TwinCathelper.PLCMotorsStatuses.mPostion[i];
                        }                        
                        else//if (CSVHelper.NegativeoffsetResume.Length > 2)//0.0F
                        {
                            try
                            {
                                    offset_resume[i] = ((float.Parse(offset_resume[i], vCulture) + (_TwinCathelper.PLCMotorsStatuses.mPostion[i]) * -1)).ToString();

                                    if (Math.Abs(float.Parse(offset_resume[i], vCulture)) >= CSVHelper.Max_mm)
                                        errmsg += " Motor" + i + "-" + offset_resume[i].ToString() + Environment.NewLine;
                            }
                            catch (Exception ee)
                            {
                                log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");
                            }
                        }



                    }
                    catch { }
                }

                if (errmsg.Length > 1)
                {
                    MessageBox.Show("Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButtons.OK);
                    log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                    errmsg = "";
                }

                if (offset_resume.Count() > 0)
                {
                    CSVHelper.NegativeoffsetResume = "";
                    String newHomingOffset = "";
                    foreach (string flt in offset_resume)
                        newHomingOffset += flt.ToString(CultureInfo.InvariantCulture).Trim() + "~";

                    CSVHelper.NegativeoffsetResume = newHomingOffset.Substring(0, newHomingOffset.Length - 1);
                    CSVHelper.SetNegativeoffsetResumeFlt();

                    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }
                }

                try
                {
                    String newHomingOffset = "";

                    //foreach (float flt in data_resume)
                    //    CSVMotors_contents += flt.ToString(vCulture) + "~";

                    int i = 0;
                    float newResumeLoc = 0.00F;
                    foreach (string flt in offset_resume)
                    {
                        newResumeLoc = data_resume[i]; //+ Math.Abs(float.Parse(flt, vCulture));
                        i++;
                        newHomingOffset += newResumeLoc.ToString(CultureInfo.InvariantCulture).Trim() + "~";
                    }
                    CSVMotors_contents = newHomingOffset.Substring(0, newHomingOffset.Length - 1); ;
                    using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume", false, Encoding.UTF8))
                    {
                        writer.Write(CSVMotors_contents);
                        writer.WriteLine();
                        writer.Flush();
                    }
                    log.Log(LogLevel.Info, $"Saving Stall CSV/Motors data for Resume Process, Completed!");
                    //}
                }
                catch (Exception ee) { log.Log(LogLevel.Error, $"Error Saving CSV/Motors data! " + ee.Message); updateHomeButtonStatus(); return false; }

                //String CSVMotors_contents2 = "";// string.Join(",", NegativeoffsetResume);
                //try
                //{
                //    //line 1 has the resume data
                //    //line 2 has the offset data
                //    //line 3 has the direction data

                //    //File.AppendAllText(finf.FullName + ".Resume", Base64Encode(CSVMotors_contents2 + Environment.NewLine));
                //    //File.AppendAllText(finf.FullName + ".Resume", Base64Encode(CSVMotors_contentsDirection + Environment.NewLine));

                //    foreach (float flt in NegativeoffsetResumeProj)
                //        CSVMotors_contents2 += flt.ToString(CultureInfo.InvariantCulture) + ",";

                //    CSVMotors_contents2 = CSVMotors_contents2.Substring(0, CSVMotors_contents2.Length - 1);

                //    //using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset", true, Encoding.UTF8))
                //    //{
                //    //    writer.Write(CSVMotors_contents2);
                //    //    writer.WriteLine();
                //    //    writer.Flush();
                //    //}
                //    CSVHelper.NegativeoffsetResume = CSVMotors_contents2;
                //    CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB();

                //    log.Log(LogLevel.Info, $"Saving CSV/Motors data for OFFSETResume Process for HOME Adjustment, Completed!");
                //}
                //catch (Exception ee) { log.Log(LogLevel.Error, $"Error Saving CSV/Motors data for HOME Adjustment! " + ee.Message); return false; }
            }

            if (LastEventPressed != 'R' && LastEventPressed != 'D')
                LastEventPressed = 'S';// H=Home R=Resume S=Stop W= WriteCSV O=Initial
            MotorStatus.IsMotorsIdle = true;

            toggleStopResumeButtonText = "Resume";
            OnPropertyChanged();

            updateHomeButtonStatus();

            return true;
        }

        private void ResumeProcess_WriteCSVData()
        {
            if (!ConfirmMotorsAreFine())
            { updateHomeButtonStatus(); return; }

            FileInfo finf;
            String CSVMotors_contents = null;
            String CSVOffset_contents = null;
            float calc_displacement = 0.00F;

            String[] resumeStr = null;
            String[] offset_resume = new String[_TwinCathelper.PLCMotors.Count()];
            FileInfo SelectedFile_finf = new FileInfo(selectedCSV);

            try
            {
                //try
                //{
                //    if (System.Globalization.CultureInfo.CurrentCulture.Name.Contains("en-DK"))
                //    {
                //        vCulture.NumberFormat.NumberDecimalSeparator = ",";
                //        vCulture.NumberFormat.CurrencyDecimalSeparator = ",";
                //    }
                //    else
                //    {
                //        vCulture.NumberFormat.NumberDecimalSeparator = ".";
                //        vCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                //    }                    
                //}
                //catch { }



                if (SelectedFile == null)
                {
                    log.Log(LogLevel.Error, "CSV data or Position is Null!");
                    return;
                }



                finf = SelectedFile_finf;
                //resumeStr = File.ReadAllLines(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\"))+"-"+finf.Name + ".Resume",Encoding.UTF8);
                CSVMotors_contents = File.ReadAllText(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + finf.Name + ".Resume", Encoding.UTF8);//resumeStr[0].ToString(CultureInfo.InvariantCulture);
                log.Log(LogLevel.Info, "" + CSVMotors_contents);
                //try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"); } catch { }
                //stArr = CSVMotors_contents.Split(Envi ronment.NewLine);
            }
            catch (Exception ee)
            {
                log.Log(LogLevel.Error, "No Resume file found or Resume file corrupted!");
                return;
            }



            ////if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" + data_resume.Count().ToString()+ ".offset"))
            //if(CSVHelper.NegativeoffsetResume.Length>0)
            //{
            //    CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
            //    offset_resume = CSVOffset_contents.Split('~');                
            //    List<float> NegativeoffsetResumeProj = new List<float>();
            //    string errmsg = "";

            //    for (int i = 0; i < DeviceCount; i++)
            //    {
            //        try
            //        {

            //            if (CSVHelper.NegativeoffsetResume.Length < 1)//when no offset exist
            //            {
            //                NegativeoffsetResumeProj[i] = 0.00F - _TwinCathelper.PLCMotorsStatuses.mPostion[i];
            //            }
            //            else//if (CSVHelper.NegativeoffsetResume.Length > 2)//0.0F
            //            {
            //                try
            //                {
            //                    log.Log(LogLevel.Info, "Resuming000>>>>"+ float.Parse(offset_resume[i], vCulture) +" --->>> "+(_TwinCathelper.PLCMotorsStatuses.mPostion[i]) * -1);

            //                    offset_resume[i] = ((float.Parse(offset_resume[i], vCulture) + (_TwinCathelper.PLCMotorsStatuses.mPostion[i]) * -1)).ToString();

            //                    log.Log(LogLevel.Info, "Resuming000>>>>" + offset_resume[i]);

            //                    if (Math.Abs(float.Parse(offset_resume[i], vCulture)) >= CSVHelper.Max_mm)
            //                        errmsg += " Motor" + i + "-" + offset_resume[i].ToString() + Environment.NewLine;
            //                }
            //                catch (Exception ee)
            //                {
            //                    log.Log(LogLevel.Info, "Resuming>>>>NegativeoffsetResume");
            //                }
            //            }



            //        }
            //        catch { }
            //    }

            //    if (NegativeoffsetResumeProj.Count > 0)
            //    { RecommendedMaxDisplacement(NegativeoffsetResumeProj);

            //        int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;//int.Parse(ConfigurationManager.AppSettings["CSV_RecommendedRPM"].ToString());
            //        RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);//CSVHelper.MotorTime

            //    }
            //}

            log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process");
            //string Max_mm = CSVHelper.MaxDisplacement.ToString(); //ConfigurationManager.AppSettings["Max_mm"].ToString();

            if (CSVHelper.MotorTime < 1)
            {
                //RecommendedMaxDisplacement(NegativeoffsetResumeProj);
                LoadCSVValues();

                int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;//int.Parse(ConfigurationManager.AppSettings["CSV_RecommendedRPM"].ToString());
                RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);//CSVHelper.MotorTime
                //_TwinCathelper.PLCMotors[i].mTime = CSVHelper.MotorTime;//float.Parse(ConfigurationManager.AppSettings["DefaultRampTime"].ToString());s               
            }

            string[] data_resume = CSVMotors_contents.Split('~');
            log.Log(LogLevel.Info, "Resuming111>>> " + data_resume.Length + " >>" + data_resume[0]);

            var list = new List<float>(data_resume.Length);
            List<float> data_resumeFlt = new List<float>(_TwinCathelper.PLCMotors.Count());

            for (int i = 0; i < data_resume.Length; i++)
            {
                //log.Log(LogLevel.Info, "Resuming1223339999>>> " + data_resume[i]);
                try
                {
                    //log.Log(LogLevel.Info, "Resuming122>>> " + data_resume[i]);
                    //log.Log(LogLevel.Info, "Resuming122>>> " + float.Parse(offset_resume[i], vCulture));

                    //log.Log(LogLevel.Info, "Resuming122>>> " + (float.Parse(data_resume[i].Replace(".",","), vCulture) + float.Parse(offset_resume[i].Replace(".", ","), vCulture)));
                    //log.Log(LogLevel.Info, "Resuming122>>> " + (float.Parse(data_resume[i].Replace(".", ",")) + float.Parse(offset_resume[i].Replace(".", ","), vCulture)));
                    //log.Log(LogLevel.Info, "Resuming122>>> " + (float.Parse(data_resume[i].Replace(".", ",")) + float.Parse(offset_resume[i].Replace(".", ","))));
                    //log.Log(LogLevel.Info, "Resuming122>>> " + (float.Parse(data_resume[i].Replace(".", ",")) + float.Parse(offset_resume[i].Replace(".", ","))));


                    //float chkexpr = 0.0F;
                    //float chkexpr2 = 0.0F;

                    //log.Log(LogLevel.Info, "Resuming1223339999222>>> " + CSVOffset_contents);

                    if (CSVOffset_contents != null && CSVOffset_contents.Length > 0)
                    {
                        //chkexpr = float.Parse(data_resume[i].Replace(".", ","), vCulture);
                        //chkexpr2 = float.Parse(offset_resume[i].Replace(".", ","), vCulture);

                        //calc_displacement = chkexpr+chkexpr;
                        calc_displacement = stringToFloatCult(data_resume[i]);
                        //log.Log(LogLevel.Info, "Resuming122333>>> " + calc_displacement);
                    }
                    else
                        calc_displacement = stringToFloatCult(data_resume[i]);//chkexpr;

                    //log.Log(LogLevel.Info, "Resuming1223339999333>>> " + calc_displacement);

                    //Max_mm
                    if (calc_displacement > float.Parse(CSVHelper.Max_mm.ToString(), vCulture))
                    {
                        //log.Log(LogLevel.Info, "Resuming1223339999444>>> " + calc_displacement);
                        calc_displacement = float.Parse(data_resume[i], vCulture);//chkexpr;
                        if (calc_displacement > float.Parse(CSVHelper.Max_mm.ToString(), vCulture))
                        {
                            log.Log(LogLevel.Error, "Calculated Value mm Exceeded! at " + i + " " + calc_displacement.ToString());
                            return;
                        }
                    }
                }
                catch
                {

                    log.Log(LogLevel.Error, "Resuming Calculation Error! ");
                }

                //log.Log(LogLevel.Info, "Resuming12233399996666>>> ");
                try
                {
                    _TwinCathelper.PLCMotors[i].mDisplacement = Math.Abs(calc_displacement);//Math.Abs(float.Parse(data_resume[i]));
                    _TwinCathelper.PLCMotors[i].mDirection = false;//calc_displacement >= 0 ? false : true;
                                                                   //_TwinCathelper.PLCMotors[i].mDirection = (int)MotorStatus.mPosition[i] < 0 ? false : true;// on -ive position "Clockwise" else "AntiClockwise"
                                                                   //if (_CSVDiaglogViewModel.MotorRampDelayValue < 1)
                    _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;// int.Parse(ConfigurationManager.AppSettings["DefaultRampDelay"].ToString());
                                                                                           //else
                                                                                           //    _TwinCathelper.PLCMotors[i].mRampDelay = _CSVDiaglogViewModel.MotorRampDelayValue;

                    //_TwinCathelper.PLCMotors[i].mTime = GetRecommendedRPM(calc_displacement);//CSVHelper.MotorTime;//int.Parse(ConfigurationManager.AppSettings["DefaultRampTime"].ToString());
                    //else
                    //    _TwinCathelper.PLCMotors[i].mTime = _CSVDiaglogViewModel.MotorTime;

                    _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                    data_resumeFlt.Add(calc_displacement);

                    calc_displacement = 0.00F;

                }
                catch (Exception ee) { log.Log(LogLevel.Info, "Resuming122333zzzzz>>> " + ee.Message); }
            }

            try
            {
                CSVHelper.MaxDisplacement = data_resumeFlt.Max();
                float calcTime = GetRecommendedRPM(CSVHelper.MaxDisplacement);
                for (int i = 0; i < data_resumeFlt.Count; i++)
                {
                    _TwinCathelper.PLCMotors[i].mTime = calcTime;

                    //                    log.Log(LogLevel.Info, "ResumingMotor_writeCV:" + i.ToString() + " : " + calc_displacement + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                    //+ "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                    //+ "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                    //+ "  >> mMode: " + (int)EMode.Project);

                    log.Log(LogLevel.Warn, "ProjectView > ResumingMotor_writeCV Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + (_TwinCathelper.PLCMotors[i].mDirection ? "Down" : "Up")
                        + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                        + "  >> mRPM: " + _TwinCathelper.PLCMotors[i].mRPM
                        + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                        + "  >> mMode: " + EMode.Project.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, "ResumeProcessTime111>>>>> " + ex.Message);
            }

            try
            {
                updateHomeButtonStatus();// method added to check if the machine is in shape or not?

                if (CSVHelper.LastSelectedCSV != selectedCSV && toggleShapeHomeButtonText == "Home")  //negative offset exist ++(Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) > 1)  && 
                {
                    if (selectedCSV != null && selectedCSV.Length > 10)
                        MessageBox.Show("You can not select other Resume while Shaping for other process, Please calibrate first to coutinue!" + Environment.NewLine + " Last selected CSV = " + CSVHelper.LastSelectedCSV, "Please calibrate first to continue!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                CSVHelper.CompletingTime = _TwinCathelper.PLCMotors.Max().mTime + 5.00F;
                if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                CSVHelper.ST_timer.Restart();

                _TwinCathelper.WriteEthercatDeviceData();
                log.Log(LogLevel.Info, $"{data_resume.Length} csv value(s) has been written sucessfully");

                Thread.Sleep(200);
                uint zero = 0;
                for (uint i = 0; i < data_resume.Length; i++)
                    _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
                log.Log(LogLevel.Info, $"{data_resume.Length} zero value(s) has been written sucessfully");

            }
            catch (Exception ep) { log.Log(LogLevel.Info, "ResumeProcess_WriteCSVData111>>>>" + ep.Message); }


            //IsPsitionDataProcessing = true;
            try { LastPositionData = MotorStatus.mPosition[0]; } catch { LastPositionData = 0; }
            try { 
                //progressbar_FinalVal = Math.Abs((int)(_TwinCathelper.PLCMotors.Max().mDisplacement + LastPositionData + Math.Abs(CSVHelper.NegativeoffsetResumeFlt.Max()))); } catch (Exception ep) { 
                progressbar_FinalVal = Math.Abs((int)(_TwinCathelper.PLCMotors[0].mDisplacement //+ LastPositionData
                    )); } catch (Exception ep) { 
                try { log.Log(LogLevel.Info, "ResumeProcess_WriteCSVData222>>>>progressbar_FinalVal" + ep.Message); 
                    progressbar_FinalVal = (int)data_resumeFlt[0]; } catch { progressbar_FinalVal = 100; } }
            progressbar_timespent = 0;
            TravelTimeUpdate = false;
            IsRampCompleted = false;

            //log.Log(LogLevel.Info, $"Resuming CSV/Motors Process, Completed!");

            string RemoveResumeAfterProcess = ConfigurationManager.AppSettings["RemoveResumeAfterProcess"].ToString();

            if (RemoveResumeAfterProcess == "1" && File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"))
            {
                log.Log(LogLevel.Info, $"Deleting Resume File!");

                try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"); } catch { }
                //try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset"); } catch { }
                MotorStatus.IsMotorsIdle = false;
            }
            data_resumeFlt.Clear();
            //LastEventPressed = 'R';// H=Home R=Resume S=Stop W= WriteCSV O=Initial

            try
            {
                //if (MotorStatus.mPosition != null)
                //    LastmaxVal = MotorStatus.mPosition.Max();
                //else
                    LastmaxVal = _TwinCathelper.PLCMotors.Max().mDisplacement;
            }
            catch (Exception ep) { try { log.Log(LogLevel.Info, "ResumeProcess_WriteCSVData222>>>>progressbar_FinalVal" + ep.Message); LastmaxVal = (int)data_resumeFlt.Max(); } catch { LastmaxVal = 100; } }

            IsRampCompleted = false;
        }

        private float stringToFloatCult(string numInFloat) {

            try
            {
                //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(st, vCulture) + Environment.NewLine;
                //var pCulture = System.Globalization.CultureInfo.CurrentCulture.Clone();
                //if (System.Globalization.CultureInfo.CurrentCulture.Name.Contains("en-DK"))
                //{
                //    pCulture.NumberFormat.NumberDecimalSeparator = ",";
                //    pCulture.NumberFormat.CurrencyDecimalSeparator = ",";
                //}
                //else
                //{
                //    pCulture.NumberFormat.NumberDecimalSeparator = ".";
                //    pCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                //}


                if (numInFloat.Contains('.'))
                    return float.Parse(numInFloat.Trim(), CultureInfo.InvariantCulture);
                else
                    return float.Parse(numInFloat.Trim(), vCulture);
            }
            catch { return 0; }

        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(
                    delegate (object f)
                    {
                        ((DispatcherFrame)f).Continue = false;
                        return null;
                    }), frame);
            Dispatcher.PushFrame(frame);
        }

        ///// <summary>
        ///// Determines how deep in the sub-folder structure the application should go.
        ///// </summary>
        //public const int Depth = 2;

        ///// <summary>
        ///// Determines current recursion level.
        ///// </summary>
        //private const int RecursionLevel = 1;


        ///// <summary>
        ///// This function is used to iterate through the main folder and (if applicable) the subfolders to look for *.tmx import files.
        ///// </summary>
        ///// <param name="sourceDirectory">Directory to search in.</param>
        ///// <param name="processSubFolders">True if subfolder processing required.</param>
        //public void ProcessDirectory(string sourceDirectory, bool processSubFolders)
        //{
        //    // Loop until the recursion level has reached the
        //    // maximum folder depth.
        //    if (RecursionLevel <= Depth)
        //    {
        //        #region "ProcessFiles"
        //        // Retrieve the names of the files found in the given folder.
        //        string[] fileEntries = Directory.GetFiles(sourceDirectory);
        //        foreach (string fileName in fileEntries)
        //        {
        //            // Only process file if it is a TMX import file.
        //            if (fileName.ToLower().EndsWith(".*"))
        //            {
        //                //Console.WriteLine("Importing " + fileName);
        //                //var tmImporter = new TmImporter();
        //                //tmImporter.Import(fileName);
        //            }
        //        }

        //        // Self-recursion to loop through the folder structure until
        //        // the folder depth has reached the recursion level value.
        //        if (processSubFolders)
        //        {
        //            string[] subdirEntries = Directory.GetDirectories(sourceDirectory);
        //            foreach (string subdir in subdirEntries)
        //            {
        //                if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
        //                {
        //                    ProcessDirectory(subdir, processSubFolders);
        //                }
        //            }
        //        }
        //    }
        //}

        private void FileProjectList()
        {

            //projectsList
            Logger log = LogManager.GetLogger("Browse Project List");
            try
            {
                try
                {
                    if (!ConfirmMotorsAreFine())
                        return;
                }
                catch { }

                

                if (Directory.Exists(CSVHelper.prjFolderLoc))
                {
                    //Regex reg = new Regex(@".las$");
                    DirectoryInfo di = new DirectoryInfo(selectedPath);
                    projectsList = di.GetDirectories("*", SearchOption.TopDirectoryOnly).ToList();
                    
                    log.Log(LogLevel.Info, $"Browsing Projects folder");
                }

            }
            catch {
                log.Log(LogLevel.Info, $"Error in Browsing Projects folder");
            }
        }

            private void BrowseProject()
        {
            Logger log = LogManager.GetLogger("Browse Project");
            try
            {
                try
                {
                    if (!ConfirmMotorsAreFine())
                        return;
                }
                catch { }

                //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                //{
                log.Log(LogLevel.Info, $"Loading Project Folder");
                //string projectfolder = ConfigurationManager.AppSettings["ProjectFolder"].ToString();
                string projectfolder = "";
                try
                {
                    projectfolder = CSVHelper.prjFolderLoc;

                    try { if (projectfolder != null) CSVHelper.prjFolderLoc = CSVHelper.prjFolderLoc; else CSVHelper.prjFolderLoc = Environment.CurrentDirectory; } catch { CSVHelper.prjFolderLoc = Environment.CurrentDirectory; }


                }
                catch { }

                //System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (Directory.Exists(projectfolder)) //(result == DialogResult.OK)
                {
                    myDataModels = new Porjects();
                    selectedPath = projectfolder;
                    //Regex reg = new Regex(@".las$|.csv$");
                    Regex reg = new Regex(@".las$");
                    DirectoryInfo di = new DirectoryInfo(selectedPath);
                    var laserFilesPath = di.GetFiles("*", SearchOption.AllDirectories)
                             .Where(path => reg.IsMatch(path.FullName))
                             .ToList();
                    var ProjectRootDir = di.GetDirectories();
                    //.GetFiles("*", SearchOption.TopDirectoryOnly)
                    //.Where(path => reg.IsMatch(path.FullName))
                    //.ToList();

                    var panels = laserFilesPath.GroupBy(p => p.Directory.Name);
                    var projectsTemp = laserFilesPath.GroupBy(p => p.Directory.Parent.Name);


                    log.Log(LogLevel.Info, $"{projectsTemp.Count()} Projects Found");
                    log.Log(LogLevel.Info, $"{panels.Count()} Panels Found");
                    log.Log(LogLevel.Info, $"{laserFilesPath.Count()} Files Found");

                    foreach (var itemroot in ProjectRootDir)
                    {
                        int cntr = 0;
                        var panelsList = itemroot.GetDirectories();
                        if (panelsList.Count() > 0)
                            foreach (var itemrootdtl in panelsList)
                            {
                                //log.Log(LogLevel.Info, item.FirstOrDefault().DirectoryName);
                                //log.Log(LogLevel.Info, item.FirstOrDefault().FullName);
                                //log.Log(LogLevel.Info, item.FirstOrDefault().Name);
                                //log.Log(LogLevel.Info, item.Key);


                                var panelFiles = itemrootdtl.GetFiles("*", SearchOption.AllDirectories)
                                         .Where(path => reg.IsMatch(path.FullName))
                                         .ToList();

                                List<ProjectFilePanel> listpfp = new List<ProjectFilePanel>();
                                foreach (var fil in panelFiles)
                                {
                                    cntr++;
                                    ProjectFilePanel Pfp = new ProjectFilePanel();
                                    Pfp.ID = cntr;
                                    Pfp.FileName = fil.Name;
                                    Pfp.FileFullName = fil.FullName;
                                    listpfp.Add(Pfp);
                                }


                                myDataModels.Add(new Model.Project()
                                {
                                    Panel = itemroot.Name,
                                    ProjectName = itemrootdtl.Name,  //item.FirstOrDefault().Directory.Name,
                                    //LaserFile = panelFiles.ToList(),
                                    LaserFile = listpfp,
                                });

                            }
                        else
                        {
                            var panelFiles = itemroot.GetFiles("*", SearchOption.AllDirectories)
                                     .Where(path => reg.IsMatch(path.FullName))
                                     .ToList();

                            List<ProjectFilePanel> listpfp = new List<ProjectFilePanel>();
                            foreach (var fil in panelFiles)
                            {
                                cntr++;
                                ProjectFilePanel Pfp = new ProjectFilePanel();
                                Pfp.ID = cntr;
                                Pfp.FileName = fil.Name;
                                Pfp.FileFullName = fil.FullName;
                                listpfp.Add(Pfp);
                            }


                            myDataModels.Add(new Model.Project()
                            {
                                Panel = itemroot.Name,
                                ProjectName = itemroot.Name,  //item.FirstOrDefault().Directory.Name,
                                LaserFile = listpfp,
                            });

                        }

                    }


                }
                else
                    log.Log(LogLevel.Warn, $"Project folder path not found!");
                //}
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
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

                //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                //{
                log.Log(LogLevel.Info, $"Loading Project Files Folder");
                //string projectfolder = ConfigurationManager.AppSettings["ProjectFolder"].ToString();
                string projectfolder = "";
                try
                {
                    projectfolder = _selectedListRootDir;//CSVHelper.prjFolderLoc+ 

                    try { if (projectfolder == null) projectfolder = CSVHelper.prjFolderLoc; } catch {  }

                }
                catch { }

                //System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (Directory.Exists(projectfolder)) //(result == DialogResult.OK)
                {
                    myDataModels = new Porjects();
                    selectedPath = projectfolder;
                    //Regex reg = new Regex(@".las$|.csv$");
                    Regex reg = new Regex(@".las$");
                    DirectoryInfo di = new DirectoryInfo(selectedPath);
                    var laserFilesPath = di.GetFiles("*", SearchOption.AllDirectories)
                             .Where(path => reg.IsMatch(path.FullName))
                             .ToList();
                    var ProjectRootDir = di.GetDirectories();
                    //.GetFiles("*", SearchOption.TopDirectoryOnly)
                    //.Where(path => reg.IsMatch(path.FullName))
                    //.ToList();

                    var panels = laserFilesPath.GroupBy(p => p.Directory.Name);
                    var projectsTemp = laserFilesPath.GroupBy(p => p.Directory.Parent.Name);


                    log.Log(LogLevel.Info, $"{projectsTemp.Count()} Projects Found");
                    log.Log(LogLevel.Info, $"{panels.Count()} Panels Found");
                    log.Log(LogLevel.Info, $"{laserFilesPath.Count()} Files Found");

                    foreach (var itemrootPanelProj in ProjectRootDir)
                    {
                        int cntr = 0;
                        
                        var panelsList = itemrootPanelProj.GetFiles().Where(path => reg.IsMatch(path.FullName)).ToList();

                        if (panelsList.Count() > 0)
                            foreach (var itemrootdtl in panelsList)
                            {
                                //log.Log(LogLevel.Info, item.FirstOrDefault().DirectoryName);
                                //log.Log(LogLevel.Info, item.FirstOrDefault().FullName);
                                //log.Log(LogLevel.Info, item.FirstOrDefault().Name);
                                //log.Log(LogLevel.Info, item.Key);


                                var panelFiles = itemrootPanelProj.GetFiles("*", SearchOption.AllDirectories)
                                         .Where(path => reg.IsMatch(path.FullName))
                                         .ToList();

                                List<ProjectFilePanel> listpfp = new List<ProjectFilePanel>();
                                foreach (var fil in panelsList)
                                {
                                    cntr++;
                                    ProjectFilePanel Pfp = new ProjectFilePanel();
                                    Pfp.ID = cntr;
                                    Pfp.FileName = fil.Name;
                                    Pfp.FileFullName = fil.FullName;
                                    listpfp.Add(Pfp);
                                }


                                myDataModels.Add(new Model.Project()
                                {
                                    Panel = itemrootPanelProj.Name,
                                    ProjectName = itemrootdtl.Name,  //item.FirstOrDefault().Directory.Name,
                                    //LaserFile = panelFiles.ToList(),
                                    LaserFile = listpfp,
                                });

                            }
                        //else
                        //{
                        //    var panelFiles = itemrootPanelProj.GetFiles("*", SearchOption.AllDirectories)
                        //             .Where(path => reg.IsMatch(path.FullName))
                        //             .ToList();

                        //    List<ProjectFilePanel> listpfp = new List<ProjectFilePanel>();
                        //    foreach (var fil in panelFiles)
                        //    {
                        //        cntr++;
                        //        ProjectFilePanel Pfp = new ProjectFilePanel();
                        //        Pfp.ID = cntr;
                        //        Pfp.FileName = fil.Name;
                        //        Pfp.FileFullName = fil.FullName;
                        //        listpfp.Add(Pfp);
                        //    }


                        //    myDataModels.Add(new Model.Project()
                        //    {
                        //        Panel = itemrootPanelProj.Name,
                        //        ProjectName = itemrootPanelProj.Name,  //item.FirstOrDefault().Directory.Name,
                        //        LaserFile = listpfp,
                        //    });

                        //}

                    }


                }
                else
                    log.Log(LogLevel.Warn, $"Project folder path not found!");
                //}
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
            CSVHelper.IsNewProjSelected = false;
        }


        private void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            try
            {
                // Or here we know about UDINT type --> can be marshalled as UINT32
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol != null)
                {
                    TCSymbols aa;
                    Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                    switch (aa)
                    {
                        case TCSymbols.mPosition:
                            Position = new float[_TwinCathelper.PLCMotors.Count()];
                            Buffer.BlockCopy(e.Data.ToArray(), 0, Position, 0, e.Data.Length);
                            log.Log(LogLevel.Info, $"Positions Updated : {String.Join(",", Position)}");
                            break;
                        case TCSymbols.MotorDiagnostic:
                            //MDiag1 = new UInt32[TwinCathelper.DeviceCount];
                            //Buffer.BlockCopy(e.Data.ToArray(), 0, MDiag1, 0, e.Data.Length);
                            //log.Log(LogLevel.Info, $"Diag0 Updated : {String.Join(",", MDiag1)}");
                            //Diag1Value = MDiag1;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }
        //private PanelData[] CalcSoftStarter(IList<float> _csvdata)
        //{
        //    // Inputs
        //    double rampDelay = 5;
        //    double missionTime = 1000;

        //    // Fixed Params
        //    double displacementFactor = 2;
        //    double maxRPM = 200;
        //    double minRPM = 1;
        //    uint rampSteps = 10;
        //    var pd = new PanelData[_csvdata.Count];

        //    // Calculations
        //    for (int i = 0; i < _csvdata.Count; i++)
        //    {
        //        double requiredRotations = _csvdata[i] / displacementFactor;
        //        double requiredRPM = (requiredRotations / missionTime) * 60;
        //        double rampRPM = requiredRPM / rampSteps;
        //        double timePerStep = rampDelay / rampSteps;
        //        pd[i].RampRpmTime = timePerStep;
        //        double displacementPerStep = timePerStep * (rampRPM / 60) * displacementFactor;
        //        double totalDisplacementPerRamp = 0;
        //        pd[i].RampRpm = new double[rampSteps];
        //        for (int j = 1; j <= rampSteps; j++)
        //        {
        //            totalDisplacementPerRamp += displacementPerStep * j;
        //            pd[i].RampRpm[j - 1] = rampRPM * j;
        //        }
        //        pd[i].RampDisplacement = totalDisplacementPerRamp;
        //        double requiredDisplacementWithoutRamps = _csvdata[i] - totalDisplacementPerRamp * 2;
        //        pd[i].Displacement = requiredDisplacementWithoutRamps;
        //        double timeWithoutRamps = missionTime - (rampDelay * 2);
        //        pd[i].Time = timeWithoutRamps;
        //        double requiredRotationsWithoutRamps = requiredDisplacementWithoutRamps / displacementFactor;
        //        double requiredRPMAfterWithoutRamps = (requiredRotationsWithoutRamps / timeWithoutRamps) * 60;
        //        pd[i].RPM = requiredRPMAfterWithoutRamps;
        //    }
        //    return pd;
        //}
        struct PanelData
        {
            public double[] RampRpm;
            public double RampRpmTime;
            public double RampDisplacement;
            public double Displacement;
            public double Time;
            public double RPM;
        }
        private async Task CSVSend()
        {
            if (!ConfirmMotorsAreFine())
                return;

            FileInfo SelectedFile_finf = new FileInfo(selectedCSV);

            csvdata = CSVHelper.ReadCSV_DoubleData(SelectedFile_finf);
            if (csvdata.Count == 0)
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                $" File doesn't conatain desired data");
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


            //_ = await DialogHost.Show(_CSVDiaglogView, "RootDialog", ClosingEventHandler);
            //MotorDirValue = _CSVDiaglogViewModel.MotorDirPValue;
            //CSVHelper.MotorDirValue = int.Parse(ConfigurationManager.AppSettings["CSV_MotorDirValue"].ToString());

            //MotorRampDelayValue = _CSVDiaglogViewModel.MotorRampDelayValue;
            //CSVHelper.MotorRampDelayValue = float.Parse(ConfigurationManager.AppSettings["CSV_DefaultTimeSec"].ToString());//CSV_DefaultTimeSec=5
                                                                                                                           //CSVHelper.MotorRampDelayValue = MotorRampDelayValue;

            //MotorTime = _CSVDiaglogViewModel.MotorTime;
            //MotorTime = float.Parse(ConfigurationManager.AppSettings["CSV_DefaultTime"].ToString());//CSV_DefaultTime=33

            string dir = CSVHelper.MotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";

            updateHomeButtonStatus();
            if (CSVHelper.LastSelectedCSV != selectedCSV && toggleShapeHomeButtonText == "Home")//negative offset exist //(Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) > 1) && 
            {
                if (selectedCSV != null && selectedCSV.Length > 10)
                    MessageBox.Show("You can not select other shape while HomeOffset exist, Please calibrate first to coutinue!"+Environment.NewLine+" Last selected CSV = "+ CSVHelper.LastSelectedCSV, "Please calibrate first to continue!", MessageBoxButtons.OK,MessageBoxIcon.Stop);
                return;
            }

            UpdateSettingsDB();

            log.Log(LogLevel.Info, $"Selected Direction : {dir}");
            WriteCSVData(csvdata);
            LastEventPressed = 'W';

            MotorStatus.IsMotorsIdle = false;
        }


        private void LoadCSVValues()
        {
            FileInfo SelectedFile_finf = new FileInfo(selectedCSV);

            if (SelectedFile_finf != null && SelectedFile_finf.Length > 1)
            {
                csvdata = CSVHelper.ReadCSV_DoubleData(SelectedFile_finf);
                if (csvdata.Count == 0)
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
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


                //_ = await DialogHost.Show(_CSVDiaglogView, "RootDialog", ClosingEventHandler);
                //MotorDirValue = _CSVDiaglogViewModel.MotorDirPValue;
                //CSVHelper.MotorDirValue = int.Parse(ConfigurationManager.AppSettings["CSV_MotorDirValue"].ToString());

                //MotorRampDelayValue = _CSVDiaglogViewModel.MotorRampDelayValue;
                //CSVHelper.MotorRampDelayValue = float.Parse(ConfigurationManager.AppSettings["CSV_DefaultTimeSec"].ToString());//CSV_DefaultTimeSec=5
                                                                                                                               //CSVHelper.MotorRampDelayValue = MotorRampDelayValue;

                //MotorTime = _CSVDiaglogViewModel.MotorTime;
                //MotorTime = float.Parse(ConfigurationManager.AppSettings["CSV_DefaultTime"].ToString());//CSV_DefaultTime=33

                string dir = CSVHelper.MotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";
                log.Log(LogLevel.Info, $"Selected Direction : {dir}");
            }
            else
            { MessageBox.Show("Warning! Please load CSV first!", "Warning! No displacement data found!", MessageBoxButtons.OK); }
        }


        public void RecommendedMaxDisplacement(List<float> negdis)
        {

            //float maxPosDisplacement = 0.00F;//Math.Abs(_TwinCathelper.PLCMotorsStatuses.mPostion.Max());
            //float maxNegDisplacement = 0.00F;

            float chkBiggest = Math.Abs(negdis.Max()) > Math.Abs(negdis.Min())? Math.Abs(negdis.Max()) : Math.Abs(negdis.Min());

            //if(negdis!=null && negdis.Count()>0)
            //    maxNegDisplacement = Math.Abs(negdis.Min());

            //if(maxPosDisplacement > maxNegDisplacement)
            //     CSVHelper.MaxDisplacement = maxPosDisplacement;
            //else 
            //    try { CSVHelper.MaxDisplacement = maxNegDisplacement; } catch { CSVHelper.MaxDisplacement = CSVHelper.MaxDisplacement; }

            CSVHelper.MaxDisplacement = chkBiggest;

        }

        public float GetRecommendedRPM(float CurrentMaxDisplacement)
        {

            //float MotorTime = (CurrentMaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);

            //Every motor must reach exactly same time ie some will move faster and some will slower --- Client requirement
            float MotorTime = (CSVHelper.MaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);

            if (MotorTime > 900.0F)//Time cant be set more than 15 min.
                MessageBox.Show("Time is out of bound! Max is 15Mins. But counted is " + (MotorTime / 60) + " Mins", "Warning! Time is out of bound!", MessageBoxButtons.OK);

            CSVHelper.CompletingTime = MotorTime + 5.00F;

            if (MotorTime < 1.0F)
                log.Log(LogLevel.Error, "Shaping: Invalid MotorTime/too Small > "+ MotorTime);


            return MotorTime;
        }

        public void RecommendedRPM(int CSV_RecommendedRPM,float maxRPM)
        { 
            //get { return recommendedRPM; }
            //set
            //{
            //if (CSVHelper.MaxDisplacement < 1)
            //{
            //    if (_TwinCathelper.PLCMotorsStatuses.mPostion.Max() > 0)
            //        CSVHelper.MaxDisplacement = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();
            //    else //if (csvdata != null && csvdata.Max() > 0)
            //        try { CSVHelper.MaxDisplacement = csvdata.Max(); } catch { CSVHelper.MaxDisplacement = 0; }
            //    //else {
            //    //    LoadCSVValues();
            //    //}
            //}

            CSVHelper.MotorTime = (CSVHelper.MaxDisplacement * 60) / (CSV_RecommendedRPM * 2);
            //log.Log(LogLevel.Info, $"New MotortimeSet >>>>>>>>>>>>>"+ CSVHelper.MotorTime);

            if (CSVHelper.MotorTime > 900F)//Time cant be set more than 15 min.
                MessageBox.Show("Time is out of bound! Max is 15Mins. But counted is "+ (CSVHelper.MotorTime/60)+" Mins","Warning! Time is out of bound!",MessageBoxButtons.OK);
                //CSVHelper.MotorTime = time;
                //CSVHelper.MotorTime = CSVHelper.MotorTime;

            //if (CSV_RecommendedRPM != recommendedRPM)
            //{
            //    //recommendedRPM = CSV_RecommendedRPM;
            //    ////total time = (d * 60) / (RPM * 2)
            //    //if (time != _MotorTime)
            //    //{
            //    //    //total time = (d * 60) / (RPM * 2)
            //    //    MotorTime = time;
            //    //}
            //    if (RecommendedRPM > maxRPM)
            //    {
            //        log.Log(LogLevel.Error, $"Value cannot exceed more than Maximum RPM");
            //        //BtnEnbl = true;
            //        //Message = "";
            //    }
            //    //}
            //    //OnPropertyChanged();
            //}
        }


        private bool ConfirmMotorsAreFine()
        {
            try
            {
                string Max_mm = CSVHelper.Max_mm.ToString();//ConfigurationManager.AppSettings["Max_mm"].ToString();

                for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                {
                    if (MotorStatus.mPosition != null)
                    {
                        if (((int)MotorStatus.mPosition[i] > int.Parse(Max_mm)) || ((int)MotorStatus.mPosition[i] < int.Parse("-" + Max_mm)))
                        {
                            log.Log(LogLevel.Error, $"Motor no:" + i + " has invalid position!");
                            String st = "";
                            var data = (int)EMode.Abort;
                            for (uint j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                            { _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray()); st+=MotorStatus.mPosition[j] + ","; }

                            log.Log(LogLevel.Warn, $"Stopping Motors > "+st);
                            return false;
                        }
                    }
                    else if (((int)_TwinCathelper.PLCMotorsStatuses.mPostion[i] > int.Parse(Max_mm)) || ((int)_TwinCathelper.PLCMotorsStatuses.mPostion[i] < int.Parse("-" + Max_mm)))
                    {
                        log.Log(LogLevel.Error, $"Motor no:" + i + " has invalid position!");
                        String st = "";
                        var data = (int)EMode.Abort;
                        for (uint j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                        { _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray()); st += MotorStatus.mPosition[j] + ","; }

                        log.Log(LogLevel.Warn, $"Stopping Motors > " + st);

                        return false;
                    }

                }

                if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                {
                    log.Log(LogLevel.Error, $"Machine status disconnected! ");
                    DialogResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButtons.RetryCancel);

                    if (dr == DialogResult.Cancel)
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

        private void HomeTrigger()
        {
            if (!ConfirmMotorsAreFine())
                return;            

            uint zero = 0;

            float MaxPosition = 0.00F;

            try
            {
                if (MotorStatus.mPosition != null)
                    MaxPosition = MotorStatus.mPosition.Max();
                else
                    MaxPosition = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();
            }
            catch { }
            //float PositionRPMs = MaxPosition / 2;

            //float RPM_Assume = CSVHelper.RPMAssume;//float.Parse(ConfigurationManager.AppSettings["RPM_Assume"].ToString());
            //float RPMTimeMins = PositionRPMs / RPM_Assume;

            //float currentRotations = MaxPosition / 2; // rotations of current motor position
            //float CurrentRPMInMins = currentRotations / RPMTimeMins;
            //float CurrentRPMInSecs = RPMTimeMins * 60F;
            //float CurrentRampDelay = RPMTimeMins * 60 * 0.1F;
            //List<float> csvdata2 = CSVHelper.ReadCSV_DoubleData(SelectedFile);
            List<float> NegativeoffsetResume = new List<float>();
            String[] offset_resume = new String[_TwinCathelper.PLCMotors.Count()];

            try
            {
                for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                {
                    NegativeoffsetResume.Add(0.00f);
                    offset_resume[i] = "";
                }
            } 
            catch(Exception ee) { log.Log(LogLevel.Info, ee.Message); }
            String[] resumeStr = null;
            String CSVOffset_contents = "";

            try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

            //if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset"))
            if(CSVHelper.NegativeoffsetResume.Length>0)//0.0F
            {
                try
                {
                    //log.Log(LogLevel.Info, "Offset file found>>>" + "ManualStopped" +DeviceCount.ToString()+ ".offset");
                    //log.Log(LogLevel.Info, "Offset Data exist >>>"+ CSVHelper.NegativeoffsetResume);

                    //resumeStr = File.ReadAllLines(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset", Encoding.UTF8);
                    //CSVOffset_contents = resumeStr[0].ToString(CultureInfo.InvariantCulture);
                    CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                    offset_resume = CSVOffset_contents.Split('~');
                    log.Log(LogLevel.Info, "" + CSVOffset_contents);
                    //try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset"); } catch { }
                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset file found or Offset file corrupted!");
                    return;
                }
            }

            float calc_displacement = 0.00F;
            //string Max_mm = CSVHelper.MaxDisplacement.ToString();//ConfigurationManager.AppSettings["Max_mm"].ToString();

            //if (CSVOffset_contents != null && CSVOffset_contents.Length > 0)
            //{
            //    DialogResult dr1 = MessageBox.Show("Do you want to continue with following offset data?"+Environment.NewLine+""+ CSVOffset_contents, "Home move confirm required!", MessageBoxButtons.OKCancel);
            //    if (dr1 == DialogResult.Cancel)
            //        return;            
            //}



            String HomingOffsetDataCommand = "";
            float LastChangedVal = 0.00F;
            List<float> homeDisplacement = new List<float>();


            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                try
                {
                    if (CSVOffset_contents != null && CSVOffset_contents.Length > 0)
                    {
                        calc_displacement = MotorStatus.mPosition[i] + Math.Abs(float.Parse(offset_resume[i], vCulture));
                    }
                    else
                        calc_displacement = MotorStatus.mPosition[i];
                    //Max_mm
                    if (calc_displacement > float.Parse(CSVHelper.Max_mm.ToString(), vCulture))
                    {
                        log.Log(LogLevel.Error, "Calculated Value mm Exceeded! at " + i + " " + calc_displacement.ToString());
                        return;
                    }

                    if ((Math.Abs(calc_displacement) - Math.Abs(LastChangedVal)) > 1)
                    {
                        LastChangedVal = calc_displacement;
                        HomingOffsetDataCommand += "Motor " + i + ":=" + calc_displacement + Environment.NewLine;
                    }
                    homeDisplacement.Add(calc_displacement);
                }
                catch
                {

                    log.Log(LogLevel.Error, "Home Displacement Calculation Error! ");
                }

                _TwinCathelper.PLCMotors[i].mDirection = calc_displacement < 0 ? false : true;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;//float.Parse(ConfigurationManager.AppSettings["DefaultRampDelay"].ToString());
                                                                                       //if (CSVHelper.MotorTime > 0)
                                                                                       //    _TwinCathelper.PLCMotors[i].mTime = CSVHelper.MotorTime;//float.Parse(ConfigurationManager.AppSettings["DefaultRampTime"].ToString());
                                                                                       //else
                                                                                       //{
                RecommendedMaxDisplacement(homeDisplacement);
                int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;//int.Parse(ConfigurationManager.AppSettings["CSV_RecommendedRPM"].ToString());
                RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);//CSVHelper.MotorTime

                //_TwinCathelper.PLCMotors[i].mTime = GetRecommendedRPM(Math.Abs(calc_displacement));//CSVHelper.MotorTime;CSVHelper.MotorTime;//float.Parse(ConfigurationManager.AppSettings["DefaultRampTime"].ToString());


                //}

                //_TwinCathelper.PLCMotors[i].mRampDelay = Math.Abs(CurrentRampDelay);//RampDelayTime(sec)=5
                //_TwinCathelper.PLCMotors[i].mTime = Math.Abs(CurrentRPMInSecs);//Time(sec)=40
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;
                _TwinCathelper.PLCMotors[i].mDisplacement = Math.Abs(calc_displacement);
            }


            CSVHelper.MaxDisplacement = Math.Abs(homeDisplacement.Max());//GetRecommendedRPM(Math.Abs(homeDisplacement.Max()));
            calc_displacement = GetRecommendedRPM(Math.Abs(CSVHelper.MaxDisplacement));
            //reset time for all motors for negativeOffset
            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            { 
                _TwinCathelper.PLCMotors[i].mTime = Math.Abs(calc_displacement);

    //            log.Log(LogLevel.Info, "HOME_writeCV:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
    //+ "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay.ToString()
    //+ "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
    //+ "  >> mMode: " + (int)EMode.Project);

                log.Log(LogLevel.Warn, "ProjectView > HomeTrigger Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + (_TwinCathelper.PLCMotors[i].mDirection ? "Down" : "Up")
    + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
    + "  >> mRPM: " + _TwinCathelper.PLCMotors[i].mRPM
    + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
    + "  >> mMode: " + EMode.Project.ToString());

            }


            if (HomingOffsetDataCommand != null && HomingOffsetDataCommand.Length > 14)
            {
                DialogResult dr1 = MessageBox.Show("Do you want to continue with following offset data?" + Environment.NewLine + "" + HomingOffsetDataCommand, "Home move confirm required!", MessageBoxButtons.OKCancel);
                if (dr1 == DialogResult.Cancel)
                    return;
            }


            if (LastEventPressed == 'R')//On Resuming After PowerCutoff it will Home to CSV location of HOME and Position will be 0s
                LastEventPressed = 'N';//-ivePositionData

            MotorStatus.IsMotorsIdle = false;

            try
            {
                //if ((Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) > 1) && CSVHelper.LastSelectedCSV != selectedCSV)//negative offset exist
                //{
                //    MessageBox.Show("You can not select other shape while HomeOffset exist, Please calibrate first to coutinue!", "Please calibrate first to continue!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //    return;
                //}                

                CSVHelper.CompletingTime = _TwinCathelper.PLCMotors.Max().mTime + 5.00F;

                if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                CSVHelper.ST_timer.Restart();

                _TwinCathelper.WriteEthercatDeviceData();
                log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} Home value(s) has been written sucessfully");

                Thread.Sleep(200);

                for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                    _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
                log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} zero value(s) has been written sucessfully");

                //IsPsitionDataProcessing = true;
                try { LastPositionData = MotorStatus.mPosition[0]; } catch { LastPositionData = 0; }

                UserStop = false;
                //EventStopToggle = false;

                toggleStopResumeButtonText = "Stop";
                OnPropertyChanged();

                if (LastEventPressed == 'N')//-ivePositionData
                    return;
                else
                    LastEventPressed = 'H';// H=Home R=Resume S=Stop W= WriteCSV O=Initial

            }
            catch (Exception ep) { log.Log(LogLevel.Info, "HomeTrigger()>>>>"+ep.Message); }

            try { progressbar_timespent = 0; TravelTimeUpdate = false; progressbar_FinalVal = (int)(Math.Abs(_TwinCathelper.PLCMotors[0].mDisplacement)); }//_TwinCathelper.PLCMotors[0].mDisplacement;
            catch { }
            IsRampCompleted = false;

            try
            {
                if(CSVHelper.NegativeoffsetResume.Length>1)
                    LastmaxVal = (int)(Math.Abs(_TwinCathelper.PLCMotors.Min().mDisplacement));
                else
                    LastmaxVal = MotorStatus.mPosition.Max();
                    
            }
            catch { }
        }

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


        private void updateHomeButtonStatus()
        {
            //set the correct homing position
            try
            {
                progressActonCounter++;                

                //if (progressActonCounter > UpdateProgressStatusSeconds)
                //{
                    float chkdiff1 = 0.00F;
                    float[] fltmPos = new float[_TwinCathelper.PLCMotors.Count()];

                    if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
                    {
                        String[] newHomingArr = CSVHelper.NegativeoffsetResume.Split("~");
                        int i = 1;
                        for (int j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                            fltmPos[j] = float.Parse(newHomingArr[j], vCulture);
                    }

                    //if (MotorStatus.mPosition.Max() > LastmaxVal)
                    //    chkdiff1 = MotorStatus.mPosition.Max() - LastmaxVal;
                    //else chkdiff1 = LastmaxVal - MotorStatus.mPosition.Max();

                    //if (chkdiff1 >2)
                    //    return;

                    //if (MotorStatus.mPosition != null)
                    //    LastmaxVal = MotorStatus.mPosition.Max();
                    //else
                    //    LastmaxVal = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();


                    
                    if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
                    {
                        try
                        {

                            //fltmPos = float.Parse(newHomingArr[0], vCulture);

                            float chkdiff = 0.0F;

                            if (fltmPos.Max() > MotorStatus.mPosition.Max())
                                chkdiff = fltmPos.Max() - MotorStatus.mPosition.Max();
                            else
                                chkdiff = MotorStatus.mPosition.Max() - fltmPos.Max();

                            //only in home when diff is > 1
                            if (chkdiff <= 1.5F && chkdiff >= -1.5F)
                                toggleShapeHomeButtonText = "Shape";
                            else
                                toggleShapeHomeButtonText = "Home";


                        }
                        catch
                        {
                            float chkdiff = MotorStatus.mPosition.Max();

                            //only in home when diff is > 1
                            if (chkdiff < 1.0F || chkdiff > -1.0F)
                                toggleShapeHomeButtonText = "Home";
                            else
                                toggleShapeHomeButtonText = "Shape";

                        }
                    }
                    else
                    {
                        float chkdiff = MotorStatus.mPosition.Max();

                        //only in home when diff is > 1
                        if (chkdiff < 1.0F || chkdiff > -1.0F)
                            toggleShapeHomeButtonText = "Home";
                        else
                            toggleShapeHomeButtonText = "Shape";

                    }

                    progressActonCounter = 0;

                //}
            }
            catch { }

        }

        private void PositionProcess_Elapsed(object sender, EventArgs e)
        {
            try
            {
                if (CSVHelper.InPositionProcess_Elapsed)
                    return;

                CSVHelper.InPositionProcess_Elapsed = true;
                
                try
                {
                    if (CSVHelper.IsNewPanelSelected)
                    {
                        updateHomeButtonStatus();

                        CSVHelper.IsNewPanelSelected = false;
                    }

                    if (CSVHelper.IsNewProjSelected)
                        BrowseProjectFiles();


                    if(CSVHelper.UpdateHomeCSVStatus)
                    {
                        updateHomeButtonStatus();
                        CSVHelper.UpdateHomeCSVStatus = false;
                    }


                }
                catch { }

                float posDiff = 0;

                if (IsRampCompleted)
                {
                    
                    //IsHomeEnabled = true;

                    //pgProcesStatusVal = 100;
                    if (pgProcesStatusVal >= 99)
                    {
                        try { motorMaxTravel = "Max Travel: " + MotorStatus.mPosition.Max() + "mm"; } catch { motorMaxTravel = "Max Travel: 0mm"; }

                        if (LastProgressval != pgProcesStatusVal)//this condition will only run once when RampCompleted
                        {
                            log.Log(LogLevel.Info, $"Process Completed Successfully!");
                            TravelTimeUpdate = true;
                            CSVHelper.TotalNoHrs += (uint)(MachineTotalTimeSW.ElapsedMilliseconds / 1000);
                            travelTime = ConvertSectoDay(CSVHelper.TotalNoHrs);
                            MachineTotalTimeSW.Reset();
                            MachineTotalTimeSW.Stop();

                        }

                        if (LastEventPressed == 'H')
                        {
                            if (CSVHelper.NegativeoffsetResumeFlt[0] < 0)//if negHomingdata exist
                                if (MotorStatus.mPosition[0] > 0)
                                {
                                    posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) + Math.Abs(MotorStatus.mPosition[0]);

                                }
                                else
                                {
                                    posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) - Math.Abs(MotorStatus.mPosition[0]);                                    
                                }
                            pgProcesStatusVal = (int)(Math.Abs(posDiff / (progressbar_FinalVal)) * 100);//LastmaxVal=MaxDistance to cover - current position
                        }
                        else
                            pgProcesStatusVal = (int)(Math.Abs((posDiff + Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0])) / (progressbar_FinalVal + Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) + LastPositionData)) * 100);//LastmaxVal=MaxDistance to cover - current position

                        LastProgressval = pgProcesStatusVal;

                        
                    }
                    //positionProcessCompleted_Timer.Stop();
                    try { //if (progressbar_FinalVal < 1) pgProcesStatusVal = 0;  else 
                            pgProcesStatusVal = (int)Math.Abs(Math.Abs((MotorStatus.mPosition.Max()+ Math.Abs(CSVHelper.NegativeoffsetResumeFlt.Min())) / progressbar_FinalVal) * 100); IsRampCompleted = false;
                    } catch { }
                }
                else {
                    //if (progressbar_FinalVal < 1) pgProcesStatusVal = 0; else pgProcesStatusVal = (int)Math.Ceiling(Math.Abs(MotorStatus.mPosition.Max() / progressbar_FinalVal) * 100); LastProgressval = 0;

                    try
                    {
                        //pgProcesStatusVal = (int)LastmaxVal - (int)(Math.Abs(MotorStatus.mPosition.Max() / progressbar_FinalVal) * 100);//LastmaxVal=MaxDistance to cover - current position
                        

                        if (CSVHelper.NegativeoffsetResume.Length > 1)
                        {
                            //progressbar_FinalVal >>>> Distance to cover
                            //MotorStatus.mPosition >>>> current distance position

                            float chkNegPositions = MotorStatus.mPosition[0];
                            //if (chkNegPositions < -1)
                            //    posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt.Min()) - Math.Abs(MotorStatus.mPosition.Min());
                            //else
                            //    posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt.Min()) + Math.Abs(MotorStatus.mPosition.Max());

                            posDiff = MotorStatus.mPosition[0];


                        }
                        else
                        { posDiff = MotorStatus.mPosition[0]; }
                        if (LastEventPressed == 'H')
                        {
                            if (CSVHelper.NegativeoffsetResumeFlt[0] < 0)//if negHomingdata exist
                                if (MotorStatus.mPosition[0] > 0)
                                { posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) + Math.Abs(MotorStatus.mPosition[0]);
                                    
                                }
                                else
                                {
                                    posDiff = Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) - Math.Abs(MotorStatus.mPosition[0]);
                                    //pgProcesStatusVal = (int)(Math.Abs((posDiff ) / (progressbar_FinalVal + Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) + LastPositionData)) * 100);//LastmaxVal=MaxDistance to cover - current position
                                }
                            pgProcesStatusVal = (int)(Math.Abs(posDiff / (progressbar_FinalVal)) * 100);//LastmaxVal=MaxDistance to cover - current position


                        }
                        else
                            pgProcesStatusVal = (int)(Math.Abs((posDiff + Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0])) / (progressbar_FinalVal + Math.Abs(CSVHelper.NegativeoffsetResumeFlt[0]) + LastPositionData)) * 100);//LastmaxVal=MaxDistance to cover - current position
                    }
                    catch { }

                    try
                    {
                        if (!TravelTimeUpdate)
                        {
                            progressbar_timespent += 1;
                            TimeSpan timesp = TimeSpan.FromSeconds(progressbar_timespent);
                            _travelTime = "Travel Time: " + timesp.ToString(@"hh\:mm\:ss\:fff");
                        }
                    }
                    catch { }

                }


                //log.Log(LogLevel.Info, $"This is four444555" + pgProcesStatusVal);
                OnPropertyChanged();



                try
                {
                    String CSVMotors_contents = "";
                    foreach (float flt in MotorStatus.mPosition)
                        CSVMotors_contents += flt.ToString(vCulture) + "~";

                    CSVMotors_contents = CSVMotors_contents.Substring(0, CSVMotors_contents.Length - 1);

                    Dictionary<string, string> dct = new Dictionary<string, string>();
                    dct.Add("LastPositionValues", CSVMotors_contents);
                    FlexMoldDB.UpdateData("FM_MotorSetting", null, dct);

                    CSVHelper.LastPositionValues = CSVMotors_contents;
                }
                catch (Exception ee) { log.Log(LogLevel.Info, $"Database Lastposition issue>>>" + ee.StackTrace); }


                // write last position file
                //using (StreamWriter writer = new StreamWriter(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\LastPositions"+ MotorStatus.mPosition.Count() + ".Backup", false, Encoding.UTF8))
                //{


                //    writer.Write(CSVMotors_contents);
                //    writer.WriteLine();
                //    writer.Flush();
                //}
                //log.Log(LogLevel.Info, $"Saving CSV/Motors data for Resume Process, Completed!");


            }
            catch { }

            CSVHelper.InPositionProcess_Elapsed = false;
        }


        private void StallStopProcess_Timer_Elapsed(object sender, EventArgs e)
        {
            if (!ConfirmMotorsAreFine())
                return;
            if (LastEventPressed != 'R' && LastEventPressed != 'D')
                LastEventPressed = 'S';//Manual Stopped is pressed Here position goes "0"
            StallStopProcess_WriteCSVData();
            StallErrMotorsCheckTimer.Stop();
            IsRampCompleted = false;
            //progressbar_FinalVal = 50;
        }


        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (Timer_ElapsedBusy)
                return;

            Timer_ElapsedBusy = true;
            if (!ConfirmMotorsAreFine())
                return;

            //LastEventPressed = 'N';//Manual Stopped is pressed Here position goes "0"
            
            //TimeSpan lastStopSecs = DateTime.Now - CSVHelper.LastStopTime;

            //if(Math.Abs(lastStopSecs.TotalSeconds) > intSpecifiedSecs)
                StopProcess_WriteCSVData();

                MotorsCheckTimer.Stop();
            Timer_ElapsedBusy = false;
        }

        //public LaserProjector MyLaserPorjector { get; set; }
        public bool SaveData { get; set; }
        //private void SambaConfigWindow()
        //{
        //    Logger log = LogManager.GetLogger("Samba Config");
        //    try
        //    {
        //        log.Log(LogLevel.Info, $"Current IP {MyLaserPorjector.IpAddres} UserName {MyLaserPorjector.UserName}");
        //        log.Log(LogLevel.Info, $"Please Enter Samba params");
        //        //Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
        //        //{
        //        //    await DialogHost.Show(_SambaConfigWindow, "RootDialog", ClosingEventHandler);
        //        //}));
        //        if (SaveData)
        //        {
        //            log.Log(LogLevel.Info, $"Saved IP {MyLaserPorjector.IpAddres}");
        //            log.Log(LogLevel.Info, $"Saved PlotFolderName {MyLaserPorjector.PlotFolderName}");
        //            log.Log(LogLevel.Info, $"Saved UserName {MyLaserPorjector.UserName}");
        //        }
        //        else
        //        {
        //            log.Log(LogLevel.Warn, $"Closing without saving");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Log(LogLevel.Error, ex, ex.Message);
        //        TNF.ShowError(ex.Message);
        //    }
        //}

        //private LaserProjSelectionView _LaserProjSelectionView { get { return App.AppHost.Services.GetRequiredService<LaserProjSelectionView>(); } }

        //private async Task LaserProjIPSelectWindow()        
        //{
        //    Logger log = LogManager.GetLogger("Please select the laser projector!");
        //    try
        //    {
        //        //string SelectedZlaserIP = "";
        //        //log.Log(LogLevel.Info, $"Current port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
        //        //log.Log(LogLevel.Info, $"Please Select Port params from available list ");
        //        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            DialogHost.Show(_LaserProjSelectionView, "RootDialog", ClosingEventHandler);
        //        }));

        //        //_ = await DialogHost.Show(_LaserProjSelectionView, "RootDialog", ClosingEventHandler);
        //        //if (SaveData)
        //        //{

        //        //DialogHost.Show(_LaserProjSelectionView, "RootDialog", ClosingEventHandler);

        //        //LaserProjector MyLaserProjector = new LaserProjector();
        //        //MyLaserPorjector.IpAddres = _LaserProjSelectionView.LP_ZLaserIPSelect.Items.CurrentItem.ToString();
                
        //        //MyLaserPorjector.TelnetPort = _LaserProjSelectionView.LP_ZLaserPortSelect;
        //        log.Log(LogLevel.Info, $"Selected laser projector {_LaserProjSelectionView.LP_ZLaserIPSelect}");
        //        //}
        //        //else
        //        //{
        //        //    log.Log(LogLevel.Warn, $"Zlaser IPselection closing without saving");
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Log(LogLevel.Error, ex.Message);
        //        TNF.ShowError(ex.Message);
        //    }
        //}



        //private SambaConfigWindow _SambaConfigWindow { get { return App.AppHost.Services.GetRequiredService<SambaConfigWindow>(); } }

        private int _LaserProjectorComMode;
        public int LaserProjectorComMode
        {
            get { return _LaserProjectorComMode; }
            set
            {
                _LaserProjectorComMode = value;
                switch ((EProjectorComMode)value)
                {
                    case EProjectorComMode.Serial:
                        //SerialConfigWindow();
                        log.Log(LogLevel.Warn, "This feature not supported in project section!");
                        break;

                    case EProjectorComMode.Samba:
                        //SambaConfigWindow();
                        log.Log(LogLevel.Warn, "This feature not supported in project section!");
                        break;

                    case EProjectorComMode.Telnet:
                        break;

                    default:
                        break;
                }
                //OnPropertyChanged();
            }
        }

        public async Task sendLasFileAsync(FileInfo _slectedFile)
        {
            try
            {
                if (_slectedFile == null)
                {
                    log.Log(LogLevel.Warn, ErrorMessages.Please_Select_File_First);
                    return;
                }
                if (!_slectedFile.FullName.EndsWith(".las"))
                {
                    log.Log(LogLevel.Warn, $"Selected File Not a .las File {_slectedFile.Name}");
                    return;
                }
                try
                {
                    LaserProjectorComMode = 2;
                    switch (LaserProjectorComMode)
                    {
                        case (int)EProjectorComMode.Serial:
                            //log.Log(LogLevel.Info, ErrorMessages.Sending_File_Via_Serial.Humanize());
                            //try
                            //{
                            //    using (FileStream fs = File.OpenRead(_slectedFile.FullName))
                            //    {
                            //        log.Log(LogLevel.Info, $"Opening Port {SelectedSerialPort.PortName} Baud rate {SelectedSerialPort.BaudRate}");
                            //        SelectedSerialPort.Open();
                            //        SelectedSerialPort.WriteTimeout = 2000;
                            //        SelectedSerialPort.Write((new BinaryReader(fs)).ReadBytes((int)fs.Length), 0, (int)fs.Length);
                            //        log.Log(LogLevel.Info, $"File Write Sucessfully");
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    TNF.ShowError(ex.Message);
                            //    log.Log(LogLevel.Error, ex.Message);
                            //}
                            //finally
                            //{
                            //    SelectedSerialPort.Close();
                            //}
                            log.Log(LogLevel.Warn, "This feature not supported in project section!");
                            break;

                        case (int)EProjectorComMode.Samba:
                            //string _tmsg = $"Sending file via samba";
                            //log.Log(LogLevel.Info, _tmsg);
                            //TNF.ShowInformation(_tmsg);
                            //try
                            //{
                            //    log.Log(LogLevel.Info, $"Connecting to {MyLaserPorjector.IpAddres}");
                            //    using (NetworkShareAccesser.Access(MyLaserPorjector.IpAddres, MyLaserPorjector.UserName, MyLaserPorjector.Password))
                            //    {
                            //        log.Log(LogLevel.Info, $"Credentials Accepted");
                            //        log.Log(LogLevel.Info, $"Copying File");
                            //        File.Copy(_slectedFile.FullName,
                            //            $@"\\{MyLaserPorjector.IpAddres}\{MyLaserPorjector.PlotFolderName}\{_slectedFile.Name}",
                            //            true);
                            //        log.Log(LogLevel.Info, $"File Copied Sucessfully");
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    TNF.ShowError(ex.Message);
                            //    log.Log(LogLevel.Error, ex.Message);
                            //}
                            log.Log(LogLevel.Warn, "This feature not supported in project section!");
                            break;

                        case (int)EProjectorComMode.Telnet:

                            log.Log(LogLevel.Info, $"Sending file via Telnet");

                            //Select the laser projector first
                            //LaserProjIPSelectWindow();
                            if (CSVHelper.IpAddres != null && CSVHelper.IpAddres.Length>8)
                            {
                                //log.Log(LogLevel.Warn, "Selected laser projector>>>" + CSVHelper.IpAddres + ":" + CSVHelper.TelnetPort);

                                try
                                {
                                    //if (TelnetHelper.Instance.ConnectTelnet(MyLaserPorjector.IpAddres, MyLaserPorjector.TelnetPort) == null)
                                    if (TelnetHelper.Instance.ConnectTelnet(CSVHelper.IpAddres, CSVHelper.TelnetPort) == null)
                                    {
                                        CSVHelper.LaserEnabled = false;
                                        var ii = await TelnetHelper.Instance.ConnectTelnet(CSVHelper.IpAddres, CSVHelper.TelnetPort);
                                        if (!ii)
                                        {
                                            var tmsg = $"Telnet Failed to connect : {CSVHelper.IpAddres} , {CSVHelper.TelnetPort}";
                                            log.Log(LogLevel.Error, tmsg);
                                            TNF.ShowError(tmsg);
                                            return;
                                        }
                                        log.Log(LogLevel.Warn, "Laser Projector is now connected, Please try again sending file!");

                                    }
                                    else
                                    {
                                        CSVHelper.LaserEnabled = true;
                                        //Send PowerOn Message
                                        TelnetHelper.Instance.SendMessage = "echo ZN; " + "\r\n";
                                        _ = TelnetHelper.Instance.SendTelnetMessgae();
                                        
                                        Thread.Sleep(300);

                                        TelnetHelper.Instance.SendMessage = await MixUtil.ReadTextFromFileAsync(_slectedFile.FullName);
                                        _ = TelnetHelper.Instance.SendTelnetMessgae();
                                        return;
                                    }
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    TNF.ShowError(ex.Message);
                                    log.Log(LogLevel.Error, ex.Message);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select the laser projector from Laser Settings!","No projector selected!",MessageBoxButtons.OK);
                                log.Log(LogLevel.Error, "Laser Projector is not selected! "); }
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(LogLevel.Error, ex.Message);
                    TNF.ShowError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }

        private void checkFirmware() {

            try
            {

                String FirmwareNotMatched = "";
                for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)  
                {
                    if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713969130 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713968323 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)//0x662917EA (1713969130=Wed Apr 24 2024 14:32:10 GMT+0000) CSVHelper.MotorsStatusGlobal[0] == "Healthy" && //1713968323=Wed Apr 24 2024 14:18:43 GMT+0000
                        continue;
                    else
                        FirmwareNotMatched += "Motor" + (i + 1) + ",";
                }

                if (!CSVHelper.MachineEnabled)
                {
                    MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (FirmwareNotMatched.Length > 0)
                {
                    MessageBox.Show("Please update the firmware/machine versions of the following motor(s)!" + Environment.NewLine + FirmwareNotMatched, "Wrong firmware/Machine found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //MessageBox.Show("Please close the application and update the firmware!" + Environment.NewLine + FirmwareNotMatched, "Wrong firmware found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CSVHelper.wrongFirmwareFound = true;
                    
                    return;
                }

            }
            catch { }
        
        }


        public ProjectViewModel(ITwinCathelper twinCathelper)
        {
            try
            {
                DoMachineUserCanceled = false;
                FlexMoldDB.InitializeFlexMoldDB();
                this._TwinCathelper = twinCathelper;
                //Machine settings view variables updated here//
                //motorCount = TwinCathelper.DeviceCount;
                //machineOperatedTime = "";
                //machinePartsProduced = 10000;
                //machineRadius = 10;
                //machineSize = 1000;
                //Machine settings view variables updated here//


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

                try
                {
                    GetSettingsDB();

                    // if the Application status was opened
                    if (CSVHelper.LastSystemStatus != 0)
                    {

                        DialogResult dr = MessageBox.Show("Application was not closed properly, Do you want to restore last state?", "Restore last machine state?", MessageBoxButtons.YesNo);
                        if (dr == DialogResult.Yes)
                            RestoreLastState();

                    }

                    UpdateSettingsDB(); // set the Application status to open ==1

                    BrowseProjectFiles();                    
                }
                catch { }


                MotorsCheckTimer.Tick += new EventHandler(Timer_Elapsed);
                MotorsCheckTimer.Interval = new TimeSpan(0, 0, 1);

                //StallStopProcess_WriteCSVData
                StallErrMotorsCheckTimer.Tick += new EventHandler(StallStopProcess_Timer_Elapsed);
                StallErrMotorsCheckTimer.Interval = new TimeSpan(0, 0, 3);

                positionProcessCompleted_Timer.Tick += new EventHandler(PositionProcess_Elapsed);
                positionProcessCompleted_Timer.Interval = new TimeSpan(0, 0, 1);
                

                this._TwinCathelper = twinCathelper;
                try
                {
                    IsHomeEnabled = false;
                    LaserSendButton = false;
                    IsStopEnable = false;

                    if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                    {
                        log.Log(LogLevel.Error, $"Machine status disconnected! ");
                        DialogResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButtons.RetryCancel);

                        if (dr == DialogResult.Cancel)
                        {
                            DoMachineUserCanceled = true;
                            return;
                        }

                        return;
                    }

                }
                catch { }

                //Browse CSV folder loaded
                try
                {
                    toggleStopResumeButtonText = "Stop";
                    OnPropertyChanged();
                    UserStop = false;
                    BrowseProject();
                }
                catch (Exception ee)
                {
                    IsHomeEnabled = false;
                    LaserSendButton = false;
                    IsStopEnable = false;
                }

                //Fill project folder list
                FileProjectList();

                BrowseProjectCommand = new RelayCommand(o =>
                {
                    try
                    {
                        toggleStopResumeButtonText = "Stop";
                        OnPropertyChanged();
                        UserStop = false;
                        //EventStopToggle = false;

                        if (!CSVHelper.MachineEnabled)
                        {
                            MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    catch (Exception ee)
                    { }

                    //BrowseProject();
                    BrowseProjectFiles();
                    //File.Delete(SelectedFile + ".Resume");

                });
                StopMotorsCommand = new RelayCommand(o =>
                {
                    if (toggleStopResumeButtonText == "Stop")
                    {

                        if (!CSVHelper.MachineEnabled)
                        {
                            MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (LastEventPressed == 'H')
                        { log.Log(LogLevel.Warn, $"Manual Stop not allowed while Homing!");
                            //return; 
                        }

                            LastEventPressed = 'S';
                        if (MachineTotalTimeSW.IsRunning)
                        {
                            CSVHelper.TotalNoHrs += (uint)(MachineTotalTimeSW.ElapsedMilliseconds / 1000);
                            MachineTotalTimeSW.Stop();
                            MachineTotalTimeSW.Reset();
                        }
                        IsRampCompleted = false;

                        if (MotorStatus.mPosition != null)
                            LastmaxVal = MotorStatus.mPosition.Max();
                        else
                            LastmaxVal = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();

                        String st = "";
                        var data = (int)EMode.Abort;
                        for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                        { _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mMode, data.CastToArray()); st += MotorStatus.mPosition[i] + ","; }
                        log.Log(LogLevel.Warn, $"Stopping Motors > " + st);

                        if (!StopProcess_WriteCSVData())
                            return;

                        toggleStopResumeButtonText = "Resume";
                        OnPropertyChanged();


                        IsHomeEnabled = true;
                    }
                    else
                    {
                        log.Log(LogLevel.Warn, $"Resuming Motors");


                        if (!CSVHelper.MachineEnabled)
                        {
                            MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        MachineTotalTimeSW.Start();
                        IsRampCompleted = false;

                        toggleStopResumeButtonText = "Stop";
                        OnPropertyChanged();

                        LastEventPressed = 'R';
                        ResumeProcess_WriteCSVData();
                        IsHomeEnabled = false;

                    }
                    //EventStopToggle = false;
                    UserStop = false;
                });


                ShapeHomCommand = new RelayCommand(async o =>
                {
                    if (!CSVHelper.MachineEnabled)
                    {
                        MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    IsHomeEnabled = false;

                    if (toggleShapeHomeButtonText == "Home")
                    {


                        if (LastEventPressed == 'S')
                        {
                            //create current offsetCovreUP file 
                        }

                        MachineTotalTimeSW.Start();
                        IsRampCompleted = false;                        

                        HomeTrigger();
                        toggleShapeHomeButtonText = "Shape";
                        IsStopEnable = false;

                        FileInfo SelectedFile_finf = new FileInfo(selectedCSV);
                        if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"))
                        {
                            log.Log(LogLevel.Info, $"Deleting Resume File!");

                            try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Resume" + selectedListRootDir.Substring(selectedListRootDir.LastIndexOf(@"\")) + "-" + SelectedFile_finf.Name + ".Resume"); } catch { }
                            //try { File.Delete(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + "ManualStopped" +DeviceCount.ToString()+ ".offset"); } catch { }
                            MotorStatus.IsMotorsIdle = false;
                        }

                        IsStopEnable = true;
                        toggleStopResumeButtonText = "Stop";
                        OnPropertyChanged();

                    }
                    else {//Shape or write csv is called
                        await CSVSend();
                        IsStopEnable = true;
                        CSVHelper.PartsProduced += 1;
                        toggleShapeHomeButtonText = "Home";

                    }
                });

                //SendCSVCommand = new RelayCommand(async o =>
                //{
                //    await CSVSend();
                //    IsStopEnable = true;
                //});

                //HomeMotorsCommand = new RelayCommand(o =>
                //{
                //    if (LastEventPressed == 'S')
                //    {
                //    //create current offsetCovreUP file 
                //}

                //    HomeTrigger();
                //    IsStopEnable = false;
                //});

                //ForceResumeCommand = new RelayCommand(o =>
                //{
                //    LastEventPressed = 'F';

                //});

                SendLasFileCommand = new RelayCommand(o => {
                    FileInfo SelectedFile_finf = new FileInfo(SelectedFile.FileFullName);
                    _ = sendLasFileAsync(SelectedFile_finf); });

                if (!CSVHelper.MachineEnabled)
                {
                    MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MyPorjector == null)
                {
                    try
                    {
                        //ZlaserIP = ConfigurationManager.AppSettings["ZlaserIP"].ToString();
                        //ZlaserPort = int.Parse(ConfigurationManager.AppSettings["ZlaserPort"].ToString());

                            DataSet dsMachSet = new DataSet();
                            dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", "LP_IsDefault=1");

                            //string[] listProjIP = new string[dsMachSet.Tables[0].Rows.Count];
                            int cntRows = 0;
                            if (dsMachSet != null)
                            {
                                foreach (DataTable dt in dsMachSet.Tables)
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        ZlaserIP = dr["LP_IPAddress"].ToString();
                                        ZlaserPort = int.Parse(dr["LP_Port"].ToString());
                                    CSVHelper.IpAddres = ZlaserIP;
                                    CSVHelper.TelnetPort = ZlaserPort;

                                }
                                break;
                                }
                            }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex, ex.Message);
                        TNF.ShowError(ex.Message);
                    }

                    MyPorjector = new LaserProjector()
                    {
                        Name = "p1",
                        IpAddres = ZlaserIP,
                        UserName = "123",
                        Status = 1,
                        TelnetPort = ZlaserPort,
                        Password = "123456",
                    };
                }

                if (csvdata == null)
                {
                    CSVHelper.MaxDisplacement = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();

                    //CSVHelper.MaxRPM = float.Parse(ConfigurationManager.AppSettings["CSV_MaxRPM"].ToString());
                    int CSV_RecommendedRPM = (int)CSVHelper.RecommendedRPM;//int.Parse(ConfigurationManager.AppSettings["CSV_RecommendedRPM"].ToString());
                    RecommendedRPM(CSV_RecommendedRPM, CSVHelper.MaxRPM);//CSVHelper.MotorTime
                    //CSVHelper.MotorDirValue = int.Parse(ConfigurationManager.AppSettings["CSV_MotorDirValue"].ToString());

                    //CSVHelper.MotorRampDelayValue = float.Parse(ConfigurationManager.AppSettings["CSV_DefaultTimeSec"].ToString());//CSV_DefaultTimeSec=5

                    string dir = CSVHelper.MotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";
                    log.Log(LogLevel.Info, $"Selected Direction : {dir}");
                }

                
                try { motorMaxTravel = "Max Travel: " + MotorStatus.mPosition.Max() + "mm"; } catch { motorMaxTravel = "Max Travel: 0mm"; }
                try { travelTime = ConvertSectoDay(CSVHelper.TotalNoHrs); } catch (Exception ex) { string st = ex.Message; travelTime = ConvertSectoDay(1); }

                if (MotorStatus.mPosition != null)
                    LastmaxVal = MotorStatus.mPosition.Max();
                else
                    LastmaxVal = _TwinCathelper.PLCMotorsStatuses.mPostion.Max();

                updateHomeButtonStatus();
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Project");
                log.Log(LogLevel.Error, ex.Message);
            }

            checkFirmware();
        }

        private void UpdateSettingsDB()
        {
            Dictionary<string, string> dct = new Dictionary<string, string>();
            //dct.Add("Size", machineSize.ToString());
            //dct.Add("MinRadius", machineRadius.ToString());
            //LastSelectedCSV
            if (selectedCSV != null && selectedCSV.Length > 1)
            { 
                dct.Add("LastSelectedCSV", selectedCSV);
                CSVHelper.LastSelectedCSV = selectedCSV;
            }

            dct.Add("MotorCount", _TwinCathelper.PLCMotors.Count().ToString());// motorCount.ToString());
            dct.Add("LastSystemStatus", "1");//System status 0=ProperShutDown
            FlexMoldDB.UpdateData("FM_MachineSetting", null, dct);
        }

        private void GetSettingsDB()
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
            //Dictionary<string, string> dct = new Dictionary<string, string>();
            //dct.Add("Size", machineSize.ToString());
            //dct.Add("MinRadius", machineRadius.ToString());
            //dct.Add("MotorCount", motorCount.ToString());
            //dct.Add("PartsProduced", machinePartsProduced.ToString());
            //dct.Add("TimeInOperation", machineOperatedTime.ToString());

            DataSet dsMachSet = FlexMoldDB.ReadData("FM_MachineSetting", null);

            if (dsMachSet != null)
            {
                foreach (DataTable dt in dsMachSet.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        CSVHelper.PartsProduced = int.Parse(dr["PartsProduced"].ToString()); 
                        CSVHelper.TotalNoHrs = uint.Parse(dr["TimeInOperation"].ToString());
                        CSVHelper.LastSystemStatus = uint.Parse(dr["LastSystemStatus"].ToString());
                        CSVHelper.LastSelectedCSV = dr["LastSelectedCSV"].ToString();

                        break;
                    }
                    break;
                }
            }
            //GetSettings ends here!
        }



    }
}

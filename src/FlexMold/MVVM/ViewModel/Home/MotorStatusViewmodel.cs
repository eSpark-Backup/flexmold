using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static FlexMold.Utility.TwinCathelper;

namespace FlexMold.MVVM.ViewModel.Home
{
    public class MotorStatusViewModel : StatusBaseViewModel, IMotorStatusViewModel
    {
        private readonly ITwinCathelper _TwinCathelper;
        private Logger log = LogManager.GetLogger("MotorStatus");
        public IMotorStatusViewModel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewModel>(); } }
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand CalibrateCommand { get; set; }
        public RelayCommand Calibrate02Command { get; set; }
        public RelayCommand CompleteCalibrateCommand { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }
        public RelayCommand OnLoadedCommand { get; set; }
        public RelayCommand OnUnLoadedCommand { get; set; }
        private Hashtable hlist = new Hashtable();
        public static Hashtable RampMotorsHlist = new Hashtable();
        private Hashtable CalibrationCompletedList = new Hashtable();
        
        private void DisplayError(MotorStatusControl o)
        {
            MessageBox.Show(o.ToolTip);
            Console.WriteLine();
        }
        private bool _IsMotorsIdle;

        public bool IsMotorsIdle
        {
            get { return _IsMotorsIdle; }
            set { _IsMotorsIdle = value; OnPropertyChanged(); }
        }

        //initialize to 0 for homeoffset
        public void UpdateHomingProcess()
        {
            log.Log(LogLevel.Info, "Update Homing started!");
            String errmsg = "";
            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            {
                try
                {
                    String newCsvHomeData = "";
                    //log.Log(LogLevel.Info, "chkMotorInput222 chkMotorInput>>>>" + DeviceCount.ToString() + "---" + CSVHelper.NegativeoffsetResume);

                    //foreach (String st in newHomingArr)
                    for (int j = 0; j < DeviceCount; j++)
                    {
                        try
                        {
                            newCsvHomeData += "0~";//vCulture
                        }
                        catch { }
                    }


                    CSVHelper.NegativeoffsetResume = newCsvHomeData.Substring(0, newCsvHomeData.Length - 1);
                    CSVHelper.SetNegativeoffsetResumeFlt();

                    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

                    log.Log(LogLevel.Info, "Successfully updated homing data!" + errmsg);

                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");

                }
            }
        }


        //public void HardCalibrateProcess()
        //{
        //    log.Log(LogLevel.Info, "HardCalibrateProcess Started successfully!");
        //    try
        //    {
        //        //if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
        //        //{
        //        //    log.Log(LogLevel.Error, $"Machine status disconnected! ");
        //        //    MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

        //        //    if (dr == MessageBoxResult.Cancel)
        //        //    {
        //        //        DoMachineUserCanceled = true;
        //        //        return;
        //        //    }

        //        //    return;
        //        //}
        //        Thread.Sleep(400);
        //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = false;
        //        CalibrateSeq02_HomeUp();


        //    //Line1:
        //    //    Thread.Sleep(1000);
        //    //    //if (_TwinCathelper.PLCMotors[0]<1.0F)
        //    //    if(MotorStatus.mPosition[0] < 1.0F)
        //    //        goto Line1;

        //    //    //while (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted)
        //    //    //    DoEvents();

        //    //    CalibrateSeq02_HomeDown();

        //    }
        //    catch (Exception ee) { log.Log(LogLevel.Warn, "CalibrateSeq02 Ended with error! " + ee.Message); }
        //    log.Log(LogLevel.Info, "HardCalibrateProcess Completed successfully!");
        //}

        void CalibrateSeq02_HomeUp()
        {

            for (int i = 0; i < DeviceCount; i++)
            {
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Resume;

                //log.Log(LogLevel.Info, "CalibrateHard-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                //+ "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                //+ "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
                //+ "  >> mMode: " + EMode.Project.ToString());
            }

            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
            log.Log(LogLevel.Warn, $"{DeviceCount} Calibration value(s) has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

        }
        //void CalibrateSeq02_HomeDown()
        //{

        //    for (int i = 0; i < DeviceCount; i++)
        //    {
        //        _TwinCathelper.PLCMotors[i].mDisplacement = 0.200F;
        //        _TwinCathelper.PLCMotors[i].mDirection = true;
        //        _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
        //        _TwinCathelper.PLCMotors[i].mTime = 6.3334F;
        //        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

        //        log.Log(LogLevel.Info, "CalibrateHard2-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
        //        + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
        //        + "  >> MotorTime: " + CSVHelper.MotorTime.ToString()
        //        + "  >> mMode: " + EMode.Project.ToString());
        //    }

        //    _TwinCathelper.WriteEthercatDeviceData();
        //    log.Log(LogLevel.Info, $"{DeviceCount} CalibrateHard value(s) has been written sucessfully");

        //    Thread.Sleep(200);
        //    uint zero = 0;
        //    for (uint i = 0; i < DeviceCount; i++)
        //        _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
        //    log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

        //}



        public void CalibrateSeq02()
        {
            //IsMotorsIdle = false;
            var log = LogManager.GetLogger("Device Status");
            log.Log(LogLevel.Warn, "Sending CalibrationSeq02 Request, Please wait.....");
            var array = new uint[DeviceCount];
            try
            {
                if (!CSVHelper.MachineEnabled)
                {
                    MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ee)
            { }



            try
            {
                //Array.Copy(_TwinCathelper.PLCMotorsStatuses.DeviceStatus, array, array.Length);
                //DeviceStatus = array;

                if (CalibrationData == null || CalibrationData.Count < 1)
                {
                    FileInfo finf = new FileInfo(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + DeviceCount + ".calibrate");
                    CalibrationData = CSVHelper.ReadCalibrate_DoubleData(finf);
                }

            }
            catch (Exception ee) { log.Log(LogLevel.Info, "CalibrateSeq02>Stack>>>>" + ee.StackTrace); }

            CSVHelper.IsLastEventCalibr = true;
            //log.Log(LogLevel.Info, $"CalibrateSeq02 befpre");
            //Motor_DeviceStatusChanged(this, null);



            //Thread.Sleep((int)_TwinCathelper.AppSettings.CalibrationSeq02Delay);
            //Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["CalibrationSeq02Delay"].ToString()));
            try
            {
                Thread.Sleep(CSVHelper.AdsStateTimeout);
                //Read the contents of the file into a stream

                //log.Log(LogLevel.Info, "Beofre sending Calibration Seq02 >>> Please wait...");
                RecommendedMaxDisplacement(CalibrationData);
                float mtime = GetRecommendedRPM(CalibrationData.Max());
                //WriteDistpacementCSVData(CalibrationData);
                //log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged333");
                for (int i = 0; i < DeviceCount; i++)
                {
                    _TwinCathelper.PLCMotors[i].mDisplacement = CalibrationData[i];
                    _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
                    _TwinCathelper.PLCMotors[i].mTime = mtime;

                    log.Log(LogLevel.Warn, "MotorTest > CalibrateSeq02 Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: Up"// + (_TwinCathelper.PLCMotors[i].mDirection ? "Down" : "Up")
                        + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                        + "  >> mRPM: " + CSVHelper.RecommendedRPM
                        + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                        + "  >> mMode: " + (int)EMode.CalibrationSeq02Request);

                }
                _TwinCathelper.WriteEthercatDeviceData();
                log.Log(LogLevel.Info, $"{EMode.CalibrationSeq02Request}. Please wait...");
                TNF.ShowSuccess("Calibration Seq02 Requested-Motorstatus");
                Thread.Sleep(200);
                for (int i = 0; i < DeviceCount; i++)
                    _TwinCathelper.PLCMotors[i].mDisplacement = 0;
                _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
                log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

                CSVHelper.UpdateHomeCSVStatus = true;

                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '2';
            }
            catch (Exception ee) { log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged>>>>" + ee.Message); }



            //log.Log(LogLevel.Info, $"CalibrateSeq02 aftert");

            ////if (DeviceStatus.SequenceEqual(validSeq01Val))
            ////{
            ////Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["CalibrationSeq02Delay"].ToString()));
            //Thread.Sleep(CSVHelper.AdsStateTimeout);

            ////Read the contents of the file into a stream


            //WriteDistpacementCSVData(CalibrationData);

            //for (int i = 0; i < DeviceCount; i++)
            //{
            //    _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
            //}

            //_TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);

            //log.Log(LogLevel.Warn, $"{EMode.CalibrationSeq02Request}. Please wait...");
            //TNF.ShowSuccess("Calibration Seq02 Requested");
            //Thread.Sleep(200);
            //for (int i = 0; i < DeviceCount; i++)
            //    _TwinCathelper.PLCMotors[i].mDisplacement = 0;
            //_TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
            //log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");
            ////}


            //if (DeviceStatus.SequenceEqual(validSeq02Val))
            //{
            //    log.Log(LogLevel.Info, $"Calibration Sucessful");
            //    for (int i = 0; i < DeviceCount; i++)
            //    {
            //        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
            //    }
            //    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
            //    log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
            //    TNF.ShowSuccess("Calibration Sucessful");
            //}

            //if (DeviceStatus.SequenceEqual(validForHome))
            //{
            //    log.Log(LogLevel.Info, $"All Motors Device Status : {EDeviceStatus.None}");
            //    //IsMotorsIdle = true;
            //}

            //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
            //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
            //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

        }
        
        public void CompleteCalibrate()
        {
            //CSVHelper.CalibrateCalledByEmergency = false;

            var log = LogManager.GetLogger("Calibration");

            try
            {
                try
                {
                    try
                    {
                        if (!CSVHelper.MachineEnabled)
                        {
                            MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch (Exception ee)
                    { }


                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'C';

                    //if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                    //CSVHelper.ST_timer.Stop(); CSVHelper.ST_timer.Reset();
                    
                    CSVHelper.IsLastEventCalibr = false;
                    for (int i = 0; i < DeviceCount; i++)
                    {
                        log.Log(LogLevel.Warn, "CompleteCalibrate Box:" + i.ToString() + " : Moving From:" + _TwinCathelper.PLCMotorsStatuses.mPostion[i] + " > Until_Stall  >> Direction: Down"
                               + "  >> RampDlay: " + _TwinCathelper.PLCMotors[i].mRampDelay
                               + "  >> mRPM: " + CSVHelper.RecommendedRPM
                               + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime
                               + "  >> mMode: CalibrationSeq01Request");

                        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request; }

                    CSVHelper.LastOffsetBeforeCalibration = new float[_TwinCathelper.PLCMotorsStatuses.mPostion.Length]; 
                    Array.Copy(_TwinCathelper.PLCMotorsStatuses.mPostion, CSVHelper.LastOffsetBeforeCalibration, _TwinCathelper.PLCMotorsStatuses.mPostion.Length);

                    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);

                    Thread.Sleep(200);
                    for (int i = 0; i < DeviceCount; i++)
                        _TwinCathelper.PLCMotors[i].mDisplacement = 0;
                    _TwinCathelper.WriteEthercatDeviceData();


                    log.Log(LogLevel.Warn, $"Sending CalibrationSeq01 Request, Please wait...");
                    TNF.ShowSuccess("Sending CalibrationSeq01 Request, Please wait...");

                    CSVHelper.UpdateHomeCSVStatus = true;

                    CSVHelper.ST_timer.Stop();
                    CSVHelper.ST_timer.Reset();
                }
                catch { }
            }
            catch (Exception ex)
            {
                CalibrateButtonEnable = true;
                log.Error(ex.Message);
                TNF.ShowError(ex.Message);
                return;
            }
            CSVHelper.IsLastEventCalibr = false;
        }


        public void Calibrate()
        {
            var log = LogManager.GetLogger("Calibration");
            //CalibrateButtonEnable = false;
            try
            {
                //log.Info("Please Select valid Calibration File ....");
                //using System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                //openFileDialog.InitialDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                //openFileDialog.Filter = "csv files (*.txt)|*.csv|All files (*.*)|*.*";
                //openFileDialog.RestoreDirectory = true;

                //CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\" + SelectedFile.Name + ".Resume", false, Encoding.UTF8)

                //if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + DeviceCount + ".calibrate"))
                //{
                //    //if (openFileDialog.FileName != _TwinCathelper.AppSettings.CalibrationFileFullPath())
                //    //    File.Copy(openFileDialog.FileName, _TwinCathelper.AppSettings.CalibrationFileFullPath(), true);
                //    //CalibrationData = CSVHelper.ReadCSV_DoubleData(new FileInfo(_TwinCathelper.AppSettings.CalibrationFileFullPath()));
                //    FileInfo finf = new FileInfo(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + DeviceCount + ".calibrate");
                //    CalibrationData = CSVHelper.ReadCalibrate_DoubleData(finf);
                //    if (ValidateCSVData())
                //    {
                try
                {
                    //CalibrateSeq02_HomeUp();
                    try
                    {
                        if (!CSVHelper.MachineEnabled)
                        {
                            MessageBox.Show("Please Power ON the machine before resuming!", "Machine state is off", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch (Exception ee)
                    { }


                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '1';
                    if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                    CSVHelper.ST_timer.Stop(); CSVHelper.ST_timer.Reset();

                    CSVHelper.IsLastEventCalibr = false;
                    for (int i = 0; i < DeviceCount; i++)
                        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request;
                    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);

                    Thread.Sleep(200);
                    for (int i = 0; i < DeviceCount; i++)
                        _TwinCathelper.PLCMotors[i].mDisplacement = 0;
                    _TwinCathelper.WriteEthercatDeviceData();


                    log.Log(LogLevel.Warn, $"Sending CalibrationSeq01 Request, Please wait...");
                    TNF.ShowSuccess("Sending CalibrationSeq01 Request, Please wait...");


                    //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
                    //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
                    //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

                    CSVHelper.UpdateHomeCSVStatus = true;
                }
                catch { }

                //    }
                //    else
                //    { CalibrateButtonEnable = true; CSVHelper.IsLastEventCalibr = false; }
                //}
                //else
                //{
                //    MessageBox.Show("Please copy "+DeviceCount+".calibrate to Appdata folder,","No Calibration file found!",MessageBoxButton.OK);
                //    CalibrateButtonEnable = true; 
                //}
            }
            catch (Exception ex)
            {
                CalibrateButtonEnable = true;
                log.Error(ex.Message);
                TNF.ShowError(ex.Message);
                return;
            }
            CSVHelper.IsLastEventCalibr = false;
        }


        private float[] _mPosition;
        public float[] mPosition
        {
            get { return _mPosition; }
            set
            {
                SetMotorPos(value);
                _mPosition = value;
            }
        }

        private uint[] _mDiag3;
        public uint[] mDiag3
        {
            get { return _mDiag3; }
            set
            {
                //SetMotorDiag3(value);
                _mDiag3 = value;
            }
        }


        private uint[] _DeviceStatus;
        public uint[] DeviceStatus
        {
            get { return _DeviceStatus; }
            set
            {
                SetDeviceStatus(value);
                _DeviceStatus = value;
            }
        }
        void DispDeviceStatus(uint[] _value, int i)
        {
            var swe = (EDeviceStatus)Enum.ToObject(typeof(EDeviceStatus), _value[i]);
            var log = LogManager.GetLogger("Calibration");
            switch (_value[i])
            {
                case (int)EDeviceStatus.None:
                    var _tmgs = SetUI(i, swe, Brushes.Transparent);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    CalibrateButtonEnable = true;

                    if (RampMotorsHlist.ContainsKey(i))
                    {
                        RampMotorsHlist.Remove(i);
                        if (RampMotorsHlist.Count < 1)
                        {
                            if (CSVHelper.ST_timer.IsRunning)
                            { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Stop(); CSVHelper.CompletingTime = 0; }

                            log.Log(LogLevel.Warn, "***************** Machine is Ready *****************");
                        }
                    }

                    break;
                case (int)EDeviceStatus.CalibrationSquence01Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    //CalibrateButtonEnable = false;                    
                    if (!CSVHelper.ST_timer.IsRunning)
                    { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Start(); CSVHelper.CompletingTime = int.MaxValue; }

                    if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'C') FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '1';

                    

                    if (!CalibrationCompletedList.ContainsKey(i))
                        CalibrationCompletedList.Add(i, i);
                    log.Log(LogLevel.Warn, "***************** CalibrationSeq01Started.....Motor:" + i);                    
                    //CSVHelper.CalibrationCompleted = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    if (CalibrationCompletedList.ContainsKey(i)) 
                        CalibrationCompletedList.Remove(i);
                    log.Log(LogLevel.Error, "***************** CalibrationSeq01Failed.....Motor: " + i);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    CalibrateButtonEnable = true;                    

                    if (i >= DeviceCount-1)
                    {
                        if(CalibrationCompletedList!=null && CalibrationCompletedList.Count<1)
                            CalibrateSeq02_HomeUp();
                        //CSVHelper.CalibrationCompleted = true;
                    }

                    break;
                case (int)EDeviceStatus.CalibrationSquence01Success:
                    _tmgs = SetUI(i, swe, Brushes.LightGreen);

                    if (CalibrationCompletedList.ContainsKey(i))
                    {
                        CalibrationCompletedList.Remove(i);
                        log.Log(LogLevel.Warn, "***************** CalibrationSquence01Success.....Motor:" + i);

                        if (CSVHelper.ST_timer.IsRunning)
                        { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Stop(); CSVHelper.CompletingTime = 0; }
                    }

                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    UpdateHomingProcess();
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;

                    //CalibrateButtonEnable = false;

                    try
                    {

                        if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled
                            && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'H')// H=Home R=Resume S=Stop W= WriteCSV O=Initial
                        {
                            FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                            FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '1';
                            log.Log(LogLevel.Error, "CalibrationSeq01Success Error occured while Homing...");
                        }

                        //if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'C')
                        //{
                        //    Thread.Sleep(1500);
                        //    CalibrateSeq02(); 
                        //}

                        // this condition will only occur when last motor has some update
                        if (CalibrationCompletedList.Count<1 && (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'C'))//CSVHelper.CalibrationCompleted if failed
                        {
                                log.Log(LogLevel.Warn, "Sending CalibrationSeq02 Request, Please wait.....Motor:" + i);
                                CalibrateSeq02_HomeUp();//EMode.Resume
                                Thread.Sleep(1500);
                                CalibrateSeq02();
                        }

                    }
                    catch { }

                    break;
                case (int)EDeviceStatus.CalibrationSquence02Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '2';
                    log.Log(LogLevel.Warn, "***************** CalibrationSeq02Started...... motor:" + i);
                    //CSVHelper.CalibrationCompleted = false;
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    //log.Log(LogLevel.Info, ">>>CalibrationSquence02Failed");
                    log.Log(LogLevel.Error, "EDeviceStatus.CalibrationSeq02Failed...... motor:" + i);
                    //CalibrateButtonEnable = false;
                    //if (i >= DeviceCount - 1)
                    //    CalibrateSeq02_HomeUp();

                    break;
                case (int)EDeviceStatus.CalibrationSquence02Success:
                    _tmgs = SetUI(i, swe, Brushes.LightGreen);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    log.Log(LogLevel.Warn, "***************** CalibrationSeq02Success...... motor:" + i);
                    //try { HardCalibrateProcess(); } catch { }
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = '2';
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;
                    UpdateHomingProcess();

                    //if (i >= DeviceCount - 1)
                    //    CalibrateSeq02_HomeUp();

                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.RampSequenceCompleted:
                    log.Log(LogLevel.Warn, "***************** RampSeqSuccess...... motor:" + i);
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;
                    HomeMotorStatus = true;
                    if(FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed=='H')
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'S';

                    if (RampMotorsHlist.ContainsKey(i))
                    {
                        RampMotorsHlist.Remove(i);
                        if (RampMotorsHlist.Count < 1)
                        {
                            if (CSVHelper.ST_timer.IsRunning)
                            { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Stop(); CSVHelper.CompletingTime = 0; }

                            log.Log(LogLevel.Warn, "***************** Machine is Ready *****************");
                        }
                    }


                    _tmgs = SetUI(i, swe, Brushes.Green);
                    TNF.ShowSuccess(_tmgs);
                    //log.Log(LogLevel.Info, _tmgs);
                    //CalibrateButtonEnable = false;                    
                    break;
                case (int)EDeviceStatus.RampSequenceRunning:// (int)EDeviceStatus.HomePosition:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowSuccess(_tmgs);
                    //log.Log(LogLevel.Warn, _tmgs);
                    //CalibrateButtonEnable = false;
                    if (!RampMotorsHlist.ContainsKey(i))
                        RampMotorsHlist.Add(i, i);
                    log.Log(LogLevel.Warn, "***************** RampSeqStarted...... motor:" + i);
                    

                    break;
                case (int)EDeviceStatus.RampSequenceFailed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    log.Log(LogLevel.Error, "***************** RampSeqFailed, Please check motor ...... :" + i);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);

                    if (RampMotorsHlist.ContainsKey(i))
                        RampMotorsHlist.Remove(i);
                    if (RampMotorsHlist.Count < 1)
                    {
                        if (CSVHelper.ST_timer.IsRunning)
                        { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Stop(); CSVHelper.CompletingTime = 0; }

                        log.Log(LogLevel.Warn, "***************** Machine is Ready *****************");
                    }

                    break;
                default:
                    _tmgs = SetUI(i, swe, Brushes.Blue);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, "! Undocumented Status ...... motor:" + i);
                    //CalibrateButtonEnable = false;
                    break;
            }
        }

        private void stoppingMotors() {

            var data = (int)EMode.Abort; String st = "";
            for (uint j = 0; j < DeviceCount; j++)
            { _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray()); st += j+":"+MotorStatus.mPosition[j] + ","; }
            log.Log(LogLevel.Warn, $"Stopping Motors > " + st);

            if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
            CSVHelper.ST_timer.Stop(); CSVHelper.ST_timer.Reset();

        }

        private string SetUI(int i, EmotorDiagnostic sw, SolidColorBrush br)
        {
            var ds = DeviceStatus == null ? "" : $"\n\t DeviceStatus : {(EDeviceStatus)DeviceStatus[i]}";
            string _tmgs = $"Motor[{i}]" +
                $"\n\t Motor Diagnostic : {sw}" +
                $"\n\t Motor Position : {MyMotors[i].MotorPosition:0.000}mm" + ds;
            MyMotors[i].FillColor = br;
            MyMotors[i].ToolTip = _tmgs;


            if (sw.ToString().ToLower().Contains("Temperature_Limit_exceed") || sw.ToString().ToLower().Contains("Stepper_Driver_Failed") || sw.ToString().ToLower().Contains("stall") || sw.ToString().ToLower().Contains("EC_LINK_STATE_NOT_PRESENT"))
            {
                CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
                if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != '1')
                {
                    //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();

                    stoppingMotors();
                    log.Log(LogLevel.Error, $"SetUI - Motor{i} --> Diagnostic : {_tmgs}");
                }
            }

            return $"Motor{i} --> Diagnostic : {sw}";
        }
        private string SetUI(int i, EDeviceStatus aa, SolidColorBrush br)
        {
            var md = MotorDiagnistics == null ? "" : $"\n\t Diagnostic : {(EmotorDiagnostic)MotorDiagnistics[i]}";
            var _tmgs = $"Motor[{i}]" + md +
                $"\n\t Position : {MyMotors[i].MotorPosition:0.000}mm" +
                $"\n\t DeviceStatus : {aa}";
            MyMotors[i].StrokeColor = br;
            MyMotors[i].ToolTip = _tmgs;
            return $"Motor{i} --> DeviceStatus : {aa}";
        }
        private void Position_SetUI(int i)
        {
            var ds = DeviceStatus == null ? "" : $"\n\t DeviceStatus : {(EDeviceStatus)DeviceStatus[i]}";
            var md = MotorDiagnistics == null ? "" : $"\n\t Diagnostic : {(EmotorDiagnostic)MotorDiagnistics[i]}";
            var _tmgs = $"Motor[{i}]" + md +
                $"\n\t Position : {MyMotors[i].MotorPosition:0.000}mm" + ds;
            MyMotors[i].ToolTip = _tmgs;
        }
        private void SetDeviceStatus(uint[] _value)
        {
            try
            {
                var v = GetMismatchedIndexes(DeviceStatus, _value);
                if (DeviceStatus != null)
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        DispDeviceStatus(_value, v[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        DispDeviceStatus(_value, i);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Device Status");
                TNF.ShowError(ex.Message);
                log.Log(LogLevel.Error, "! SetDevice Status Error =" + ex.Message);
            }
        }
        private void SetMotorPos(float[] _value)
        {
            try
            {
                var v = GetMismatchedIndexes(mPosition, _value);
                    
                if (mPosition != null)
                {
                    String st = "";
                    for (int i = 0; i < v.Length; i++)
                    {
                        MyMotors[v[i]].MotorPosition = _value[v[i]];
                        try { if(!hlist.ContainsKey(v[i])) hlist.Add(v[i], v[i]); } catch { }
                        Position_SetUI(v[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        MyMotors[i].MotorPosition = _value[i];
                        Position_SetUI(i);
                    }

                    log.Log(LogLevel.Warn, "********************** MotorStatusView>>GetMismatchedIndexes Machine Ready **********************");
                }

                CSVHelper.CompletingTime_count += 1;                

                if (CSVHelper.ST_timer != null && CSVHelper.ST_timer.IsRunning)
                {
                    if (CSVHelper.CompletingTime_count > 3 && ((CSVHelper.ST_timer.ElapsedMilliseconds / 1000) > CSVHelper.CompletingTime))
                    {
                        CSVHelper.CompletingTime_count = 0;
                        log.Log(LogLevel.Error, "*** Motors Malfunctioning, Please Turnoff Machine!");

                        //log.Log(LogLevel.Warn, $"Stopping Motors");
                        //var data = (int)EMode.Abort;
                        //for (uint j = 0; j < DeviceCount; j++)
                        //    _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray());
                        //log.Log(LogLevel.Warn, $"{EMode.Abort}");

                        CSVHelper.ST_timer.Stop();
                        CSVHelper.ST_timer.Reset();
                        //Application.Current.MainWindow.Hide();

                        if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'N')//&& FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'R'
                        {
                            stoppingMotors();
                            MessageBox.Show("*** Please calibrate machine first!", "Motors Malfunctioning Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            //Application.Current.MainWindow.Show();

                            if (hlist != null && hlist.Count > 0)
                            {
                                string ss = "";
                                foreach (int k in hlist.Keys)
                                    ss += k + ", ";

                                log.Log(LogLevel.Error, " Errors in PCB > " + ss);
                                MessageBox.Show("Motors still running after time elapsed Box(s) > " + ss, "Error in setup! ", MessageBoxButton.OK);

                            }
                        }
                    }
                    else {
                        if ((CSVHelper.CompletingTime - (CSVHelper.ST_timer.ElapsedMilliseconds / 1000)) < 1.00f) { log.Log(LogLevel.Warn, "*** Maximum motor time expired!"); }
                    }
                }

                if (hlist != null && hlist.Count > 0)
                    hlist.Clear();
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Device Status");
                TNF.ShowError(ex.Message);
                log.Log(LogLevel.Error, "! SetMotorPos Error =" + ex.Message);
            }
        }
        private uint[] _MotorDiagnistics;
        public UInt32[] MotorDiagnistics
        {
            get { return _MotorDiagnistics; }
            set
            {
                SetMotorDiagnisticStatus(value);
                _MotorDiagnistics = value;
            }
        }
        void DisplayMotorDiagStatus(uint[] _value, int i, Logger log)
        {
            MyMotors[i].MotorPosition = mPosition[i];
            var sw = (EmotorDiagnostic)Enum.ToObject(typeof(EmotorDiagnostic), _value[i]);
            switch (sw)
            {
                case EmotorDiagnostic.No_Error://EmotorDiagnostic.Healthy:
                    var _tmgs = SetUI(i, sw, Brushes.Green);

                    CSVHelper.MotorsStatusGlobal[i] = "Healthy";
                    HomeMotorStatus = true;
                    CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    break;
                case EmotorDiagnostic.Temperature_Limit_exceed://EmotorDiagnostic.Stepper_Driver_Failed:
                    CSVHelper.NoLinkHappened = true;
                    //if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == '1')
                    //{
                    log.Log(LogLevel.Info, ">>->>IsEnabled=" + FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled + " ->>LastEventPressed=" + FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed
                        + " ->>MotorCalled=" + i
                        );
                    log.Log(LogLevel.Error, $"Stepper_Driver_Failed > Stopping Motors");

                    _tmgs = SetUI(i, sw, Brushes.Red);
                    CSVHelper.MotorsStatusGlobal[i] = "Stepper_Driver_Failed";
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    CSVHelper.MotorsStatusGlobal[i] = sw.ToString();

                    if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                        return;

                    //log.Log(LogLevel.Info, $"Stopping Motors");
                    //var data = (int)EMode.Abort;
                    //for (uint j = 0; j < DeviceCount; j++)
                    //    _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data.CastToArray());
                    stoppingMotors();

                    //if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != '1' && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'N')
                    //{

                    //    //if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'W')//already called by twincatHelper class
                    //    //{
                    //    //    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'N';
                    //    //    //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start(); 
                    //    //    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();
                    //    //}
                    //    //else 
                    //    if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'R' || FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'S' || FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'H')
                    //    {
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'N';
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start(); //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'D';
                    //    }
                    //    else
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start(); //FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                    //}

                    break;
                case EmotorDiagnostic.Motor_stalled://EmotorDiagnostic.Stall:
                    CSVHelper.NoLinkHappened = true;
                    log.Log(LogLevel.Error, $"StallError > Stopping Motors:"+i);
                    //var data2 = (int)EMode.Abort;
                    //for (uint j = 0; j < DeviceCount; j++)
                    //    _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data2.CastToArray());
                    //log.Log(LogLevel.Info, $"{EMode.Abort}");
                    stoppingMotors();
                    //}
                    CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
                    if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled && (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'W' || FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'R'))
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();

                    _tmgs = SetUI(i, sw, Brushes.Orange);
                    CSVHelper.MotorsStatusGlobal[i] = "Stall";
                    TNF.ShowWarning(_tmgs);
                    log.Log(LogLevel.Warn, _tmgs);

                    //if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != '1' && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'N')
                    //{
                    //    if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'W')
                    //    {
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start(); FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'N';
                    //    }
                    //    else if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'R' || FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed != 'S')
                    //    {
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start(); FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'D';
                    //    }
                    //    else
                    //        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                    //}

                    //if (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == '1')
                    //{

                    if (CSVHelper.ST_timer.IsRunning)
                    { CSVHelper.ST_timer.Reset(); CSVHelper.ST_timer.Stop(); CSVHelper.CompletingTime = 0;
                        RampMotorsHlist.Clear();
                        CalibrationCompletedList.Clear();
                    }



                    break;
                case EmotorDiagnostic.Invalid_Ramp_Seq://newely added
                    CSVHelper.NoLinkHappened = true;
                    log.Log(LogLevel.Error, $"EmotorDiagnostic Invalid_Ramp_Seq > Stopping Motors");
                    //var data2 = (int)EMode.Abort;
                    //for (uint j = 0; j < DeviceCount; j++)
                    //    _TwinCathelper.WriteEthercatDeviceData(j, DeviceSymbol.mMode, data2.CastToArray());
                    //log.Log(LogLevel.Info, $"{EMode.Abort}");
                    stoppingMotors();
                    //}
                    //CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
                    //if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled && (FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'W' || FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'R'))
                    //    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();

                    _tmgs = SetUI(i, sw, Brushes.Pink);
                    CSVHelper.MotorsStatusGlobal[i] = "InvalidRmp";
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Warn, _tmgs);
                    break;

                default:
                    _tmgs = SetUI(i, sw, Brushes.Blue);
                    CSVHelper.MotorsStatusGlobal[i] = "Debug";
                    TNF.ShowError(_tmgs);
                    CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
                    log.Log(LogLevel.Error, "! Undocumented Status !" + _tmgs);
                    break;
            }
        }

        private void SetMotorDiagnisticStatus(uint[] _value)
        {
            try
            {
                var log = LogManager.GetLogger("Motor Diagnostics Status");
                var v = GetMismatchedIndexes(MotorDiagnistics, _value);
                if (MotorDiagnistics != null)
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        DisplayMotorDiagStatus(_value, v[i], log);
                    }
                    SetDeviceMode(_value, log);
                }
                else
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        DisplayMotorDiagStatus(_value, i, log);
                    }
                }
                if (v.Length != 0)
                    SetDeviceMode(_value, log);
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Device Status");
                TNF.ShowError(ex.Message);
                log.Log(LogLevel.Error, "! SetMotorDiagnisticStatus Error =" + ex.Message);
            }
        }
        private void SetDeviceMode(uint[] _value, Logger log)
        {
            if (_value.SequenceEqual(healthyMotorDiag))
            {
                _TwinCathelper.Motor_Diagnostic_Abort = false;
                _TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                _TwinCathelper.Motor_Diagnostic_Abort = true;
                _TwinCathelper.SetAbortMode(log);
            }
        }
        uint[] validSeq01Val = new uint[DeviceCount];
        uint[] validSeq02Val = new uint[DeviceCount];
        uint[] validRampSeqVal = new uint[DeviceCount];
        uint[] healthyMotorDiag = new uint[DeviceCount];
        uint[] RunningRampSeqVal = new uint[DeviceCount];
        uint[] FailedRampSeqVal = new uint[DeviceCount];
        uint[] validForHome = new uint[DeviceCount];
        uint[] TempSeq = new uint[DeviceCount];

        public IList<float> CalibrationData { get; set; }
        //private bool ValidateCSVData()
        //{
        //    var log = LogManager.GetLogger("CSV Validator");
        //    if (CalibrationData.Count != 0)
        //    {
        //        if (_TwinCathelper.PLCMotors.Length == CalibrationData.Count)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            log.Log(LogLevel.Error, $"Please select correct CSV");
        //            log.Log(LogLevel.Error, $"Data and Box count are not equal");
        //            log.Log(LogLevel.Error, $"Data Count : {CalibrationData.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
        //            TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{CalibrationData.Count}");
        //        }
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
        //    //if (CSVHelper.IsLastEventCalibr)
        //    //{
        //    var log = LogManager.GetLogger("Calibration");
        //    try
        //    {
        //        //log.Log(LogLevel.Info, $"WriteDistpacementCSVData before>>>>");
        //        float mtime = GetRecommendedRPM(data.Max());
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
        //            _TwinCathelper.PLCMotors[i].mTime = mtime;
        //        }
        //        _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);

        //        if (mtime < 1.0F)
        //            log.Log(LogLevel.Error, "MotorStaus-WriteDistpacementCSVData: Invalid MotorTime/too Small > " + mtime);


        //        log.Log(LogLevel.Info, $"{data.Count} csv value(s) for Calibration has been written sucessfully");
        //        log.Log(LogLevel.Info, $"{JsonSerializer.Serialize(data)}");
        //        //log.Log(LogLevel.Info, $"WriteDistpacementCSVData afer>>>>");
        //    }
        //    catch (Exception ep) { log.Log(LogLevel.Info, $"MotorStatusViewmodel>>>>WriteDistpacementCSVData >>>>" + ep.Message); }
        //    //}
        //}
        private void Motor_PositionChanged(object sender, EventArgs e)
        {
            var array = new float[DeviceCount];
            Array.Copy(_TwinCathelper.PLCMotorsStatuses.mPostion, array, array.Length);
            mPosition = array;
        }

        public void RecommendedMaxDisplacement(IList<float> negdis)
        {
            float chkBiggest = Math.Abs(negdis.Max()) > Math.Abs(negdis.Min()) ? Math.Abs(negdis.Max()) : Math.Abs(negdis.Min());
            CSVHelper.MaxDisplacement = chkBiggest;
        }

        public float GetRecommendedRPM(float CurrentMaxDisplacement)
        {

            //float MotorTime = (CurrentMaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);

            //Every motor must reach exactly same time ie some will move faster and some will slower --- Client requirement

            float MotorTime = (CurrentMaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);
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
                else  MotorTime = fltDelayval;
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


        private void Motor_DeviceStatusChanged(object sender, EventArgs e)
        {
            IsMotorsIdle = false;
            var log = LogManager.GetLogger("Device Status");
            //try { log.Log(LogLevel.Info, "current status1>>>" + String.Join(",", _TwinCathelper.PLCMotorsStatuses.DeviceStatus)); }catch{ }

            //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
            //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
            //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);


            //log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged >>> CSVHelper.IsLastEventCalibr");


            //if (CSVHelper.IsLastEventCalibr)
            //{
            //    log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged111");
            var array = new uint[DeviceCount];
            Array.Copy(_TwinCathelper.PLCMotorsStatuses.DeviceStatus, array, array.Length);
            DeviceStatus = array;
            //    log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged222");

            if (DeviceStatus.SequenceEqual(validSeq01Val))
            {
                //Thread.Sleep((int)_TwinCathelper.AppSettings.CalibrationSeq02Delay);
                //Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["CalibrationSeq02Delay"].ToString()));
                try
                {
                    //Thread.Sleep(CSVHelper.AdsStateTimeout);
                    ////Read the contents of the file into a stream

                    ////log.Log(LogLevel.Info, "Beofre sending Calibration Seq02 >>> Please wait...");
                    //float mtime = GetRecommendedRPM(CalibrationData.Max());
                    //WriteDistpacementCSVData(CalibrationData);
                    ////log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged333");
                    //for (int i = 0; i < DeviceCount; i++)
                    //{
                    //    _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
                    //    _TwinCathelper.PLCMotors[i].mTime = mtime;
                    //}
                    //_TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                    //log.Log(LogLevel.Warn, $"{EMode.CalibrationSeq02Request}. Please wait...");
                    //TNF.ShowSuccess("Calibration Seq02 Requested-Motorstatus");
                    //Thread.Sleep(200);
                    //for (int i = 0; i < DeviceCount; i++)
                    //    _TwinCathelper.PLCMotors[i].mDisplacement = 0;
                    //_TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
                    //log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

                    //CalibrateSeq02_HomeUp();
                }
                catch (Exception ee) { log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged>>>>" + ee.Message); }
            }

            if (DeviceStatus.SequenceEqual(validSeq02Val))
            {
                log.Log(LogLevel.Info, $"Calibration Sucessful");
                //float mtime = GetRecommendedRPM(CalibrationData.Max());
                //for (int i = 0; i < DeviceCount; i++)
                //{
                //    _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
                //    _TwinCathelper.PLCMotors[i].mTime = mtime;
                //}
                //_TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                //log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
                TNF.ShowSuccess("Calibration Sucessful");
                //CalibrateSeq02_HomeUp();
            }
            if (DeviceStatus.SequenceEqual(validForHome))// || DeviceStatus.SequenceEqual(uint.Parse("6")) || DeviceStatus.SequenceEqual(7))//RampSequenceCompleted=7  and CalibrationSquence02Success=6
            {
                log.Log(LogLevel.Info, $"All Motors Device Status : {EDeviceStatus.None}");
                IsMotorsIdle = true;
            }

            //}
            //CSVHelper.IsLastEventCalibr = false;
        }

        private void Motor_MotorDiagnosticChanged(object sender, EventArgs e)
        {
            var array = new uint[DeviceCount];
            Array.Copy(_TwinCathelper.PLCMotorsStatuses.MotorDiagnistic, array, array.Length);
            MotorDiagnistics = array;
        }
        private bool _CalibrateButton;

        public bool CalibrateButtonEnable
        {
            get { return _CalibrateButton; }
            set { _CalibrateButton = value; OnPropertyChanged(); }
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

        public MotorStatusViewModel(ITwinCathelper twinCathelper)
        {
            try
            {

                HomeMotorStatus = false;

                this._TwinCathelper = twinCathelper;
                if (_TwinCathelper.Client == null)
                {
                    return;
                }


                CalibrateButtonEnable = true;
                CalibrationData = new List<float>();
                //CalibrationData = CSVHelper.ReadCalibrate_DoubleData(new FileInfo(_TwinCathelper.AppSettings.CalibrationFileFullPath()));
                CalibrationData = CSVHelper.ReadCalibrate_DoubleData(new FileInfo(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + DeviceCount + ".calibrate"));
                MyMotors = new ObservableCollection<MotorStatusControl>() { };
                for (int j = 0; j < DeviceCount; j++)
                {
                    MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Green, ToolTip = "", StrokeColor = Brushes.White });
                }
                _TwinCathelper.TMotor_MotorDiagnosticChanged += Motor_MotorDiagnosticChanged;
                _TwinCathelper.TMotor_DeviceStatusChanged += Motor_DeviceStatusChanged;
                _TwinCathelper.TMotor_PositionChanged += Motor_PositionChanged;

                CalibrateCommand = new RelayCommand(async => { Calibrate(); });
                Calibrate02Command = new RelayCommand(async => {
                    CalibrateSeq02();

                    //while (CSVHelper.IsLastEventCalibr)
                    //    DoEvents();

                    //HardCalibrateProcess();
                });

                CompleteCalibrateCommand = new RelayCommand(async => { //CSVHelper.CalibrateCalledByEmergency = false; 
                    CompleteCalibrate(); });


                DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
                Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
                Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
                Array.Fill<uint>(validRampSeqVal, (int)EDeviceStatus.RampSequenceCompleted);
                //Array.Fill<uint>(validForHome, (int)EDeviceStatus.HomePosition);
                //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
                Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.No_Error);
                Array.Fill<uint>(RunningRampSeqVal, (int)EDeviceStatus.RampSequenceRunning);
                Array.Fill<uint>(FailedRampSeqVal, (int)EDeviceStatus.RampSequenceFailed);

                HomeVacuumStatus = false;
                HomeMachineStatus = true;
                HomeMotorStatus = false;
                HomeHeaterStatus = false;
                HomePowerStatus = true;
                HomeEmergencyStatus = false;
                HomeLaserStatus = false;

                CSVHelper._statusBaseViewModel = this;

                try { if (CSVHelper.MotorsStatusGlobal[0] == "Healthy") HomeMotorStatus = true; } catch { }

                checkFirmware();
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Device Status");
                TNF.ShowError(ex.Message);
                log.Log(LogLevel.Error, "! MotorStatusViewmodel Error =" + ex.Message);
            }

        }

        private void checkFirmware()
        {

            try
            {

                String FirmwareNotMatched = "";
                for (uint i = 0; i < DeviceCount; i++)
                {
                    //if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713969130 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1713968323 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)//0x662917EA (1713969130=Wed Apr 24 2024 14:32:10 GMT+0000) CSVHelper.MotorsStatusGlobal[0] == "Healthy" && //1713968323=Wed Apr 24 2024 14:18:43 GMT+0000
                    //if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1737720238 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)
                    if (_TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 1741256250 || _TwinCathelper.PLCMotorsStatuses.mDiag3[i] == 0)
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


    }
}

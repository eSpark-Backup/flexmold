using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
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
    public class MotorStatusViewmodel : StatusBaseViewModel, IMotorStatusViewmodel
    {
        private readonly ITwinCathelper _TwinCathelper;
        private Logger log = LogManager.GetLogger("MotorStatus");
        public IMotorStatusViewmodel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewmodel>(); } }
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand CalibrateCommand { get; set; }
        public RelayCommand Calibrate02Command { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }
        public RelayCommand OnLoadedCommand { get; set; }
        public RelayCommand OnUnLoadedCommand { get; set; }

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


                    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

                    log.Log(LogLevel.Info, "Successfully updated homing data!" + errmsg);

                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");

                }
            }
        }


        public void HardCalibrateProcess()
        {
            log.Log(LogLevel.Info, "HardCalibrateProcess Started successfully!");
            try
            {
                //if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                //{
                //    log.Log(LogLevel.Error, $"Machine status disconnected! ");
                //    MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

                //    if (dr == MessageBoxResult.Cancel)
                //    {
                //        DoMachineUserCanceled = true;
                //        return;
                //    }

                //    return;
                //}
                Thread.Sleep(400);
                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = false;
                CalibrateSeq02_HomeUp();


            //Line1:
            //    Thread.Sleep(1000);
            //    //if (_TwinCathelper.PLCMotors[0]<1.0F)
            //    if(MotorStatus.mPosition[0] < 1.0F)
            //        goto Line1;

            //    //while (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted)
            //    //    DoEvents();

            //    CalibrateSeq02_HomeDown();

            }
            catch (Exception ee) { log.Log(LogLevel.Warn, "CalibrateSeq02 Ended with error! " + ee.Message); }
            log.Log(LogLevel.Info, "HardCalibrateProcess Completed successfully!");
        }

        void CalibrateSeq02_HomeUp()
        {

            for (int i = 0; i < DeviceCount; i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = 0.200F;
                _TwinCathelper.PLCMotors[i].mDirection = false;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = 6.3334F;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                log.Log(LogLevel.Info, "CalibrateHard-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
                + "  >> mMode: " + EMode.Project.ToString());
            }

            _TwinCathelper.WriteEthercatDeviceData();
            log.Log(LogLevel.Info, $"{DeviceCount} CalibrateHard value(s) has been written sucessfully");

            Thread.Sleep(400);
            uint zero = 0;
            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

        }
        void CalibrateSeq02_HomeDown()
        {

            for (int i = 0; i < DeviceCount; i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = 0.200F;
                _TwinCathelper.PLCMotors[i].mDirection = true;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = 6.3334F;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                log.Log(LogLevel.Info, "CalibrateHard2-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                + "  >> MotorTime: " + CSVHelper.MotorTime.ToString()
                + "  >> mMode: " + EMode.Project.ToString());
            }

            _TwinCathelper.WriteEthercatDeviceData();
            log.Log(LogLevel.Info, $"{DeviceCount} CalibrateHard value(s) has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");

        }



        public void CalibrateSeq02()
        {
            //IsMotorsIdle = false;
            var log = LogManager.GetLogger("Device Status");
            log.Log(LogLevel.Info, $"Starting Calibration Seq02 process!");
            var array = new uint[DeviceCount];
            try
            {
                Array.Copy(_TwinCathelper.PLCMotorsStatuses.DeviceStatus, array, array.Length);
                DeviceStatus = array;

                if (CalibrationData == null || CalibrationData.Count < 1)
                {
                    FileInfo finf = new FileInfo(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + DeviceCount + ".calibrate");
                    CalibrationData = CSVHelper.ReadCalibrate_DoubleData(finf);
                }

            }
            catch (Exception ee) { log.Log(LogLevel.Info, "CalibrateSeq02>Stack>>>>" + ee.StackTrace); }

            CSVHelper.IsLastEventCalibr = true;
            //log.Log(LogLevel.Info, $"CalibrateSeq02 befpre");
            Motor_DeviceStatusChanged(this, null);
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
                    CSVHelper.IsLastEventCalibr = false;
                    for (int i = 0; i < DeviceCount; i++)
                        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request;
                    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                    log.Log(LogLevel.Warn, $"Calibration Seq01 Request Sent. Please wait...");
                    TNF.ShowSuccess("Calibration Request Sent. Please wait...");
                    

                    //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
                    //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
                    //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

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
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    CalibrateButtonEnable = true;
                    log.Log(LogLevel.Info, ">>>CalibrationSquence01Failed");
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Success:
                    _tmgs = SetUI(i, swe, Brushes.LightBlue);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    UpdateHomingProcess();
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;
                    //CalibrateButtonEnable = false;

                    try
                    {

                        if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled
                            && FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed == 'H')// H=Home R=Resume S=Stop W= WriteCSV O=Initial
                        { FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                            FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'V';
                            log.Log(LogLevel.Error, ">>>CalibrationSquence01 occured while Homing");
                        }
                    }
                    catch { }

                    break;
                case (int)EDeviceStatus.CalibrationSquence02Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    log.Log(LogLevel.Info, ">>>CalibrationSquence02Failed");
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Success:
                    _tmgs = SetUI(i, swe, Brushes.Blue);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    try { HardCalibrateProcess(); } catch { }
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.RampSequenceCompleted:
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = true;
                    HomeMotorStatus = true;

                    _tmgs = SetUI(i, swe, Brushes.Green);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    //CalibrateButtonEnable = false;
                    break;
                case (int)EDeviceStatus.HomePosition:
                    _tmgs = SetUI(i, swe, Brushes.Gray);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Warn, _tmgs);
                    //CalibrateButtonEnable = false;
                    break;
                default:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, "! Undocumented Status !" + _tmgs);
                    //CalibrateButtonEnable = false;
                    break;
            }
        }
        private string SetUI(int i, EmotorDiagnostic sw, SolidColorBrush br)
        {
            var ds = DeviceStatus == null ? "" : $"\n\t DeviceStatus : {(EDeviceStatus)DeviceStatus[i]}";
            string _tmgs = $"Motor[{i}]" +
                $"\n\t Motor Diagnostic : {sw}" +
                $"\n\t Motor Position : {MyMotors[i].MotorPosition:0.000}mm" + ds;
            MyMotors[i].FillColor = br;
            MyMotors[i].ToolTip = _tmgs;


            if (sw.ToString().ToLower().Contains("stepper_driver_failed") || sw.ToString().ToLower().Contains("stall") || sw.ToString().ToLower().Contains("ec_link_state_not_present"))
            {
                if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                    FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.Start();
                log.Log(LogLevel.Error, $"Motor{i} --> Diagnostic : {_tmgs}");
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
            catch(Exception ex)
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
                    for (int i = 0; i < v.Length; i++)
                    {
                        MyMotors[v[i]].MotorPosition = _value[v[i]];
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
                }
            }
            catch(Exception ex)
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
                case EmotorDiagnostic.Healthy:
                    var _tmgs = SetUI(i, sw, Brushes.Green);
                    
                    CSVHelper.MotorsStatusGlobal[i] = "Healthy";
                    HomeMotorStatus = true;

                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    break;
                case EmotorDiagnostic.Stepper_Driver_Failed:
                    if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                    {
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'V';
                    }
                    _tmgs = SetUI(i, sw, Brushes.Red);
                    CSVHelper.MotorsStatusGlobal[i] = "Stepper_Driver_Failed";
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    break;
                case EmotorDiagnostic.Stall:
                    if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.MotorsCheckTimer.IsEnabled)
                    {
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.StallErrMotorsCheckTimer.Start();
                        FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed = 'V';
                    }
                    _tmgs = SetUI(i, sw, Brushes.Orange);
                    CSVHelper.MotorsStatusGlobal[i] = "Stall";
                    TNF.ShowWarning(_tmgs);
                    log.Log(LogLevel.Warn, _tmgs);
                    break;
                default:
                    _tmgs = SetUI(i, sw, Brushes.Red);
                    CSVHelper.MotorsStatusGlobal[i] = "Debug";
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, "! Undocumented Status !" + _tmgs);
                    break;
            }
        }

        private void SetMotorDiagnisticStatus(uint[] _value)
        {
            try
            {
                var log = LogManager.GetLogger("Motor Diagnistic Status");
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
            }catch(Exception ex)
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
        uint[] healthyMotorDiag = new uint[DeviceCount];
        uint[] validForHome = new uint[DeviceCount];

        public IList<float> CalibrationData { get; set; }
        private bool ValidateCSVData()
        {
            var log = LogManager.GetLogger("CSV Validator");
            if (CalibrationData.Count != 0)
            {
                if (_TwinCathelper.PLCMotors.Length == CalibrationData.Count)
                {
                    return true;
                }
                else
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV");
                    log.Log(LogLevel.Error, $"Data and Box count are not equal");
                    log.Log(LogLevel.Error, $"Data Count : {CalibrationData.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
                    TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{CalibrationData.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                TNF.ShowError($"CSV Failed");
            }
            return false;
        }
        private void WriteDistpacementCSVData(IList<float> data)
        {
            if (CSVHelper.IsLastEventCalibr)
            {
                var log = LogManager.GetLogger("Calibration");
                try
                {
                    //log.Log(LogLevel.Info, $"WriteDistpacementCSVData before>>>>");
                    float mtime = GetRecommendedRPM(data.Max());
                    for (int i = 0; i < data.Count; i++)
                    {
                        _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
                        _TwinCathelper.PLCMotors[i].mTime = mtime;
                    }
                    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
                    log.Log(LogLevel.Info, $"{data.Count} csv value(s) for Calibration has been written sucessfully");
                    log.Log(LogLevel.Info, $"{JsonSerializer.Serialize(data)}");
                    //log.Log(LogLevel.Info, $"WriteDistpacementCSVData afer>>>>");
                }
                catch (Exception ep) { log.Log(LogLevel.Info, $"MotorStatusViewmodel>>>>WriteDistpacementCSVData >>>>"+ep.Message); }
            }
        }
        private void Motor_PositionChanged(object sender, EventArgs e)
        {
            var array = new float[DeviceCount];
            Array.Copy(_TwinCathelper.PLCMotorsStatuses.mPostion, array, array.Length);
            mPosition = array;
        }

        public float GetRecommendedRPM(float CurrentMaxDisplacement)
        {

            //float MotorTime = (CurrentMaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);

            //Every motor must reach exactly same time ie some will move faster and some will slower --- Client requirement

            float MotorTime = 0.00F;
            try
            {
                MotorTime = (CurrentMaxDisplacement * 60) / (CSVHelper.RecommendedRPM * 2);

                if (MotorTime > 900.0F)//Time cant be set more than 15 min.
                    MessageBox.Show("Time is out of bound! Max is 15Mins. But counted is " + (MotorTime / 60) + " Mins", "Warning! Time is out of bound!", MessageBoxButton.OK);
            }
            catch(Exception ex) { log.Log(LogLevel.Info, "GetRecommendedRPM>>>>" + ex.Message); }
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


            if (CSVHelper.IsLastEventCalibr)
            {
                log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged111");
                var array = new uint[DeviceCount];
                Array.Copy(_TwinCathelper.PLCMotorsStatuses.DeviceStatus, array, array.Length);
                DeviceStatus = array;
                log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged222");

                if (DeviceStatus.SequenceEqual(validSeq01Val))
                {
                    //Thread.Sleep((int)_TwinCathelper.AppSettings.CalibrationSeq02Delay);
                    //Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["CalibrationSeq02Delay"].ToString()));
                    try
                    {
                        Thread.Sleep(CSVHelper.AdsStateTimeout);
                        //Read the contents of the file into a stream

                        //log.Log(LogLevel.Info, "Beofre sending Calibration Seq02 >>> Please wait...");
                        float mtime = GetRecommendedRPM(CalibrationData.Max());
                        WriteDistpacementCSVData(CalibrationData);
                        //log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged333");
                        for (int i = 0; i < DeviceCount; i++)
                        {
                            _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
                            _TwinCathelper.PLCMotors[i].mTime = mtime;
                        }
                        _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                        log.Log(LogLevel.Warn, $"{EMode.CalibrationSeq02Request}. Please wait...");
                        TNF.ShowSuccess("Calibration Seq02 Requested-Motorstatus");
                        Thread.Sleep(200);
                        for (int i = 0; i < DeviceCount; i++)
                            _TwinCathelper.PLCMotors[i].mDisplacement = 0;
                        _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
                        log.Log(LogLevel.Info, $"{DeviceCount} zero value(s) has been written sucessfully");
                    }
                    catch (Exception ee) { log.Log(LogLevel.Info, $"Motor_DeviceStatusChanged>>>>"+ee.Message);}
                }
                if (DeviceStatus.SequenceEqual(validSeq02Val))
                {
                    log.Log(LogLevel.Info, $"Calibration Sucessful");
                    float mtime = GetRecommendedRPM(CalibrationData.Max());
                    for (int i = 0; i < DeviceCount; i++)
                    {
                        _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
                        _TwinCathelper.PLCMotors[i].mTime = mtime;
                    }
                    _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                    log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
                    TNF.ShowSuccess("Calibration Sucessful");
                }
                if (DeviceStatus.SequenceEqual(validForHome))
                {
                    log.Log(LogLevel.Info, $"All Motors Device Status : {EDeviceStatus.None}");
                    IsMotorsIdle = true;
                }

            }
            CSVHelper.IsLastEventCalibr = false;
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

        public MotorStatusViewmodel(ITwinCathelper twinCathelper)
        {
            try {

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
                MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Red, ToolTip = "", StrokeColor = Brushes.White });
            }
            _TwinCathelper.TMotor_MotorDiagnosticChanged += Motor_MotorDiagnosticChanged;
            _TwinCathelper.TMotor_DeviceStatusChanged += Motor_DeviceStatusChanged;
            _TwinCathelper.TMotor_PositionChanged += Motor_PositionChanged;

            CalibrateCommand = new RelayCommand(async => { Calibrate(); });
            Calibrate02Command = new RelayCommand(async => { CalibrateSeq02();

                //while (CSVHelper.IsLastEventCalibr)
                //    DoEvents();

                //HardCalibrateProcess();
            });


            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
            Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
            Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
            Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

                HomeVacuumStatus = false;
                HomeMachineStatus = true;
                HomeMotorStatus = false;
                HomeHeaterStatus = false;
                HomePowerStatus = true;
                HomeEmergencyStatus = false;
                HomeLaserStatus = false;

                CSVHelper._statusBaseViewModel = this;

                try{ if (CSVHelper.MotorsStatusGlobal[0] == "Healthy") HomeMotorStatus = true;} catch { }
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Device Status");
                TNF.ShowError(ex.Message);
                log.Log(LogLevel.Error, "! MotorStatusViewmodel Error =" + ex.Message);
            }

        }
    }
}

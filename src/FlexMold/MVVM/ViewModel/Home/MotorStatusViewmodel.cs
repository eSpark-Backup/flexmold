using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MotorStatusViewmodel : StatusBaseViewModel
    {
        private readonly IMachineDetailViewModel _IMachineDetailViewModel;
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand CalibrateCommand { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }
        public RelayCommand OnLoadedCommand { get; set; }
        public RelayCommand OnUnLoadedCommand { get; set; }

        private void DisplayError(MotorStatusControl o)
        {
            MessageBox.Show(o.ToolTip);
            Console.WriteLine();
        }

        private void Calibrate()
        {
            var log = LogManager.GetLogger("Calibration");
            CalibrateButton = false;

            try
            {
                log.Info("Please Select valid Calibration File ....");
                using System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.InitialDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                openFileDialog.Filter = "csv files (*.txt)|*.csv|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog.FileName != TwinCathelper.AppSettings.CalibrationFileFullPath())
                        File.Copy(openFileDialog.FileName, TwinCathelper.AppSettings.CalibrationFileFullPath(), true);
                }
                else
                {
                    CalibrateButton = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                CalibrateButton = true;
                log.Error(ex.Message);
                TNF.ShowError(ex.Message);
                return;
            }
            CalibrationData = CSVHelper.ReadCSV_DoubleData(new FileInfo(TwinCathelper.AppSettings.CalibrationFileFullPath()));
            if (CalibrationData.Count == 0)
            {
                log.Log(LogLevel.Error, $"No data found in Calibration.csv");
                TNF.ShowError("Calibration.csv");
                return;
            }
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request;
            TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
            log.Log(LogLevel.Warn, $"Calibration Seq01 Request Sent. Please wait...");
            TNF.ShowSuccess("Calibration Request Sent. Please wait...");
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
                    CalibrateButton = true;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    CalibrateButton = true;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Success:
                    _tmgs = SetUI(i, swe, Brushes.LightBlue);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Started:
                    _tmgs = SetUI(i, swe, Brushes.Yellow);
                    TNF.ShowInformation(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Failed:
                    _tmgs = SetUI(i, swe, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Success:
                    _tmgs = SetUI(i, swe, Brushes.Blue);
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    CalibrateButton = false;
                    break;
                default:
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
                    DispDeviceStatus(_value, v[i]);
                }
            }
        }
        private void SetMotorPos(float[] _value)
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
                    TNF.ShowSuccess(_tmgs);
                    log.Log(LogLevel.Info, _tmgs);
                    break;
                case EmotorDiagnostic.Stepper_Driver_Failed:
                    _tmgs = SetUI(i, sw, Brushes.Red);
                    TNF.ShowError(_tmgs);
                    log.Log(LogLevel.Error, _tmgs);
                    break;
                case EmotorDiagnostic.Stall:
                    _tmgs = SetUI(i, sw, Brushes.Orange);
                    TNF.ShowWarning(_tmgs);
                    log.Log(LogLevel.Warn, _tmgs);
                    break;
                default:
                    break;
            }
        }

        private void SetMotorDiagnisticStatus(uint[] _value)
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
        }
        private void SetDeviceMode(uint[] _value, Logger log)
        {
            if (_value.SequenceEqual(healthyMotorDiag))
            {
                TwinCathelper.Motor_Diagnostic_Abort = false;
                TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                TwinCathelper.Motor_Diagnostic_Abort = true;
                TwinCathelper.SetAbortMode(log);
            }
        }
        uint[] validSeq01Val = new uint[TwinCathelper.DeviceCount];
        uint[] validSeq02Val = new uint[TwinCathelper.DeviceCount];
        uint[] healthyMotorDiag = new uint[TwinCathelper.DeviceCount];

        public IList<double> CalibrationData { get; set; }

        private void WriteDistpacementCSVData(IList<double> data)
        {
            var log = LogManager.GetLogger("Calibration");
            if (data.Count != 0)
            {
                if (TwinCathelper.PLCMotors.Length == data.Count)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        TwinCathelper.PLCMotors[i].mDisplacement = (uint)data[i] * 1600;
                    }
                    TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mDisplacement);
                    log.Log(LogLevel.Info, $"{data.Count} csv value/s has been written sucessfully");
                    log.Log(LogLevel.Info, $"{JsonSerializer.Serialize(data)}");
                }
                else
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV");
                    log.Log(LogLevel.Error, $"Data and Box count are not equal");
                    log.Log(LogLevel.Error, $"Data Count : {data.Count} | Device Count : {TwinCathelper.PLCMotors.Length}");
                    TNF.ShowError($"CSV Failed\nDevice Count :{TwinCathelper.PLCMotors.Length} != Data Count :{data.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                TNF.ShowError($"CSV Failed");
            }
        }
        private void Motor_PositionChanged(object sender, EventArgs e)
        {
            var array = new float[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.mPostion, array, array.Length);
            mPosition = array;
        }
        private void Motor_DeviceStatusChanged(object sender, EventArgs e)
        {
            var log = LogManager.GetLogger("Device Status");
            var array = new uint[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.DeviceStatus, array, array.Length);
            DeviceStatus = array;
            if (DeviceStatus.SequenceEqual(validSeq01Val))
            {
                Thread.Sleep((int)TwinCathelper.AppSettings.CalibrationSeq02Delay);
                //Read the contents of the file into a stream
                WriteDistpacementCSVData(CalibrationData);
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
                }
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
                log.Log(LogLevel.Warn, $"{EMode.CalibrationSeq02Request}. Please wait...");
                TNF.ShowSuccess("Calibration Seq02 Requested");
                Thread.Sleep(200);
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                    TwinCathelper.PLCMotors[i].mDisplacement = 0;
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mDisplacement);
                log.Log(LogLevel.Info, $"{TwinCathelper.DeviceCount} zero value/s has been written sucessfully");
            }
            if (DeviceStatus.SequenceEqual(validSeq02Val))
            {
                log.Log(LogLevel.Info, $"Calibration Sucessful");
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
                }
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
                log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
                TNF.ShowSuccess("Calibration Sucessful");
            }
        }
        private void Motor_MotorDiagnosticChanged(object sender, EventArgs e)
        {
            var array = new uint[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.MotorDiagnistic, array, array.Length);
            MotorDiagnistics = array;
        }
        private bool _CalibrateButton;

        public bool CalibrateButton
        {
            get { return _CalibrateButton; }
            set { _CalibrateButton = value; OnPropertyChanged(); }
        }

        public MotorStatusViewmodel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            CalibrateButton = true;
            CalibrationData = new List<double>();
            CalibrationData = CSVHelper.ReadCSV_DoubleData(new FileInfo(TwinCathelper.AppSettings.CalibrationFileFullPath()));
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Red, ToolTip = "", StrokeColor = Brushes.White });
            }
            TwinCathelper.TMotor_MotorDiagnosticChanged += Motor_MotorDiagnosticChanged;
            TwinCathelper.TMotor_DeviceStatusChanged += Motor_DeviceStatusChanged;
            TwinCathelper.TMotor_PositionChanged += Motor_PositionChanged;
            CalibrateCommand = new RelayCommand(async => { Calibrate(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
            Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
        }
    }
}
using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MotorStatusViewmodel : ObservableObject
    {
        private ToastNotificationUtil _TstN;
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
                using System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.InitialDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                openFileDialog.Filter = "csv files (*.txt)|*.csv|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Copy to File Appsetting's root
                    File.Copy(openFileDialog.FileName, TwinCathelper.AppSettings.CalibrationFileFullPath(), true);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            CalibrationData = CSVHelper.ReadCSV_DoubleData(new FileInfo(TwinCathelper.AppSettings.CalibrationFileFullPath()));
            if (CalibrationData.Count == 0)
                return;
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request;
            }
            TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
            log.Log(LogLevel.Warn, $"Calibration Seq01 Request Sent. Please wait...");
            _TstN.ShowSuccess("Calibration Request Sent. Please wait...");
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

            switch (_value[i])
            {
                case (int)EDeviceStatus.None:
                    DeviceStatus_SetUI(i, swe, Brushes.Transparent);
                    CalibrateButton = true;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Started:
                    DeviceStatus_SetUI(i, swe, Brushes.Yellow);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Failed:
                    DeviceStatus_SetUI(i, swe, Brushes.Orange);
                    CalibrateButton = true;
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Success:
                    DeviceStatus_SetUI(i, swe, Brushes.LightBlue);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Started:
                    DeviceStatus_SetUI(i, swe, Brushes.Yellow);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Failed:
                    DeviceStatus_SetUI(i, swe, Brushes.Pink);
                    CalibrateButton = false;
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Success:
                    DeviceStatus_SetUI(i, swe, Brushes.Blue);
                    CalibrateButton = false;
                    break;
                default:
                    break;
            }
        }
        private void MotorStatus_SetUI(int i, EmDiag1 sw, SolidColorBrush br)
        {
            var log = LogManager.GetLogger("Motor Status");
            var ds = DeviceStatus == null ? "" : $"\n\t Motor DeviceStatus : {(EDeviceStatus)DeviceStatus[i]}";
            string _tmgs = $"Motor[{i}]" +
                $"\n\t Motor Diagnostic : {sw}" +
                $"\n\t Motor Position : {MyMotors[i].MotorPosition:0.000}mm" + ds;
            MyMotors[i].FillColor = br;
            MyMotors[i].ToolTip = _tmgs;
            log.Log(LogLevel.Info, _tmgs);
            _TstN.ShowSuccess(_tmgs);
        }
        private void DeviceStatus_SetUI(int i, EDeviceStatus aa, SolidColorBrush br)
        {
            var log = LogManager.GetLogger("Calibration");
            var md = MotorDiagnisticx == null ? "" : $"\n\t Motor Diagnostic : {(EmDiag1)MotorDiagnisticx[i]}";
            var _tmgs = $"Motor[{i}]" + md +
                $"\n\t Motor Position : {MyMotors[i].MotorPosition:0.000}mm" +
                $"\n\t Motor DeviceStatus : {aa}";
            MyMotors[i].StrokeColor = br;
            MyMotors[i].ToolTip = _tmgs;
            log.Log(LogLevel.Info, _tmgs);
            //_TstN.ShowSuccess(_tmgs);
        }
        private void Position_SetUI(int i)
        {
            var ds = DeviceStatus == null ? "" : $"\n\t Motor DeviceStatus : {(EDeviceStatus)DeviceStatus[i]}";
            var md = MotorDiagnisticx == null ? "" : $"\n\t Motor Diagnostic : {(EmDiag1)MotorDiagnisticx[i]}";
            var _tmgs = $"Motor[{i}]" + md +
                $"\n\t Motor Position : {MyMotors[i].MotorPosition:0.000}mm" + ds;
            MyMotors[i].ToolTip = _tmgs;
        }
        private void SetDeviceStatus(uint[] _value)
        {
            int[] v;
            if (DeviceStatus != null)
            {
                v = _value
                    .Select((b, i) => b != 10 ? i : -1)
                    .Where(i => i != -1)
                    .ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    if (DeviceStatus[v[i]] != _value[v[i]])
                    {
                        DispDeviceStatus(_value, v[i]);
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    DispDeviceStatus(_value, v[i]);
                }
            }
        }
        private void SetMotorPos(float[] _value)
        {
            int[] v;
            if (mPosition != null)
            {
                v = _value
                    .Select((b, i) => b != -1 ? i : -1)
                    .Where(i => i != -1)
                    .ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    if (mPosition[v[i]] != _value[v[i]])
                    {
                        MyMotors[v[i]].MotorPosition = _value[v[i]];
                        Position_SetUI(v[i]);
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    MyMotors[v[i]].MotorPosition = _value[v[i]];
                    Position_SetUI(v[i]);
                }
            }
        }
        private uint[] _MotorDiagnisticx;
        public UInt32[] MotorDiagnisticx
        {
            get { return _MotorDiagnisticx; }
            set
            {
                SetMotorStatus(value);
                _MotorDiagnisticx = value;
            }
        }
        void DisplayMotorStatus(uint[] _value, int[] v, int i)
        {
            MyMotors[v[i]].MotorPosition = mPosition[v[i]];
            var sw = (EmDiag1)Enum.ToObject(typeof(EmDiag1), _value[v[i]]);
            switch (sw)
            {
                case EmDiag1.Healthy:
                    MotorStatus_SetUI(v[i], sw, Brushes.Green);
                    break;
                case EmDiag1.Stepper_Driver_Failed:
                    MotorStatus_SetUI(v[i], sw, Brushes.Red);
                    break;
                default:
                    break;
            }
        }

        private void SetMotorStatus(uint[] _value)
        {
            int[] v;
            if (MotorDiagnisticx != null)
            {
                v = _value
                    .Select((b, i) => b != 5 ? i : -1)
                    .Where(i => i != -1)
                    .ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    if (MotorDiagnisticx[v[i]] != _value[v[i]])
                    {
                        DisplayMotorStatus(_value, v, i);
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    DisplayMotorStatus(_value, v, i);
                }
            }
        }
        uint[] validSeq01Val = new uint[TwinCathelper.DeviceCount];
        uint[] validSeq02Val = new uint[TwinCathelper.DeviceCount];

        public IList<double> CalibrationData { get; set; }

        private void WriteCSVData(IList<double> data)
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
                }
                else
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV");
                    log.Log(LogLevel.Error, $"Data and Box count are not equal");
                    log.Log(LogLevel.Error, $"Data Count : {data.Count} | Device Count : {TwinCathelper.PLCMotors.Length}");
                    _TstN.ShowError($"CSV Failed\nDevice Count :{TwinCathelper.PLCMotors.Length} != Data Count :{data.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                _TstN.ShowError($"CSV Failed");
            }
        }
        private void Client_AdsNotificationPS(object sender, EventArgs e)
        {
            var array = new float[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.mPostion, array, array.Length);
            mPosition = array;
        }
        private void Client_AdsNotificationDS(object sender, EventArgs e)
        {
            var log = LogManager.GetLogger("Calibration");
            var array = new uint[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.DeviceStatus, array, array.Length);
            DeviceStatus = array;
            if (validSeq01Val.SequenceEqual(DeviceStatus))
            {
                Thread.Sleep((int)TwinCathelper.AppSettings.CalibrationSeq02Delay);
                //Read the contents of the file into a stream
                if (CalibrationData.Count == 0)
                    CalibrationData = CSVHelper.ReadCSV_DoubleData(new FileInfo(TwinCathelper.AppSettings.CalibrationFileFullPath()));
                WriteCSVData(CalibrationData);
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq02Request;
                }
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
                log.Log(LogLevel.Warn, $"{EMode.CalibrationSeq02Request}. Please wait...");
                _TstN.ShowSuccess("Calibration Seq02 Requested");
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                    TwinCathelper.PLCMotors[i].mDisplacement = 0;
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mDisplacement);
                log.Log(LogLevel.Info, $"{TwinCathelper.DeviceCount} zero value/s has been written sucessfully");
            }
            if (validSeq02Val.SequenceEqual(DeviceStatus))
            {
                log.Log(LogLevel.Info, $"Calibration Sucessful");
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
                }
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
                log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
                _TstN.ShowSuccess("Calibration Sucessful");
            }
        }
        private void Client_AdsNotificationMS(object sender, EventArgs e)
        {
            var array = new uint[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.MotorDiagnistic, array, array.Length);
            MotorDiagnisticx = array;
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
            _TstN = new ToastNotificationUtil();
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Red, ToolTip = "", StrokeColor = Brushes.White });
            }
            TwinCathelper.TMotor_MotorDiagnosticChanged += Client_AdsNotificationMS;
            TwinCathelper.TMotor_DeviceStatusChanged += Client_AdsNotificationDS;
            TwinCathelper.TMotor_PositionChanged += Client_AdsNotificationPS;
            CalibrateCommand = new RelayCommand(async => { Calibrate(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
            Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
        }
    }
}
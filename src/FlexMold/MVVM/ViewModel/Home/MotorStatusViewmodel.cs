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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MotorStatusViewmodel : ObservableObject
    {
        private Logger log;
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
            CalibrateButton = false;
            try
            {
                using System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.InitialDirectory = Assembly.GetExecutingAssembly().Location;
                openFileDialog.Filter = "csv files (*.csv)";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    CSVCalibration = new FileInfo(openFileDialog.FileName);

                    //Read the contents of the file into a stream
                    CalibrationData = CSVHelper.ReadCSV_DoubleData(CSVCalibration);
                }
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationRequest;
                }
                TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
                log.Log(LogLevel.Warn, $"Callibration Request Sent. Please wait...");
                _TstN.ShowSuccess("Callibration Request Sent. Please wait...");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            
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
        void DispDeviceStatus(uint[] _value, int[] v, int i)
        {
            var swe = (EDeviceStatus)Enum.ToObject(typeof(EDeviceStatus), _value[v[i]]);

            switch (_value[v[i]])
            {
                case (int)EDeviceStatus.None:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Transparent);
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Started:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Yellow);
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Failed:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Orange);
                    break;
                case (int)EDeviceStatus.CalibrationSquence01Success:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.LightBlue);
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Started:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Yellow);
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Failed:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Pink);
                    break;
                case (int)EDeviceStatus.CalibrationSquence02Success:
                    DeviceStatus_SetPrintValues(v, i, swe, Brushes.Blue);
                    break;
                default:
                    break;
            }
        }

        private void DeviceStatus_SetPrintValues(int[] v, int i, EDeviceStatus aa, SolidColorBrush br)
        {
            var md = MotorDiagnisticx == null ? "" : $"\n\t Motor Diagnostic : {(EmDiag1)MotorDiagnisticx[v[i]]}";
            var _tmgs = $"Motor[{v[i]}]" + md +
                $"\n\t Motor Position : {MyMotors[v[i]].MotorPosition:0.000}mm" +
                $"\n\t Motor DeviceStatus : {aa}";
            MyMotors[v[i]].StrokeColor = br;
            MyMotors[v[i]].ToolTip = _tmgs;
            log.Log(LogLevel.Info, _tmgs);
            _TstN.ShowSuccess(_tmgs);
        }

        private void SetDeviceStatus(UInt32[] _value)
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
                        DispDeviceStatus(_value, v, i);
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    DispDeviceStatus(_value, v, i);
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
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    MyMotors[v[i]].MotorPosition = _value[v[i]];
                }
            }
        }
        private UInt32[] _MotorDiagnisticx;
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
                    MotorStatus_SetPrintValues(v, i, sw, Brushes.Green);
                    break;
                case EmDiag1.Stepper_Driver_Failed:
                    MotorStatus_SetPrintValues(v, i, sw, Brushes.Red);
                    break;
                default:
                    break;
            }
        }

        private void MotorStatus_SetPrintValues(int[] v, int i, EmDiag1 sw, SolidColorBrush br)
        {
            var ds = DeviceStatus == null ? "" : $"\n\t Motor DeviceStatus : {(EDeviceStatus)DeviceStatus[v[i]]}";
            string _tmgs = $"Motor[{v[i]}]" +
                $"\n\t Motor Diagnostic : {sw}" +
                $"\n\t Motor Position : {MyMotors[v[i]].MotorPosition:0.000}mm";
            MyMotors[v[i]].FillColor = br;
            MyMotors[v[i]].ToolTip = _tmgs;
            log.Log(LogLevel.Info, _tmgs);
            _TstN.ShowSuccess(_tmgs);
        }

        private void SetMotorStatus(UInt32[] _value)
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
        public FileInfo CSVCalibration { get; set; }
        public IEnumerable<double> CalibrationData { get; set; }
        private void Client_AdsNotificationDS(object sender, EventArgs e)
        {
            var array = new uint[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.DeviceStatus, array, array.Length);
            DeviceStatus = array;
            uint[] validVal = new uint[DeviceStatus.Length];
            Array.Fill<uint>(validVal, (int)EDeviceStatus.CalibrationSquence01Success);
            if (validVal == DeviceStatus)
            {
                TwinCathelper.WriteEthercatDeviceData
            }
        }
        private void Client_AdsNotificationPS(object sender, EventArgs e)
        {
            var array = new float[TwinCathelper.DeviceCount];
            Array.Copy(TwinCathelper.PLCMotorsStates.mPostion, array, array.Length);
            mPosition = array;
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
            log = LogManager.GetLogger("Motor Status");
            _TstN = new ToastNotificationUtil();
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Red, ToolTip = "", StrokeColor = Brushes.White });
            }
            TwinCathelper.TMotor_PositionChanged += Client_AdsNotificationPS;
            //MotorDiagnisticx = new uint[TwinCathelper.DeviceCount];
            TwinCathelper.TMotor_MotorDiagnosticChanged += Client_AdsNotificationMS;
            TwinCathelper.TMotor_DeviceStatusChanged += Client_AdsNotificationDS;
            CalibrateCommand = new RelayCommand(async => { Calibrate(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
        }
    }
}
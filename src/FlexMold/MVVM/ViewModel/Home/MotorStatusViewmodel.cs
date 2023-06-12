using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

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
            MessageBox.Show(o.ErrorDeatil);
            Console.WriteLine();
        }

        private void Calibrate()
        {
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationRequest;
            }
            TwinCathelper.WriteEthercatDeviceData();
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
                        MyMotors[v[i]].MotorPosition = mPosition[v[i]];
                        var _tmgs = "";
                        switch (_value[v[i]])
                        {
                            case (int)EmDiag1.Healthy:
                                _tmgs = $"Motor #{v[i]} : {EmDiag1.Healthy}";
                                MyMotors[v[i]].FillColor = Brushes.Green;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Info, _tmgs);
                                _TstN.ShowSuccess(_tmgs);
                                break;

                            case (int)EmDiag1.Stepper_Driver_Failed:
                                _tmgs = $"Motor #{v[i]} : {EmDiag1.Stepper_Driver_Failed}";
                                MyMotors[v[i]].FillColor = Brushes.Red;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Error, _tmgs);
                                _TstN.ShowError(_tmgs);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                v = Enumerable.Range(0, TwinCathelper.DeviceCount).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    MyMotors[v[i]].MotorPosition = mPosition[v[i]];
                    var _tmgs = "";
                    switch (_value[v[i]])
                    {
                        case (int)EmDiag1.Healthy:
                            _tmgs = $"Motor #{v[i]} : {EmDiag1.Healthy}";
                            MyMotors[v[i]].FillColor = Brushes.Green;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Info, _tmgs);
                            _TstN.ShowSuccess(_tmgs);
                            break;

                        case (int)EmDiag1.Stepper_Driver_Failed:
                            _tmgs = $"Motor #{v[i]} : {EmDiag1.Stepper_Driver_Failed}";
                            MyMotors[v[i]].FillColor = Brushes.Red;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Error, _tmgs);
                            _TstN.ShowError(_tmgs);
                            break;
                        default:
                            break;
                    }
                }
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
        public MotorStatusViewmodel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Status");
            _TstN = new ToastNotificationUtil();
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorID = j, FillColor = Brushes.Red, ErrorDeatil = "" });
            }
            TwinCathelper.TMotor_PositionChanged += Client_AdsNotificationPS;
            //MotorDiagnisticx = new uint[TwinCathelper.DeviceCount];
            TwinCathelper.TMotor_MotorDiagnosticChanged += Client_AdsNotificationMS;
            CalibrateCommand = new RelayCommand(async => { Calibrate(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
        }
    }
}
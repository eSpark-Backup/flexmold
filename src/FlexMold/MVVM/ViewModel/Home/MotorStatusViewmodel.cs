using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads;
using NLog;
using System.Reflection.Metadata.Ecma335;
using TwinCAT.TypeSystem;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MotorStatusViewmodel : ObservableObject
    {
        Logger log;
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand GenerateErrorD1Command { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }

      
        void DisplayError(MotorStatusControl o)
        {
            MessageBox.Show(o.ErrorDeatil);
            Console.WriteLine();
        }
        void GenerateD1Error()
        {
            var SymbolDiag1Variable = TwinCathelper.MySymbols.Where(a => a.InstancePath == TCVarName.mDiag1).FirstOrDefault();
            var tt = new UInt32[TwinCathelper.DeviceCount];
            Random rnd = new Random();
            for (int i = 0; i < tt.Length; i++)
            {
                tt[i] = (uint)rnd.Next(0, 3);
            }
            SymbolDiag1Variable.WriteValue(tt);
        }
        private async Task RegisterNotification(string _deviceStatusVariableName)
        {
            var deviceStatusSymbol = TwinCathelper.MySymbols.Where(a => a.InstancePath == _deviceStatusVariableName).FirstOrDefault();
            await TwinCathelper.Client.AddDeviceNotificationAsync(deviceStatusSymbol.IndexGroup,
             deviceStatusSymbol.IndexOffset, (int)(deviceStatusSymbol.Size), NotificationSettings.Default, deviceStatusSymbol, CancellationToken.None);
            //log.Log(LogLevel.Info, $"{_deviceStatusVariableName} Notification Registered");
        }
        public UInt32[] OldValue { get; set; }

        private UInt32[] _Diag1Value;
        public UInt32[] Diag1Value
        {
            get { return _Diag1Value; }
            set
            {
                _Diag1Value = value;
                SetMotorStatus(value);
            }
        }
        void SetMotorStatus(UInt32[] _value) 
        {
            int[] v;
            if (Diag1Value != null)
            {
                v = _value.Select((b, i) => b != 5 ? i : -1).Where(i => i != -1).ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    if (Diag1Value[v[i]] != _value[v[i]])
                    {
                        var _tmgs = "";
                        switch (_value[v[i]])
                        {
                            case (int)EmDiag1.Healthy:
                                _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Healthy}";
                                MyMotors[v[i]].FillColor = Brushes.Transparent;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Info, _tmgs);
                                break;
                            case (int)EmDiag1.Recovered:
                                _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Recovered}";
                                MyMotors[v[i]].FillColor = Brushes.Gray;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Info, _tmgs);
                                //ToastNotificationUtil.ShowInformation(_tmgs);
                                break;
                            case (int)EmDiag1.Stepper_Driver_Failed:
                                _tmgs = $"[Err{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
                                MyMotors[v[i]].FillColor = Brushes.Red;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Warn, _tmgs);
                                //ToastNotificationUtil.ShowError(_tmgs);
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
                    var _tmgs = "";
                    switch (_value[v[i]])
                    {
                        case (int)EmDiag1.Healthy:
                            _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Healthy}";
                            MyMotors[v[i]].FillColor = Brushes.Transparent;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Info, _tmgs);
                            break;
                        case (int)EmDiag1.Recovered:
                            _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Recovered}";
                            MyMotors[v[i]].FillColor = Brushes.Gray;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Info, _tmgs);
                            //ToastNotificationUtil.ShowInformation(_tmgs);
                            break;
                        case (int)EmDiag1.Stepper_Driver_Failed:
                            _tmgs = $"[Err{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
                            MyMotors[v[i]].FillColor = Brushes.Red;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Warn, _tmgs);
                            //ToastNotificationUtil.ShowError(_tmgs);
                            break;
                        default:
                            break;
                    }
                }
            }

        }
        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
        {
            try
            {


                // Or here we know about UDINT type --> can be marshalled as UINT32
                var _mysymbol = e.UserData as Symbol;
                switch (_mysymbol.InstancePath)
                {
                    case TCVarName.mPosition:
                        log.Log(LogLevel.Warn, $"Position Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                        break;
                    case TCVarName.mTemperature:
                        log.Log(LogLevel.Warn, $"Temperature Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                        break;
                    case TCVarName.mDiag1:
                        Diag1Value = (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[]));
                        break;
                    case TCVarName.mDiag2:
                        log.Log(LogLevel.Warn, $"Diag2 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                        break;
                    case TCVarName.mDiag3:
                        log.Log(LogLevel.Warn, $"Diag3 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                        break;
                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }
        public MotorStatusViewmodel()
        {
            log = LogManager.GetLogger("Motor Status");
            TwinCathelper.Client.AdsNotification += Client_AdsNotification2;
            log.Log(LogLevel.Info, $"Registring Device Status Notification...");
            //_ = RegisterNotification(TwinCATVariableIfo.mPositionVariableName);
            //_ = RegisterNotification(TwinCATVariableIfo.mTemperatureVariableName);
            _ = RegisterNotification(TCVarName.mDiag1);
            //_ = RegisterNotification(TwinCATVariableIfo.mDiag2VariableName);
            //_ = RegisterNotification(TwinCATVariableIfo.mDiag3VariableName);

            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            int mcount = 0;
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorId = mcount, FillColor = Brushes.Transparent, ErrorDeatil = $"Motor #{mcount} \n No anomaly found" });
                mcount++;
            }
            GenerateErrorD1Command = new RelayCommand(o => { GenerateD1Error(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
        }
    }

}
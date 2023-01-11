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

namespace FlexMold.MVVM.ViewModel.Home
{
    class MotorStatusViewmodel : ObservableObject
    {
        public Logger log;
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand GenerateErrorD1Command { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }

        public MotorStatusViewmodel()
        {
            log = LogManager.GetLogger("Motor Status");
            TwinCathelper._connection.AdsNotification += Client_AdsNotification2;
            log.Log(LogLevel.Info, $"Registring Device Status Notification...");
            //_ = RegisterNotification(TwinCATVariableIfo.mPositionVariableName);
            //_ = RegisterNotification(TwinCATVariableIfo.mTemperatureVariableName);
            _ = RegisterNotification(TwinCATVariableIfo.mDiag1VariableName);
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
        void DisplayError(MotorStatusControl o)
        {
            MessageBox.Show(o.ErrorDeatil);
            Console.WriteLine();
        }
        void GenerateD1Error()
        {
            var SymbolDiag1Variable = TwinCathelper.MySymbols.Where(a => a.InstancePath == TwinCATVariableIfo.mDiag1VariableName).FirstOrDefault();
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
            await TwinCathelper._connection.AddDeviceNotificationAsync(deviceStatusSymbol.IndexGroup,
             deviceStatusSymbol.IndexOffset, (int)(deviceStatusSymbol.Size), NotificationSettings.Default, deviceStatusSymbol, CancellationToken.None);
            //log.Log(LogLevel.Info, $"{_deviceStatusVariableName} Notification Registered");
        }
        private UInt32[] _Diag1Value;
        public UInt32[] Diag1Value
        {
            get { return _Diag1Value; }
            set
            {
                int[] v;
                if (Diag1Value != null)
                {
                    v = value.Select((b, i) => b != 0 ? i : -1).Where(i => i != -1).ToArray();
                    for (int i = 0; i < v.Length; i++)
                    {
                        if (Diag1Value[v[i]] != value[v[i]])
                        {
                            var _tmgs = "";
                            switch (value[v[i]])
                            {
                                case (int)EmDiag1.Recovered:
                                    _tmgs = $"[Info{value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Recovered}";
                                    MyMotors[v[i]].FillColor = Brushes.Gray;
                                    MyMotors[v[i]].ErrorDeatil = _tmgs;
                                    log.Log(LogLevel.Info, _tmgs);
                                    //ToastNotificationUtil.ShowInformation(_tmgs);
                                    break;
                                case (int)EmDiag1.Stepper_Driver_Failed:
                                    _tmgs = $"[Err{value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
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
                        switch (value[v[i]])
                        {
                            case (int)EmDiag1.Recovered:
                                _tmgs = $"[Info{value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Recovered}";
                                MyMotors[v[i]].FillColor = Brushes.Gray;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Info, _tmgs);
                                //ToastNotificationUtil.ShowInformation(_tmgs);
                                break;
                            case (int)EmDiag1.Stepper_Driver_Failed:
                                _tmgs = $"[Err{value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
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

                _Diag1Value = value;
            }
        }
        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
        {
            // Or here we know about UDINT type --> can be marshalled as UINT32
            var _mysymbol = e.UserData as Symbol;
            switch (_mysymbol.InstancePath)
            {
                case TwinCATVariableIfo.mPositionVariableName:
                    log.Log(LogLevel.Warn, $"Position Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    break;
                case TwinCATVariableIfo.mTemperatureVariableName:
                    log.Log(LogLevel.Warn, $"Temperature Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    break;
                case TwinCATVariableIfo.mDiag1VariableName:
                    Diag1Value = (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[]));
                    break;
                case TwinCATVariableIfo.mDiag2VariableName:
                    log.Log(LogLevel.Warn, $"Diag2 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    break;
                case TwinCATVariableIfo.mDiag3VariableName:
                    log.Log(LogLevel.Warn, $"Diag3 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    break;
                default:
                    break;

            }
        }
    }

}
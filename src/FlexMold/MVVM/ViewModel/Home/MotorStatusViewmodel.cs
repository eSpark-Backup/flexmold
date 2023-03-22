using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MotorStatusViewmodel : ObservableObject
    {
        Logger log;
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        public RelayCommand GenerateErrorD1Command { get; set; }
        public RelayCommand DebugDetailClickCommand { get; set; }
        public RelayCommand OnLoadedCommand { get; set; }
        public RelayCommand OnUnLoadedCommand { get; set; }

        void DisplayError(MotorStatusControl o)
        {
            MessageBox.Show(o.ErrorDeatil);
            Console.WriteLine();
        }
        void GenerateD1Error()
        {
            var SymbolDiag1Variable = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.mDiag1}").FirstOrDefault();
            var tt = new UInt32[TwinCathelper.DeviceCount];
            Random rnd = new Random();
            for (int i = 0; i < tt.Length; i++)
            {
                tt[i] = (uint)rnd.Next(0, 3);
            }
            SymbolDiag1Variable.WriteValue(tt);
        }

        public UInt32[] OldValue { get; set; }

        private UInt32[] _Diag1Value;
        public UInt32[] Diag1Value
        {
            get { return _Diag1Value; }
            set
            {
                SetMotorStatus(value);
                _Diag1Value = value;
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
                                _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} {EmDiag1.Healthy}";
                                MyMotors[v[i]].FillColor = Brushes.Green;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Info, _tmgs);
                                ToastNotificationUtil.ShowSuccess(_tmgs);
                                break;
                            //case (int)EmDiag1.Recovered:
                            //    _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} {EmDiag1.Recovered}";
                            //    MyMotors[v[i]].FillColor = Brushes.Gray;
                            //    MyMotors[v[i]].ErrorDeatil = _tmgs;
                            //    log.Log(LogLevel.Info, _tmgs);
                            //    ToastNotificationUtil.ShowInformation(_tmgs);
                            //    break;
                            case (int)EmDiag1.Stepper_Driver_Failed:
                                _tmgs = $"[Err{_value[v[i]]}] Motor #{v[i]} {EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
                                MyMotors[v[i]].FillColor = Brushes.Red;
                                MyMotors[v[i]].ErrorDeatil = _tmgs;
                                log.Log(LogLevel.Error, _tmgs);
                                ToastNotificationUtil.ShowError(_tmgs);
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
                            MyMotors[v[i]].FillColor = Brushes.Green;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Info, _tmgs);
                            ToastNotificationUtil.ShowSuccess(_tmgs);
                            break;
                        //case (int)EmDiag1.Recovered:
                        //    _tmgs = $"[Info{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Recovered}";
                        //    MyMotors[v[i]].FillColor = Brushes.Gray;
                        //    MyMotors[v[i]].ErrorDeatil = _tmgs;
                        //    log.Log(LogLevel.Info, _tmgs);
                        //    ToastNotificationUtil.ShowInformation(_tmgs);
                        //    break;
                        case (int)EmDiag1.Stepper_Driver_Failed:
                            _tmgs = $"[Err{_value[v[i]]}] Motor #{v[i]} \n{EmDiag1.Stepper_Driver_Failed.ToString().Replace("_", " ")}";
                            MyMotors[v[i]].FillColor = Brushes.Red;
                            MyMotors[v[i]].ErrorDeatil = _tmgs;
                            log.Log(LogLevel.Error, _tmgs);
                            ToastNotificationUtil.ShowError(_tmgs);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public UInt32[] MPostion = new UInt32[TwinCathelper.DeviceCount];
        public UInt32[] MTemperature = new UInt32[TwinCathelper.DeviceCount];
        UInt32[] MDiag1;
        public UInt32[] MDiag2 = new UInt32[TwinCathelper.DeviceCount];
        public UInt32[] MDiag3 = new UInt32[TwinCathelper.DeviceCount];
        List<int> NotificatioHandles;
        private void Client_AdsNotificationMS(object sender, AdsNotificationEventArgs e)
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
                        //case TCVarName.mPosition:
                        //    Buffer.BlockCopy(e.Data.ToArray(), 0, MPostion, 0, e.Data.Length);
                        //    log.Log(LogLevel.Warn, $"Position Updated : {String.Join(",", MPostion)}");
                        //    break;
                        //case TCVarName.mTemperature:
                        //    Buffer.BlockCopy(e.Data.ToArray(), 0, MTemperature, 0, e.Data.Length);
                        //    log.Log(LogLevel.Warn, $"Temperature Updated : {String.Join(",", MTemperature)}");
                        //    break;
                        case TCSymbols.mDiag1:
                            MDiag1 = new UInt32[TwinCathelper.DeviceCount];
                            Buffer.BlockCopy(e.Data.ToArray(), 0, MDiag1, 0, e.Data.Length);
                            log.Log(LogLevel.Warn, $"Diag1 Updated : {String.Join(",", MDiag1)}");
                            Diag1Value = MDiag1;
                            break;
                            //case TCVarName.mDiag2:
                            //    Buffer.BlockCopy(e.Data.ToArray(), 0, MDiag2, 0, e.Data.Length);
                            //    log.Log(LogLevel.Warn, $"Diag2 Updated : {String.Join(",", MDiag2)}");
                            //    break;
                            //case TCVarName.mDiag3:
                            //    Buffer.BlockCopy(e.Data.ToArray(), 0, MDiag3, 0, e.Data.Length);$"MAIN.{TCSymbols.ECAbError}"
                            //    log.Log(LogLevel.Warn, $"Diag3 Updated : {String.Join(",", MDiag3)}");
                            //    break;
                    }
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
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            int mcount = 0;
            RegisterNotification();
            for (int j = 0; j < TwinCathelper.DeviceCount; j++)
            {
                MyMotors.Add(new MotorStatusControl() { MotorID = j+1, FillColor = Brushes.Transparent, ErrorDeatil = $"Motor #{mcount} \n No anomaly found" });
                mcount++;
            }
            GenerateErrorD1Command = new RelayCommand(o => { GenerateD1Error(); });
            DebugDetailClickCommand = new RelayCommand(o => { DisplayError(o as MotorStatusControl); });
        }
        void RegisterNotification()
        {
            log.Log(LogLevel.Info, $"Registring Device Status Notification...");
            NotificatioHandles = new List<int>();
            TwinCathelper.Client.AdsNotification += Client_AdsNotificationMS;
            //_ = RegisterNotification(TwinCATVariableIfo.mPositionVariableName);
            //_ = RegisterNotification(TwinCATVariableIfo.mTemperatureVariableName);
            NotificatioHandles.Add(TwinCathelper.RegisterNotification($"MAIN.{TCSymbols.mDiag1}"));
            //_ = RegisterNotification(TwinCATVariableIfo.mDiag2VariableName);
            //_ = RegisterNotification(TwinCATVariableIfo.mDiag3VariableName);
        }
    }

}
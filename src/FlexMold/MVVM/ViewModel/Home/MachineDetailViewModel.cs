using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads;
using NLog;
using System.Timers;
using FlexMold.Utility;
using System.Collections;
using System.Reactive.Linq;
using FlexMold.Core;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using TwinCAT.TypeSystem;
using System.Windows.Media;
using System.Windows.Forms;
using System.Reflection;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MachineDetailViewModel : ObservableObject
    {
        Logger log;
        public UInt16 ECAnSlaves { get; set; }
        public bool ECAbError { get; set; }
        public UInt32 ECAnErrId { get; set; }
        public ST_EcSlaveState[] MySlaves { get => _MySlaves; set { _MySlaves = value; OnPropertyChanged(); } }

        private ST_EcSlaveState[] _MySlaves;
        byte[] ECAdevStatesB;

        public MachineDetailViewModel()
        {
            log = LogManager.GetLogger("Motor Status");
            MySlaves = new ST_EcSlaveState[TwinCathelper.DeviceCount];
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                MySlaves[i] = new ST_EcSlaveState()
                {
                    SlaveID = i+1,
                    SlaveName = $"Box_{i + 1}",
                    deviceState = ESlaveStateMachine.EC_DEVICE_STATE_OP,
                    linkState = ElinkState.EC_LINK_STATE_OK,
                    FillColor = Brushes.Green,
                    ErrorDeatil = $"Slave : {$"Box_{i + 1}"} \n Machine State : {ESlaveStateMachine.EC_DEVICE_STATE_OP}\n Link State : {ElinkState.EC_LINK_STATE_OK}"
                };
            }
            ECAdevStatesB = new byte[TwinCathelper.DeviceCount * sizeof(ElinkState) * 2];
            GetSlaveMachineState();
            var timer = new System.Timers.Timer(3000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetSlaveMachineState();
        }
        void GetSlaveMachineState()
        {
            try
            {
                var pp = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().TryWriteValue(1, 200);
                ECAbError = (bool)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbError}").FirstOrDefault().ReadAnyValue(typeof(bool));
                if (!ECAbError)
                {
                    ushort index = 0;
                    ECAdevStatesB = (byte[])TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAdevStates}").FirstOrDefault().ReadValue();
                    for (int i = 0; i < ECAdevStatesB.Length;)
                    {
                        if (MySlaves[index].deviceState != (ESlaveStateMachine)ECAdevStatesB[i])
                        {
                            MySlaves[index].deviceState = (ESlaveStateMachine)ECAdevStatesB[i];
                            switch (MySlaves[index].deviceState)
                            {
                                case ESlaveStateMachine.EC_DEVICE_STATE_INIT:
                                    MySlaves[index].FillColor = Brushes.White;
                                    var _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    ToastNotificationUtil.ShowWarning(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    ToastNotificationUtil.ShowWarning(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    ToastNotificationUtil.ShowWarning(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    ToastNotificationUtil.ShowWarning(_tmgs); break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                                    MySlaves[index].FillColor = Brushes.Green;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    ToastNotificationUtil.ShowSuccess(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (MySlaves[index].linkState != (ElinkState)ECAdevStatesB[i + 1])
                        {
                            MySlaves[index].linkState = (ElinkState)ECAdevStatesB[i + 1];
                            switch (MySlaves[index].linkState)
                            {
                                case ElinkState.EC_LINK_STATE_OK:
                                    MySlaves[index].FillColor = Brushes.OrangeRed;
                                    var _tmgs = $"Motor {MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    ToastNotificationUtil.ShowSuccess(_tmgs);
                                    break;
                                case ElinkState.EC_LINK_STATE_NOT_PRESENT:
                                    MySlaves[index].FillColor = Brushes.OrangeRed;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                case ElinkState.EC_LINK_STATE_LINK_WITHOUT_COMM:
                                    break;
                                case ElinkState.EC_LINK_STATE_MISSING_LINK:
                                    MySlaves[index].FillColor = Brushes.OrangeRed;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    ToastNotificationUtil.ShowError(_tmgs);
                                    break;
                                case ElinkState.EC_LINK_STATE_ADDITIONAL_LINK:
                                    break;
                                case ElinkState.EC_LINK_STATE_PORT_A:
                                    break;
                                case ElinkState.EC_LINK_STATE_PORT_B:
                                    break;
                                case ElinkState.EC_LINK_STATE_PORT_C:
                                    break;
                                case ElinkState.EC_LINK_STATE_PORT_D:
                                    break;
                                default:
                                    break;
                            }
                        }
                        MySlaves[index].ErrorDeatil = $"Slave : {MySlaves[index].SlaveName} \n Machine State : {MySlaves[index].deviceState}\n Link State : {MySlaves[index].linkState}";
                        i = i + 2;
                        index++;
                    }
                }
                TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().WriteValue(0);
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }

        }


    }
}

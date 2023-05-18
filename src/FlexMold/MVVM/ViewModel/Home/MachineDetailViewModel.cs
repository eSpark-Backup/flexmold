using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Timers;
using System.Windows.Media;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MachineDetailViewModel : ObservableObject
    {
        private Logger log;
        public UInt16 ECAnSlaves { get; set; }
        public bool ECAbError { get; set; }
        public UInt32 ECAnErrId { get; set; }

        public ST_EcSlaveState[] MySlaves
        { get => _MySlaves; set { _MySlaves = value; OnPropertyChanged(); } }

        private ST_EcSlaveState[] _MySlaves;
        private byte[] ECAdevStatesB;
        private ToastNotificationUtil _TstN;

        public MachineDetailViewModel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Status");
            _TstN = new ToastNotificationUtil();
            MySlaves = new ST_EcSlaveState[TwinCathelper.DeviceCount];
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                MySlaves[i] = new ST_EcSlaveState()
                {
                    SlaveID = i + 1,
                    SlaveName = $"Box_{i + 1}",
                    deviceState = ESlaveStateMachine.EC_DEVICE_STATE_OP,
                    linkState = ElinkState.EC_LINK_STATE_OK,
                    FillColor = Brushes.Green,
                    ErrorDeatil = $"Slave : {$"Box_{i + 1}"} \n Machine State : {ESlaveStateMachine.EC_DEVICE_STATE_OP}\n Link State : {ElinkState.EC_LINK_STATE_OK}"
                };
            }
            ECAdevStatesB = new byte[TwinCathelper.DeviceCount * sizeof(ElinkState) * 2];
            var timer = new Timer(3000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetSlaveMachineState();
        }

        private void GetSlaveMachineState()
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
                                    _TstN.ShowWarning(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    _TstN.ShowWarning(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    _TstN.ShowWarning(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                                    MySlaves[index].FillColor = Brushes.Yellow;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Warn, _tmgs);
                                    _TstN.ShowWarning(_tmgs); break;
                                case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                                    MySlaves[index].FillColor = Brushes.Green;
                                    _tmgs = $"Slave {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    _TstN.ShowSuccess(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
                                    break;

                                case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
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
                                    _TstN.ShowSuccess(_tmgs);
                                    break;

                                case ElinkState.EC_LINK_STATE_NOT_PRESENT:
                                    MySlaves[index].FillColor = Brushes.OrangeRed;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
                                    break;

                                case ElinkState.EC_LINK_STATE_LINK_WITHOUT_COMM:
                                    break;

                                case ElinkState.EC_LINK_STATE_MISSING_LINK:
                                    MySlaves[index].FillColor = Brushes.OrangeRed;
                                    _tmgs = $"Motor {MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    _TstN.ShowError(_tmgs);
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
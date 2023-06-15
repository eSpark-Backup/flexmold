using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Humanizer;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MachineDetailViewModel : ObservableObject, IMachineDetailViewModel
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
            log = LogManager.GetLogger("Machine Status");
            _TstN = new ToastNotificationUtil();
            MySlaves = new ST_EcSlaveState[TwinCathelper.DeviceCount];
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                MySlaves[i] = new ST_EcSlaveState()
                {
                    SlaveID = i,
                    SlaveName = $"Box_{i}",
                    deviceState = ESlaveStateMachine.EC_DEVICE_STATE_OP,
                    linkState = ElinkState.EC_LINK_STATE_OK,
                    FillColor = Brushes.Transparent,
                    ErrorDeatil =
                    $"Box_{i} \n" +
                    $" Link State : {ElinkState.EC_LINK_STATE_OK.Humanize()}\n" +
                    $" Machine State : {ESlaveStateMachine.EC_DEVICE_STATE_OP.Humanize()}\n"
                };
            }
            ECAdevStatesB = new byte[TwinCathelper.DeviceCount * sizeof(ElinkState) * 2];
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Timer_Elapsed);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }
        void SetDefaultMode()
        {
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;

            TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
            log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Displacement}");
            _TstN.ShowSuccess("Calibration Sucessful");
        }
        void SetAbortMode()
        {
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.Abort;

            TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mMode);
            log.Log(LogLevel.Info, $"Device Mode Changed to : {EMode.Abort}");
            _TstN.ShowSuccess("Calibration Sucessful");
        }
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            GetSlaveMachineState();
        }

        private void GetSlaveMachineState()
        {
            try
            {
                var pp = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().TryWriteValue(1, 200);

                ECAbError = (bool)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbError}").FirstOrDefault().ReadAnyValue(typeof(bool));

                // only odd indexes
                ESlaveStateMachine[] enumArray = ECAdevStatesB
                    .Where((value, index) => index % 2 != 0)
                    .Select(b => (ESlaveStateMachine)Enum.ToObject(typeof(ESlaveStateMachine), b)).ToArray();
                if (!ECAbError)
                {
                    ushort index = 0;

                    ECAdevStatesB = (byte[])TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAdevStates}").FirstOrDefault().ReadValue();

                    for (int i = 0; i < ECAdevStatesB.Length;)
                    {
                        if (MySlaves[index].linkState != (ElinkState)ECAdevStatesB[i])
                        {
                            MySlaves[index].linkState = (ElinkState)ECAdevStatesB[i];
                            switch (MySlaves[index].linkState)
                            {
                                case ElinkState.EC_LINK_STATE_OK:
                                case ElinkState.EC_LINK_STATE_ADDITIONAL_LINK:
                                    MySlaves[index].FillColor = Brushes.Green;
                                    var _tmgs = $"{MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Info, _tmgs);
                                    _TstN.ShowSuccess(_tmgs);
                                    break;

                                case ElinkState.EC_LINK_STATE_NOT_PRESENT:
                                case ElinkState.EC_LINK_STATE_LINK_WITHOUT_COMM:
                                case ElinkState.EC_LINK_STATE_MISSING_LINK:
                                    MySlaves[index].FillColor = Brushes.Red;
                                    _tmgs = $"{MySlaves[index].SlaveName} : Link State {MySlaves[index].linkState}";
                                    log.Log(LogLevel.Error, _tmgs);
                                    _TstN.ShowError(_tmgs);
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (MySlaves[index].linkState == ElinkState.EC_LINK_STATE_OK ||
                            MySlaves[index].linkState == ElinkState.EC_LINK_STATE_ADDITIONAL_LINK)
                        {
                            if (MySlaves[index].deviceState != (ESlaveStateMachine)ECAdevStatesB[i])
                            {
                                MySlaves[index].deviceState = (ESlaveStateMachine)ECAdevStatesB[i];
                                switch (MySlaves[index].deviceState)
                                {
                                    case ESlaveStateMachine.EC_DEVICE_STATE_INIT:
                                        MySlaves[index].FillColor = Brushes.White;
                                        var _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Warn, _tmgs);
                                        _TstN.ShowWarning(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                                        MySlaves[index].FillColor = Brushes.Green;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Info, _tmgs);
                                        _TstN.ShowSuccess(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                                        MySlaves[index].FillColor = Brushes.Yellow;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Warn, _tmgs);
                                        _TstN.ShowWarning(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                                        MySlaves[index].FillColor = Brushes.Yellow;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Warn, _tmgs);
                                        _TstN.ShowWarning(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                                        MySlaves[index].FillColor = Brushes.Yellow;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Warn, _tmgs);
                                        _TstN.ShowWarning(_tmgs); break;
 

                                    case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                                        MySlaves[index].FillColor = Brushes.Red;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Error, _tmgs);
                                        _TstN.ShowError(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                                        MySlaves[index].FillColor = Brushes.Red;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Error, _tmgs);
                                        _TstN.ShowError(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                                        MySlaves[index].FillColor = Brushes.Red;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Error, _tmgs);
                                        _TstN.ShowError(_tmgs);
                                        break;

                                    case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                                        MySlaves[index].FillColor = Brushes.Red;
                                        _tmgs = $"{MySlaves[index].SlaveName} : State {MySlaves[index].deviceState}";
                                        log.Log(LogLevel.Error, _tmgs);
                                        _TstN.ShowError(_tmgs);
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        MySlaves[index].ErrorDeatil =
                            $"{MySlaves[index].SlaveName} \n" +
                             $" Link State : {MySlaves[index].linkState}\n" +
                            $" Machine State : {MySlaves[index].deviceState}\n";
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
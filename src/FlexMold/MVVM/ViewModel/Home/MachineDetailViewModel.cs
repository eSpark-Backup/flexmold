using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal class MachineDetailViewModel : StatusBaseViewModel, IMachineDetailViewModel
    {
        public ST_EcSlaveState[] MySlaves
        { get => _MySlaves; set { _MySlaves = value; OnPropertyChanged(); } }

        private ST_EcSlaveState[] _MySlaves;
        private ESlaveLink[] _Slave_Link_Status;
        public ESlaveLink[] Slave_Link_Status
        {
            get => _Slave_Link_Status;
            set
            {
                SetLinkStatus(value);
                _Slave_Link_Status = value;
            }
        }
        private void SetLinkStatus(ESlaveLink[] _value)
        {
            var log = LogManager.GetLogger("Machine Link Status");
            var v = GetMismatchedIndexes(Slave_Link_Status, _value);
            if (Slave_Link_Status != null)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    if (Slave_Link_Status[i] != _value[i])
                    {
                        DispLinkStatus(_value, i, log);
                    }
                }
            }
            else
            {
                for (int i = 0; i < v.Length; i++)
                {
                    DispLinkStatus(_value, i, log);
                }
            }
            if (v.Length != 0)
                SetDeviceMode(_value, log);
        }

        void DispLinkStatus(ESlaveLink[] _value, int index, Logger log)
        {
            MySlaves[index].Slave_Link_State = _value[index];
            switch (MySlaves[index].Slave_Link_State)
            {
                case ESlaveLink.EC_LINK_STATE_OK:
                    var _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Green);
                    log.Log(LogLevel.Warn, _tmg);
                    TNF.ShowSuccess(_tmg); 
                    break;
                case ESlaveLink.EC_LINK_STATE_Other_Fail:
                case ESlaveLink.EC_LINK_STATE_ADDITIONAL_LINK:
                case ESlaveLink.EC_LINK_STATE_NOT_PRESENT:
                case ESlaveLink.EC_LINK_STATE_LINK_WITHOUT_COMM:
                case ESlaveLink.EC_LINK_STATE_MISSING_LINK:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Red);
                    log.Log(LogLevel.Warn, _tmg);
                    TNF.ShowWarning(_tmg);
                    break;
                case ESlaveLink.UnAvailable:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.White);
                    //log.Log(LogLevel.Warn, _tmg);
                    //TNF.ShowWarning(_tmg);
                    break;
                default:
                    break;
            }
        }

        private string SetUI(int i, ESlaveLink sw, SolidColorBrush br)
        {
            var ds = Slave_StateMachine_Status == null ? "" : $"{Slave_StateMachine_Status[i]}";
            MySlaves[i].ToolTip =
                $"Box_{i} \n" +
                $" Link State : {sw}\n" +
                $" State Machine : {ds}\n";
            MySlaves[i].StrokeColor = br;
            var _tmg = $"{MySlaves[i].SlaveName} --> Link State {MySlaves[i].Slave_Link_State}";
            return _tmg;
        }
        private string SetUI(int i, ESlaveStateMachine sw, SolidColorBrush br)
        {
            var ds = Slave_Link_Status == null ? "" : $"{Slave_Link_Status[i]}";
            MySlaves[i].ToolTip =
                $"Box_{i} \n" +
                $" Link State : {ds}\n" +
                $" State Machine : {sw}\n";
            MySlaves[i].FillColor = br;
            var _tmg = $"{MySlaves[i].SlaveName} --> State Machine {MySlaves[i].Slave_State_Machine}";
            return _tmg;
        }

        private ESlaveStateMachine[] _Slave_StateMachine_Status;

        public ESlaveStateMachine[] Slave_StateMachine_Status
        {
            get => _Slave_StateMachine_Status;
            set
            {
                SetStateMachine(value);
                _Slave_StateMachine_Status = value;
            }
        }
        private void SetStateMachine(ESlaveStateMachine[] _value)
        {
            var log = LogManager.GetLogger("State Machine Status");
            var v = GetMismatchedIndexes(Slave_StateMachine_Status, _value);
            if (Slave_StateMachine_Status != null)
            {
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    if (Slave_StateMachine_Status[i] != _value[i])
                    {
                        DispStateMachine(_value, i, log);
                    }
                }
            }
            else
            {
                for (int i = 0; i < v.Length; i++)
                {
                    DispStateMachine(_value, i, log);
                }
            }
            if (v.Length != 0)
                SetDeviceMode(_value, log);
        }

        private void SetDeviceMode(ESlaveStateMachine[] _value, Logger log)
        {
            if (_value.SequenceEqual(TwinCathelper.ValidStateMachines))
            {
                TwinCathelper.Slave_StateMachine_Abort = false;
                TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                TwinCathelper.Slave_StateMachine_Abort = true;
                TwinCathelper.SetAbortMode(log);
            }
        }
        private void SetDeviceMode(ESlaveLink[] _value, Logger log)
        {
            if (_value.SequenceEqual(TwinCathelper.ValidElinkStates))
            {
                TwinCathelper.Slave_Link_Abort = false;
                TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                TwinCathelper.Slave_Link_Abort = true;
                TwinCathelper.SetAbortMode(log);
            }
        }
        void DispStateMachine(ESlaveStateMachine[] _value, int index, Logger log)
        {
            MySlaves[index].Slave_State_Machine = _value[index];
            {
                switch (MySlaves[index].Slave_State_Machine)
                {
                    case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                        var _tmg = SetUI(index, MySlaves[index].Slave_State_Machine, Brushes.Green);
                        TNF.ShowSuccess(_tmg);
                        log.Log(LogLevel.Info, _tmg);
                        break;

                    case ESlaveStateMachine.EC_DEVICE_STATE_INIT:
                    case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                    case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                    case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                    case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                    case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                    case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                    case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                        _tmg = SetUI(index, MySlaves[index].Slave_State_Machine, Brushes.Red);
                        TNF.ShowError(_tmg);
                        log.Log(LogLevel.Error, _tmg);
                        break;
                    default:
                        break;
                }
            }
        }
        private void Slave_Link_Status_Changed(object? sender, EventArgs e)
        {
            Slave_Link_Status = TwinCathelper.Slave_Link_Status;
        }

        private void Slave_StateMachine_Status_Changed(object? sender, EventArgs e)
        {
            Slave_StateMachine_Status = TwinCathelper.Slave_StateMachine_Status;
        }
        public MachineDetailViewModel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            MySlaves = new ST_EcSlaveState[TwinCathelper.DeviceCount];
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                MySlaves[i] = new ST_EcSlaveState()
                {
                    SlaveID = i,
                    SlaveName = $"Box_{i}",
                    Slave_State_Machine = ESlaveStateMachine.UnAvailable,
                    Slave_Link_State = ESlaveLink.UnAvailable,
                    FillColor = Brushes.White,
                    StrokeColor = Brushes.White,
                    ToolTip =
                    $"Box_{i} \n" +
                    $" Link State : {ESlaveLink.UnAvailable}\n" +
                    $" State Machine : {ESlaveStateMachine.UnAvailable}\n"
                };
            }
            TwinCathelper.TSlave_Link_Status_Changed += Slave_Link_Status_Changed;
            TwinCathelper.TSlave_StateMachine_Status_Changed += Slave_StateMachine_Status_Changed;
            TwinCathelper.SetMachineStatusTimer();
        }
    }
}
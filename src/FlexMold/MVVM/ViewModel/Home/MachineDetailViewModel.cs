using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media;

namespace FlexMold.MVVM.ViewModel.Home
{
    public class MachineDetailViewModel : StatusBaseViewModel, IMachineDetailViewModel
    {
        private Logger log = LogManager.GetLogger("Machine Link Status");

        public ST_EcSlaveState[] MySlaves
        { get => _MySlaves; set { _MySlaves = value; OnPropertyChanged(); } }

        private ST_EcSlaveState[] _MySlaves;
        private ESlaveLink[] _Slave_Link_Status;
        public ESlaveLink[] Slave_Link_Status
        {
            get => _Slave_Link_Status;
            set
            {
                try
                {
                    SetLinkStatus(value);
                    _Slave_Link_Status = value;
                }
                catch (Exception ex)
                {

                    //log.Log(LogLevel.Error, ex.Message);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>Slave_Link_Status>>>>" + ex.Message + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                }
            }
        }
        private void SetLinkStatus(ESlaveLink[] _value)
        {
            //var log = LogManager.GetLogger("Machine Link Status");
            var v = GetMismatchedIndexes(Slave_Link_Status, _value);

            if (Slave_Link_Status != null)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    DispLinkStatus(_value, v[i]);
                }
            }
            else
            {
                for (int i = 0; i < v.Length; i++)
                {
                    DispLinkStatus(_value, i);
                }
            }
            if (v.Length != 0)
                SetDeviceMode(_value, log);
        }

        void DispLinkStatus(ESlaveLink[] _value, int index)
        {
            MySlaves[index].Slave_Link_State = _value[index];
            CSVHelper.MachineEnabled = false;
            HomeMachineStatus = false;

            switch (MySlaves[index].Slave_Link_State)
            {
                case ESlaveLink.EC_LINK_STATE_OK:
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_OK PCB:" + index);
                    CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus >>>>>>>>EC_LINK_STATE_OK PCB:" + index + Environment.NewLine+ CSVHelper.WriteWarnMsgsOnce;
                    var _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Green);

                    CSVHelper.MachineEnabled = true;
                    HomeMachineStatus = true;
                    
                    if(CSVHelper.PCBsInError.Contains(index))
                        CSVHelper.PCBsInError.Remove(index);//, "EC_LINK_STATE_NOT_PRESENT"

                    if(CSVHelper.PCBsInError.Count<1)
                        CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " > MotorStatusView >> DispDevStatus>> ***************** Machine is Ready *****************" + Environment.NewLine + CSVHelper.WriteWarnMsgsOnce;

                    log.Log(LogLevel.Info, _tmg);
                    TNF.ShowSuccess(_tmg);
                    break;
                case ESlaveLink.EC_LINK_STATE_Other_Fail:
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_Other_Fail PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_Other_Fail PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.EC_LINK_STATE_NOT_PRESENT:
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_NOT_PRESENT PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_NOT_PRESENT PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;

                    if (!CSVHelper.PCBsInError.Contains(index)) CSVHelper.PCBsInError.Add(index, "EC_LINK_STATE_NOT_PRESENT");                    
                    break;
                case ESlaveLink.EC_LINK_STATE_ADDITIONAL_LINK:
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_ADDITIONAL_LINK PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_ADDITIONAL_LINK PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.EC_LINK_STATE_LINK_WITHOUT_COMM:
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_LINK_WITHOUT_COMM PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_LINK_WITHOUT_COMM PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.ERR_PREOP_INIT_ERR:
                    //log.Log(LogLevel.Error, ">>>>>>>>ERR_PREOP_INIT_ERR PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  >>>>>>>>ERR_PREOP_INIT_ERR PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.EC_LINK_STATE_MISSING_LINK:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Red);
                    //log.Log(LogLevel.Warn, _tmg);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_MISSING_LINK PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    TNF.ShowWarning(_tmg);
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_STATE_MISSING_LINK PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_STATE_MISSING_LINK PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.UnAvailable:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.White);
                    log.Log(LogLevel.Trace, _tmg);
                    //log.Log(LogLevel.Error, ">>>>>>>>UnAvailable PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>UnAvailable PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;

                    //TNF.ShowWarning(_tmg);
                    break;
                case ESlaveLink.EC_DEVICE_STATE_INIT:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Blue);
                    log.Log(LogLevel.Trace, _tmg);
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_DEVICE_STATE_INIT PCB:"+index);
                    CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_DEVICE_STATE_INIT PCB:" + index + Environment.NewLine + CSVHelper.WriteWarnMsgsOnce;

                    if (CSVHelper.PCBsInError.Contains(index))
                        CSVHelper.PCBsInError.Remove(index);//, "EC_LINK_STATE_NOT_PRESENT"
                    //else if(FlexMold.MVVM.ViewModel.Project.ProjectViewModel.LastEventPressed=='W' && !CSVHelper.PCBsInError.Contains(index))//https://github.com/eSpark-Consultants/flexmold/issues/52
                    //    CSVHelper.PCBsInError.Add(index, "EC_LINK_STATE_NOT_PRESENT");//, "EC_LINK_STATE_NOT_PRESENT"
                    //TNF.ShowWarning(_tmg);
                    break;
                case ESlaveLink.EC_LINK_20://StatusByTwinCat
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_20 PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_20 PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                case ESlaveLink.EC_LINK_40://StatusByTwinCat
                    //log.Log(LogLevel.Error, ">>>>>>>>EC_LINK_40 PCB:" + index);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>>>>>>>EC_LINK_40 PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                    break;
                default:
                    _tmg = SetUI(index, MySlaves[index].Slave_Link_State, Brushes.Orange);
                    //log.Log(LogLevel.Error, "Undocumented State>>>>>>>>PCB:" + index);

                    //log.Log(LogLevel.Error, "! Undocumented State ! " + _tmg);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispLinkStatus>>> ! Undocumented State ! >>>>>>>>PCB:" + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;

                    TNF.ShowError(_tmg);
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
            if(CSVHelper.MotorsStatusGlobal==null || CSVHelper.MotorsStatusGlobal.Length<1)
                CSVHelper.MotorsStatusGlobal = new String[TwinCathelper.DeviceCount];
            if (CSVHelper.MotorsStatusGlobal.Length > 0) CSVHelper.MotorsStatusGlobal[i] = sw.ToString();
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
        private readonly ITwinCathelper _TwinCathelper;

        public ESlaveStateMachine[] Slave_StateMachine_Status
        {
            get => _Slave_StateMachine_Status;
            set
            {
                try { 
                SetStateMachine(value);
                _Slave_StateMachine_Status = value;
                }
                catch (Exception ex)
                {
                    var log = LogManager.GetLogger("Machine Link Status");
                    //log.Log(LogLevel.Error, ex.Message);
                    CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>Slave_StateMachine_Status >> Slave_StateMachine_Status>>>>" + ex.Message + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                }

            }
        }
        private void SetStateMachine(ESlaveStateMachine[] _value)
        {
            var log = LogManager.GetLogger("State Machine Status");
            var v = GetMismatchedIndexes(Slave_StateMachine_Status, _value);
            if (Slave_StateMachine_Status != null)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    DispStateMachine(_value, v[i], log);
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
            if (_value.SequenceEqual(_TwinCathelper.ValidStateMachines))
            {
                _TwinCathelper.Slave_StateMachine_Abort = false;
                _TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                _TwinCathelper.Slave_StateMachine_Abort = true;
                _TwinCathelper.SetAbortMode(log);
            }
        }
        private void SetDeviceMode(ESlaveLink[] _value, Logger log)
        {
            if (_value.SequenceEqual(_TwinCathelper.ValidElinkStates))
            {
                _TwinCathelper.Slave_Link_Abort = false;
                _TwinCathelper.SetDefaultMode(log);
            }
            else
            {
                _TwinCathelper.Slave_Link_Abort = true;
                _TwinCathelper.SetAbortMode(log);
            }
        }
        void DispStateMachine(ESlaveStateMachine[] _value, int index, Logger log)
        {
            MySlaves[index].Slave_State_Machine = _value[index];
            {
                CSVHelper.MachineEnabled = false;
                HomeMachineStatus = false;

                switch (MySlaves[index].Slave_State_Machine)
                {
                    case ESlaveStateMachine.EC_DEVICE_STATE_OK:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_OK>>> " + index);
                        CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_OK>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;

                    case ESlaveStateMachine.EC_DEVICE_STATE_OP:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_OP>>> " + index);
                        CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_OP>>> " + index + Environment.NewLine + CSVHelper.WriteWarnMsgsOnce;
                        var _tmg = SetUI(index, MySlaves[index].Slave_State_Machine, Brushes.Green);
                        if (CSVHelper.MotorsStatusGlobal.Length > 0) CSVHelper.MotorsStatusGlobal[index] = "Healthy";
                        
                        CSVHelper.MachineEnabled = true;

                        if (CSVHelper.PCBsInError.Count < 1)
                            CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " > MotorStatusView >> DispStateMachine>> ***************** Machine is Ready *****************" + Environment.NewLine + CSVHelper.WriteWarnMsgsOnce;

                        HomeMachineStatus = true;

                        TNF.ShowSuccess(_tmg);
                        log.Log(LogLevel.Info, _tmg);
                        break;
                    
                    case ESlaveStateMachine.EC_DEVICE_STATE_INIT:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_INIT>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_INIT>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_PREOP:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_PREOP>>> " + index);
                        CSVHelper.WriteWarnMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_PREOP>>> " + index + Environment.NewLine + CSVHelper.WriteWarnMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_BOOTSTRAP>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_SAFEOP>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_ERROR:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_ERROR>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_ERROR>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_INVALID_VPRS>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_INITCMD_ERROR>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                    case ESlaveStateMachine.EC_DEVICE_STATE_DISABLED:
                        //log.Log(LogLevel.Error, "ESlaveStateMachine.EC_DEVICE_STATE_DISABLED>>> " + index);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ESlaveStateMachine.EC_DEVICE_STATE_DISABLED>>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        _tmg = SetUI(index, MySlaves[index].Slave_State_Machine, Brushes.Red);
                        TNF.ShowError(_tmg);
                        if(CSVHelper.MotorsStatusGlobal.Length>0)
                            CSVHelper.MotorsStatusGlobal[index] = MySlaves[index].Slave_State_Machine.ToString();
                        log.Log(LogLevel.Error, _tmg);
                        break;
                    default:
                        _tmg = SetUI(index, MySlaves[index].Slave_State_Machine, Brushes.Red);
                        TNF.ShowError(_tmg);
                        //log.Log(LogLevel.Error, "! Undocumented State ! " + _tmg);
                        CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>DispStateMachine ! Undocumented State ! >>> " + index + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
                        break;
                }
            }
        }
        private void Slave_Link_Status_Changed(object? sender, EventArgs e)
        {
            Slave_Link_Status = _TwinCathelper.Slave_Link_Status;
        }

        private void Slave_StateMachine_Status_Changed(object? sender, EventArgs e)
        {
            Slave_StateMachine_Status = _TwinCathelper.Slave_StateMachine_Status;
        }
        public MachineDetailViewModel(ITwinCathelper twinCathelper)
        {
            try { 
            this._TwinCathelper = twinCathelper;
            if (_TwinCathelper.Client == null)
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
            _TwinCathelper.TSlave_Link_Status_Changed += Slave_Link_Status_Changed;
            _TwinCathelper.TSlave_StateMachine_Status_Changed += Slave_StateMachine_Status_Changed;
            _TwinCathelper.SetMachineStatusTimer();

                HomeMachineStatus = false;
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Machine Link Status");
                //log.Log(LogLevel.Error, ex.Message);
                CSVHelper.WriteErrMsgsOnce = DateTime.Now + " >  MachineDetailView>>> !MachineDetailViewModel >>> " + ex.Message + Environment.NewLine + CSVHelper.WriteErrMsgsOnce;
            }

        }
    }
}
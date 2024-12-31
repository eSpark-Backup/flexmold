using FlexMold.MVVM.Model;

namespace FlexMold.MVVM.ViewModel.Home
{
    public interface IMachineDetailViewModel
    {
        ST_EcSlaveState[] MySlaves { get; set; }
        ESlaveLink[] Slave_Link_Status { get; set; }
        ESlaveStateMachine[] Slave_StateMachine_Status { get; set; }
    }
}
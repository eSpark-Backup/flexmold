using FlexMold.MVVM.Model;

namespace FlexMold.MVVM.ViewModel.Home
{
    internal interface IMachineDetailViewModel
    {
        public ST_EcSlaveState[] MySlaves { get; set; }
    }
}

using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    public interface IResetViewModel
    {
        RelayCommand HardZeroCommand { get; set; }
        RelayCommand HardCalibrCommand { get; set; }
        RelayCommand UpdateHomingCommand { get; set; }
        RelayCommand GetHomingCommand { get; set; }
    }
}
using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    public interface IReadMotorViewModel
    {
        RelayCommand ReadMotorVCommand { get; set; }
        int NumMotorFrom { get; set; }
        int NumMotorTo { get; set; }
    }
}
using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    public interface IPositionViewModel
    {
        RelayCommand GetFileBrowse { get; set; }
        RelayCommand doMoveUp { get; set; }
        RelayCommand doMoveDown { get; set; }
        RelayCommand doMoveManually { get; set; }

        RelayCommand NumMotorFrom { get; set; }
        RelayCommand NumMotorTo { get; set; }
        RelayCommand NumMotorManual { get; set; }
    }
}
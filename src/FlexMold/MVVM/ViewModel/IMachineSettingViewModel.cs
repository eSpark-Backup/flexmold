using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexMold.MVVM.ViewModel
{
    public interface IMachineSettingViewModel
    {
        string motorCount { get; set; }
        string machineOperatedTime { get; set; }
        string machinePartsProduced { get; set; }
        string machineRadius { get; set; }
        string machineSize { get; set; }
        
    }
}
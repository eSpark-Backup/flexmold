using FlexMold.Core;

namespace FlexMold.MVVM.ViewModel
{
    public interface ISettingViewModel
    {
        object CurrentView { get; set; }
        RelayCommand LaserSettingCommand { get; set; }
        RelayCommand MachineSettingCommand { get; set; }
        RelayCommand MotorSettingCommand { get; set; }
        RelayCommand PowerSettingCommand { get; set; }
        RelayCommand VacuumSettingCommand { get; set; }
    }
}
using FlexMold.Core;

namespace FlexMold.MVVM.ViewModel
{
    public interface IHomeViewModel
    {
        object CurrentView { get; set; }
        RelayCommand EmergencyStatusCommand { get; set; }
        RelayCommand HeaterStatusCommand { get; set; }
        RelayCommand LaserStatusCommand { get; set; }
        RelayCommand MachineDetailCommand { get; set; }
        RelayCommand MotorStatusCommand { get; set; }
        RelayCommand PowerStatusCommand { get; set; }
        RelayCommand VaccumStatusCommand { get; set; }
        RelayCommand VisualDisplayCommand { get; set; }
        RelayCommand HomingStatusCommand { get; set; }
    }
}
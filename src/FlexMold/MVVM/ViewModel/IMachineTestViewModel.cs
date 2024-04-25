using FlexMold.Core;

namespace FlexMold.MVVM.ViewModel
{
    public interface IMachineTestViewModel
    {
        object CurrentView { get; set; }
        RelayCommand MotorTestCommand { get; set; }
        RelayCommand PositionCommand { get; set; }
        RelayCommand PowerOnOffCommand { get; set; }
        RelayCommand ReadMotorCommand { get; set; }
        //RelayCommand RenumberingCommand { get; set; }
        RelayCommand ResetCommand { get; set; }
    }
}
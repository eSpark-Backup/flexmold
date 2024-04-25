using FlexMold.Core;

namespace FlexMold.MVVM.ViewModel.Setting
{
    public interface IMotorSettingViewModel
    {
        uint[] DeviceIds { get; set; }
        //RelayCommand FirmwareCommand { get; set; }
        bool IsDialogOpen { get; set; }
        uint SelectedMotorIndex { get; set; }
    }
}
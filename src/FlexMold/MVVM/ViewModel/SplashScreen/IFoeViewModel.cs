using FlexMold.Core;
using FlexMold.MVVM.Model;
using System;
using System.Collections.ObjectModel;

namespace FlexMold.MVVM.ViewModel.SplashScreen
{
    public interface IFoeViewModel: IViewModel
    {
        RelayCommand BrowseFirmwareCommand { get; set; }
        bool BT_FirmwareEnable { get; set; }
        RelayCommand CheckAllCommand { get; set; }
        string DeviceStatusIndex { get; set; }
        string Foe_dwPass { get; set; }
        string Foe_Remarks { get; set; }
        string Foe_sNetId { get; }
        string Foe_sPathName { get; set; }
        bool? IsCheckedAll { get; set; }
        ObservableCollection<FoeModel> MyDevicesAndBoxes { get; set; }
        uint OverAllProgress { get; set; }
        RelayCommand RefreshDataGridCommand { get; set; }
        FoeModel SelectedDeviceAndBox { get; set; }
        RelayCommand UpdateFirmwareCommand { get; set; }

        event EventHandler<bool> TUpdate_Status;
    }
}
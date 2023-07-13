using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel.Home;
using System.IO;

namespace FlexMold.MVVM.ViewModel.Project
{
    public interface IProjectViewModel
    {
        IMotorStatusViewmodel _MotorStatus { get; set; }
        RelayCommand BrowseProjectCommand { get; set; }
        bool CSVWriteButton { get; set; }
        bool DeviceStatus { get; }
        RelayCommand HomeMotorsCommand { get; set; }
        bool LaserSendButton { get; set; }
        Porjects myDataModels { get; set; }
        float[] Position { get; set; }
        FileInfo SelectedFile { get; set; }
        Model.Project selectedMyDataModel { get; set; }
        RelayCommand SendCSVCommand { get; set; }
        RelayCommand StopMotorsCommand { get; set; }
    }
}
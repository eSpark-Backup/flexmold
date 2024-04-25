using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel.Home;
using System.IO;

namespace FlexMold.MVVM.ViewModel.Project
{
    public interface IProjectViewModel
    {
        RelayCommand BrowseProjectCommand { get; set; }
        bool CSVWriteButton { get; set; }
        RelayCommand HomeMotorsCommand { get; set; }
        bool LaserSendButton { get; set; }
        IMotorStatusViewmodel MotorStatus { get; }
        Porjects myDataModels { get; set; }
        float[] Position { get; set; }
        ProjectFilePanel SelectedFile { get; set; }
        Model.Project selectedMyDataModel { get; set; }
        Model.Project rootSelectedMyDataModel { get; set; }
        RelayCommand SendCSVCommand { get; set; }
        RelayCommand StopMotorsCommand { get; set; }
        string selectedListRootDir { get; set; }
    //RelayCommand StopResumeButtonText { get; set; }
}
}
using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Project
{
    public interface ILaserProjectorViewModel
    {
        IProjectViewModel _ProjectViewModel { get; }
        RelayCommand CallibrateProjectorCommand { get; set; }
        int LaserProjectorComMode { get; set; }
        RelayCommand LaunchLMPCommand { get; set; }
        RelayCommand LaunchLPMCommand { get; set; }
        LaserProjector MyPorjector { get; set; }
        RelayCommand ProjectionOffCommand { get; set; }
        RelayCommand ProjectionOnCommand { get; set; }
        RelayCommand ProjectorModeSelectionCommand { get; set; }
        RelayCommand SambaConfigCommand { get; set; }
        bool SaveData { get; set; }
        SerialPort SelectedSerialPort { get; set; }
        RelayCommand SendLasFileCommand { get; set; }
        RelayCommand SerialConfigCommand { get; set; }

        Task sendLasFileAsync(FileInfo _slectedFile);
    }
}
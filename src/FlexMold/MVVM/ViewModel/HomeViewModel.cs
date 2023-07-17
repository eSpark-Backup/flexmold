using FlexMold.Core;
using FlexMold.MVVM.View.Home;

namespace FlexMold.MVVM.ViewModel
{
    public class HomeViewModel : ObservableObject, IHomeViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand VisualDisplayCommand { get; set; }

        public RelayCommand VaccumStatusCommand { get; set; }
        public RelayCommand MachineDetailCommand { get; set; }
        public RelayCommand MotorStatusCommand { get; set; }
        public RelayCommand HeaterStatusCommand { get; set; }
        public RelayCommand PowerStatusCommand { get; set; }
        public RelayCommand EmergencyStatusCommand { get; set; }
        public RelayCommand LaserStatusCommand { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public HomeViewModel(
            MachineDetailView machineDetailView,
            MotorStatusView motorStatusView,
            VisualDisplayView visualDisplayView,
            VaccumStatusView vaccumStatusView,
            HeaterStatusView heaterStatusView,
            PowerStatusView powerStatusView,
            EmergencyStatusView emergencyStatusView,
            LaserStatusView laserStatusView
            )
        {

            CurrentView = visualDisplayView;

            VisualDisplayCommand = new RelayCommand(o => { CurrentView = visualDisplayView; });
            VaccumStatusCommand = new RelayCommand(o => { CurrentView = vaccumStatusView; });
            MachineDetailCommand = new RelayCommand(o => { CurrentView = machineDetailView; });
            MotorStatusCommand = new RelayCommand(o => { CurrentView = motorStatusView; });
            HeaterStatusCommand = new RelayCommand(o => { CurrentView = heaterStatusView; });
            PowerStatusCommand = new RelayCommand(o => { CurrentView = powerStatusView; });
            EmergencyStatusCommand = new RelayCommand(o => { CurrentView = emergencyStatusView; });
            LaserStatusCommand = new RelayCommand(o => { CurrentView = laserStatusView; });
        }
    }
}
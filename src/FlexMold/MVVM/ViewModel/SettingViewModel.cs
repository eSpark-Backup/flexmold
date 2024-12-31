using FlexMold.Core;
using FlexMold.MVVM.View.Setting;

namespace FlexMold.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject, ISettingViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand MachineSettingCommand { get; set; }

        public RelayCommand PowerSettingCommand { get; set; }
        public RelayCommand MotorSettingCommand { get; set; }
        public RelayCommand VacuumSettingCommand { get; set; }
        public RelayCommand LaserSettingCommand { get; set; }

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

        public SettingViewModel(
            MachineSettingView machineSettingView,
            PowerSettingView powerSettingView,
            MotorSettingView motorSettingView,
            VacuumSettingView vacuumSettingView,
            LaserProjectorSettingView laserProjectorSettingView
            )
        {
            CurrentView = machineSettingView;

            MachineSettingCommand = new RelayCommand(o => { CurrentView = machineSettingView; });
            PowerSettingCommand = new RelayCommand(o => { CurrentView = powerSettingView; });
            MotorSettingCommand = new RelayCommand(o => { CurrentView = motorSettingView; });
            VacuumSettingCommand = new RelayCommand(o => { CurrentView = vacuumSettingView; });
            LaserSettingCommand = new RelayCommand(o => { CurrentView = laserProjectorSettingView; });
        }
    }
}
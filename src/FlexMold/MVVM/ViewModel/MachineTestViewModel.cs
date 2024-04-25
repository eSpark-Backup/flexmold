using FlexMold.Core;
using FlexMold.MVVM.View.MachineTest;

namespace FlexMold.MVVM.ViewModel
{
    public class MachineTestViewModel : ObservableObject, IMachineTestViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand MotorTestCommand { get; set; }

        public RelayCommand ReadMotorCommand { get; set; }
        public RelayCommand PowerOnOffCommand { get; set; }
        //public RelayCommand RenumberingCommand { get; set; }
        public RelayCommand PositionCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }

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

        public MachineTestViewModel(
            MotorTest motorTest,
            ReadMotorView readMotorView,
            PowerOnOffView powerOnOffView,
            //RenumberingView renumberingView,
            PositionView positionView,
            ResetView resetView)
        {
            CurrentView = motorTest;

            MotorTestCommand = new RelayCommand(o => { CurrentView = motorTest; });
            ReadMotorCommand = new RelayCommand(o => { CurrentView = readMotorView; });
            PowerOnOffCommand = new RelayCommand(o => { CurrentView = powerOnOffView; });
            //RenumberingCommand = new RelayCommand(o => { CurrentView = renumberingView; });
            PositionCommand = new RelayCommand(o => { CurrentView = positionView; });
            ResetCommand = new RelayCommand(o => { CurrentView = resetView; });
        }
    }
}
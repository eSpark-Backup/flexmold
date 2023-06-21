using FlexMold.Core;
using FlexMold.MVVM.View.MachineTest;

namespace FlexMold.MVVM.ViewModel
{
    internal class MachineTestViewModel : ObservableObject
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand MotorTestCommand { get; set; }

        public RelayCommand ReadMotorCommand { get; set; }
        public RelayCommand PowerOnOffCommand { get; set; }
        public RelayCommand RenumberingCommand { get; set; }
        public RelayCommand PositionCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }

        public MotorTest MotorTestVM { get; set; }
        public ReadMotorView ReadMotorVM { get; set; }
        public PowerOnOffView PowerOnOfVM { get; set; }
        public RenumberingView RenumberingVM { get; set; }
        public PositionView PositionVM { get; set; }
        public ResetView ResetVM { get; set; }

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

        public MachineTestViewModel()
        {
            MotorTestVM = new MotorTest();
            ReadMotorVM = new ReadMotorView();
            PowerOnOfVM = new PowerOnOffView();
            RenumberingVM = new RenumberingView();
            PositionVM = new PositionView();
            ResetVM = new ResetView();

            CurrentView = MotorTestVM;

            MotorTestCommand = new RelayCommand(o => { CurrentView = MotorTestVM; });
            ReadMotorCommand = new RelayCommand(o => { CurrentView = ReadMotorVM; });
            PowerOnOffCommand = new RelayCommand(o => { CurrentView = PowerOnOfVM; });
            RenumberingCommand = new RelayCommand(o => { CurrentView = RenumberingVM; });
            PositionCommand = new RelayCommand(o => { CurrentView = PositionVM; });
            ResetCommand = new RelayCommand(o => { CurrentView = ResetVM; });
        }
    }
}
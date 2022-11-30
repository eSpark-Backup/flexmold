using FlexMold.Core;
using FlexMold.MVVM.View.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel
{
    class HomeViewModel : ObservableObject
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



        public VisualDisplayView VisualDisplayVM { get; set; }
        public VaccumStatusView VaccumStatusVM { get; set; }
        public MachineDetailView MachineDetailVM { get; set; }
        public MotorStatusView MotorStatusMV { get; set; }
        public HeaterStatusView HeaterStatusVM { get; set; }
        public PowerStatusView PowerStatusVM { get; set; }
        public EmergencyStatusView EmergencyStatusVM { get; set; }
        public LaserStatusView LaserStatusVM { get; set; }

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
        public HomeViewModel()
        {
            VisualDisplayVM = new VisualDisplayView();
            VaccumStatusVM = new VaccumStatusView();
            MachineDetailVM = new MachineDetailView();
            MotorStatusMV = new MotorStatusView();
            HeaterStatusVM = new HeaterStatusView();
            PowerStatusVM = new PowerStatusView();
            EmergencyStatusVM = new EmergencyStatusView();
            LaserStatusVM = new LaserStatusView();

            CurrentView = VisualDisplayVM;

            VisualDisplayCommand = new RelayCommand(o => { CurrentView = VisualDisplayVM; });
            VaccumStatusCommand = new RelayCommand(o => { CurrentView = VaccumStatusVM; });
            MachineDetailCommand = new RelayCommand(o => { CurrentView = MachineDetailVM; });
            MotorStatusCommand = new RelayCommand(o => { CurrentView = MotorStatusMV; });
            HeaterStatusCommand = new RelayCommand(o => { CurrentView = HeaterStatusVM; });
            PowerStatusCommand = new RelayCommand(o => { CurrentView = PowerStatusVM; });
            EmergencyStatusCommand = new RelayCommand(o => { CurrentView = EmergencyStatusVM; });
            LaserStatusCommand = new RelayCommand(o => { CurrentView = LaserStatusVM; });
        }

    }
}

using FlexMold.Core;
using FlexMold.MVVM.View.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel
{
    class SettingViewModel : ObservableObject
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand MachineSettingCommand { get; set; }
        public RelayCommand PowerSettingCommand { get; set; }
        public RelayCommand MotorSettingCommand { get; set; }
        public RelayCommand VacuumSettingCommand { get; set; }

        public MachineSettingView MachineSettingVM { get; set; }
        public PowerSettingView PowerSettingVM { get; set; }
        public MotorSettingView MotorSettingVM { get; set; }
        public VacuumSettingView VacuumSettingMV { get; set; }

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
        public SettingViewModel()
        {
            MachineSettingVM = new MachineSettingView();
            PowerSettingVM = new PowerSettingView();
            MotorSettingVM = new MotorSettingView();
            VacuumSettingMV = new VacuumSettingView();

            CurrentView = MachineSettingVM;

            MachineSettingCommand = new RelayCommand(o => { CurrentView = MachineSettingVM; });
            PowerSettingCommand = new RelayCommand(o => { CurrentView = PowerSettingVM; });
            MotorSettingCommand = new RelayCommand(o => { CurrentView = MotorSettingVM; });
            VacuumSettingCommand = new RelayCommand(o => { CurrentView = VacuumSettingMV; });

        }

    }
}

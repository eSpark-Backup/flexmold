using FlexMold.Core;
using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace FlexMold.MVVM.ViewModel
{

    class SplashScreenViewModel 
    {
        public Machine SelectedMachine {  get;set; }
        public RelayCommand MachineClickCommand { get; set; }
        public ObservableCollection<Machine> MyMachines { get; set; }
        public event EventHandler OnRequestClose;
        public SplashScreenViewModel()
        {
            SelectedMachine = new Machine();
            MyMachines = new ObservableCollection<Machine>
            {
                new Machine()
                { ID = 1, Location = "R0", MachineType = EMachineType.Motor, Name = "M1" },
                new Machine()
                { ID = 2, Location = "R1", MachineType = EMachineType.Motor, Name = "M2" },
                new Machine()
                { ID = 3, Location = "R2", MachineType = EMachineType.Projector, Name = "P1" },

            };
            MachineClickCommand = new RelayCommand(o => MachineSelectMethod(o as Machine));
        }
        void MachineSelectMethod(Machine a)
        {
            SelectedMachine = a;
            OnRequestClose(this, new EventArgs());
        }
    }
  

}

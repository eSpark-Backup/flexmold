using FlexMold.Core;
using FlexMold.MVVM.Model;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexMold.MVVM.ViewModel
{
    public interface ISplashScreenViewModel : IViewModel
    {
        ConfigurationFactory _factory { get; set; }
        Script _runningScript { get; set; }
        IWorker _worker { get; set; }
        List<VSInfo> AvailableVS { get; }
        RelayCommand CloseButtonCommand { get; set; }
        bool CloseButtonEnable { get; set; }
        bool ComRegistration { get; set; }
        bool ICEnable { get; set; }
        RelayCommand MachineClickCommand { get; set; }
        ObservableCollection<Machine> MyMachines { get; set; }
        int ProgressPercentage { get; set; }
        string ProgressStatuses { get; set; }
        RelayCommand RunButtonCommand { get; set; }
        bool RunButtonEnable { get; set; }
        Machine SelectedMachine { get; set; }
        Script SelectedScript { get; set; }
        VSInfo SelectedVS { get; set; }
        event EventHandler OnRequestCollapsed;

        event EventHandler OnRequestClose;

        void RunScript();
        void updateVSInstallations();
    }
}
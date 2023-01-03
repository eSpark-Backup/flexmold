using FlexMold.Core;
using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace FlexMold.MVVM.ViewModel
{

    class SplashScreenViewModel : ObservableObject
    {
        public VSInfo SelectedVS { get; set; }
        private bool _RunButtonEnable;

        public bool RunButtonEnable
        {
            get { return _RunButtonEnable; }
            set
            {
                _RunButtonEnable = value;
                OnPropertyChanged();
            }
        }

        public Script SelectedScript { get; set; }

        private int _ProgressPercentage;

        public int ProgressPercentage
        {
            get { return _ProgressPercentage; }
            set
            {
                _ProgressPercentage = value;
                OnPropertyChanged();
            }
        }

        private string _ProgressStatuses;

        public string ProgressStatuses
        {
            get { return _ProgressStatuses; }
            set
            {
                _ProgressStatuses = value+"\n"+ ProgressStatuses;
                OnPropertyChanged();
            }
        }
        public bool ComRegistration { get; set; }
        public IWorker _worker { get; set; }
        public ConfigurationFactory _factory { get; set; }
        public Script _runningScript { get; set; }
        public Machine SelectedMachine { get; set; }
        public RelayCommand MachineClickCommand { get; set; }
        public RelayCommand CloseButtonCommand { get; set; }
        public RelayCommand RunButtonCommand { get; set; }
        public ObservableCollection<Machine> MyMachines { get; set; }
        public List<VSInfo> AvailableVS { get; private set; }

        public event EventHandler OnRequestClose;
        public SplashScreenViewModel()
        {
            RunButtonEnable = false;
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
            RunButtonCommand = new RelayCommand(o => RunMainWindow());
            CloseButtonCommand = new RelayCommand(o =>
            OnRequestClose(this, new EventArgs())
            );

        }

        void MachineSelectMethod(Machine _a)
        {
            SelectedMachine = _a;
            switch (SelectedMachine.MachineType)
            {
                case EMachineType.Motor:
                    if (MessageBox.Show("Do You Want to build TwinCAT Solution?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        RunMainWindow();
                    }
                    else
                    {
                        SelectedScript = new FlexMoldMachine1();
                        ComRegistration = true;
                        updateVSInstallations();
                        string TCVS = "TcXaeShell 15.0";
                        SelectedVS = AvailableVS.Where(a => a.Name == TCVS).FirstOrDefault();
                        if (SelectedVS != null)
                        {
                            RunScript();
                        }
                        else
                        {
                            MessageBox.Show($"IDE Not Found Please Install {TCVS}");
                            OnRequestClose(this, new EventArgs());
                        }
                    }

                    break;
                case EMachineType.Projector:
                    break;
                default:
                    break;
            }
        }
        public void updateVSInstallations()
        {
            VSInfo[] vsInstallations = null;
            if (ComRegistration)
            {
                int currentIndex;
                vsInstallations = ConfigurationFactory.GetVSInstallationsByRegistry(out currentIndex);
                //VSInfo selected = currentIndex >= 0 ? progIdDict2[currentIndex] : progIdDict2[0];
            }
            else
            {
                vsInstallations = ConfigurationFactory.GetVSInstallationsBySetupConfiguration();
            }
            UpdateVSInfos(vsInstallations);
        }
        private void UpdateVSInfos(VSInfo[] progIdDict2)
        {

            int CompareTo(VSInfo info1, VSInfo info2)
            {
                return info1.Version.CompareTo(info2.Version);
            }

            List<VSInfo> sorted = progIdDict2.ToList();
            sorted.Sort(CompareTo);

            AvailableVS = new List<VSInfo>();
            foreach (VSInfo vsInfo in sorted)
            {
                AvailableVS.Add(vsInfo);
            }
        }
        void RunMainWindow()
        {
            MainWindow mw = new MainWindow(SelectedMachine);
            OnRequestClose(this, new EventArgs());
            mw.Show();
            Application.Current.MainWindow = mw;
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
            if (ProgressPercentage == 100)
            {
                if (_worker != null)
                    _worker.CancelAndWait(TimeSpan.FromSeconds(1.0));
                RunButtonEnable = true;
            }
        }

        void _worker_ProgressStatusChanged(object sender, ProgressStatusChangedArgs e)
        {
            ProgressStatuses = e.Status;
        
        }
        public void RunScript()
        {
            Debug.Assert(this._factory == null);
            if (this.SelectedScript != null)
            {
                _runningScript = this.SelectedScript;
                VsFactory vsFactory = new VsFactory();

                if (_runningScript is ScriptEarlyBound)
                    this._factory = new EarlyBoundFactory(vsFactory, ComRegistration);
                else if (_runningScript is ScriptLateBound)
                    this._factory = new LateBoundFactory(vsFactory, ComRegistration);

                if (this._factory == null)
                {
                    throw new ApplicationException("Generator not found!");
                }

                Dictionary<string, dynamic> parameterSet = new Dictionary<string, dynamic>();
                ScriptContext context = new ScriptContext(_factory, null, parameterSet);

                _worker = new ScriptBackgroundWorker(/*this._factory,*/ _runningScript, context);
                ProgressPercentage = 5;
                ProgressStatuses = "Closing Opened Instances Cuurent TwinCAT sln";
                //Access COM Running Objects Table
                Dictionary<string, List<object>> rot = ROTAccess.GetRunningObjectTable();
                ROTAccess.CloseRunningDTETable(_runningScript.ScriptRootFolder + @"\" + _runningScript.ScriptName + ".sln");

                _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
                _worker.ProgressStatusChanged += new EventHandler<ProgressStatusChangedArgs>(_worker_ProgressStatusChanged);
#if OLD
                _factory.AppID = this.DTEInfo.ProgId;
#else
                _factory.VSInfo = SelectedVS;
#endif
                _factory.IsIdeVisible = false;
                _factory.IsIdeUserControl = false;
                _factory.SuppressUI = false;
                _worker.BeginScriptExecution();
            }
        }
    }
}

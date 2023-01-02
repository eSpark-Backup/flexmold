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

        public Script SelectedScript { get; set; }

        private int ProgressPercentage;

        public int _ProgressPercentage
        {
            get { return ProgressPercentage; }
            set
            {
                ProgressPercentage = value;
                OnPropertyChanged();
            }
        }
        private List<string> ProgressStatuses;

        public List<string> _ProgressStatuses
        {
            get { return ProgressStatuses; }
            set
            {
                ProgressStatuses = value;
                OnPropertyChanged();
            }
        }


        public bool ComRegistration { get; set; }
        public IWorker _worker { get; set; }
        public ConfigurationFactory _factory { get; set; }
        public Script _runningScript { get; set; }
        public Machine SelectedMachine { get; set; }
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
            updateVSInstallations();
            MachineClickCommand = new RelayCommand(o => MachineSelectMethod(o as Machine));
        }

        void MachineSelectMethod(Machine a)
        {
            SelectedMachine = a;
            OnRequestClose(this, new EventArgs());
        }
        private void updateVSInstallations()
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

            var AvailableVS = new List<VSInfo>();
            foreach (VSInfo vsInfo in sorted)
            {
                AvailableVS.Add(vsInfo);
            }
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
        }
        void _worker_ProgressStatusChanged(object sender, ProgressStatusChangedArgs e)
        {

            ProgressStatuses.Add(e.Status);
        }
        private void btnExecute_Click()
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

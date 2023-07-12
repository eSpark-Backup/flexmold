using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Setting;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.MVVM.ViewModel.SplashScreen;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using NLog;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using TwinCAT.Ads;

namespace FlexMold.MVVM.ViewModel
{
    internal class SplashScreenViewModel : ObservableObject
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

        private bool _CloseButtonEnable;
        public bool CloseButtonEnable
        {
            get { return _CloseButtonEnable; }
            set
            {
                _CloseButtonEnable = value;
                OnPropertyChanged();
            }
        }

        private Logger log = LogManager.GetLogger("Machine Selection");
        private Logger logtb = LogManager.GetLogger("TwiCAT Builder");
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
                _ProgressStatuses = value + "\n" + ProgressStatuses;
                logtb.Log(LogLevel.Info, $"{value}");
                OnPropertyChanged();
            }
        }

        private bool _ICEnable;

        public bool ICEnable
        {
            get { return _ICEnable; }
            set { _ICEnable = value; OnPropertyChanged(); }
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
            if (!TwinCathelper.LoadSetting())
                Environment.Exit(0);
            ICEnable = true;
            log.Log(LogLevel.Info, $"Flexmold application loading");
            RunButtonEnable = false;
            CloseButtonEnable = true;
            SelectedMachine = new Machine();
            MyMachines = new ObservableCollection<Machine>
            {
                new Machine()
                { ID = 1, Location = "R0", MachineType = EMachineType.Motor, Name = "M1" },
                new Machine()
                { ID = 2, Location = "R1", MachineType = EMachineType.Motor, Name = "M2" },
                new Machine()
                { ID = 3, Location = "R2", MachineType = EMachineType.Projector, Name = "P1" },
                new Machine()
                { ID = 3, Location = "Device Scan", MachineType = EMachineType.ScanDevices, Name = "TwinCat Config" },
            };
            log.Log(LogLevel.Info, $"{MyMachines.Count} Machines Found");

            MachineClickCommand = new RelayCommand(o => MachineSelectMethod(o as Machine));
            RunButtonCommand = new RelayCommand(o =>
            {
                if (Validataion())
                    RunMainWindow();
            });
            CloseButtonCommand = new RelayCommand(o =>
            {
                log.Log(LogLevel.Info, $"Closing ....");
                OnRequestClose(this, new EventArgs());
            });
        }
        private void TwincatConfig()
        {
            if (Validataion())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await DialogHost.Show(new TwinCatConfigView() { DataContext = new TwinCatConfigViewModel() }, "SplashDialog", ClosingEventHandler);
                }));
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            TwinCathelper.TwinCatDiconnectConnect851();
            Console.WriteLine();
        }
        private void MachineSelectMethod(Machine _a)
        {
            SelectedMachine = _a;
            switch (SelectedMachine.MachineType)
            {
                case EMachineType.ScanDevices:
                    log.Log(LogLevel.Info, $"Building Scanning Script");
                    if (MessageBox.Show("Do You Want to build TwinCAT Solution?", "Question",
                      MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        TwincatConfig();
                    }
                    else
                    {
                        SelectedScript = new TcDeviceScanning();
                        ComRegistration = true;
                        updateVSInstallations();

                        string TCVS = "TcXaeShell 15.0";
                        log.Log(LogLevel.Info, $"TwinCAT IDE : {TCVS}");

                        SelectedVS = AvailableVS.Where(a => a.Name == TCVS).FirstOrDefault();

                        if (SelectedVS != null)
                        {
                            RunScript();
                        }
                        else
                        {
                            string msg = $"IDE Not Found Please Install {TCVS}";
                            log.Log(LogLevel.Error, $"{msg}");
                            MessageBox.Show($"{msg}");
                            OnRequestClose(this, new EventArgs());
                        }
                    }
                    break;
                case EMachineType.Motor:
                    log.Log(LogLevel.Info, $"Building Machine Script");
                    if (MessageBox.Show("Do You Want to build TwinCAT Solution?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        if (Validataion())
                            RunMainWindow();
                    }
                    else
                    {
                        SelectedScript = new FlexMoldMachine1();
                        //SelectedScript = new FlexMoldMachineDebug();
                        ComRegistration = true;
                        updateVSInstallations();

                        var TCVS = "TcXaeShell 15.0";
                        log.Log(LogLevel.Info, $"TwinCAT IDE : {TCVS}");

                        SelectedVS = AvailableVS.Where(a => a.Name == TCVS).FirstOrDefault();

                        if (SelectedVS != null)
                        {
                            RunScript();
                        }
                        else
                        {
                            string msg = $"IDE Not Found Please Install {TCVS}";
                            log.Log(LogLevel.Error, $"{msg}");
                            MessageBox.Show($"{msg}");
                            OnRequestClose(this, new EventArgs());
                        }
                    }

                    break;

                case EMachineType.Projector:
                    RunMainWindow();
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
            log.Log(LogLevel.Info, $"Available IDEs :");
            AvailableVS = new List<VSInfo>();
            foreach (VSInfo vsInfo in sorted)
            {
                log.Log(LogLevel.Info, $"\t ProgId: {vsInfo.ProgId}");
                log.Log(LogLevel.Info, $"\t\t Name: {vsInfo.Name}");
                log.Log(LogLevel.Info, $"\t\t Version: {vsInfo.Version}");
                AvailableVS.Add(vsInfo);
            }
        }

        private void RunMainWindow()
        {
            MainWindow mw = new MainWindow(SelectedMachine);
            OnRequestClose(this, new EventArgs());
            mw.Show();
            Application.Current.MainWindow = mw;
        }

        private bool Validataion()
        {
            try
            {
                if (TwinCathelper.TwinCatConnect851())
                {
                    if (TwinCathelper.InitializeSlaves(SelectedMachine.MachineType))
                    {
                        return true;
                    }
                    else
                    {
                        ProgressStatuses = "Slaves initalization failed see logs or loaded different TwinCat project";
                    }
                }
                else
                {
                    ProgressStatuses = $"{AdsErrorCode.DeviceSymbolNotFound}\n" +
                $"TwinCAT Script is not running or TwinCAT is in Config mode \n" +
                $"Please Start/Restart TwinCAT serivce or Build TwinCAT Solution from FCA \n";
                }
                TwinCathelper.TwinCatDiconnectConnect851();
            }
            catch (Exception ex)
            {
                ProgressStatuses = ex.Message;
            }
            return false;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
            switch (SelectedMachine.MachineType)
            {
                case EMachineType.Motor:
                    if (ProgressPercentage == 100 && _worker.IsBuildSucceeded)
                    {
                        if (_worker != null)
                            _ = _worker.CancelAndWait(TimeSpan.FromSeconds(1.0));
                        RunButtonEnable = true;
                    }
                    else if (!_worker.IsBuildSucceeded)
                    {
                        ICEnable = true;
                    }
                    break;
                case EMachineType.Projector:
                    break;
                case EMachineType.ScanDevices:
                    if (ProgressPercentage == 100 && _worker.IsBuildSucceeded)
                    {
                        if (_worker != null)
                            _worker.CancelAndWait(TimeSpan.FromSeconds(1.0));
                        TwincatConfig();
                    }
                    else if (!_worker.IsBuildSucceeded)
                    {
                        ICEnable = true;
                    }
                    break;
                default:
                    break;
            }

        }

        private void Worker_ProgressStatusChanged(object sender, ProgressStatusChangedArgs e) => ProgressStatuses = e.Status;

        public void RunScript()
        {
            ICEnable = false;
            RunButtonEnable = false;
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

                ProgressStatuses = "Closing Opened Instances of current TwinCAT sln";
                //Access COM Running Objects Table
                ROTAccess.CloseRunningDTETable(_runningScript.ScriptRootFolder + @"\" + _runningScript.ScriptName + ".sln");

                _worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);

                _worker.ProgressStatusChanged += new EventHandler<ProgressStatusChangedArgs>(Worker_ProgressStatusChanged);

#if OLD
                _factory.AppID = this.DTEInfo.ProgId;
#else
                _factory.VSInfo = SelectedVS;
#endif
                _factory.IsIdeVisible = false;
                _factory.IsIdeUserControl = false;
                _factory.SuppressUI = true;
                _worker.BeginScriptExecution();
            }
        }
    }
}
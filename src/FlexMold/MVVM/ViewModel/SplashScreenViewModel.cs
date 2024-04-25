using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View;
using FlexMold.MVVM.View.Setting;
using FlexMold.MVVM.ViewModel.SplashScreen;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using TwinCAT.Ads;

namespace FlexMold.MVVM.ViewModel
{
    public class SplashScreenViewModel : ObservableObject, ISplashScreenViewModel
    {
        public VSInfo SelectedVS { get; set; }
        private bool _RunButtonEnable;
        private object Prog_o;
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
        private FlexMoldMachine1 _FlexMoldMachine1 { get { return App.AppHost.Services.GetRequiredService<FlexMoldMachine1>(); } }
        private TcDeviceScanning _TcDeviceScanning { get { return App.AppHost.Services.GetRequiredService<TcDeviceScanning>(); } }
        private MainWindow _mainWindow { get { return App.AppHost.Services.GetRequiredService<MainWindow>(); } }
        private ITwinCatConfigViewModel _TwinCatConfigViewModel { get { return App.AppHost.Services.GetRequiredService<ITwinCatConfigViewModel>(); } }
        private ITwinCathelper _TwinCathelper { get { return App.AppHost.Services.GetRequiredService<ITwinCathelper>(); } }
        public event EventHandler OnRequestClose;
        public event EventHandler OnRequestCollapsed;

        private void TwincatConfig()
        {
            if (Validataion())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await DialogHost.Show(new TwinCatConfigView() { DataContext = _TwinCatConfigViewModel }, "SplashDialog", ClosingEventHandler);
                }));
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            bool isclose = false;
            try { isclose = ((MaterialDesignThemes.Wpf.DialogHost)(sender)).Identifier.ToString().Contains("SplashDialog"); }catch{ }

            if(isclose)
                GetPriviledgeUp();
            else
                _TwinCathelper.TwinCatDiconnectConnect851();
            //Console.WriteLine();
        }

        private void GetPriviledgeUp()
        {
            FlexMoldDB.InitializeFlexMoldDB();
            if (_PriveledgeUpView.LP_ZLaserIPSelect.Text.Length < 1)
            {
                FlexMoldDB.IsEditingEnabled = false;
                MachineSelectMethod(Prog_o as Machine);
                log.Log(LogLevel.Info, $"Logged In as Normal user!");
                return;
            }

            if (FlexMoldDB.checkAdminPriv(_PriveledgeUpView.LP_ZLaserIPSelect.Text))
            {
                String dispmsg = "Now you are in Admin mode " + Environment.NewLine
                                +"Be Carefull when Updating values as it may effect on performance!";
                MessageBox.Show(dispmsg, "Priveledge Mode Enabled!", MessageBoxButton.OK);
                log.Log(LogLevel.Info, $"Logged In as Admin!");
                FlexMoldDB.IsEditingEnabled = true;
                OnPropertyChanged();
                //UpdateSettingsDB();

                MachineSelectMethod(Prog_o as Machine);
            }
            else
            {
                MessageBox.Show("Please try again!", "Incorrect Password",MessageBoxButton.OK);
                FlexMoldDB.IsEditingEnabled = false;
            }

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
                        SelectedScript = _TcDeviceScanning;
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
                        SelectedScript = _FlexMoldMachine1;
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
            try
            {
                OnRequestCollapsed(this, new EventArgs());
                Application.Current.MainWindow = _mainWindow;
                _mainWindow.Show();
            }
            catch(Exception ee) {
                log.Log(LogLevel.Error, $"MAIN Application Exception = "+ee.Message);
                MessageBox.Show(ee.Message, "MAIN Application Exception");
            }
        }

        private bool Validataion()
        {
            try
            {
                if (_TwinCathelper.TwinCatConnect851())
                {
                    if (_TwinCathelper.InitializeSlaves(SelectedMachine.MachineType))
                    {
                        return true;
                    }
                    else
                    {
                        ProgressStatuses = "Slaves initalization failed see logs or Rebuild TwinCat Project";
                    }
                }
                else
                {
                    ProgressStatuses = $"{AdsErrorCode.DeviceSymbolNotFound}\n" +
                $"TwinCAT Script is not running or TwinCAT is in Config mode \n" +
                $"Please Start/Restart TwinCAT serivce or Build TwinCAT Solution from FCA \n";
                }
                _TwinCathelper.TwinCatDiconnectConnect851();
            }
            catch (Exception ex)
            {
                ProgressStatuses = ex.Message;
            }
            return false;
        }
        /// <summary>
        /// Worker Progeress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ApplicationException"></exception>
        public void RunScript()
        {
            try { 
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

                if (!File.Exists(_runningScript.ScriptRootFolder + @"\" + _runningScript.ScriptName + ".sln"))
                {
                    //throw new ApplicationException("Twincat solution"+ _runningScript.ScriptName + ".sln not found!");
                    MessageBox.Show("Twincat solution" + _runningScript.ScriptName + ".sln not found!","Twincat config error");
                    return;
                }

                try { 
                //Access COM Running Objects Table
                ROTAccess.CloseRunningDTETable(_runningScript.ScriptRootFolder + @"\" + _runningScript.ScriptName + ".sln");
                }
                catch (Exception ex)
                {
                    var log = LogManager.GetLogger("Machine Selection");
                    //log.Log(LogLevel.Error, "SplashScreenViewModel>>>" + ex.Message);
                }

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
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Machine Selection");
                //log.Log(LogLevel.Error, "SplashScreenViewModel-22222222>>>" + ex.Message);
            }

        }
        public SplashScreenViewModel()
        {
            try { 
            if (!_TwinCathelper.LoadSetting())
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

            MachineClickCommand = new RelayCommand(async o => {
                
                await PriveledgeUpView();
                Prog_o = o;

                //MachineSelectMethod(o as Machine);
            
            });
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
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Machine Selection");
                log.Log(LogLevel.Error, ex.Message);
            }

        }



        private PriveledgeUpView _PriveledgeUpView { get { return App.AppHost.Services.GetRequiredService<PriveledgeUpView>(); } }

        private async Task PriveledgeUpView()
        {
            Logger log = LogManager.GetLogger("Please enter admin password!");
            try
            {
                //string SelectedZlaserIP = "";
                //log.Log(LogLevel.Info, $"Current port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
                //log.Log(LogLevel.Info, $"Please Select Port params from available list ");
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    DialogHost.Show(_PriveledgeUpView, "SplashDialog", OpenEventHandler, ClosingEventHandler);
                }));

                //log.Log(LogLevel.Info, $"Selected laser projector {_PriveledgeUpView.LP_ZLaserIPSelect}");
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
                TNF.ShowError(ex.Message);
            }
        }
        private void OpenEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            var src = eventArgs.Source;
        }
    }
}
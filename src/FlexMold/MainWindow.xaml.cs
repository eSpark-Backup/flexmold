using FlexMold.MVVM.Model;
using FlexMold.MVVM.View;
using FlexMold.MVVM.ViewModel;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using NLog;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using TwinCAT;
using TwinCAT.Ads;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Logger log = LogManager.GetLogger("Main");

        //MainViewModel _vm;
        public MainWindow(Machine SelectedMachine)
        {
            InitializeComponent();
            Version v = GetRunningVersion();
            string About = string.Format(CultureInfo.InvariantCulture, @"YourApp Version {0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            this.Title = "FlexMold V:" + GetRunningVersion().ToString();
            //_vm = DataContext as MainViewModel;
            //_vm.OnRequestClose += (s, e) => this.Close();
            //Unloaded += OnUnload;
            switch (SelectedMachine.MachineType)
            {
                case EMachineType.Motor:
                    TwinCathelper.InitializeNotifications();
                    TwinCathelper.TSession_ConnectionStateChanged += Session_ConnectionStateChanged;
                    TwinCathelper.TConnection_ConnectionStatusChanged += Connection_ConnectionStatusChanged;
                    TwinCathelper.TConnection_RouterStateChanged += Connection_RouterStateChanged;
                    TwinCathelper.TConnection_AdsStateChanged += Connection_AdsStateChanged;
                    break;

                case EMachineType.Projector:
                    break;

                default:
                    break;
            }
            logCtrl.ItemAdded += OnLogMessageItemAdded;
            log.Log(LogLevel.Info, $"Application Loaded Sucessfully.....");
            log.Log(LogLevel.Info, $"Selected Machine Type : {SelectedMachine.MachineType} Name : {SelectedMachine.Name} ID : {SelectedMachine.ID}");
        }

        //private void OnUnload(object sender, RoutedEventArgs e)
        //{
        //    _vm.OnUnloaded();
        //}

        private static Version GetRunningVersion()
        {
            try
            {
                return Assembly.GetEntryAssembly().GetName().Version;
            }
            catch (Exception)
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        private void Session_ConnectionStateChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });           
        }

        private void Connection_AdsStateChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
        }

        private void Connection_RouterStateChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
        }

        private void Connection_ConnectionStatusChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });            
        }

        private void TwinCATSystemUpdate()
        {
            if (TwinCathelper.Client != null && TwinCathelper.Client.IsConnected)
            {
                bt_TCC.Content = TwinCathelper.Client.State.ToString();
                TwinCathelper.CurrentConnectionState = TwinCathelper.Client.State;
                switch (TwinCathelper.Client.State)
                {
                    case ConnectionState.Connected:
                        bt_TCC.Background = new SolidColorBrush(Colors.LightGreen);
                        bt_TCC.Foreground = new SolidColorBrush(Colors.Black);
                        break;

                    case ConnectionState.None:
                    case ConnectionState.Disconnected:
                    case ConnectionState.Lost:
                        bt_TCC.Background = new SolidColorBrush(Colors.Red);
                        bt_TCC.Foreground = new SolidColorBrush(Colors.White);
                        ConnectionLost("Connection State Lost");
                        break;

                    default:
                        break;
                }
                var info = TwinCathelper.GetTwincatState();
                bt_TCS.Content = info.AdsState.ToString();
                switch (info.AdsState)
                {
                    case AdsState.Run:
                        bt_TCS.Background = new SolidColorBrush(Colors.DarkGreen);
                        bt_TCS.Foreground = new SolidColorBrush(Colors.White);
                        break;

                    case AdsState.Config:
                    case AdsState.Stop:
                    case AdsState.Error:
                    case AdsState.Invalid:
                    case AdsState.Idle:
                    case AdsState.Reset:
                    case AdsState.Init:
                    case AdsState.Start:
                    case AdsState.SaveConfig:
                    case AdsState.LoadConfig:
                    case AdsState.PowerFailure:
                    case AdsState.PowerGood:
                    case AdsState.Shutdown:
                    case AdsState.Suspend:
                    case AdsState.Resume:
                    case AdsState.Reconfig:
                    case AdsState.Stopping:
                    case AdsState.Incompatible:
                    case AdsState.Exception:
                        bt_TCS.Background = new SolidColorBrush(Colors.DarkRed);
                        bt_TCS.Foreground = new SolidColorBrush(Colors.White);
                        ConnectionLost("Ads State Lost");
                        break;
                }
            }
            else
            {
                bt_TCC.Content = ConnectionState.None.ToString();
                bt_TCC.Content = AdsState.Invalid.ToString();
            }
        }

        private async void ConnectionLost(string _msg)
        {
            //var view = new ConnectionLostDialog()
            //{
            //    DataContext = new ConnectionLostViewModel(_msg)
            //};
            //var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
        }

        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= LogLevel.Error)
                SystemSounds.Beep.Play();
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (TwinCathelper.Client != null)
            {
                TwinCathelper.Client.Close();
            }
        }
    }
}
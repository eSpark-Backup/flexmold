using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using TwinCAT;
using TwinCAT.Ads;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Logger log = LogManager.GetLogger("Main");
        //MainViewModel _vm;
        public MainWindow(Machine SelectedMachine)
        {
            InitializeComponent();
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string About = string.Format(CultureInfo.InvariantCulture, @"YourApp Version {0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            this.Title = "FlexMold V:" + getRunningVersion().ToString();
            //_vm = DataContext as MainViewModel;
            //_vm.OnRequestClose += (s, e) => this.Close();
            //Unloaded += OnUnload;
            logCtrl.ItemAdded += OnLogMessageItemAdded;
            TwinCathelper._session.ConnectionStateChanged += session_ConnectionStateChanged;
            TwinCathelper._connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
            TwinCathelper._connection.RouterStateChanged += _connection_RouterStateChanged;
            TwinCathelper._connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
            log.Log(LogLevel.Info, $"Application Loaded Sucessfully.....");
            log.Log(LogLevel.Info, $"Selected Machine Type : {SelectedMachine.MachineType} Name : {SelectedMachine.Name} ID : {SelectedMachine.ID}");
            
        }
        //private void OnUnload(object sender, RoutedEventArgs e)
        //{
        //    _vm.OnUnloaded();
        //}

        private Version getRunningVersion()
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
        private void session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
        }

        void _connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
            TwinCathelper.MyAdsState = e.State.AdsState;
            log.Log(LogLevel.Info, $"AdsState : {TwinCathelper.MyAdsState}");
        }

        void _connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
            TwinCathelper.MyAmsRouterState = e.State;
            log.Log(LogLevel.Info, $"Router State : {TwinCathelper.MyAmsRouterState}");
        }

        void _connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
            TwinCathelper.MyConnectionStateOld = e.OldState;
            TwinCathelper.MyConnectionStateNew = e.NewState;
            TwinCathelper.MyConnectionStateChangedReason = e.Reason;
            log.Log(LogLevel.Info, $"Connection Status Old : {TwinCathelper.MyConnectionStateOld}");
            log.Log(LogLevel.Info, $"Connection Status New : {TwinCathelper.MyConnectionStateNew}");
            log.Log(LogLevel.Info, $"Connection Status Reason : {TwinCathelper.MyConnectionStateChangedReason}");
        }

        void _session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TwinCATSystemUpdate();
            });
            TwinCathelper.MyConnectionStateOld = e.OldState;
            TwinCathelper.MyConnectionStateNew = e.NewState;
            TwinCathelper.MyConnectionStateChangedReason = e.Reason;
            log.Log(LogLevel.Info, $"Connection State Old : {TwinCathelper.MyConnectionStateOld}");
            log.Log(LogLevel.Info, $"Connection State New : {TwinCathelper.MyConnectionStateNew}");
            log.Log(LogLevel.Info, $"Connection State Reason : {TwinCathelper.MyConnectionStateChangedReason}");
        }
        private void TwinCATSystemUpdate()
        {

            if (TwinCathelper._connection != null)
            {
                tBConnectionState.Text = TwinCathelper._connection.State.ToString();

                switch (TwinCathelper._connection.State)
                {
                    case ConnectionState.None:
                        tBConnectionState.Background = new SolidColorBrush();
                        break;
                    case ConnectionState.Disconnected:
                        borCon.Background = new SolidColorBrush(Colors.LightGray);
                        tBConnectionState.Foreground = new SolidColorBrush(Colors.Black);
                        break;
                    case ConnectionState.Connected:
                        borCon.Background = new SolidColorBrush(Colors.LightGreen);
                        tBConnectionState.Foreground = new SolidColorBrush(Colors.Black);
                        break;
                    case ConnectionState.Lost:
                        borCon.Background = new SolidColorBrush(Colors.Yellow);
                        tBConnectionState.Foreground = new SolidColorBrush(Colors.Black);
                        break;
                    default:
                        break;
                }
                var info = TwinCathelper.GetTwincatState();

                tBAdsState.Text = info.AdsState.ToString();

                switch (info.AdsState)
                {
                    case AdsState.Run:
                        borState.Background = new SolidColorBrush(Colors.DarkGreen);
                        tBAdsState.Foreground = new SolidColorBrush(Colors.White);

                        break;
                    case AdsState.Stop:
                    case AdsState.Error:
                        borState.Background = new SolidColorBrush(Colors.DarkRed);
                        tBAdsState.Foreground = new SolidColorBrush(Colors.White);

                        break;
                    case AdsState.Config:
                        borState.Background = new SolidColorBrush(Colors.DarkBlue);
                        tBAdsState.Foreground = new SolidColorBrush(Colors.White);
                        break;
                }
            }
            else
            {
                tBConnectionState.Text = ConnectionState.None.ToString();
                tBAdsState.Text = AdsState.Invalid.ToString();
            }
        }
        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= LogLevel.Error)
                SystemSounds.Beep.Play();
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    _vm.ShowInformation("sdfsdf");
        //}
    }
}

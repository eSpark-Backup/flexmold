using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using NLog;
using NLog.Fluent;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Machine SelectedMachine { get; set; }
        Logger log = LogManager.GetLogger("Main");
        public MainWindow()
        {

            /// Test Area please comment

            /// Region closed 

            ShowMachineSlectionWindow();
            
            switch (SelectedMachine.MachineType)
            {
                case EMachineType.Motor:
                    TwinCathelper.TwinCatConnect();
                    TwinCathelper._session.ConnectionStateChanged += session_ConnectionStateChanged;
                    TwinCathelper._connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
                    TwinCathelper._connection.RouterStateChanged += _connection_RouterStateChanged;

                    // Add the Notification event handler
                    try
                    {
                        TwinCathelper._connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    if (TwinCathelper._connection.IsConnected)
                    {
                        if (TwinCathelper._connection.IsActive)
                        {
                            if (TwinCathelper.MySymbols.Count == 0)
                            {
                                MessageBox.Show("Twincat Project Not Login or Properly Configured");
                                this.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Twicat Is Not Active");
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Twicat Not Connected");
                        this.Close();
                    }
                    break;
                case EMachineType.Projector:
                    break;
                default:
                    break;
            }

            InitializeComponent();
            logCtrl.ItemAdded += OnLogMessageItemAdded;
            log.Log(LogLevel.Info, $"Application Loaded Scussfully.....");
            log.Log(LogLevel.Info, $"Selected Machine Type : {SelectedMachine.MachineType} Name : {SelectedMachine.Name} ID : {SelectedMachine.ID}");
            Console.WriteLine();
        }
        private void session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
            });
        }

         void _connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
            });
            TwinCathelper.MyAdsState = e.State.AdsState;
            log.Log(LogLevel.Info, $"AdsState : {TwinCathelper.MyAdsState}");
        }

         void _connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
            });
            TwinCathelper.MyAmsRouterState = e.State;
            log.Log(LogLevel.Info, $"Router State : {TwinCathelper.MyAmsRouterState}");
        }

         void _connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update();
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
                Update();
            });
            TwinCathelper.MyConnectionStateOld = e.OldState;
            TwinCathelper.MyConnectionStateNew = e.NewState;
            TwinCathelper.MyConnectionStateChangedReason = e.Reason;
            log.Log(LogLevel.Info, $"Connection State Old : {TwinCathelper.MyConnectionStateOld}");
            log.Log(LogLevel.Info, $"Connection State New : {TwinCathelper.MyConnectionStateNew}");
            log.Log(LogLevel.Info, $"Connection State Reason : {TwinCathelper.MyConnectionStateChangedReason}");
        }
        private void Update()
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


                if (TwinCathelper._connection.IsConnected)
                {
                    StateInfo info = new StateInfo();
                    AdsErrorCode errorCode = TwinCathelper._connection.TryReadState(out info);
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
            }
            else
            {
                tBConnectionState.Text = ConnectionState.None.ToString();
                tBAdsState.Text = AdsState.Invalid.ToString();
            }
        }
        void ShowMachineSlectionWindow()
        {
            SelectedMachine = new Machine();
            try
            {
                var vm = new SplashScreenViewModel();
                var splashscreen = new SplashMachineSlection
                {
                    DataContext = vm
                };
                vm.OnRequestClose += (s, e) => splashscreen.Close();
                splashscreen.ShowDialog();
                if (vm.SelectedMachine.Name == null)
                {
                    ShowMachineSlectionWindow();
                }
                else
                {
                    SelectedMachine = vm.SelectedMachine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= NLog.LogLevel.Error)
                SystemSounds.Beep.Play();
        }

    }
}

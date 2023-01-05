using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows;
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
        Logger log = LogManager.GetLogger("Main");
        public MainWindow(Machine SelectedMachine)
        {
            /// Test Area please comment

            /// Region closed 
            try
            {
                
                TwinCathelper.TwinCatConnect851();
                TwinCathelper._session.ConnectionStateChanged += session_ConnectionStateChanged;
                TwinCathelper._connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
                TwinCathelper._connection.RouterStateChanged += _connection_RouterStateChanged;
                TwinCathelper._connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
                if (TwinCathelper._connection.IsConnected)
                {
                    if (TwinCathelper._connection.IsActive)
                    {
                        if (TwinCathelper.MySymbols.Count > 0 && TwinCathelper.MySymbols.Where(a=>a.InstancePath== TwinCATVariableIfo.TwicatDeviceCountVariableName).Count()>0)
                        {
                            TwinCathelper.DeviceCount = (short)TwinCathelper.MySymbols.Where(a => 
                            a.InstancePath == TwinCATVariableIfo.TwicatDeviceCountVariableName).FirstOrDefault().ReadAnyValue(typeof(short));
                            if (TwinCathelper.DeviceCount > 0)
                            {
                                InitializeComponent();
                                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                                string About = string.Format(CultureInfo.InvariantCulture, @"YourApp Version {0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
                                this.Title = "FlexMold MainDeviceVariableName:" + getRunningVersion().ToString();
                                logCtrl.ItemAdded += OnLogMessageItemAdded;
                                log.Log(LogLevel.Info, $"Application Loaded Sucessfully.....");
                                log.Log(LogLevel.Info, $"Selected Machine Type : {SelectedMachine.MachineType} Name : {SelectedMachine.Name} ID : {SelectedMachine.ID}");
                            }
                            else
                            {
                                var tmsg = "No PLC Device Found";
                                log.Log(LogLevel.Info, $"{tmsg}");
                                MessageBox.Show(tmsg);
                                this.Close();
                            }
                        }
                        else
                        {
                            var tmsg = "Twincat Project Not Running or Properly Configured";
                            log.Log(LogLevel.Info, $"{tmsg}");
                            MessageBox.Show(tmsg);
                            this.Close();
                        }
                    }
                    else
                    {
                        var tmsg = "Twicat Is Not Active \n Compile and run TwinCAT Project First";
                        log.Log(LogLevel.Info, $"{tmsg}");
                        MessageBox.Show(tmsg);
                        this.Close();
                    }
                }
                else
                {
                    var tmsg = "Twicat Not Connected";
                    log.Log(LogLevel.Info, $"{tmsg}");
                    MessageBox.Show(tmsg);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Info, $"{ex.Message}");
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
        private Version getRunningVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
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
                if (info != null)
                {
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
        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= LogLevel.Error)
                SystemSounds.Beep.Play();
        }

    }
}

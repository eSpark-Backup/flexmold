using FlexMold.Contols;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.Utility;
using Humanizer;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlexMold.MVVM.ViewModel.Project
{
    class LaserProjectorViewModel : TelnetHelper
    {
        private readonly ProjectViewModel _parent;

        public ProjectViewModel Parent => _parent;
        Logger log;
        public RelayCommand ProjectorModeSelectionCommand { get; set; }
        public RelayCommand SendLasFileCommand { get; set; }
        public RelayCommand SerialConfigCommand { get; set; }
        public RelayCommand SambaConfigCommand { get; set; }
        public RelayCommand ProjectionOnCommand { get; set; }
        public RelayCommand ProjectionOffCommand { get; set; }
        public RelayCommand CallibrateProjectorCommand { get; set; }
        public RelayCommand LaunchLMPCommand { get; set; }
        private int _LaserProjectorComMode;
        public int LaserProjectorComMode
        {
            get { return _LaserProjectorComMode; }
            set
            {

                _LaserProjectorComMode = value;
                switch ((EProjectorComMode)value)
                {
                    case EProjectorComMode.Serial:
                        SerialConfigWindow();
                        break;
                    case EProjectorComMode.Samba:
                        SambaConfigWindow();
                        break;
                    case EProjectorComMode.Telnet:
                        break;
                    default:
                        break;
                }
                OnPropertyChanged();
            }
        }
        public LaserProjector MyPorjector { get; set; }
        private SerialPort _SelectedSerialPort;
        public SerialPort SelectedSerialPort
        {
            get
            {
                return _SelectedSerialPort;
            }
            set
            {
                _SelectedSerialPort = value;
            }
        }
        public LaserProjectorViewModel(ProjectViewModel parent)
        {
            log = LogManager.GetLogger("LaseProjector");
            _parent = parent;
            SendLasFileCommand = new RelayCommand(o => { _ = sendLasFileAsync(Parent.slectedFile); });
            SerialConfigCommand = new RelayCommand(o => { SerialConfigWindow(); });
            SambaConfigCommand = new RelayCommand(o => { SambaConfigWindow(); });
            ProjectionOnCommand = new RelayCommand(async o =>
            {
                if (MyTelnetClient == null)
                {
                    if (await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort))
                    {
                        SendMessage = "ZN; " + "\r\n";
                        _ = SendTelnetMessgae();
                    }
                    else
                    {
                        log.Log(LogLevel.Error, ErrorMessages.Connection_Failure);
                    }
                }
            });
            ProjectionOffCommand = new RelayCommand(async o =>
            {
                if (MyTelnetClient == null)
                {
                    if (await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort))
                    {
                        SendMessage = "ZF; " + "\r\n";
                        _ = SendTelnetMessgae();
                    }
                    else
                    {
                        log.Log(LogLevel.Error, ErrorMessages.Connection_Failure.Humanize());
                    }
                }
            });
            CallibrateProjectorCommand = new RelayCommand(async o =>
            {
                if (MyTelnetClient == null)
                {
                    if (await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort))
                    {
                        SendMessage = "ZD; " + "\r\n";
                        _ = SendTelnetMessgae();
                    }
                    else
                    {
                        log.Log(LogLevel.Error, ErrorMessages.Connection_Failure.Humanize());
                    }
                }
            });
            LaunchLMPCommand = new RelayCommand(o => { DisconnectTelnet(); });
            if (MyPorjector == null)
            {
                MyPorjector = new LaserProjector()
                {
                    Name = "p1",
                    IpAddres = "192.168.50.22",
                    UserName = "123",
                    Status = 1,
                    TelnetPort = 23,
                    Password = "123456",
                };
            }
            LaserProjectorComMode = (int)EProjectorComMode.Telnet;
            if (SelectedSerialPort == null)
            {
                SelectedSerialPort = new SerialPort();
            }
        }
        public async Task sendLasFileAsync(FileInfo _slectedFile)
        {
            try
            {
                if (_slectedFile == null)
                {
                    log.Log(LogLevel.Warn, ErrorMessages.Please_Select_File_First);
                    return;
                }
                if (!_slectedFile.FullName.EndsWith(".las"))
                {
                    log.Log(LogLevel.Warn, $"Selected File Not a .las File {_slectedFile.Name}");
                    return;
                }
                try
                {
                    switch (LaserProjectorComMode)
                    {
                        case (int)EProjectorComMode.Serial:
                            log.Log(LogLevel.Info, ErrorMessages.Sending_File_Via_Serial.Humanize());
                            try
                            {
                                using (FileStream fs = File.OpenRead(_slectedFile.FullName))
                                {
                                    log.Log(LogLevel.Info, $"Opening Port {SelectedSerialPort.PortName} Baud rate {SelectedSerialPort.BaudRate}");
                                    SelectedSerialPort.Open();
                                    SelectedSerialPort.WriteTimeout = 2000;
                                    SelectedSerialPort.Write((new BinaryReader(fs)).ReadBytes((int)fs.Length), 0, (int)fs.Length);
                                    log.Log(LogLevel.Info, $"File Write Sucessfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                ToastNotificationUtil.ShowError(ex.Message);
                                log.Log(LogLevel.Error, ex.Message);
                            }
                            finally
                            {
                                SelectedSerialPort.Close();
                            }
                            break;
                        case (int)EProjectorComMode.Samba:
                            string _tmsg = $"Sending file via samba";
                            log.Log(LogLevel.Info, _tmsg);
                            ToastNotificationUtil.ShowInformation(_tmsg);
                            try
                            {
                                log.Log(LogLevel.Info, $"Connecting to {MyPorjector.IpAddres}");
                                using (NetworkShareAccesser.Access(MyPorjector.IpAddres, MyPorjector.UserName, MyPorjector.Password))
                                {
                                    log.Log(LogLevel.Info, $"Credentials Accepted");
                                    log.Log(LogLevel.Info, $"Copying File");
                                    File.Copy(_slectedFile.FullName,
                                        $@"\\{MyPorjector.IpAddres}\{MyPorjector.PlotFolderName}\{_slectedFile.Name}",
                                        true);
                                    log.Log(LogLevel.Info, $"File Copied Sucessfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                ToastNotificationUtil.ShowError(ex.Message);
                                log.Log(LogLevel.Error, ex.Message);
                            }
                            break;
                        case (int)EProjectorComMode.Telnet:
                            log.Log(LogLevel.Info, $"Sending file via Telnet");
                            try
                            {
                                TelnetHelper th = new TelnetHelper();

                                //th.SendMessage = "echo "; //

                                th.SendMessage += await MixUtil.ReadTextFromFileAsync(_slectedFile.FullName);
                                var ii = await th.ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort);
                                //th.SendMessage = "ss";
                                if (!ii)
                                {
                                    var tmsg = $"Telnet Failed to connect : {MyPorjector.IpAddres} , {MyPorjector.TelnetPort}";
                                    log.Log(LogLevel.Error, tmsg);
                                    ToastNotificationUtil.ShowError(tmsg);
                                    return;
                                }
                                await th.SendTelnetMessgae();
                                th.DisconnectTelnet();
                            }
                            catch (Exception ex)
                            {
                                ToastNotificationUtil.ShowError(ex.Message);
                                log.Log(LogLevel.Error, ex.Message);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(LogLevel.Error, ex.Message);
                    ToastNotificationUtil.ShowError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }

        private void SerialConfigWindow()
        {
            Logger log = LogManager.GetLogger("Serial Port Config");
            try
            {
                log.Log(LogLevel.Info, $"Current port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
                log.Log(LogLevel.Info, $"Please Select Port params from available list ");
                var main = App.Current.MainWindow as MainWindow;
                var sv = new SerialPortConfigView(main, SelectedSerialPort);
                sv.ShowDialog();
                if (sv.SaveData)
                {
                    SelectedSerialPort = sv.CurrentPort;
                    log.Log(LogLevel.Info, $"Saved Port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
                }
                else
                {
                    log.Log(LogLevel.Warn, $"Closing without saving");
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
                ToastNotificationUtil.ShowError(ex.Message);
            }
        }
        private void SambaConfigWindow()
        {
            Logger log = LogManager.GetLogger("Samba Config");
            try
            {
                log.Log(LogLevel.Info, $"Current IP {MyPorjector.IpAddres} UserName {MyPorjector.UserName}");
                log.Log(LogLevel.Info, $"Please Enter Samba params");
                var main = App.Current.MainWindow as MainWindow;
                var sv = new SambaConfigWindow(main, MyPorjector);
                sv.ShowDialog();
                if (sv.SaveData)
                {
                    MyPorjector = sv.CurrentProjector;
                    log.Log(LogLevel.Info, $"Saved IP {MyPorjector.IpAddres}");
                    log.Log(LogLevel.Info, $"Saved PlotFolderName {MyPorjector.PlotFolderName}");
                    log.Log(LogLevel.Info, $"Saved UserName {MyPorjector.UserName}");
                }
                else
                {
                    log.Log(LogLevel.Warn, $"Closing without saving");
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex, ex.Message);
                ToastNotificationUtil.ShowError(ex.Message);
            }
        }
    }
}

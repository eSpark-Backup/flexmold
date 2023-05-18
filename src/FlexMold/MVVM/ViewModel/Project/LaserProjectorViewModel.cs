using FlexMold.Contols;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.Utility;
using Humanizer;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Project
{
    internal class LaserProjectorViewModel : TelnetHelper
    {
        private readonly ProjectViewModel _parent;

        public ProjectViewModel Parent => _parent;
        private Logger log;
        public RelayCommand ProjectorModeSelectionCommand { get; set; }
        public RelayCommand SendLasFileCommand { get; set; }
        public RelayCommand SerialConfigCommand { get; set; }
        public RelayCommand SambaConfigCommand { get; set; }
        public RelayCommand ProjectionOnCommand { get; set; }
        public RelayCommand LaunchLPMCommand { get; set; }
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

        private ToastNotificationUtil _TstN;

        public LaserProjectorViewModel(ProjectViewModel parent)
        {
            log = LogManager.GetLogger("LaseProjector");
            _TstN = new ToastNotificationUtil();
            _parent = parent;
            SendLasFileCommand = new RelayCommand(o => { _ = sendLasFileAsync(Parent.slectedFile); });
            SerialConfigCommand = new RelayCommand(o => { SerialConfigWindow(); });
            LaunchLPMCommand = new RelayCommand(o =>
            {
                Process secondProc = new Process();
                secondProc.StartInfo.FileName = @"C:\Z-LASER\LPM 9.0\LPM.exe";
                secondProc.Start();
            });
            SambaConfigCommand = new RelayCommand(o => { SambaConfigWindow(); });
            ProjectionOnCommand = new RelayCommand(async o =>
            {
                if (ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort) == null)
                {
                    var ii = await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort);
                    if (!ii)
                    {
                        var tmsg = $"Telnet Failed to connect : {MyPorjector.IpAddres} , {MyPorjector.TelnetPort}";
                        log.Log(LogLevel.Error, tmsg);
                        _TstN.ShowError(tmsg);
                        return;
                    }
                }
                else
                {
                    SendMessage = "ZN; " + "\r\n";
                    _ = SendTelnetMessgae();
                }
            });
            ProjectionOffCommand = new RelayCommand(async o =>
            {
                if (ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort) == null)
                {
                    var ii = await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort);
                    if (!ii)
                    {
                        var tmsg = $"Telnet Failed to connect : {MyPorjector.IpAddres} , {MyPorjector.TelnetPort}";
                        log.Log(LogLevel.Error, tmsg);
                        _TstN.ShowError(tmsg);
                        return;
                    }
                }
                else
                {
                    SendMessage = "ZF; " + "\r\n";
                    _ = SendTelnetMessgae();
                }
            });
            CallibrateProjectorCommand = new RelayCommand(async o =>
            {
                if (ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort) == null)
                {
                    var ii = await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort);
                    if (!ii)
                    {
                        var tmsg = $"Telnet Failed to connect : {MyPorjector.IpAddres} , {MyPorjector.TelnetPort}";
                        log.Log(LogLevel.Error, tmsg);
                        _TstN.ShowError(tmsg);
                        return;
                    }
                }
                else
                {
                    SendMessage = "ZD; " + "\r\n";
                    _ = SendTelnetMessgae();
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
                                _TstN.ShowError(ex.Message);
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
                            _TstN.ShowInformation(_tmsg);
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
                                _TstN.ShowError(ex.Message);
                                log.Log(LogLevel.Error, ex.Message);
                            }
                            break;

                        case (int)EProjectorComMode.Telnet:
                            log.Log(LogLevel.Info, $"Sending file via Telnet");
                            try
                            {
                                if (ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort) == null)
                                {
                                    var ii = await ConnectTelnet(MyPorjector.IpAddres, MyPorjector.TelnetPort);
                                    if (!ii)
                                    {
                                        var tmsg = $"Telnet Failed to connect : {MyPorjector.IpAddres} , {MyPorjector.TelnetPort}";
                                        log.Log(LogLevel.Error, tmsg);
                                        _TstN.ShowError(tmsg);
                                        return;
                                    }
                                }
                                else
                                {
                                    SendMessage = await MixUtil.ReadTextFromFileAsync(_slectedFile.FullName);
                                    _ = SendTelnetMessgae();
                                    return;
                                }
                                return;
                            }
                            catch (Exception ex)
                            {
                                _TstN.ShowError(ex.Message);
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
                    _TstN.ShowError(ex.Message);
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
                _TstN.ShowError(ex.Message);
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
                _TstN.ShowError(ex.Message);
            }
        }
    }
}
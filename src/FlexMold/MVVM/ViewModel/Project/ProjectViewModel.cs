using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.Utility;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Project
{
    class ProjectViewModel : ObservableObject
    {
        Logger log = LogManager.GetLogger("Project");
        public RelayCommand ReadCSVFileCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        public RelayCommand ProjectorModeSelectionCommand { get; set; }
        public RelayCommand SendLasFileCommand { get; set; }
        public RelayCommand SerialConfigCommand { get; set; }
        public RelayCommand SambaConfigCommand { get; set; }

        private int _LaserProjectorComMode;
        public int LaserProjectorComMode
        {
            get { return _LaserProjectorComMode; }
            set
            {
                _LaserProjectorComMode = value;
                OnPropertyChanged();
            }
        }
        public LaserProjector MyPorjector { get; set; }
        private Model.Project _selectedMyDataModel;
        public Model.Project selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                slectedFile = null;
                OnPropertyChanged();
            }
        }
        private Porjects _myDataModels;
        public Porjects myDataModels
        {
            get { return _myDataModels; }
            set
            {
                _myDataModels = value;
                OnPropertyChanged();
            }
        }
        private FileInfo _slectedFile;
        public FileInfo slectedFile
        {
            get { return _slectedFile; }
            set
            {
                _slectedFile = value;
                OnPropertyChanged();
            }
        }
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
        void WriteCSVData()
        {
            var data = readCsvFile();
            if (data.Count != 0)
            {
                if (TwinCathelper.PLCMotors.Length == data.Count)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        TwinCathelper.PLCMotors[i].mSpeed = (uint)data[i];
                    }
                    TwinCathelper.WriteEthercatDeviceData();
                    log.Log(LogLevel.Info, $"{data.Count} value/s has been written sucessfully");
                }
                else
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV\n " +
                        $"CSV Data & Box count not equal \n " +
                        $"Data Count : {data.Count} | Device Count : {TwinCathelper.PLCMotors.Length}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
            }
        }
        public ProjectViewModel()
        {
            ReadCSVFileCommand = new RelayCommand(o => { WriteCSVData(); });
            SendLasFileCommand = new RelayCommand(o => { _ = sendLasFileAsync(slectedFile); });
            SerialConfigCommand = new RelayCommand(o => { SerialConfigWindow(); });
            SambaConfigCommand = new RelayCommand(o => { SambaConfigWindow(); });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
            if (MyPorjector == null)
            {
                MyPorjector = new LaserProjector()
                {
                    Name = "p1",
                    IpAddres = "telehack.com",
                    UserName = "123",
                    Status = 1,
                    TelnetPort = 23,
                    Password = "123456",
                };
            }
            LaserProjectorComMode = 0;
            if (SelectedSerialPort == null)
            {
                SelectedSerialPort = new SerialPort();
            }
        }

        List<double> readCsvFile()
        {
            var data = new List<double>();
            if (slectedFile != null)
            {
                if (slectedFile.FullName.EndsWith(".csv"))
                {
                    log.Log(LogLevel.Info, $"Reading CSV File {slectedFile.Name}");
                    try
                    {
                        //var csv = @"C:\Users\123\Downloads\Sylvan-main\Sylvan-main\source\Sylvan.Data.Csv.Tests\Data\Quote.csv";
                        //var csv = @"D:\smhraza\Projects\FLEXMOLD\flexmolddata\FlexMain\Projects\M&M\S3.2\S3.2.csv";
                        using (var reader = new StreamReader(slectedFile.FullName))
                        {
                            var options = new CsvDataReaderOptions
                            {
                                HasHeaders = false,
                                BufferSize = 0x10000,
                            };
                            var csvr = CsvDataReader.Create(reader, options);

                            while (csvr.Read())
                            {
                                for (int i = 0; i < csvr.FieldCount; i++)
                                {
                                    data.Add(csvr.GetDouble(i)/2);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex.Message);
                        ToastNotificationUtil.ShowError(ex.Message);
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .csv File {slectedFile.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select File First");
            return data;
        }
        void BrowseProject()
        {
            Logger log = LogManager.GetLogger("Browse Project");
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    log.Log(LogLevel.Info, $"Select Project Folder");
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        myDataModels = new Porjects();
                        //string selectedPath = @"D:\smhraza\Projects\FLEXMOLD\flexmolddata\FlexMain\Projects\";
                        string selectedPath = dialog.SelectedPath;
                        Regex reg = new Regex(@".las$|.csv$");
                        DirectoryInfo di = new DirectoryInfo(selectedPath);
                        var laserFilesPath = di.GetFiles("*", SearchOption.AllDirectories)
                                 .Where(path => reg.IsMatch(path.FullName))
                                 .ToList();
                        var panels = laserFilesPath.GroupBy(p => p.Directory.Name);
                        var projectsTemp = laserFilesPath.GroupBy(p => p.Directory.Parent.Name);
                        log.Log(LogLevel.Info, $"{projectsTemp.Count()} Projects Found");
                        log.Log(LogLevel.Info, $"{panels.Count()} Panels Found");
                        log.Log(LogLevel.Info, $"{laserFilesPath.Count()} Files Found");
                        foreach (var item in panels)
                        {
                            myDataModels.Add(new Model.Project()
                            {
                                Panel = item.Key,
                                ProjectName = item.FirstOrDefault().Directory.Parent.Name,
                                LaserFile = item.ToList(),
                                Complete = false
                            });
                        }
                    }
                    else
                        log.Log(LogLevel.Warn, $"User Cancelled Project Browser");
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }
        public async Task sendLasFileAsync(FileInfo _slectedFile)
        {
            Logger log = LogManager.GetLogger("Las File Send");
            try
            {
                if (_slectedFile != null)
                {
                    if (_slectedFile.FullName.EndsWith(".las"))
                    {
                        try
                        {
                            switch (LaserProjectorComMode)
                            {
                                case (int)EProjectorComMode.Serial:
                                    log.Log(LogLevel.Info, $"Sending file via serial");
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
                    else
                        log.Log(LogLevel.Warn, $"Selected File Not a .las File {_slectedFile.Name}");
                }
                else
                    log.Log(LogLevel.Warn, $"Please Select File First");
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
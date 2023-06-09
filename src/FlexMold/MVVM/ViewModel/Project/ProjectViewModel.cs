using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.MVVM.View.Setting;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Project
{
    internal class ProjectViewModel : ObservableObject
    {
        private Logger log = LogManager.GetLogger("Project");
        public RelayCommand SendCSVCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        public RelayCommand StopMotorsCommand { get; set; }
        public int SelectedMotorDirValue;
        private Model.Project _selectedMyDataModel;

        public Model.Project selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                SelectedFile = null;
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

        private FileInfo _selectedFile;

        public FileInfo SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
            }
        }

        private ToastNotificationUtil _TstN;

        public LaserProjectorViewModel lVM { get; set; }

        private void WriteCSVData(IList<double> data)
        {
            if (data.Count != 0)
            {
                if (TwinCathelper.PLCMotors.Length == data.Count)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        TwinCathelper.PLCMotors[i].mDisplacement = (uint)data[i] * 1600;
                        TwinCathelper.PLCMotors[i].mDirection = SelectedMotorDirValue == 0 ? false : true;
                    }
                    TwinCathelper.WriteEthercatDeviceData();
                    log.Log(LogLevel.Info, $"{data.Count} csv value/s has been written sucessfully");

                    Thread.Sleep(200);

                    for (int i = 0; i < data.Count; i++)
                        TwinCathelper.PLCMotors[i].mDisplacement = 0;
                    TwinCathelper.WriteEthercatDeviceData();
                    log.Log(LogLevel.Info, $"{data.Count} zero value/s has been written sucessfully");
                }
                else
                {
                    log.Log(LogLevel.Error, $"Please select correct CSV");
                    log.Log(LogLevel.Error, $"Data and Box count are not equal");
                    log.Log(LogLevel.Error, $"Data Count : {data.Count} | Device Count : {TwinCathelper.PLCMotors.Length}");
                    _TstN.ShowError($"CSV Failed\nDevice:{TwinCathelper.PLCMotors.Length} != Data:{data.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                _TstN.ShowError($"CSV Failed");
            }
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine();
        }

        private List<double> StopCsv(int dataLength)
        {
            var data = new List<double>();
            for (int i = 0; i < dataLength; i++)
            {
                data.Add(0);
            }
            return data;

        }
        private List<double> readCsvFile(FileInfo filename)
        {
            var data = new List<double>();
            if (filename != null)
            {
                if (SelectedFile.FullName.EndsWith(".csv"))
                {
                    log.Log(LogLevel.Info, $"Reading CSV File {filename.Name}");
                    try
                    {
                        using (var reader = new StreamReader(SelectedFile.FullName))
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
                                    data.Add(csvr.GetDouble(i));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex.Message);
                        _TstN.ShowError(ex.Message);
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .csv File {SelectedFile.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select File First");
            log.Info($"CSV data : {JsonConvert.SerializeObject(data)}");
            return data;
        }
        private void BrowseProject()
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
        public ProjectViewModel()
        {
            _TstN = new ToastNotificationUtil();
            lVM = new LaserProjectorViewModel(this);
            SendCSVCommand = new RelayCommand(async o =>
            {
                var csvdlgvm = new CSVDiaglogViewModel();
                var view = new CSVDiaglog() { DataContext = csvdlgvm };
                var res = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                SelectedMotorDirValue = csvdlgvm.SelectedMotorDirPValue;
                string dir =  SelectedMotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";
                log.Log(LogLevel.Info, $"Selected Direction : {dir}");
                WriteCSVData(readCsvFile(SelectedFile));
            });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
            StopMotorsCommand = new RelayCommand(o =>
            {
                log.Log(LogLevel.Warn, $"Stopping Motors");
                WriteCSVData(StopCsv(TwinCathelper.DeviceCount));
            });
        }
    }
}
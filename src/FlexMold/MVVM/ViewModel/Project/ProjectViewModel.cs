using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlexMold.MVVM.ViewModel.Project
{
    class ProjectViewModel : ObservableObject
    {
        Logger log = LogManager.GetLogger("Project");
        public RelayCommand ReadCSVFileCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
     
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
        private ToastNotificationUtil _TstN;
        public LaserProjectorViewModel lVM { get; set; }
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
                    _TstN.ShowError($"CSV Failed\nDevice:{TwinCathelper.PLCMotors.Length} Data:{data.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                _TstN.ShowError($"CSV Failed");
            }
        }
        public ProjectViewModel()
        {
            _TstN =  new ToastNotificationUtil();
            lVM = new LaserProjectorViewModel(this);
            ReadCSVFileCommand = new RelayCommand(o => { WriteCSVData(); });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
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
                                    data.Add(csvr.GetDouble(i) / 2);
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
      
    }
}
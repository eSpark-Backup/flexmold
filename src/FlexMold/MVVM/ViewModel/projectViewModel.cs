using FlexMold.Core;
using FlexMold.MVVM.Model;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel
{

    class ProjectViewModel : ObservableObject
    {
        public RelayCommand ReadCSVFileCommand { get; set; }
        public RelayCommand SendLasFileCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        private MyDataModel _selectedMyDataModel;
        public MyDataModel selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                slectedFile = null;
                OnPropertyChanged();
            }
        }
        private MyDataModels _myDataModels;

        public MyDataModels myDataModels
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
        void readCsvFile()
        {
            Logger log = LogManager.GetLogger("CSV File Reader");
            if (slectedFile != null)
            {
                if (slectedFile.FullName.EndsWith(".csv"))
                {
                    log.Log(LogLevel.Info, $"Reading File {slectedFile.Name}");
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
                            var data = new List<double>();
                            while (csvr.Read())
                            {
                                for (int i = 0; i < csvr.FieldCount; i++)
                                {
                                    data.Add(Math.Ceiling(csvr.GetDouble(i)));
                                }
                            }
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex, ex.Message);
                        throw;
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .csv File {slectedFile.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select File First");
        }
        public bool IsSerialMode { get; set; }
        void sendLasFileToProjector()
        {
            var pjvm = new ProjectorViewModel(new Projector()
            {
                Name = "p1", IpAddress = "192.168.1.132", Status = 1,
                Port = new System.IO.Ports.SerialPort("COM5")
            });
            pjvm.sendLasFile(slectedFile, IsSerialMode);
        }
        void BrowseProject()
        {
            Logger log = LogManager.GetLogger("Browse Project");
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                log.Log(LogLevel.Info, $"Select Project Folder");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    myDataModels = new MyDataModels();
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
                        myDataModels.Add(new MyDataModel()
                        {
                            Panel = item.Key,
                            ProjectName = item.FirstOrDefault().Directory.Parent.Name,
                            LaserFile = item.ToList(),
                            Complete = false
                        });
                    }
                }
                else log.Log(LogLevel.Warn, $"User Cancelled Project Browser");
            }
        }
        public ProjectViewModel()
        {
            //selectedMyDataModel = new MyDataModel();
            //selectedMyDataModel.LaserFile = new List<FileInfo>() { new FileInfo("")};
            ReadCSVFileCommand = new RelayCommand(o => { readCsvFile(); });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
            SendLasFileCommand = new RelayCommand(o => { sendLasFileToProjector(); });
        }
    }
}

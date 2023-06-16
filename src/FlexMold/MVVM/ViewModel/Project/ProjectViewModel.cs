using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.MVVM.View.Setting;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.Utility;
using MahApps.Metro.Controls;
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
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads;

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
        private bool _CSVWriteButton;

        public bool CSVWriteButton
        {
            get { return _CSVWriteButton; }
            set { _CSVWriteButton = value; OnPropertyChanged(); }
        }
        private bool _LaserSendButton;

        public bool LaserSendButton
        {
            get { return _LaserSendButton; }
            set { _LaserSendButton = value; OnPropertyChanged(); }
        }
        private FileInfo _selectedFile;

        public FileInfo SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                CSVWriteButton = false;
                LaserSendButton = false;
                if (value != null)
                {
                    var se = value.Name.Substring(value.Name.Length - 4);
                    switch (se)
                    {
                        case ".csv":
                            CSVWriteButton = true;
                            break;
                        case ".las":
                            LaserSendButton = true;
                            break;
                        default:
                            break;
                    }
                }
                OnPropertyChanged();
            }
        }

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
                        TwinCathelper.PLCMotors[i].mMode = (int)EMode.Displacement;
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
                    TNF.ShowError($"CSV Failed\nDevice Count :{TwinCathelper.PLCMotors.Length} != Data Count :{data.Count}");
                }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                TNF.ShowError($"CSV Failed");
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

        private float[] _Position;
        public float[] Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
            }
        }
        private void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            try
            {
                // Or here we know about UDINT type --> can be marshalled as UINT32
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol != null)
                {
                    TCSymbols aa;
                    Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                    switch (aa)
                    {
                        case TCSymbols.mPosition:
                            Position = new float[TwinCathelper.DeviceCount];
                            Buffer.BlockCopy(e.Data.ToArray(), 0, Position, 0, e.Data.Length);
                            log.Log(LogLevel.Info, $"Positions Updated : {String.Join(",", Position)}");
                            break;
                        case TCSymbols.MotorDiagnostic:
                            //MDiag1 = new UInt32[TwinCathelper.DeviceCount];
                            //Buffer.BlockCopy(e.Data.ToArray(), 0, MDiag1, 0, e.Data.Length);
                            //log.Log(LogLevel.Info, $"Diag0 Updated : {String.Join(",", MDiag1)}");
                            //Diag1Value = MDiag1;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }
        public ProjectViewModel()
        {
            lVM = new LaserProjectorViewModel(this);
            SendCSVCommand = new RelayCommand(async o =>
            {
                var csvdlgvm = new CSVDiaglogViewModel();
                var view = new CSVDiaglog() { DataContext = csvdlgvm };
                var res = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
                SelectedMotorDirValue = csvdlgvm.SelectedMotorDirPValue;
                string dir = SelectedMotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";
                log.Log(LogLevel.Info, $"Selected Direction : {dir}");
                WriteCSVData(CSVHelper.ReadCSV_DoubleData(SelectedFile));
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
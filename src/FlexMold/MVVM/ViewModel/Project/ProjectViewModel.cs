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
using System.Runtime.Intrinsics.Arm;

namespace FlexMold.MVVM.ViewModel.Project
{
    internal class ProjectViewModel : ObservableObject
    {
        private Logger log = LogManager.GetLogger("Project");
        public RelayCommand SendCSVCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        public RelayCommand StopMotorsCommand { get; set; }
        public int MotorDirValue;
        public float MotorRampDelayValue;
        public float MotorTime;
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
            for (int i = 0; i < data.Count; i++)
            {
                TwinCathelper.PLCMotors[i].mDisplacement = (uint)data[i];
                TwinCathelper.PLCMotors[i].mDirection = MotorDirValue == 0 ? false : true;
                TwinCathelper.PLCMotors[i].mRampDelay = MotorRampDelayValue;
                TwinCathelper.PLCMotors[i].mTime = MotorTime;
                TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;
            }
            TwinCathelper.WriteEthercatDeviceData();
            log.Log(LogLevel.Info, $"{data.Count} csv value/s has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < data.Count; i++)
                TwinCathelper.WriteEthercatDeviceData(i, TwinCathelper.DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{data.Count} zero value/s has been written sucessfully");
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine();
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
        private PanelData[] CalcSoftStarter(IList<double> _csvdata)
        {
            // Inputs
            double rampDelay = 5;
            double missionTime = 1000;

            // Fixed Params
            double displacementFactor = 2;
            double maxRPM = 200;
            double minRPM = 1;
            uint rampSteps = 10;
            var pd = new PanelData[_csvdata.Count];

            // Calculations
            for (int i = 0; i < _csvdata.Count; i++)
            {
                double requiredRotations = _csvdata[i] / displacementFactor;
                double requiredRPM = (requiredRotations / missionTime) * 60;
                double rampRPM = requiredRPM / rampSteps;
                double timePerStep = rampDelay / rampSteps;
                pd[i].RampRpmTime = timePerStep;
                double displacementPerStep = timePerStep * (rampRPM / 60) * displacementFactor;
                double totalDisplacementPerRamp = 0;
                pd[i].RampRpm = new double[rampSteps];
                for (int j = 1; j <= rampSteps; j++)
                {
                    totalDisplacementPerRamp += displacementPerStep * j;
                    pd[i].RampRpm[j - 1] = rampRPM * j;
                }
                pd[i].RampDisplacement = totalDisplacementPerRamp;
                double requiredDisplacementWithoutRamps = _csvdata[i] - totalDisplacementPerRamp * 2;
                pd[i].Displacement = requiredDisplacementWithoutRamps;
                double timeWithoutRamps = missionTime - (rampDelay * 2);
                pd[i].Time = timeWithoutRamps;
                double requiredRotationsWithoutRamps = requiredDisplacementWithoutRamps / displacementFactor;
                double requiredRPMAfterWithoutRamps = (requiredRotationsWithoutRamps / timeWithoutRamps) * 60;
                pd[i].RPM = requiredRPMAfterWithoutRamps;
            }
            return pd;
        }
        struct PanelData
        {
            public double[] RampRpm;
            public double RampRpmTime;
            public double RampDisplacement;
            public double Displacement;
            public double Time;
            public double RPM;
        }
        private async Task CSVSend()
        {
            var csvdlgvm = new CSVDiaglogViewModel();
            var view = new CSVDiaglog() { DataContext = csvdlgvm };
            _ = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            MotorDirValue = csvdlgvm.MotorDirPValue;
            MotorRampDelayValue = csvdlgvm.MotorRampDelayValue;
            MotorTime = csvdlgvm.MotorTime;
            string dir = MotorDirValue == 0 ? "Clockwise" : "Anti-Clockwise";
            log.Log(LogLevel.Info, $"Selected Direction : {dir}");
            var csvdata = CSVHelper.ReadCSV_DoubleData(SelectedFile);
            if (csvdata.Count == 0)
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                $" File doesn't conatain desired data");
                TNF.ShowError($"CSV Failed");
                return;
            }
            if (TwinCathelper.PLCMotors.Length != csvdata.Count)
            {
                log.Log(LogLevel.Error, $"Please select correct CSV");
                log.Log(LogLevel.Error, $"Data and Box counts are not equal");
                log.Log(LogLevel.Error, $"Data Count : {csvdata.Count} | Device Count : {TwinCathelper.PLCMotors.Length}");
                TNF.ShowError($"CSV Failed\nDevice Count :{TwinCathelper.PLCMotors.Length} != Data Count :{csvdata.Count}");
                return;
            }
            WriteCSVData(csvdata);
        }

        public ProjectViewModel()
        {
            lVM = new LaserProjectorViewModel(this);
            SendCSVCommand = new RelayCommand(async o =>
            {
                await CSVSend();
            });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
            StopMotorsCommand = new RelayCommand(o =>
            {
                log.Log(LogLevel.Warn, $"Stopping Motors");
                var data = (int)EMode.Abort;
                for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
                    TwinCathelper.WriteEthercatDeviceData(i, TwinCathelper.DeviceSymbol.mMode, data.CastToArray());
                log.Log(LogLevel.Warn, $"{EMode.Abort}");
            });
        }
    }
}
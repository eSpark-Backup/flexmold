using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Project;
using FlexMold.MVVM.ViewModel.Home;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using static FlexMold.Utility.TwinCathelper;

namespace FlexMold.MVVM.ViewModel.Project
{
    public class ProjectViewModel : ObservableObject, IProjectViewModel
    {
        private Logger log = LogManager.GetLogger("Project");
        public RelayCommand SendCSVCommand { get; set; }
        public RelayCommand BrowseProjectCommand { get; set; }
        public RelayCommand StopMotorsCommand { get; set; }
        public RelayCommand HomeMotorsCommand { get; set; }
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
        private float[] _Position;
        public float[] Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
            }
        }
        private FileInfo _selectedFile;
        private readonly ITwinCathelper _TwinCathelper;
        private readonly LaserProjectorViewModel _laserProjectorViewModel;
        private ICSVDiaglogViewModel _CSVDiaglogViewModel { get { return App.AppHost.Services.GetRequiredService<ICSVDiaglogViewModel>(); } }
        private CSVDiaglogView _CSVDiaglogView { get { return App.AppHost.Services.GetRequiredService<CSVDiaglogView>(); } }

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
        private void WriteCSVData(IList<double> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = (uint)data[i];
                _TwinCathelper.PLCMotors[i].mDirection = MotorDirValue == 0 ? false : true;
                _TwinCathelper.PLCMotors[i].mRampDelay = MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = MotorTime;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;
            }
            _TwinCathelper.WriteEthercatDeviceData();
            log.Log(LogLevel.Info, $"{data.Count} csv value/s has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < data.Count; i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
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
                    if (result == DialogResult.OK)
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
                            Position = new float[DeviceCount];
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
            _ = await DialogHost.Show(_CSVDiaglogView, "RootDialog", ClosingEventHandler);
            MotorDirValue = _CSVDiaglogViewModel.MotorDirPValue;
            MotorRampDelayValue = _CSVDiaglogViewModel.MotorRampDelayValue;
            MotorTime = _CSVDiaglogViewModel.MotorTime;
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
            if (_TwinCathelper.PLCMotors.Length != csvdata.Count)
            {
                log.Log(LogLevel.Error, $"Please select correct CSV");
                log.Log(LogLevel.Error, $"Data and Box counts are not equal");
                log.Log(LogLevel.Error, $"Data Count : {csvdata.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
                TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{csvdata.Count}");
                return;
            }
            WriteCSVData(csvdata);
        }
        public IMotorStatusViewmodel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewmodel>(); } }
        private void HomeTrigger()
        {
            log.Log(LogLevel.Info, $"Home Command Triggered");

            var data = (uint)EMode.Displacement;
            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.PLCMotors[i].mMode = data;
            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
            log.Log(LogLevel.Info, $"Mode values written to all Slaves : {EMode.Displacement}");

            _TwinCathelper.ReadEthercatDeviceData(DeviceSymbol.mDirection);
            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.PLCMotors[i].mDirection = !_TwinCathelper.PLCMotors[i].mDirection;
            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDirection);
            log.Log(LogLevel.Info, $"Reverse Direction written to all Slaves");

            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.PLCMotors[i].mDisplacement = MotorStatus.mPosition[i] * 1600;
            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
            log.Log(LogLevel.Info, $"Displacement values written to all Slaves : \n\t {JsonSerializer.Serialize(MotorStatus.mPosition)}");

            Thread.Sleep(200);

            for (uint i = 0; i < DeviceCount; i++)
                _TwinCathelper.PLCMotors[i].mDisplacement = 0;
            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
            log.Log(LogLevel.Info, $"Zero Displacement values written to all Slaves");
        }
        public ProjectViewModel(ITwinCathelper twinCathelper)
        {
            this._TwinCathelper = twinCathelper;
            SendCSVCommand = new RelayCommand(async o =>
            {
                await CSVSend();
            });
            BrowseProjectCommand = new RelayCommand(o => { BrowseProject(); });
            StopMotorsCommand = new RelayCommand(o =>
            {
                log.Log(LogLevel.Warn, $"Stopping Motors");
                var data = (int)EMode.Abort;
                for (uint i = 0; i < DeviceCount; i++)
                    _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mMode, data.CastToArray());
                log.Log(LogLevel.Warn, $"{EMode.Abort}");
            });
            HomeMotorsCommand = new RelayCommand(o =>
            {
                HomeTrigger();
            });

        }

        
    }
}
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using static FlexMold.Utility.TwinCathelper;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    internal class ResetViewModel : ObservableObject, IResetViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        /// 
        private Logger log = LogManager.GetLogger("MachineTest");
        private CultureInfo vCulture = System.Globalization.CultureInfo.CurrentCulture;
        private ITwinCathelper _TwinCathelper;
        //private IResetViewModel _ResetViewModel { get { return App.AppHost.Services.GetRequiredService<IResetViewModel>(); } }
        public bool DoMachineUserCanceled = false;
        public IList<float> CalibrationData { get; set; }
        uint[] validSeq01Val = new uint[DeviceCount];
        uint[] validSeq02Val = new uint[DeviceCount];
        uint[] healthyMotorDiag = new uint[DeviceCount];
        uint[] validForHome = new uint[DeviceCount];

        private bool _editingEnable;
        public bool editingEnable
        {
            get { return _editingEnable; }
            set { _editingEnable = value; OnPropertyChanged(); }
        }

        private uint[] _DeviceStatus;
        public uint[] DeviceStatus
        {
            get { return _DeviceStatus; }
            set
            {
                //SetDeviceStatus(value);
                _DeviceStatus = value;
            }
        }

        public RelayCommand HardZeroCommand { get; set; }
        public RelayCommand HardCalibrCommand { get; set; }

        public RelayCommand UpdateHomingCommand { get; set; }
        public RelayCommand GetHomingCommand { get; set; }



        private string _HomingOffsetDataCommand;

        public string HomingOffsetDataCommand
        {
            get { return _HomingOffsetDataCommand; }
            set
            {
                _HomingOffsetDataCommand = value;
                OnPropertyChanged();
            }
        }

        //private Logger log;

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ResetViewModel() { 
            GetHomingProcess();
            editingEnable = FlexMoldDB.IsEditingEnabled;
        }
        public ResetViewModel(ITwinCathelper twinCathelper)
        {
            //ReadMotorVM = new HeaterView();
            //CurrentView = HeaterStatusVM;
            editingEnable = FlexMoldDB.IsEditingEnabled;

            DoMachineUserCanceled = false;
            HardZeroCommand = new RelayCommand(o => { _TwinCathelper = twinCathelper; HardZeroProcess(); });
            HardCalibrCommand = new RelayCommand(o => { _TwinCathelper = twinCathelper; HardCalibrateProcess(); });
            UpdateHomingCommand = new RelayCommand(o => { _TwinCathelper = twinCathelper; UpdateHomingProcess(); });
            GetHomingCommand = new RelayCommand(o => { _TwinCathelper = twinCathelper; GetHomingProcess(); });

            //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
            //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
            //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
            //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

            try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }
            //try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            {
                try
                {

                    //for (int j = 0; j < DeviceCount; j++)
                    //{
                    //    try
                    //    {
                    //        NegativeoffsetResumeProj[j] = 0.00F - _TwinCathelper.PLCMotorsStatuses.mPostion[j];
                    //    }
                    //    catch { }
                    //}

                    //if (System.Globalization.CultureInfo.CurrentCulture.Name.Contains("en-DK"))
                    //{
                    //    vCulture.NumberFormat.NumberDecimalSeparator = ",";
                    //    vCulture.NumberFormat.CurrencyDecimalSeparator = ",";
                    //}
                    //else
                    //{
                    //    vCulture.NumberFormat.NumberDecimalSeparator = ".";
                    //    vCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                    //}



                    String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                    String[] offset_resume = CSVOffset_contents.Split('~');
                    int i = 1;
                    string errmsg = "";
                    //foreach (string negHomin in offset_resume)
                    for (int j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)//-10,849304
                    {                        
                        HomingOffsetDataCommand += "Motor "+j +":="+float.Parse(offset_resume[j])+Environment.NewLine;

                        //if (Math.Abs(NegativeoffsetResumeProj[i]) >= CSVHelper.Max_mm)
                        //    errmsg += " Motor" + i + "-" + NegativeoffsetResumeProj[i].ToString() + Environment.NewLine;
                        i++;
                    }

                    i = 0;

                    //if (errmsg.Length > 1)
                    //{
                    //    MessageBox.Show("Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButtons.OK);
                    //    log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                    //    errmsg = "";
                    //}

                    //if (NegativeoffsetResumeProj.Count > 0)
                    //{
                    //    CSVHelper.NegativeoffsetResume = "";
                    //    //foreach (float negHomin in NegativeoffsetResume)
                    //    //{
                    //    //    NegativeoffsetResume[i] = negHomin + float.Parse(offset_resume[i], vCulture);
                    //    //}
                    //    String newHomingOffset = "";
                    //    foreach (float flt in NegativeoffsetResumeProj)
                    //        newHomingOffset += flt.ToString(CultureInfo.InvariantCulture) + ",";

                    //    CSVHelper.NegativeoffsetResume = newHomingOffset.Substring(0, newHomingOffset.Length - 1);

                    //    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }
                    //}
                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");
                }
            }
            
        }

        public void HardZeroProcess() {

            log.Log(LogLevel.Info, "HardZeroProcess Started successfully!");
            try{
                if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                {
                    log.Log(LogLevel.Error, $"Machine status disconnected! ");
                    MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

                    if (dr == MessageBoxResult.Cancel)
                    {
                        DoMachineUserCanceled = true;
                        return;
                    }

                    return;
                }

                CalibrateSeq01(); } catch (Exception ee) { log.Log(LogLevel.Warn, "HardZeroProcess Ended with error! " + ee.Message); }
            log.Log(LogLevel.Info, "HardZeroProcess Completed successfully!");
        }


        public void GetHomingProcess()
        {
            log.Log(LogLevel.Info, "Getting Homming data started!");

            try { CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            {
                try
                {
                    String[] newHomingArr = CSVHelper.NegativeoffsetResume.Split("~");
                    //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(negHomin, vCulture) + Environment.NewLine;
                    String newCsvHomeData = "";
                    int i = 1;
                    HomingOffsetDataCommand = "";

                    for (int j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                    {
                        try
                        {
                            //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(st, vCulture) + Environment.NewLine;
                            if ((newHomingArr[j]).Contains('"'))
                                HomingOffsetDataCommand += "Motor " + j + ":=" + float.Parse(newHomingArr[j]) + Environment.NewLine;
                            else
                                HomingOffsetDataCommand += "Motor " + j + ":=" + float.Parse(newHomingArr[j], vCulture) + Environment.NewLine;
                            i++;
                        }
                        catch { }
                    }
                }
                catch { }
            }
            else
            {
                int i = 1;
                for (int j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                {
                    try
                    {
                        //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(st, vCulture) + Environment.NewLine;
                            HomingOffsetDataCommand += "Motor " + j + ":=-0" + Environment.NewLine;
                        CSVHelper.NegativeoffsetResume += "0~";
                        i++;
                    }
                    catch { }

                }
                CSVHelper.NegativeoffsetResume = CSVHelper.NegativeoffsetResume.Remove(CSVHelper.NegativeoffsetResume.Length - 1);
                CSVHelper.SetNegativeoffsetResumeFlt();
            }

                log.Log(LogLevel.Info, "Getting Homming data ended!");
        }


        public void UpdateHomingProcess()
        {
            log.Log(LogLevel.Info, "Update Homing started!");
            String errmsg = "";
            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
            {
                try
                {
                    String[] newHomingArr = HomingOffsetDataCommand.Split(Environment.NewLine);
                    //HomingOffsetDataCommand += "Motor " + i + ":=" + float.Parse(negHomin, vCulture) + Environment.NewLine;
                    String newCsvHomeData = "";
                    int i = 1;
                    //log.Log(LogLevel.Info, "chkMotorInput11 chkMotorInput>>>>" + DeviceCount.ToString()+"---"+ newHomingArr[0]);
                    //foreach (String st in newHomingArr)
                    for (int j = 0; j < _TwinCathelper.PLCMotors.Count(); j++)
                    {
                        try
                        {
                            if (newHomingArr[j].Length > 0)
                            {
                                float chkMotorInput = float.Parse((newHomingArr[j].Substring(newHomingArr[j].IndexOf('=') + 1)).Trim(), vCulture);

                                //log.Log(LogLevel.Info, "chkMotorInput chkMotorInput>>>>" + chkMotorInput);

                                if (Math.Abs(chkMotorInput) >= CSVHelper.Max_mm)
                                    errmsg += " Motor" + i + "-" + chkMotorInput.ToString() + Environment.NewLine;

                                newCsvHomeData += chkMotorInput.ToString() + "~";//vCulture
                            }
                            i++;                            
                        }
                        catch { errmsg += " Motor" + i + "-" + newHomingArr[j] + Environment.NewLine; }
                    }

                    if (errmsg.Length > 0)
                    {
                        MessageBox.Show(errmsg, "Alert! Max homing exceed ", MessageBoxButton.OK);
                        log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                        errmsg = "";
                        return;                    
                    }


                    CSVHelper.NegativeoffsetResume = newCsvHomeData.Substring(0, newCsvHomeData.Length - 1);
                    CSVHelper.SetNegativeoffsetResumeFlt();


                    try { CSVHelper.offsetRes_MotorSettingViewModel.UpdateMotorSettingsDB(); } catch { }

                    log.Log(LogLevel.Info, "Successfully updated homing data!" + errmsg);

                }
                catch (Exception ee)
                {
                    log.Log(LogLevel.Error, "No Offset Data found or Offset data corrupted!");

                    if (errmsg.Length > 0)
                    {
                        MessageBox.Show(errmsg, "Alert! Max homing exceed ", MessageBoxButton.OK);
                        log.Log(LogLevel.Error, "Alert! Max homing exceed " + "Homing for motors exceeds at " + errmsg);
                        errmsg = "";
                        return;
                    }
                }
            }

        }


        public void HardCalibrateProcess()
        {
            log.Log(LogLevel.Info, "HardCalibrateProcess Started successfully!");
            try {
                if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
                {
                    log.Log(LogLevel.Error, $"Machine status disconnected! ");
                    MessageBoxResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButton.OKCancel);

                    if (dr == MessageBoxResult.Cancel)
                    {
                        DoMachineUserCanceled = true;
                        return;
                    }

                    return;
                }

                FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted = false;
                //CalibrateSeq02(); 
                CalibrateSeq02_HomeUp();

            //Line1:
            //    Thread.Sleep(1000);
            //    if (!FlexMold.MVVM.ViewModel.Project.ProjectViewModel.IsRampCompleted)
            //        goto Line1;

            //    CalibrateSeq02_HomeDown();

            } catch(Exception ee) { log.Log(LogLevel.Warn, "CalibrateSeq02 Ended with error! "+ee.Message); }
            log.Log(LogLevel.Info, "HardCalibrateProcess Completed successfully!");
        }

        void CalibrateSeq02_HomeUp() {

            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                //_TwinCathelper.PLCMotors[i].mDisplacement = 0.200F;
                //_TwinCathelper.PLCMotors[i].mDirection = false;
                //_TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                //_TwinCathelper.PLCMotors[i].mTime = 6.3334F;
                //_TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Resume;

                log.Log(LogLevel.Info, "CalibrateHard-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                + "  >> MotorTime: " + _TwinCathelper.PLCMotors[i].mTime.ToString()
                + "  >> mMode: " + EMode.Project.ToString());
            }

            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
            log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} CalibrateHard value(s) has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} zero value(s) has been written sucessfully");

        }
        void CalibrateSeq02_HomeDown()
        {

            for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = 0.200F;
                _TwinCathelper.PLCMotors[i].mDirection = true;
                _TwinCathelper.PLCMotors[i].mRampDelay = CSVHelper.MotorRampDelayValue;
                _TwinCathelper.PLCMotors[i].mTime = 6.3334F;
                _TwinCathelper.PLCMotors[i].mMode = (int)EMode.Project;

                log.Log(LogLevel.Info, "CalibrateHard2-WriteCSVData loop1:" + i.ToString() + " : " + _TwinCathelper.PLCMotors[i].mDisplacement.ToString() + "  >> Direction: " + _TwinCathelper.PLCMotors[i].mDirection.ToString()
                + "  >> RampDlay: " + CSVHelper.MotorRampDelayValue.ToString()
                + "  >> MotorTime: " + CSVHelper.MotorTime.ToString()
                + "  >> mMode: " + EMode.Project.ToString());
            }

            _TwinCathelper.WriteEthercatDeviceData();
            log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} CalibrateHard value(s) has been written sucessfully");

            Thread.Sleep(200);
            uint zero = 0;
            for (uint i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                _TwinCathelper.WriteEthercatDeviceData(i, DeviceSymbol.mDisplacement, zero.CastToArray());
            log.Log(LogLevel.Info, $"{_TwinCathelper.PLCMotors.Count()} zero value(s) has been written sucessfully");

        }


        //private void SetDeviceMode(uint[] _value, Logger log)
        //{
        //    if (_value.SequenceEqual(healthyMotorDiag))
        //    {
        //        _TwinCathelper.Motor_Diagnostic_Abort = false;
        //        _TwinCathelper.SetDefaultMode(log);
        //    }
        //    else
        //    {
        //        _TwinCathelper.Motor_Diagnostic_Abort = true;
        //        _TwinCathelper.SetAbortMode(log);
        //    }
        //}

        private bool ValidateCSVData()
        {
            var log = LogManager.GetLogger("CSV Validator");
            if (CalibrationData.Count != 0)
            {

                try
                {
                    if (_TwinCathelper!=null && _TwinCathelper.PLCMotors != null && _TwinCathelper.PLCMotors.Length == CalibrationData.Count)
                    {
                        return true;
                    }
                    else
                    {
                        log.Log(LogLevel.Error, $"Please select correct CSV");
                        log.Log(LogLevel.Error, $"Data and Box count are not equal");
                        log.Log(LogLevel.Error, $"Data Count : {CalibrationData.Count} | Device Count : {_TwinCathelper.PLCMotors.Length}");
                        TNF.ShowError($"CSV Failed\nDevice Count :{_TwinCathelper.PLCMotors.Length} != Data Count :{CalibrationData.Count}");
                    }
                }
                catch { log.Log(LogLevel.Info, $"motorsCountError>>>>"); }
            }
            else
            {
                log.Log(LogLevel.Error, $"Please select correct CSV\n" +
                    $" File doesn't conatain desired data");
                TNF.ShowError($"CSV Failed");
            }
            return false;
        }

        public void CalibrateSeq01()
        {
            CSVHelper.IsLastEventCalibr = false;
            var log = LogManager.GetLogger("Calibration");
            log.Log(LogLevel.Info, $"Starting Calibration Seq01 process");
            try
            {
                if (File.Exists(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + _TwinCathelper.PLCMotors.Count() + ".calibrate"))
                {
                    FileInfo finf = new FileInfo(CSVHelper.GetCurrentDirectoryPath() + "\\AppData\\Calibration\\" + _TwinCathelper.PLCMotors.Count() + ".calibrate");
                    CalibrationData = CSVHelper.ReadCalibrate_DoubleData(finf);

                    //try{log.Log(LogLevel.Info, "CalibrationData>>>" + CalibrationData.Count); } catch { log.Log(LogLevel.Info, "CalibrationData>>>DataError"); }

                    if (ValidateCSVData())
                    {
                        if (CSVHelper.ST_timer == null) CSVHelper.ST_timer = new System.Diagnostics.Stopwatch();
                        CSVHelper.ST_timer.Stop(); CSVHelper.ST_timer.Reset();

                        for (int i = 0; i < _TwinCathelper.PLCMotors.Count(); i++)
                            _TwinCathelper.PLCMotors[i].mMode = (int)EMode.CalibrationSeq01Request;
                        _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mMode);
                        log.Log(LogLevel.Warn, $"Calibration Seq01 Request Sent. Please wait...");
                        TNF.ShowSuccess("Calibration Request Sent. Please wait...");
                        
                    }
                    else
                        CSVHelper.IsLastEventCalibr = false;

                    //Array.Fill<uint>(validSeq01Val, (int)EDeviceStatus.CalibrationSquence01Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.CalibrationSquence02Success);
                    //Array.Fill<uint>(validSeq02Val, (int)EDeviceStatus.RampSequenceCompleted);
                    //Array.Fill<uint>(healthyMotorDiag, (int)EmotorDiagnostic.Healthy);
                    //Array.Fill<uint>(validForHome, (int)EDeviceStatus.None);

                }
                else
                {
                    MessageBox.Show("Please copy " + _TwinCathelper.PLCMotors.Count() + ".calibrate to Appdata folder,", "No Calibration file found!", MessageBoxButton.OK);
                    //CalibrateButtonEnable = true;
                }
            }
            catch (Exception ex)
            {
                //CalibrateButtonEnable = true;
                log.Log(LogLevel.Error, $"Error while Calibration Seq01 process!");
                log.Error(ex.Message);
                TNF.ShowError(ex.Message);
                return;
            }
        }        

        private void WriteDistpacementCSVData(IList<float> data)
        {
            var log = LogManager.GetLogger("Calibration");
            for (int i = 0; i < data.Count; i++)
            {
                _TwinCathelper.PLCMotors[i].mDisplacement = (float)data[i];
            }
            _TwinCathelper.WriteEthercatDeviceData(DeviceSymbol.mDisplacement);
            log.Log(LogLevel.Info, $"{data.Count} csv value(s) for Calibration has been written sucessfully");
            log.Log(LogLevel.Info, $"{JsonSerializer.Serialize(data)}");
        }

    }
}
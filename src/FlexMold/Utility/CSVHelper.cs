using FlexMold.MVVM.ViewModel.Home;
using Newtonsoft.Json;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace FlexMold.Utility
{
    public static class CSVHelper
    {
        public static float MotorRampDelayValue = 0.00F;
        public static float MotorTime = 0.00F;
        public static float MaxDisplacement = 0.00F;
        public static float Max_mm = 0.00F;
        public static float RecommendedRPM = 0.00F;
        public static float MaxRPM = 0.00F;
        public static int MotorDirValue=0;
        public static uint TotalNoHrs = 0;
        public static int PartsProduced = 0;
        public static uint LastSystemStatus = 0;
        public static string LastSelectedCSV = "";
        public static float CompletingTime = 0.00F;
        public static float CompletingTime_count = 0;
        public static Stopwatch ST_timer = new Stopwatch();
        //public static int RPMAssume = 0;
        //public static int CalibrationSeq02Delay = 0;

        public static int AdsStateTimeout = 6;
        public static string prjFolderLoc = Environment.CurrentDirectory;
        public static string NegativeoffsetResume = "";
        public static List<float> NegativeoffsetResumeFlt = null;
        public static string LastPositionValues = "";
        public static MVVM.ViewModel.Setting.MotorSettingViewModel offsetRes_MotorSettingViewModel=null;


        public static string Name = "";
        public static string IpAddres = "";
        public static int TelnetPort = 0;
        public static StatusBaseViewModel _statusBaseViewModel;
        public static String[] MotorsStatusGlobal = new String[TwinCathelper.DeviceCount];
        public static bool MachineEnabled = true;
        public static bool wrongFirmwareFound = false;
        public static bool UpdateHomeCSVStatus = false;
        public static bool LaserEnabled = false;
        public static bool IsLastEventCalibr = false;
        public static bool IsNewPanelSelected = false;
        public static bool IsNewProjSelected = false;
        public static bool InPositionProcess_Elapsed = false;
        public static bool NoLinkHappened = false;
        public static bool CalibrationCompleted = false;
        //public static bool StopAlreadyCalled = false;
        public static DateTime LastStopTime = DateTime.Now;

        //public int Status { get; set; }
        //public string UserName { get; set; }
        //public string Password { get; set; }
        //public string WorkGroup { get; set; }
        //public string DataFolderName { get; set; }
        //public string PlotFolderName { get; set; }


        public static void SetNegativeoffsetResumeFlt()
        {
            try
            {
                List<float> NegativeoffsetResumeProj = new List<float>();
                String CSVOffset_contents = NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                String[] offset_resume = CSVOffset_contents.Split('~');
                NegativeoffsetResumeFlt = new List<float>(TwinCathelper.DeviceCount);

                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                    NegativeoffsetResumeFlt.Add(float.Parse(offset_resume[i]));
            }
            catch { }
        }

        public static List<float> ReadCSV_DoubleData(FileInfo filename)
        {
            Logger log = LogManager.GetLogger("CSVHelper");
            
            List<float> NegativeoffsetResumeProj = new List<float>(TwinCathelper.DeviceCount);
            CultureInfo vCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
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
                
                CSVHelper.offsetRes_MotorSettingViewModel.LoadMotorSettingsDB(); } catch { }

            var data = new List<float>();
            if (filename != null)
            {
                if (filename.FullName.EndsWith(".csv"))
                {
                    log.Log(LogLevel.Info, $"Reading CSV File {filename.Name}");
                    try
                    {
                        using (var reader = new StreamReader(filename.FullName))
                        {
                            var options = new CsvDataReaderOptions
                            {
                                HasHeaders = false,
                                BufferSize = 0x10000,
                            };
                            var csvr = CsvDataReader.Create(reader, options);

                            //if old Homing data exist then add to it ie -5 + 10 = 5
                            if (CSVHelper.NegativeoffsetResume.Length > 0)//0.0F
                            {
                                try
                                {
                                    String CSVOffset_contents = CSVHelper.NegativeoffsetResume.ToString(CultureInfo.InvariantCulture);
                                    String[] offset_resume = CSVOffset_contents.Split('~');
                                    int i = 0;
                                    
                                    foreach (string negHomin in offset_resume)
                                    {
                                        try { NegativeoffsetResumeProj[i] = float.Parse(negHomin, vCulture); } catch { }
                                        i++;
                                    }
                                }
                                catch { }


                            }


                            float Homingadd = 0.00F;
                            String errmsg = "";
                            while (csvr.Read())
                            {
                                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                                {
                                    try {
                                        Homingadd = NegativeoffsetResumeProj[i] + csvr.GetFloat(i);

                                        if (Math.Abs(Homingadd) >= CSVHelper.Max_mm)
                                            errmsg += " Motor" + i + "-" + Homingadd.ToString() + Environment.NewLine;
                                    }
                                    catch { Homingadd = csvr.GetFloat(i); }                                    

                                    data.Add(Homingadd);
                                }
                                break;
                            }

                            if (errmsg.Length > 1)
                            {
                                MessageBox.Show("CSV Homing for motors exceeds at " + errmsg, "Alert! Max homing exceed ", MessageBoxButtons.OK);
                                log.Log(LogLevel.Error, "Alert! Max homing exceed "+ "CSV Homing for motors exceeds at " + errmsg);
                                errmsg = "";
                                return new List<float>();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex.Message);
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .csv File {filename.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select a valid File");
            log.Info($"CSV data : {JsonConvert.SerializeObject(data)}");
            return data;
        }

        public static List<float> ReadCalibrate_DoubleData(FileInfo filename)
        {
            Logger log = LogManager.GetLogger("CalibrateHelper");
            var data = new List<float>();
            if (filename != null)
            {
                if (filename.FullName.EndsWith(".calibrate"))
                {
                    log.Log(LogLevel.Info, $"Reading CSV File {filename.Name}");
                    try
                    {
                        using (var reader = new StreamReader(filename.FullName))
                        {
                            var options = new CsvDataReaderOptions
                            {
                                HasHeaders = false,
                                BufferSize = 0x10000,
                            };
                            var csvr = CsvDataReader.Create(reader, options);

                            while (csvr.Read())
                            {
                                for (int i = 0; i < TwinCathelper.DeviceCount; i++)//csvr.FieldCount
                                {
                                    data.Add(csvr.GetFloat(i));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex.Message);
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .calibrate File {filename.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select a valid File");
            log.Info($"Calibrate data : {JsonConvert.SerializeObject(data)}");
            return data;
        }

        public static string GetCurrentDirectoryPath()
        {
            string filePath = new Uri(Assembly.GetExecutingAssembly().Location).LocalPath;
            return Path.GetDirectoryName(filePath);
        }
    }
}

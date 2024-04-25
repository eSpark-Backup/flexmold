using System;
using System.IO;

namespace FlexMold.MVVM.Model
{
    public class AppSettings
    {
        public string SettingRoot { get; set; } = @"./AppData/";
        //public string CalibrationFile { get; set; } = "Calibration.calibrate";
        //public uint CalibrationSeq02Delay { get; set; } = 5000;
        //public uint MaxRPM { get; set; } = 200;

        //public string CalibrationFileFullPath()
        //{
        //    return new FileInfo(Environment.CurrentDirectory+SettingRoot.Trim('.') + "\\Calibration\\"+CalibrationFile).FullName;
        //}
    }
}

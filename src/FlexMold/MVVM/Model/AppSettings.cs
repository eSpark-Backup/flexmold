using FlexMold.Utility;
using Microsoft.VisualStudio.WCFReference.Interop;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace FlexMold.MVVM.Model
{
    public class AppSettings
    {
        public string SettingRoot { get; set; } = @"./AppData/";
        public string CalibrationFile { get; set; } = "Calibration.csv";
        public uint CalibrationSeq02Delay { get; set; } = 5000;

        public string CalibrationFileFullPath()
        {
            return new FileInfo(Environment.CurrentDirectory+SettingRoot.Trim('.') + CalibrationFile).FullName;
        }
    }
}

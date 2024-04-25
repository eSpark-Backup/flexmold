using System.Collections.Generic;
using System.IO;

namespace FlexMold.Utility
{
    public interface ICSVHelper
    {
        List<double> ReadCSV_DoubleData(FileInfo filename);
        List<double> ReadCalibrate_DoubleData(FileInfo filename);
    }
}
using ControlzEx.Native;
using FlexMold.Utility;
using StructPacker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.Model
{
    [Pack]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct EthercatDevice
    {
        public bool mDirection;
        public bool t1;
        public bool t2;
        public bool t3;
        public UInt32 mSpeed;
        public UInt32 mTime;
        public bool mMode;
        public bool t4;
        public bool t5;
        public bool t6;
        [SkipPack]
        public uint DeviceID;
    }
    public static class TwinCATVariableIfo
    {
        public const string TwicatDeviceCountVariableName = "MAIN.deviceCount";
        public const string TwicatDeviceMainPOUName = "MAIN.";
        public const string MainDeviceVariableName = "MAIN.device";
        public const string mPositionVariableName = "MAIN.mPosition";
        public const string mTemperatureVariableName = "MAIN.mTemperature";
        public const string mDiag1VariableName = "MAIN.mDiag1";
        public const string mDiag2VariableName = "MAIN.mDiag2";
        public const string mDiag3VariableName = "MAIN.mDiag3";
    }
    public enum EmDiag1
    {
        Healthy,
        Recovered,
        Stepper_Driver_Failed,
    }
}

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
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    class PLCMotor
    {
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public EthercatDevice[] MyEthercatDevice = new EthercatDevice[10];
    }
    [Pack]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct EthercatDevice
    {
        public byte dir;
        public byte temp;
        public UInt16 speed;
        public UInt32 dummyDataOut;
        public UInt32 dummyDataIn;
        [SkipPack]
        public uint DeviceID;
    }
}

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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class PLCMotor
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public EthercatDevice[] MyEthercatDevice = new EthercatDevice[3];
    }
    public struct EthercatDevice
    {
        public byte dir;
        public UInt16 speed;
        public UInt32 dummyDataOut;
        public UInt32 dummyDataIn;
    }
}

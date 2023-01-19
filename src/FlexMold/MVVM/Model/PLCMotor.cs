using ControlzEx.Native;
using FlexMold.Core;
using FlexMold.Utility;
using StructPacker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
    public static class TCVarName
    {
        public const string DeviceCount = "MAIN.deviceCount";
        public const string DeviceMainPOU = "MAIN.";
        public const string MainDevice = "MAIN.device";
        public const string mPosition = "MAIN.mPosition";
        public const string mTemperature = "MAIN.mTemperature";
        public const string mDiag1 = "MAIN.mDiag1";
        public const string mDiag2 = "MAIN.mDiag2";
        public const string mDiag3 = "MAIN.mDiag3";
        public const string foe_Download = "MAIN.foe_Download";
        public const string foe_dwPass = "MAIN.foe_dwPass";
        public const string foe_sNetId = "MAIN.foe_sNetId";
        public const string foe_nSlaveAddr = "MAIN.foe_nSlaveAddr";
        public const string foe_eMode = "MAIN.foe_eMode";
        public const string foe_sPathName = "MAIN.foe_sPathName";
        public const string foe_bLoad = "MAIN.foe_bLoad";
        public const string foe_bBusy = "MAIN.foe_bBusy";
        public const string foe_bError = "MAIN.foe_bError";
        public const string foe_nErrID = "MAIN.foe_nErrID";
        public const string foe_nBytesWritten = "MAIN.foe_nBytesWritten";
        public const string foe_nPercent= "MAIN.foe_nPercent";
        public const string foe_Scrip= "" +
            "foe_Download : FB_EcFoeLoad ;\r\n" +
            $"foe_dwPass\t: DWORD := 287454020;\r\n" +
            $"foe_sNetId     : T_AmsNetId := '192.168.18.191.3.1' ;\r\n" +
            $"foe_nSlaveAddr : UINT := 1001;  \r\n" +
            $"foe_eMode      : E_EcFoeMode := eFoeMode_Write;\r\n" +
            $"foe_sPathName : STRING := 'D:\\LAN9255Updated.bin';\r\n" +
            $"foe_bLoad      : BOOL;\r\n" +
            $"foe_bBusy      : BOOL;\r\n" +
            $"foe_bError     : BOOL;\r\n" +
            $"foe_nErrID     : UDINT;\r\n" +
            $"foe_nBytesWritten : UDINT;\r\n" +
            $"foe_nPercent      : UDINT;";

    }
    public enum EmDiag1
    {
        Healthy,
        Recovered,
        Stepper_Driver_Failed,
    }
    
    public class MotorStatusControl : ObservableObject
    {
        private int _MotorId;
        public int MotorId
        {
            get { return _MotorId; }
            set
            {
                _MotorId = value; OnPropertyChanged();
            }
        }
        private Brush _FillColor;

        public Brush FillColor
        {
            get { return _FillColor; }
            set
            {
                _FillColor = value;
                OnPropertyChanged();
            }
        }
        private string _ErrorDeatil;
        public string ErrorDeatil
        {
            get { return _ErrorDeatil; }
            set
            {
                _ErrorDeatil = value;
                OnPropertyChanged();
            }
        }

    }
}

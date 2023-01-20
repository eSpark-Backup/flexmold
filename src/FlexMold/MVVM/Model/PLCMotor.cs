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
        public const string foe_Scrip = "" +
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
    public enum EFoeSymbols
    {
        foe_dwPass,
        foe_sNetId,
        foe_nSlaveAddr,
        foe_eMode,
        foe_sPathName,
        foe_bLoad,
        foe_bBusy,
        foe_bError,
        foe_nErrID,
        foe_nBytesWritten,
        foe_nPercent,
    }
    public class Foe : ObservableObject
    {
        private uint foe_dwPass;
        public UInt32 Foe_dwPass { get => foe_dwPass; set => foe_dwPass = value; }
        public UInt16 Foe_nSlaveAddr { get; set; }
        private string foe_sPathName;
        public string Foe_sPathName { get => foe_sPathName; set { foe_sPathName = value; OnPropertyChanged(); } }

        private bool _Foe_bLoad;
        public bool Foe_bLoad { get => _Foe_bLoad; set { _Foe_bLoad = value; OnPropertyChanged(); } }

        private bool _Foe_bBusy;
        public bool Foe_bBusy { get => _Foe_bBusy; set { _Foe_bBusy = value; OnPropertyChanged(); } }

        private bool _Foe_bError;
        public bool Foe_bError { get => _Foe_bError; set { _Foe_bError = value; OnPropertyChanged();  } }

        private UInt32 _Foe_nErrID;
        public UInt32 Foe_nErrID { get => _Foe_nErrID; set { _Foe_nErrID = value; OnPropertyChanged(); } }

        private UInt32 _Foe_nBytesWritten;
        public UInt32 Foe_nBytesWritten { get => _Foe_nBytesWritten; set { _Foe_nBytesWritten = value; OnPropertyChanged(); } }

        private UInt32 _Foe_nPercent;
        public UInt32 Foe_nPercent { get => _Foe_nPercent; set { _Foe_nPercent = value; OnPropertyChanged(); } }
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

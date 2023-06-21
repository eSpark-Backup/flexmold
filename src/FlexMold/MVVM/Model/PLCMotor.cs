using FlexMold.Core;
using Microsoft.VisualStudio.Shell.Interop;
using StructPacker;
using System;
using System.Runtime.InteropServices;
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
        public UInt32 mRPM;
        public UInt32 mDisplacement;
        public UInt32 mTime;
        public UInt32 mMode;

        [SkipPack]
        public uint DeviceID;
    }
    public class DeviceStatuses
    {
        public uint[] mTemperature { get; set; }
        public uint[] MotorDiagnistic { get; set; }
        public uint[] DeviceStatus { get; set; }
        public uint[] mDiag3 { get; set; }
        public float[] mPostion { get; set; }
        public DeviceStatuses(uint deviceCount)
        {
            mTemperature = new uint[deviceCount];
            MotorDiagnistic = new uint[deviceCount];
            DeviceStatus = new uint[deviceCount];
            mDiag3 = new uint[deviceCount];
            mPostion = new float[deviceCount];
        }
    }
    public enum EMode
    {
        Displacement = 0,
        Time,
        CalibrationSeq01Request,
        CalibrationSeq02Request,
        Abort
    }
    public enum ETMode
    {
        Displacement = 0,
        Time
    }
    public enum EDeviceStatus
    {
        None = 0,
        CalibrationSquence01Started = 1,
        CalibrationSquence01Failed,
        CalibrationSquence01Success,
        CalibrationSquence02Started,
        CalibrationSquence02Failed,
        CalibrationSquence02Success,
        ShapeFormingStarted,
        ShapeFormingCompleted,
    }
    public static class TCVarName
    {
        public const string DeviceCount = "MAIN.deviceCount";
        public const string MainDevice = "MAIN.device";
        public const string mPosition = "MAIN.mPosition";
        public const string mTemperature = "MAIN.mTemperature";
    }

    public enum EmotorDiagnostic
    {
        Healthy,
        Stepper_Driver_Failed,
        Stall
    }

    public enum TCSymbols
    {
        deviceCount,
        device,
        mPosition,
        mTemperature,
        foe_dwPass,
        foe_nSlaveAddr,
        foe_eMode,
        foe_sPathName,
        foe_bLoad,
        foe_bBusy,
        foe_bError,
        foe_nErrID,
        foe_nBytesWritten,
        foe_nPercent,
        foe_sAmsNetId,
        foe_sAmsPort,
        ecs_bExecute,
        ecs_reqState,
        ecs_bBusy,
        ecs_bError,
        ecs_nErrId,
        ecs_currState,
        fbGetSlaveState,
        ecg_bExecute,
        state,
        ecg_bError,
        ecg_nErrId,
        ECAbExecute,
        ECAdevStates,
        ECAnSlaves,
        ECAbError,
        ECAnErrId,
        MotorDiagnostic,
        DeviceStatus,
        mDiag3,
    }

    public enum ESlaveStateMachine
    {
        EC_DEVICE_STATE_INIT = 0x01,
        EC_DEVICE_STATE_PREOP = 0x02,
        EC_DEVICE_STATE_BOOTSTRAP = 0x03,
        EC_DEVICE_STATE_SAFEOP = 0x04,
        EC_DEVICE_STATE_OP = 0x08,
        EC_DEVICE_STATE_ERROR = 0x10,
        EC_DEVICE_STATE_INVALID_VPRS = 0x20,
        EC_DEVICE_STATE_INITCMD_ERROR = 0x40,
        EC_DEVICE_STATE_DISABLED = 0x80,
        UnAvailable = 0x99
    }

    public enum ESlaveLink
    {
        EC_LINK_STATE_OK = 0x00,
        EC_LINK_STATE_NOT_PRESENT = 0x01,
        EC_LINK_STATE_LINK_WITHOUT_COMM = 0x02,
        EC_LINK_STATE_MISSING_LINK = 0x04,
        EC_LINK_STATE_ADDITIONAL_LINK = 0x08,
        EC_LINK_STATE_PORT_A = 0x10,
        EC_LINK_STATE_PORT_B = 0x20,
        EC_LINK_STATE_PORT_C = 0x40,
        EC_LINK_STATE_PORT_D = 0x80,
        EC_LINK_STATE_Other_Fail = 0x24,
        UnAvailable = 0x99
    }

    internal class ST_EcSlaveState : ObservableObject
    {
        private int slaveID;

        public int SlaveID
        {
            get { return slaveID; }
            set { slaveID = value; }
        }

        private string slaveName;
        private ESlaveStateMachine _Slave_State_Machine;
        private ESlaveLink _Slave_Link_State;

        public string SlaveName
        { get => slaveName; set { slaveName = value; OnPropertyChanged(); } }

        public ESlaveStateMachine Slave_State_Machine
        {
            get => _Slave_State_Machine;
            set
            {
                _Slave_State_Machine = value;
                OnPropertyChanged();
            }
        }

        public ESlaveLink Slave_Link_State
        {
            get => _Slave_Link_State;

            set
            {
                _Slave_Link_State = value;
                OnPropertyChanged();
            }
        }

        private string _ToolTip;

        public string ToolTip
        {
            get { return _ToolTip; }
            set
            {
                _ToolTip = value;
                OnPropertyChanged();
            }
        }
        private SolidColorBrush _StrokeColor;
        public SolidColorBrush StrokeColor
        {
            get { return _StrokeColor; }
            set
            {
                _StrokeColor = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush _FillColor;

        public SolidColorBrush FillColor
        {
            get { return _FillColor; }
            set
            {
                _FillColor = value;
                OnPropertyChanged();
            }
        }
    }

    public class Foe : ObservableObject
    {
        private SolidColorBrush _ErrorForeground;

        public SolidColorBrush ErrorForeground
        { get => _ErrorForeground; set { _ErrorForeground = value; OnPropertyChanged(); } }

        private SolidColorBrush _BusyForeground;

        public SolidColorBrush BusyForeground
        { get => _BusyForeground; set { _BusyForeground = value; OnPropertyChanged(); } }

        private uint _Foe_dwPass;

        public UInt32 Foe_dwPass
        {
            get => _Foe_dwPass;
            set
            {
                _Foe_dwPass = value;
                OnPropertyChanged();
            }
        }

        public bool Init_Status { get; set; }
        public bool Foe_Status { get; set; }
        public bool Op_Status { get; set; }
        public UInt16 Foe_nSlaveAddr { get; set; }
        private string _Foe_sPathName;

        public string Foe_sPathName
        {
            get => _Foe_sPathName; set { _Foe_sPathName = value; OnPropertyChanged(); }
        }

        public string Foe_sNetId { get; set; }
        private bool _Foe_bLoad;

        public bool Foe_bLoad
        { get => _Foe_bLoad; set { _Foe_bLoad = value; OnPropertyChanged(); } }

        private bool _Foe_bBusy;

        public bool Foe_bBusy
        {
            get => _Foe_bBusy; set
            {
                _Foe_bBusy = value;
                if (value)
                {
                    BusyForeground = Brushes.Yellow;
                }
                else
                {
                    BusyForeground = Brushes.White;
                }
            }
        }

        public bool Status { get; set; }
        public bool Ecs_bBusy { get; set; }
        public bool Ecs_bError { get; set; }
        public UInt32 Ecs_nErrId { get; set; }
        private bool _Foe_bError;

        public bool Foe_bError
        {
            get => _Foe_bError; set
            {
                _Foe_bError = value;
                if (value)
                {
                    ErrorForeground = Brushes.Red;
                }
                else
                {
                    ErrorForeground = Brushes.White;
                }
            }
        }

        private UInt32 _Foe_nErrID;

        public UInt32 Foe_nErrID
        { get => _Foe_nErrID; set { _Foe_nErrID = value; OnPropertyChanged(); } }

        private UInt32 _Foe_nBytesWritten;

        public UInt32 Foe_nBytesWritten
        { get => _Foe_nBytesWritten; set { _Foe_nBytesWritten = value; OnPropertyChanged(); } }

        private UInt32 _Foe_nPercent;

        public UInt32 Foe_nPercent
        { get => _Foe_nPercent; set { _Foe_nPercent = value; OnPropertyChanged(); } }

        public Foe ShallowCopy()
        {
            return (Foe)this.MemberwiseClone();
        }
    }
    public class MotorStatusControl : ObservableObject
    {
        private int motorID;
        public int MotorID
        { get => motorID; set { motorID = value; OnPropertyChanged(); } }
        public float MotorPosition
        { get => motorPosition; set { motorPosition = value; OnPropertyChanged(); } }
        private float motorPosition;

        private SolidColorBrush _FillColor;
        public SolidColorBrush FillColor
        {
            get { return _FillColor; }
            set
            {
                _FillColor = value;
                OnPropertyChanged();
            }
        }
        private SolidColorBrush _StrokeColor;
        public SolidColorBrush StrokeColor
        {
            get { return _StrokeColor; }
            set
            {
                _StrokeColor = value;
                OnPropertyChanged();
            }
        }
        private string _ToolTip;
        public string ToolTip
        {
            get { return _ToolTip; }
            set
            {
                _ToolTip = value;
                OnPropertyChanged();
            }
        }
    }
}
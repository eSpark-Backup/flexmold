using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    public class MotorTestViewModel : ObservableObject, IMotorTestViewModel
    {
        private Logger log;
        public RelayCommand SendAllCommand { get; set; }
        public RelayCommand SendRPMCommand { get; set; }
        public RelayCommand SendDisplacementCommand { get; set; }
        public RelayCommand SendTimeCommand { get; set; }
        public RelayCommand SendRampCommand { get; set; }
        public RelayCommand OnLoadedCommand { get; set; }
        public RelayCommand OnUnLoadedCommand { get; set; }
        public int structSize { get; set; }
        public bool justread { get; set; }
        public uint DeviceVariableHandle { get; set; }
        public Symbol SymbolEthercatDevice { get; private set; }
        private EthercatDevice[] _MyPLCMotor;

        public EthercatDevice[] MyPLCMotor
        {
            get { return _MyPLCMotor; }
            set
            {
                _MyPLCMotor = value;
                OnPropertyChanged();
            }
        }

        private uint[] _DeviceIds;

        public uint[] DeviceIds
        {
            get { return _DeviceIds; }
            set
            {
                _DeviceIds = value;
                OnPropertyChanged();
            }
        }
        
        private double _motorCount;
        public double motorCount
        {
            get { return _motorCount; }
            set
            {
                _motorCount = value;
                OnPropertyChanged();
            }
        }

        private double _Position; 
        public double Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                OnPropertyChanged();
            }
        }

        private uint _SelectedMotorIndex;

        public uint SelectedMotorIndex
        {
            get { return _SelectedMotorIndex; }
            set
            {
                _SelectedMotorIndex = value;
                _TwinCathelper.ReadEthercatDeviceData(value);
                SetContols(value);
                OnPropertyChanged();
            }
        }
        private void SetContols(uint value)
        {
            justread = true;
            SelectedMotorDirValue = MyPLCMotor[value].mDirection == false ? 0 : 1;
            SelectedMotorModeValue = (ETMode)MyPLCMotor[value].mMode;
            SelectedMotorRPM = MyPLCMotor[value].mRPM;
            SelectedMotorDisplacementValue = MyPLCMotor[value].mDisplacement;
            SelectedMotorTimeValue = MyPLCMotor[value].mTime;
            SelectedMotorRampDelayValue = MyPLCMotor[value].mRampDelay;
            Position = _TwinCathelper.PLCMotorsStatuses.mPostion[SelectedMotorIndex];
            justread = false;
        }

        public Symbol PositionSymbol { get; set; }
        public IEnumerable<ETMode> EnumModes => Enum.GetValues(typeof(ETMode)).Cast<ETMode>();

        private ETMode _SelectedMotorModeValue;
        public ETMode SelectedMotorModeValue
        {
            get { return _SelectedMotorModeValue; }
            set
            {
                if (!justread)
                {
                    MyPLCMotor[SelectedMotorIndex].mMode = (uint)value;
                    _TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex,TwinCathelper.DeviceSymbol.mMode,MyPLCMotor[SelectedMotorIndex].mMode.CastToArray());
                }
                _SelectedMotorModeValue = (ETMode)MyPLCMotor[SelectedMotorIndex].mMode;
                OnPropertyChanged();
            }
        }

        private int _SelectedMotorDirValue;

        public int SelectedMotorDirValue
        {
            get
            {
                return _SelectedMotorDirValue;
            }
            set
            {
                if (!justread)
                {
                    MyPLCMotor[SelectedMotorIndex].mDirection = value == 0 ? false : true;
                    _TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex, TwinCathelper.DeviceSymbol.mDirection,
                   new byte[] { MyPLCMotor[SelectedMotorIndex].mDirection.CastToArray().FirstOrDefault() });
                }
                _SelectedMotorDirValue = MyPLCMotor[SelectedMotorIndex].mDirection == false ? 0 : 1;
                OnPropertyChanged();
            }
        }

        private double _SelectedMotorTimeValue;

        public double SelectedMotorTimeValue
        {
            get { return _SelectedMotorTimeValue; }
            set
            {
                _SelectedMotorTimeValue = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }
        private double _SelectedMotorRampDelayValue;

        public double SelectedMotorRampDelayValue
        {
            get { return _SelectedMotorRampDelayValue; }
            set
            {
                _SelectedMotorRampDelayValue = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        private double _SelectedMotorRPM;
        public double SelectedMotorRPM
        {
            get { return _SelectedMotorRPM; }
            set
            {
                _SelectedMotorRPM = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }
        private double _SelectedMotorDisplacementValue;

        public double SelectedMotorDisplacementValue
        {
            get { return _SelectedMotorDisplacementValue; }
            set
            {
                _SelectedMotorDisplacementValue = Math.Round(value, 2);

                OnPropertyChanged();
            }
        }

        public byte[] buffer { get; set; }

        bool positionNofication = false;
        private readonly ITwinCathelper _TwinCathelper;

        public void OnLoaded()
        {
            if (!positionNofication)
            {

                _TwinCathelper.TMotor_PositionChanged += Client_AdsNotificationMT;
                SelectedMotorIndex = 0;
                positionNofication = true;
            }
        }

        private void OnUnLoaded()
        {
            //if (_TwinCathelper.Client.State == ConnectionState.Connected)
            //{
            //    foreach (var item in NotificatioHandles)
            //    {
            //        if (item != -1)
            //        {
            //            _TwinCathelper.Client.DeleteDeviceNotification((uint)item);
            //        }
            //    }
            //    _TwinCathelper.Client.AdsNotification -= Client_AdsNotificationMT;
            //}
        }
        private void Client_AdsNotificationMT(object sender, EventArgs e)
        {
            Position = _TwinCathelper.PLCMotorsStatuses.mPostion[SelectedMotorIndex];
        }
        void WriteRPMValue()
        {
            if (!justread)
            {
                MyPLCMotor[SelectedMotorIndex].mRPM = (float)SelectedMotorRPM;
                _TwinCathelper.WriteEthercatDeviceData(
                   SelectedMotorIndex,
                   TwinCathelper.DeviceSymbol.mRPM,
                   MyPLCMotor[SelectedMotorIndex].mRPM.CastToArray());
                log.Log(LogLevel.Info, "Motor RPM Value Has Been Written");

            }
            else
            {
                justread = false;
            }
        }
        void WriteSpeedValue()
        {
            if (!justread)
            {
                MyPLCMotor[SelectedMotorIndex].mDisplacement = (float)SelectedMotorDisplacementValue;
                _TwinCathelper.WriteEthercatDeviceData(
                    SelectedMotorIndex,
                    TwinCathelper.DeviceSymbol.mDisplacement,
                    MyPLCMotor[SelectedMotorIndex].mDisplacement.CastToArray());
                log.Log(LogLevel.Info, "Motor Displacement Value Has Been Written");

                Thread.Sleep(200);
                MyPLCMotor[SelectedMotorIndex].mDisplacement = 0;
                _TwinCathelper.WriteEthercatDeviceData(
                   SelectedMotorIndex,
                   TwinCathelper.DeviceSymbol.mDisplacement,
                   MyPLCMotor[SelectedMotorIndex].mDisplacement.CastToArray());
                log.Log(LogLevel.Info, "Motor Displacement Value Has Been Written 0");

            }
            else
            {
                justread = false;
            }
        }
        void WriteTimeValue()
        {
            if (!justread)
            {
                MyPLCMotor[SelectedMotorIndex].mTime = (float)SelectedMotorTimeValue;
                _TwinCathelper.WriteEthercatDeviceData(
                    SelectedMotorIndex,
                    TwinCathelper.DeviceSymbol.mTime,
                    MyPLCMotor[SelectedMotorIndex].mTime.CastToArray());
                log.Log(LogLevel.Info, "Motor Time Value Has Been Written");

            }
            else
            {
                justread = false;
            }
        }
        private void WriteRampValue()
        {
            if (!justread)
            {
                MyPLCMotor[SelectedMotorIndex].mRampDelay = (float)SelectedMotorRampDelayValue;
                _TwinCathelper.WriteEthercatDeviceData(
                    SelectedMotorIndex,
                    TwinCathelper.DeviceSymbol.mRampDelay,
                    MyPLCMotor[SelectedMotorIndex].mRampDelay.CastToArray());
                log.Log(LogLevel.Info, "Motor Ramp Delay Value Has Been Written");
            }
            else
            {
                justread = false;
            }
        }

        private void WriteAllValue()
        {
            if (!justread)
            {
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    MyPLCMotor[i].mDisplacement = (float)SelectedMotorDisplacementValue;
                    MyPLCMotor[i].mRPM = (float)SelectedMotorRPM;
                }
                _TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mDisplacement);
                log.Log(LogLevel.Info, "All Motors Displacement Value Has Been Written");
                _TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mRPM);
                log.Log(LogLevel.Info, "All Motors RPM Value Has Been Written");
                Thread.Sleep(200);
                for (int i = 0; i < TwinCathelper.DeviceCount; i++)
                {
                    MyPLCMotor[i].mDisplacement = 0;
                }
                _TwinCathelper.WriteEthercatDeviceData(TwinCathelper.DeviceSymbol.mDisplacement);
                log.Log(LogLevel.Info, "All Motors Displacement Value Has Been Written : 0");
            }
            else
            {
                justread = false;
            }
        }
        public MotorTestViewModel(ITwinCathelper twinCathelper)
        {
            this._TwinCathelper = twinCathelper;
            if (_TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Test");
            MyPLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            MyPLCMotor = _TwinCathelper.PLCMotors;
            SetContols(SelectedMotorIndex);
            OnLoadedCommand = new RelayCommand(o => OnLoaded());
            OnUnLoadedCommand = new RelayCommand(o => { OnUnLoaded(); });
            SendRPMCommand = new RelayCommand(o => { WriteRPMValue(); });
            SendDisplacementCommand = new RelayCommand(o => { WriteSpeedValue(); });
            SendTimeCommand = new RelayCommand(o => { WriteTimeValue(); });
            SendRampCommand = new RelayCommand(o => { WriteRampValue(); });
            SendAllCommand = new RelayCommand(o => { WriteAllValue(); });
        }
    }
}

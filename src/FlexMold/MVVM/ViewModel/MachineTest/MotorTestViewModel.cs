using EnvDTE80;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    internal class MotorTestViewModel : ObservableObject
    {
        private Logger log;
        public RelayCommand SendRPMCommand { get; set; }
        public RelayCommand SendDisplacementCommand { get; set; }
        public RelayCommand SendTimeCommand { get; set; }
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
                TwinCathelper.ReadEthercatDeviceData(value);
                SetContols(value);
                var mtarray = new byte[sizeof(UInt32)];
                TwinCathelper.Client.Read(PositionSymbol.IndexGroup,
                   PositionSymbol.IndexOffset + (uint)SelectedMotorIndex * sizeof(UInt32), mtarray.AsMemory());
                Position = BitConverter.ToUInt32(mtarray) / 1000;
                OnPropertyChanged();
            }
        }

        private void SetContols(uint value)
        {
            justread = true;
            SelectedMotorDirValue = MyPLCMotor[value].mDirection == false ? 0 : 1;
            SelectedMotorModeValue = MyPLCMotor[value].mMode == false ? 0 : 1;
            SelectedMotorRPM = MyPLCMotor[value].mRPM;
            SelectedMotorDisplacementValue = MyPLCMotor[value].mDisplacement;
            SelectedMotorTimeValue = MyPLCMotor[value].mTime;
            justread = false;
        }

        public Symbol PositionSymbol { get; set; }

        private int _SelectedMotorModeValue;

        public int SelectedMotorModeValue
        {
            get { return _SelectedMotorModeValue; }
            set
            {
                if (!justread)
                {
                    MyPLCMotor[SelectedMotorIndex].mMode = value == 0 ? false : true;
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                _SelectedMotorModeValue = MyPLCMotor[SelectedMotorIndex].mMode == false ? 0 : 1;
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
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                _SelectedMotorDirValue = MyPLCMotor[SelectedMotorIndex].mDirection == false ? 0 : 1;
                OnPropertyChanged();
            }
        }

        private UInt32 _SelectedMotorTimeValue;

        public UInt32 SelectedMotorTimeValue
        {
            get { return _SelectedMotorTimeValue; }
            set
            {
                _SelectedMotorTimeValue = value;
                OnPropertyChanged();
            }
        }

        private UInt32 _SelectedMotorRPM;
        public UInt32 SelectedMotorRPM
        {
            get { return _SelectedMotorRPM; }
            set
            {
                _SelectedMotorRPM = value;

                OnPropertyChanged();
            }
        }
        private UInt32 _SelectedMotorDisplacementValue;

        public UInt32 SelectedMotorDisplacementValue
        {
            get { return _SelectedMotorDisplacementValue; }
            set
            {
                _SelectedMotorDisplacementValue = value;

                OnPropertyChanged();
            }
        }

        public byte[] buffer { get; set; }

        private List<int> NotificatioHandles;
        bool positionNofication = false;
        public void OnLoaded()
        {
            if (!positionNofication)
            {
                NotificatioHandles = new List<int>
                {
                    TwinCathelper.RegisterNotification(TCVarName.mPosition)
                };
                TwinCathelper.Client.AdsNotification += Client_AdsNotificationMT;
                SelectedMotorIndex = 0;
                positionNofication = true;
            }
        }

        private void OnUnLoaded()
        {
            //if (TwinCathelper.Client.State == ConnectionState.Connected)
            //{
            //    foreach (var item in NotificatioHandles)
            //    {
            //        if (item != -1)
            //        {
            //            TwinCathelper.Client.DeleteDeviceNotification((uint)item);
            //        }
            //    }
            //    TwinCathelper.Client.AdsNotification -= Client_AdsNotificationMT;
            //}
        }
        double factorPostion = (double)2 / (double)360;
        private void Client_AdsNotificationMT(object sender, AdsNotificationEventArgs e)
        {
            try
            {
                // Or here we know about UDINT type --> can be marshalled as UINT32
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol != null)
                {
                    switch (_mysymbol.InstancePath)
                    {
                        case TCVarName.mPosition:
                            Position = BitConverter.ToSingle(e.Data.ToArray(), (int)SelectedMotorIndex * sizeof(UInt32)) / 1000;
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
        }
        void WriteRPMValue()
        {
            if (!justread)
            {
                MyPLCMotor[SelectedMotorIndex].mRPM = SelectedMotorRPM;
                TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
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
                MyPLCMotor[SelectedMotorIndex].mDisplacement = SelectedMotorDisplacementValue * 1600;
                TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                Thread.Sleep(200);
                MyPLCMotor[SelectedMotorIndex].mDisplacement = 0;
                TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
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
                MyPLCMotor[SelectedMotorIndex].mTime = SelectedMotorTimeValue;
                TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
            }
            else
            {
                justread = false;
            }
        }

        public MotorTestViewModel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Test");
            MyPLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            PositionSymbol = TwinCathelper.MySymbols.Where(a => a.InstancePath == $"{TCVarName.mPosition}").FirstOrDefault();
            SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == TCVarName.MainDevice).FirstOrDefault();
            structSize = (SymbolEthercatDevice.Size) / TwinCathelper.DeviceCount;
            buffer = new byte[structSize * TwinCathelper.DeviceCount];
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            MyPLCMotor = TwinCathelper.PLCMotors;
            SetContols(SelectedMotorIndex);

            OnLoadedCommand = new RelayCommand(o => OnLoaded());
            OnUnLoadedCommand = new RelayCommand(o => { OnUnLoaded(); });
            SendRPMCommand = new RelayCommand(o => { WriteRPMValue(); });
            SendDisplacementCommand = new RelayCommand(o => { WriteSpeedValue(); });
            SendTimeCommand = new RelayCommand(o => { WriteTimeValue(); });
        }
    }
}
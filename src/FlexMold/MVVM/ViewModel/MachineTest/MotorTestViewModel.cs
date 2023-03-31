using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using LogLevel = NLog.LogLevel;
using System.Windows;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    internal class MotorTestViewModel : ObservableObject
    {
        Logger log;
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
        private float _Position;
        public float Position
        {
            get { return _Position; }
            set { _Position = value; OnPropertyChanged(); }
        }
        private uint _SelectedMotorIndex;
        public uint SelectedMotorIndex
        {
            get { return _SelectedMotorIndex; }
            set
            {
                _SelectedMotorIndex = value;
                justread = true;
                TwinCathelper.ReadEthercatDeviceData(value);
                SelectedMotorSpeedValue = MyPLCMotor[value].mSpeed;
                SelectedMotorTimeValue = MyPLCMotor[value].mTime;
                SelectedMotorDirValue = MyPLCMotor[value].mDirection;
                SelectedMotorModeValue = MyPLCMotor[value].mMode;
                var mtarray = new byte[sizeof(UInt32)];
                TwinCathelper.Client.Read(PositionSymbol.IndexGroup,
                   PositionSymbol.IndexOffset + (uint)SelectedMotorIndex * sizeof(UInt32), mtarray.AsMemory());
                Position = BitConverter.ToUInt32(mtarray) / 1000;
                OnPropertyChanged();
            }
        }
        public Symbol PositionSymbol { get; set; }

        private bool _SelectedMotorModeValue;
        public bool SelectedMotorModeValue
        {
            get { return _SelectedMotorModeValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMotorModeValue = value;
                    MyPLCMotor[SelectedMotorIndex].mMode = SelectedMotorModeValue;
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorModeValue = value;
                    justread = false;
                }
                OnPropertyChanged();
            }
        }
        private bool _SelectedMotorDirValue;
        public bool SelectedMotorDirValue
        {
            get { return _SelectedMotorDirValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMotorDirValue = value;
                    MyPLCMotor[SelectedMotorIndex].mDirection = SelectedMotorDirValue;
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorDirValue = value;
                    justread = false;
                }
                OnPropertyChanged();
            }
        }
        private UInt32 _SelectedMotorTimeValue;
        public UInt32 SelectedMotorTimeValue
        {
            get { return _SelectedMotorTimeValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMotorTimeValue = value;
                    MyPLCMotor[SelectedMotorIndex].mTime = SelectedMotorTimeValue;
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorSpeedValue = value;
                    justread = false;
                }
                OnPropertyChanged();
            }
        }
        private UInt32 _SelectedMotorSpeedValue;
        public UInt32 SelectedMotorSpeedValue
        {
            get { return _SelectedMotorSpeedValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMotorSpeedValue = value;
                    MyPLCMotor[SelectedMotorIndex].mSpeed = SelectedMotorSpeedValue;
                    TwinCathelper.WriteEthercatDeviceData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorSpeedValue = value;
                    justread = false;
                }
                OnPropertyChanged();
            }
        }
        public byte[] buffer { get; set; }
        
        List<int> NotificatioHandles;
        public void OnLoaded()
        {
            NotificatioHandles = new List<int>
            {
                TwinCathelper.RegisterNotification(TCVarName.mPosition)
            };
            TwinCathelper.Client.AdsNotification += Client_AdsNotificationMT;
            SelectedMotorIndex = 0;
        }
        void OnUnLoaded()
        {
            if (TwinCathelper.Client.State == ConnectionState.Connected)
            {
                foreach (var item in NotificatioHandles)
                {
                    if (item != -1)
                    {
                        TwinCathelper.Client.DeleteDeviceNotification((uint)item);
                    }
                }
                TwinCathelper.Client.AdsNotification -= Client_AdsNotificationMT;
            }
        }
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
                            Position = BitConverter.ToUInt32(e.Data.ToArray(), (int)SelectedMotorIndex * sizeof(UInt32)) / 1000;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(NLog.LogLevel.Error, ex.Message);
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
            justread = true;
            SelectedMotorSpeedValue = MyPLCMotor[SelectedMotorIndex].mSpeed;
            SelectedMotorDirValue = Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].mDirection);
            justread = false;
            OnLoadedCommand = new RelayCommand(o => { OnLoaded(); });
            OnUnLoadedCommand = new RelayCommand(o => { OnUnLoaded(); });
        }
    }
}

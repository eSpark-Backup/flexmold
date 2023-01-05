using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.VisualStudio.OLE.Interop;
using NLog;
using NLog.Fluent;
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineSetting
{
    internal class MotorTestViewModel : ObservableObject
    {
        public Logger log;
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
        private uint _SelectedMotorIndex;
        public uint SelectedMotorIndex
        {
            get { return _SelectedMotorIndex; }
            set
            {
                _SelectedMotorIndex = value;
                justread = true;
                ReadData(value);
                SelectedMotorSpeedValue = MyPLCMotor[value].mSpeed;
                SelectedMotorTimeValue = MyPLCMotor[value].mTime;
                SelectedMotorDirValue = MyPLCMotor[value].mDirection;
                SelectedMotorModeValue = MyPLCMotor[value].mMode;
                OnPropertyChanged();
            }
        }

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
                    WriteData(SelectedMotorIndex);
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
                    WriteData(SelectedMotorIndex);
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
                    WriteData(SelectedMotorIndex);
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
                    WriteData(SelectedMotorIndex);
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
        void ReadDataAll()
        {
            justread = true;
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                var mtarray = new byte[structSize];
                Array.Copy(buffer, structSize * i, mtarray, 0, structSize);
                MyPLCMotor[i].Unpack(mtarray);
            }
            SelectedMotorSpeedValue = MyPLCMotor[SelectedMotorIndex].mSpeed;
            SelectedMotorDirValue = Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].mDirection);
            justread = false;
        }
        void ReadData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            MyPLCMotor[_motorNumber].Unpack(mtarray);
        }
        void WriteDataAll()
        {
            for (int i = 0; i < MyPLCMotor.Length; i++)
            {
                MyPLCMotor[i].Pack().CopyTo(buffer, structSize * i);
            }
            TwinCathelper._connection.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
        }
        void WriteData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            MyPLCMotor[_motorNumber].Pack().CopyTo(mtarray, 0);
            TwinCathelper._connection.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
        }
        public int structSize { get; set; }
        public UInt32[] mPosition { get; set; }


        public MotorTestViewModel()
        {
            log = LogManager.GetLogger("Motor Test");
            MyPLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.device").FirstOrDefault();
            RegisternNotificationAsyn();
            structSize = (SymbolEthercatDevice.Size) / TwinCathelper.DeviceCount;
            buffer = new byte[structSize * TwinCathelper.DeviceCount];
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            ReadDataAll();
        }
        async void RegisternNotificationAsyn()
        {
            TwinCathelper._connection.AdsNotification += Client_AdsNotification2;

            var SymbolmPosition = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.mPosition").FirstOrDefault();
             await TwinCathelper._connection.AddDeviceNotificationAsync(SymbolmPosition.IndexGroup,
              SymbolmPosition.IndexOffset, (int)(SymbolmPosition.Size), NotificationSettings.Default, SymbolmPosition, CancellationToken.None);
            
            var SymbolmTemperature = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.mTemperature").FirstOrDefault();
            await TwinCathelper._connection.AddDeviceNotificationAsync(SymbolmTemperature.IndexGroup,
                  SymbolmTemperature.IndexOffset, (int)(SymbolmTemperature.Size), NotificationSettings.Default, SymbolmTemperature, CancellationToken.None);
            
            var SymbolmDiag1 = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.mDiag1").FirstOrDefault();
            await TwinCathelper._connection.AddDeviceNotificationAsync(SymbolmDiag1.IndexGroup,
                         SymbolmDiag1.IndexOffset, (int)(SymbolmDiag1.Size), NotificationSettings.Default, SymbolmDiag1, CancellationToken.None);

            var SymbolmDiag2 = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.mDiag2").FirstOrDefault();
            await TwinCathelper._connection.AddDeviceNotificationAsync(SymbolmDiag2.IndexGroup,
                         SymbolmDiag2.IndexOffset, (int)(SymbolmDiag2.Size), NotificationSettings.Default, SymbolmDiag2, CancellationToken.None);
            
            
            var SymbolmDiag3 = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.mDiag3").FirstOrDefault();
            await TwinCathelper._connection.AddDeviceNotificationAsync(SymbolmDiag3.IndexGroup,
                     SymbolmDiag3.IndexOffset, (int)(SymbolmDiag3.Size), NotificationSettings.Default, SymbolmDiag3, CancellationToken.None);

        }
        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
        {
            // Or here we know about UDINT type --> can be marshalled as UINT32
            var _mysymbol = e.UserData as Symbol;
            Console.WriteLine();
            switch (_mysymbol.InstanceName)
            {
                case "mPosition":
                    log.Log(LogLevel.Info, $"Position Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    break;
                case "mTemperature":
                    log.Log(LogLevel.Info, $"Temperature Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    Console.WriteLine();
                    break;
                case "mDiag1":
                    log.Log(LogLevel.Info, $"Diag1 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    Console.WriteLine();
                    break;
                case "mDiag2":
                    log.Log(LogLevel.Info, $"Diag3 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    Console.WriteLine();
                    break;
                case "mDiag3":
                    log.Log(LogLevel.Info, $"Diag3 Updated : {String.Join(",", (UInt32[])_mysymbol.ReadAnyValue(typeof(UInt32[])))}");
                    Console.WriteLine();
                    break;
                default:
                    break;
            }

        }
    }
}

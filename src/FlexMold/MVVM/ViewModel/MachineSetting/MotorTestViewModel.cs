using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.Linq;
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
                ReadData(value);
                SelectedMotorSpeedValue = MyPLCMotor[value].speed;
                SelectedMotorDirValue = Convert.ToBoolean(MyPLCMotor[value].dir);
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
                    MyPLCMotor[SelectedMotorIndex].dir = Convert.ToByte(SelectedMotorDirValue);
                    WriteData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorDirValue = value;
                }
                OnPropertyChanged();
            }
        }
        private UInt16 _SelectedMotorSpeedValue;
        public UInt16 SelectedMotorSpeedValue
        {
            get { return _SelectedMotorSpeedValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMotorSpeedValue = value;
                    MyPLCMotor[SelectedMotorIndex].speed = SelectedMotorSpeedValue;
                    WriteData(SelectedMotorIndex);
                }
                else
                {
                    _SelectedMotorSpeedValue = value;
                }
                OnPropertyChanged();
            }
        }
        public byte[] buffer { get; set; }
        void ReadData()
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
            SelectedMotorSpeedValue = MyPLCMotor[SelectedMotorIndex].speed;
            SelectedMotorDirValue = Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].dir);
            justread = false;
        }
        void ReadData(uint _motorNumber)
        {
            justread = true;
            var mtarray = new byte[structSize];
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            Array.Copy(buffer, structSize * _motorNumber, mtarray, 0, structSize);
            MyPLCMotor[_motorNumber].Unpack(mtarray);
            
            justread = false;
        }
        void WriteData()
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
        public MotorTestViewModel()
        {

            log = LogManager.GetLogger("Motor Test");
            MyPLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.device").FirstOrDefault();
            structSize = (SymbolEthercatDevice.Size) / TwinCathelper.DeviceCount;
            buffer = new byte[structSize * TwinCathelper.DeviceCount];
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup, SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            justread = true;
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            ReadData();
            
            justread = false;
        }
    }
}

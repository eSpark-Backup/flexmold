using FlexMold.Core;
using FlexMold.Utility;
using NLog.Fluent;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using System.Windows.Media;
using TwinCAT;
using System.Data.Common;
using TwinCAT.Ads;
using FlexMold.MVVM.Model;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Documents;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.CodeDom;

namespace FlexMold.MVVM.ViewModel.MachineSetting
{
    internal class MotorTestViewModel : ObservableObject
    {
        public Logger log;
        public bool justread { get; set; }
        public uint DeviceVariableHandle { get; set; }
        public Symbol SymbolEthercatDevice { get; private set; }
        public IDataType DTEthercatDevice { get; private set; }

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
                ReadDeviceValue();
                OnPropertyChanged();
            }
        }
        private bool _SelectedMototDirValue;
        public bool SelectedMototDirValue
        {
            get { return _SelectedMototDirValue; }
            set
            {
                if (!justread)
                {
                    _SelectedMototDirValue = value;
                    UpdatePLCMotorToTwicat();
                }
                else
                {
                    _SelectedMototDirValue = value;
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
                    UpdatePLCMotorToTwicat();
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
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            for (int i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                var mtarray = new byte[structSize];
                Array.Copy(buffer, structSize * i, mtarray, 0, structSize);
                MyPLCMotor[i] = MixUtil.CastToStruct<EthercatDevice>(mtarray);
            }
        }
        void ReadData(int _motorNumber)
        {
            var mtarray = new byte[structSize];
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup, 
                SymbolEthercatDevice.IndexOffset+ (uint)_motorNumber*(uint)structSize, mtarray.AsMemory());
            Array.Copy(buffer, structSize * _motorNumber, mtarray, 0, structSize);
            MyPLCMotor[_motorNumber] = MixUtil.CastToStruct<EthercatDevice>(mtarray);
        }
        void WriteData()
        {
            for (int i = 0; i < MyPLCMotor.Length; i++)
            {
                MixUtil.CastToArray(MyPLCMotor[i]).CopyTo(buffer, structSize * i);
            }
            TwinCathelper._connection.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
        }
        void WriteData(int _motorNumber)
        {
            var mtarray = new byte[structSize];
            MixUtil.CastToArray(MyPLCMotor[_motorNumber]).CopyTo(mtarray,0);
            TwinCathelper._connection.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
        }
        public int structSize { get; set; }
        public MotorTestViewModel()
        {
            log = LogManager.GetLogger("Motor Test");
            MyPLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            DeviceVariableHandle = TwinCathelper._connection.CreateVariableHandle("MAIN.device");
            SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == "MAIN.device").FirstOrDefault();
            DTEthercatDevice = TwinCathelper.dataTypes.Where(a => a.Name.EndsWith("EthercatDevice")).FirstOrDefault();
            structSize = (DTEthercatDevice.Size) / TwinCathelper.DeviceCount;
            buffer = new byte[structSize * TwinCathelper.DeviceCount];
            TwinCathelper._connection.Read(SymbolEthercatDevice.IndexGroup, SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            ReadData(2);
            MyPLCMotor[1].speed = 1100;
            MyPLCMotor[1].dir = 11;
            MyPLCMotor[1].dummyDataIn = 110000;
            MyPLCMotor[1].dummyDataOut = 11000;
            WriteData(1);
            justread = true;
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            SelectedMotorSpeedValue = MyPLCMotor[SelectedMotorIndex].speed;
            SelectedMototDirValue = Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].dir);
            justread = false;
        }
        void ReadDeviceValue()
        {
            justread = true;
            //MyPLCMotor = (PLCMotor)TwinCathelper._connection.ReadAny(DeviceVariableHandle, typeof(PLCMotor));
            if (MyPLCMotor[SelectedMotorIndex].speed != SelectedMotorSpeedValue)
                SelectedMotorSpeedValue = MyPLCMotor[SelectedMotorIndex].speed;
            if (Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].dir) != SelectedMototDirValue)
                SelectedMototDirValue = Convert.ToBoolean(MyPLCMotor[SelectedMotorIndex].dir);
            log.Log(LogLevel.Info, $"Readed Value Sucessfully");
            justread = false;
        }
        bool UpdatePLCMotorToTwicat()
        {
            var _PLCMotor = new EthercatDevice[TwinCathelper.DeviceCount];
            _PLCMotor = MyPLCMotor;
            _PLCMotor[SelectedMotorIndex].dir = Convert.ToByte(SelectedMototDirValue);
            _PLCMotor[SelectedMotorIndex].speed = SelectedMotorSpeedValue;
            TwinCathelper._connection.WriteAny(DeviceVariableHandle, _PLCMotor);
            log.Log(LogLevel.Info, $"Set");
            MyPLCMotor = _PLCMotor;
            return true;
        }
    }
}

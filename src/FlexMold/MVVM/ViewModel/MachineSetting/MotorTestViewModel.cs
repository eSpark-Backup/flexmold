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

namespace FlexMold.MVVM.ViewModel.MachineSetting
{
    internal class MotorTestViewModel : ObservableObject
    {
        public Logger log;
        public bool justread { get; set; }
        public uint DeviceVariableHandle { get; set; }

        private PLCMotor _MyPLCMotor;

        public PLCMotor MyPLCMotor
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
        private uint _SelectedMotorDirIndex;
        public uint SelectedMotorDirIndex
        {
            get { return _SelectedMotorDirIndex; }
            set
            {
                _SelectedMotorDirIndex = value;
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
                    if (UpdatePLCMotorToTwicat())
                    {
                        _SelectedMototDirValue = value;
                    }
                }
                else
                {
                    _SelectedMototDirValue = value;
                }
                OnPropertyChanged();
            }
        }
        private uint _SelectedMotorSpeedIndex;
        public uint SelectedMotorSpeedIndex
        {
            get { return _SelectedMotorSpeedIndex; }
            set
            {
                _SelectedMotorSpeedIndex = value;
                ReadDeviceValue();
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
                    if (UpdatePLCMotorToTwicat())
                    {
                        _SelectedMotorSpeedValue = value;
                    }
                }
                else
                {
                    _SelectedMotorSpeedValue = value;
                }
                OnPropertyChanged();
            }
        }
        public MotorTestViewModel()
        {
            log = LogManager.GetLogger("Motor Test");
            DeviceVariableHandle = TwinCathelper._connection.CreateVariableHandle("MAIN.device");
            MyPLCMotor = (PLCMotor)TwinCathelper._connection.ReadAny(DeviceVariableHandle, typeof(PLCMotor));
            justread = true;
            DeviceIds = new uint[MyPLCMotor.MyEthercatDevice.Count()];
            for (uint i = 0; i < MyPLCMotor.MyEthercatDevice.Count(); i++)
            {
                DeviceIds[i] = i;
            }
            SelectedMotorSpeedValue = MyPLCMotor.MyEthercatDevice[SelectedMotorSpeedIndex].speed;
            SelectedMototDirValue = Convert.ToBoolean(MyPLCMotor.MyEthercatDevice[_SelectedMotorDirIndex].dir);
            justread = false;
        }
        void ReadDeviceValue()
        {
            justread = true;
            MyPLCMotor = (PLCMotor)TwinCathelper._connection.ReadAny(DeviceVariableHandle, typeof(PLCMotor));
            if (MyPLCMotor.MyEthercatDevice[SelectedMotorSpeedIndex].speed != SelectedMotorSpeedValue)
                SelectedMotorSpeedValue = MyPLCMotor.MyEthercatDevice[SelectedMotorSpeedIndex].speed;
            if (Convert.ToBoolean(MyPLCMotor.MyEthercatDevice[_SelectedMotorDirIndex].dir) != SelectedMototDirValue)
                SelectedMototDirValue = Convert.ToBoolean(MyPLCMotor.MyEthercatDevice[_SelectedMotorDirIndex].dir);
            log.Log(LogLevel.Info, $"Readed Value Sucessfully");
            justread = false;
        }
        bool UpdatePLCMotorToTwicat()
        {
            var _PLCMotor = new PLCMotor();
            _PLCMotor = MyPLCMotor;
            _PLCMotor.MyEthercatDevice[SelectedMotorDirIndex].dir = Convert.ToByte(SelectedMototDirValue);
            _PLCMotor.MyEthercatDevice[SelectedMotorSpeedIndex].speed = SelectedMotorSpeedValue;
            TwinCathelper._connection.WriteAny(DeviceVariableHandle, _PLCMotor);
            log.Log(LogLevel.Info, $"Set");
            MyPLCMotor = _PLCMotor;
            return true;
        }
    }
}

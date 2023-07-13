using FlexMold.Core;
using FlexMold.MVVM.View.Setting;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using NLog;
using System;

namespace FlexMold.MVVM.ViewModel.Setting
{
    public class MotorSettingViewModel : ObservableObject, IMotorSettingViewModel
    {
        private Logger log;
        public RelayCommand FirmwareCommand { get; set; }
        private uint _SelectedMotorIndex;
        private bool _IsDialogOpen;

        public bool IsDialogOpen
        {
            get { return _IsDialogOpen; }
            set
            {
                _IsDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public uint SelectedMotorIndex
        {
            get { return _SelectedMotorIndex; }
            set
            {
                _SelectedMotorIndex = value;
                OnPropertyChanged();
            }
        }

        private uint[] _DeviceIds;
        private readonly ITwinCathelper _TwinCathelper;

        public uint[] DeviceIds
        {
            get { return _DeviceIds; }
            set
            {
                _DeviceIds = value;
                OnPropertyChanged();
            }
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine();
        }
        public MotorSettingViewModel(ITwinCathelper twinCathelper)
        {
            this._TwinCathelper = twinCathelper;
            if (_TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Setting");
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
        }
    }
}
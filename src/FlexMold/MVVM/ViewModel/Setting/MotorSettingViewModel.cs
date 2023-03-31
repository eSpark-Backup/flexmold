using ControlzEx.Standard;
using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.Setting;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using NLog;
using SMBLibrary.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FlexMold.MVVM.ViewModel.Setting
{
    class MotorSettingViewModel : ObservableObject
    {
        Logger log;
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
        public uint[] DeviceIds
        {
            get { return _DeviceIds; }
            set
            {
                _DeviceIds = value;
                OnPropertyChanged();
            }
        }
        public MotorSettingViewModel()
        {
            if (TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Motor Setting");
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            FirmwareCommand = new RelayCommand(o => { UpdateFirmware(); });
        }
        async void UpdateFirmware()
        {

            var view = new FoeSettingDiaglog()
            {
                DataContext = new FoeSettingDiaglogViewModel()
            };
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
         }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        { 
            Console.WriteLine();
        }
    }
}

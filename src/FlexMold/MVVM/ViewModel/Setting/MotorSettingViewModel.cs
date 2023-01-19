using FlexMold.Core;
using FlexMold.MVVM.View.Setting;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using NLog;
using SMBLibrary.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Setting
{
    class MotorSettingViewModel : ObservableObject
    {
        Logger log;
        public RelayCommand UpdateFirmwareCommand { get; set; }
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
            log = LogManager.GetLogger("Motor Test");
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            UpdateFirmwareCommand = new RelayCommand(o => { UpdateFirmware(); });
        }
        async void UpdateFirmware()
        {
            IsDialogOpen = true;
            var view = new FoeSettingDiaglog
            {

            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", null, ClosingEventHandler, ClosedEventHandler);

            //var result = TwinCathelper.Client.InvokeRpcMethod("MAIN.fbAdsManager", "adsRemoteCall", new object[] { });

            // Call a Method that has no parameter and returns VOID
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs) 
        {
            Console.WriteLine();
        }
        private void ClosedEventHandler(object sender, DialogClosedEventArgs eventArgs)
        {
            Console.WriteLine();
        }
    }
}

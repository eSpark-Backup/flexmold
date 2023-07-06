using FlexMold.Core;
using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.SplashScreen
{
    internal class DeviceScanDialogViewModel : ObservableObject
    {
        private DevicesAndBoxes _MyDevicesAndBoxes;
        public DevicesAndBoxes MyDevicesAndBoxes
        {
            get { return _MyDevicesAndBoxes; }
            set
            {
                _MyDevicesAndBoxes = value;
                OnPropertyChanged();
            }
        }
        private Foe _SelectedDeviceAndBox;
        public Foe SelectedDeviceAndBox
        {
            get { return _SelectedDeviceAndBox; }
            set
            {
                _SelectedDeviceAndBox = value;
                OnPropertyChanged();
            }
        }
        private int _OverAllProgress;

        public int OverAllProgress
        {
            get { return _OverAllProgress; }
            set { _OverAllProgress = value; }
        }

    }
}

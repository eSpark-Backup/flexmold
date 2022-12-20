using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;

namespace FlexMold.MVVM.ViewModel.Setting
{
    class LaserProjectorSettingViewModel : TelnetHelper
    {
        public RelayCommand TelnetConnectCommand { get; set; }
        public RelayCommand TelnetDisConnectCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        private LaserProjector _LP;
        public LaserProjector LP
        {
            get { return _LP; }
            set
            {
                _LP = value;
                OnPropertyChanged();
            }
        }
        public LaserProjectorSettingViewModel()
        {
            LP = new LaserProjector();
            LP.IpAddres = "aardmud.org";
            LP.TelnetPort = 4000;
            TelnetConnectCommand = new RelayCommand(o => { ConnectSendTelnet(LP.IpAddres, LP.TelnetPort); });
            TelnetDisConnectCommand = new RelayCommand(o => { DisconnectTelnet(); });
            SaveCommand = new RelayCommand(o => { SaveSettings(); });
        }
        void SaveSettings()
        {
            //log.Log(LogLevel.Error, $"Not Implemented");
        }



    }
}

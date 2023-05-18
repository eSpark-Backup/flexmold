using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;

namespace FlexMold.MVVM.ViewModel.Setting
{
    internal class LaserProjectorSettingViewModel : TelnetHelper
    {
        public RelayCommand TelnetConnectCommand { get; set; }
        public RelayCommand TelnetMessageSendCommand { get; set; }
        public RelayCommand TelnetDisConnectCommand { get; set; }
        public RelayCommand ProjectionOnCommand { get; set; }
        public RelayCommand ProjectionOffCommand { get; set; }
        public RelayCommand CallibrateProjectorCommand { get; set; }
        public RelayCommand LaunchLMPCommand { get; set; }
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
            LP.IpAddres = "telehack.com";
            LP.TelnetPort = 23;
            TelnetConnectCommand = new RelayCommand(o => { _ = ConnectTelnet(LP.IpAddres, LP.TelnetPort); });
            TelnetMessageSendCommand = new RelayCommand(o => { _ = SendTelnetMessgae(); });
            TelnetDisConnectCommand = new RelayCommand(o => { DisconnectTelnet(); });

            ProjectionOnCommand = new RelayCommand(o =>
            {
                SendMessage = "echo ZN; " + "\r\n";
                _ = SendTelnetMessgae();
            });
            ProjectionOffCommand = new RelayCommand(o =>
            {
                SendMessage = "echo ZF; " + "\r\n";
                _ = SendTelnetMessgae();
            });
            CallibrateProjectorCommand = new RelayCommand(o =>
            {
                SendMessage = "echo ZD; " + "\r\n";
                _ = SendTelnetMessgae();
            });
            LaunchLMPCommand = new RelayCommand(o => { DisconnectTelnet(); });

            SaveCommand = new RelayCommand(o => { SaveSettings(); });
        }

        private void SaveSettings()
        {
            //log.Log(LogLevel.Error, $"Not Implemented");
        }
    }
}
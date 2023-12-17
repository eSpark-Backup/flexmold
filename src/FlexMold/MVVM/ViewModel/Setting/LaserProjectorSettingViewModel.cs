using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using System.Configuration;

namespace FlexMold.MVVM.ViewModel.Setting
{
    internal class LaserProjectorSettingViewModel //: TelnetHelper
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
                    string ZlaserIP = "";
            int ZlaserPort = 23;

            try
            {
                ZlaserIP = ConfigurationManager.AppSettings["ZlaserIP"].ToString();
                ZlaserPort = int.Parse(ConfigurationManager.AppSettings["ZlaserPort"].ToString());
            }
            catch (System.Exception ex)
            {
                //log.Log(LogLevel.Error, ex, ex.Message);
                TNF.ShowError(ex.Message);
            }



            LP = new LaserProjector();
            LP.IpAddres = ZlaserIP;
            LP.TelnetPort = ZlaserPort;
            TelnetConnectCommand = new RelayCommand(o => { _ = TelnetHelper.Instance.ConnectTelnet(LP.IpAddres, LP.TelnetPort); });
            TelnetMessageSendCommand = new RelayCommand(o => { _ = TelnetHelper.Instance.SendTelnetMessgae(); });
            TelnetDisConnectCommand = new RelayCommand(o => { TelnetHelper.Instance.DisconnectTelnet(); });

            ProjectionOnCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZN; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();
            });
            ProjectionOffCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZF; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();
            });
            CallibrateProjectorCommand = new RelayCommand(o =>
            {
                TelnetHelper.Instance.SendMessage = "echo ZD; " + "\r\n";
                _ = TelnetHelper.Instance.SendTelnetMessgae();
            });
            LaunchLMPCommand = new RelayCommand(o => { TelnetHelper.Instance.DisconnectTelnet(); });

            SaveCommand = new RelayCommand(o => { SaveSettings(); });
        }

        private void SaveSettings()
        {
            //log.Log(LogLevel.Error, $"Not Implemented");
        }
    }
}

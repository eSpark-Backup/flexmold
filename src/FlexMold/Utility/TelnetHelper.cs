using FlexMold.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TentacleSoftware.Telnet;

namespace FlexMold.Utility
{
    class TelnetHelper : ObservableObject
    {
        public string ReceivedMessage
        {
            get { return _ReceivedMessage; }
            set
            {
                _ReceivedMessage = value;
                OnPropertyChanged();
            }
        }
        private string _ReceivedMessage;

        private string _SendMessage;
        public string SendMessage
        {
            get { return _SendMessage; }
            set
            {
                _SendMessage = value;
                OnPropertyChanged();
            }
        }
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public TelnetClient MyTelnetClient { get; set; }

        Logger log;
        public TelnetHelper()
        {
            log = LogManager.GetLogger("Laser Projector");
            SendMessage = "magic!";
           
        }
       
        public void DisconnectTelnet()
        {
            if (MyTelnetClient != null)
            {
                MyTelnetClient.Disconnect();
                MyTelnetClient = null;
                log.Log(LogLevel.Info, $"TelNet Disconnected Sucessfully");
            }
            else
            {
                log.Log(LogLevel.Info, $"No Active Connection Found");
            }
        }
        public async Task<bool> ConnectSendTelnet(string IpAddres, int TelnetPort)
        {
            log.Log(LogLevel.Info, $"Testing TelNet Connection....");
            ReceivedMessage = "";
            // CancellationTokenSource provides the token and have authority to cancel the token
            try
            {
                MyTelnetClient = new TelnetClient(IpAddres, TelnetPort, TimeSpan.FromSeconds(3), cancellationTokenSource.Token);
                MyTelnetClient.ConnectionClosed += HandleConnectionClosed;
                MyTelnetClient.MessageReceived += HandleMessageReceived;
                log.Log(LogLevel.Info, $"Connecting....");
                await MyTelnetClient.Connect();
                log.Log(LogLevel.Info, $"Sending Sessage....");
                await MyTelnetClient.Send(SendMessage);
                log.Log(LogLevel.Info, $"Message Sent Sucessfully");
                return true;
            }
            catch (Exception err)
            {
                log.Log(LogLevel.Error, err.Message);
                return false;
            }
        }
        private void HandleConnectionClosed(object sender, EventArgs e)
        {
            ReceivedMessage = "Connection Closed";
            log.Log(LogLevel.Info, $"Connection Closed");
        }

        private void HandleMessageReceived(object sender, string e)
        {
            ReceivedMessage += e + "\n";
        }
    }
}

using FlexMold.Core;
using FlexMold.MVVM.ViewModel;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlexMold.Utility
{
    public class TelnetHelper : ObservableObject
    {
        private Logger log;
        private static Logger logst;
        public static TelnetHelper instance;
        public static TelnetHelper Instance
        {
            get
            {
                if (instance == null)
                { 
                    instance = new TelnetHelper();
                    logst = LogManager.GetLogger("Laser Projector");
                    logst.Log(LogLevel.Info, $"Zlaser new laser connected!!!!!!!");
                }
                return instance;
            }
        }
        public TelnetHelper()
        {
            log = LogManager.GetLogger("Laser Projector");
            //SendMessage = "echo";
        }

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

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public TelnetClient MyTelnetClient { get; set; }


        public void DisconnectTelnet()
        {
            if (MyTelnetClient != null)
            {
                MyTelnetClient.Disconnect();
                MyTelnetClient = null;
            }
            else
            {
                log.Log(LogLevel.Info, $"No Active Connection Found");
            }
        }

        public async Task<bool> ConnectTelnet(string IpAddres, int TelnetPort)
        {
            ReceivedMessage = "";
            // CancellationTokenSource provides the token and have authority to cancel the token
            try
            {
                if (MyTelnetClient == null)
                {
                    MyTelnetClient = new TelnetClient(IpAddres, TelnetPort, TimeSpan.FromSeconds(3), cancellationTokenSource.Token);
                    MyTelnetClient.ConnectionClosed += HandleConnectionClosed;
                    MyTelnetClient.MessageReceived += HandleMessageReceived;
                    log.Log(LogLevel.Info, $"Connecting to {IpAddres}:{TelnetPort}...");
                    await MyTelnetClient.Connect();
                }
                return true;
            }
            catch (Exception err)
            {
                TNF.ShowError(err.Message);
                log.Log(LogLevel.Error, err.Message);
                return false;
            }
        }

        public async Task SendTelnetMessgae()
        {
            if (MyTelnetClient != null)
            {
                ReceivedMessage = "";
                log.Log(LogLevel.Info, $"Sending TelNet Message....");
                log.Log(LogLevel.Info, $"{SendMessage}");
                await MyTelnetClient.Send(SendMessage);
                log.Log(LogLevel.Info, $"Message Sent Sucessfully");
            }
            else
            {
                log.Log(LogLevel.Error, $"Connect First");
            }
        }

        private void HandleConnectionClosed(object sender, EventArgs e)
        {
            log.Log(LogLevel.Info, $"Connection Closed");
        }

        private void HandleMessageReceived(object sender, string e)
        {
            ReceivedMessage += e + "\n";
            log.Log(LogLevel.Info, $"Receive Message {ReceivedMessage}");
            log.Log(LogLevel.Info, $"Message Received Sucessfully");
        }
    }
}

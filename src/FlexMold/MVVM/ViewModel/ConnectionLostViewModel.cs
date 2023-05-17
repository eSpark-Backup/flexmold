using FlexMold.Core;
using FlexMold.Utility;
using NLog;
using System;
using System.Threading;
using System.Windows;
using TwinCAT.Ads;
using Thread = System.Threading.Thread;

namespace FlexMold.MVVM.ViewModel
{
    internal class ConnectionLostViewModel : ObservableObject
    {
        static private Logger log = LogManager.GetLogger("TwinCat Connection");
        private string _EventMessage;

        public string EventMessage
        {
            get { return _EventMessage; }
            set { _EventMessage = value; OnPropertyChanged(); }
        }
        private bool _Restore;

        public ConnectionLostViewModel(string _msg)
        {
            EventMessage = _msg;
            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();
        }

        private void WorkThreadFunction()
        {
            try
            {
                switch (EventMessage)
                {
                    case "Connection State Lost":
                        Thread.Sleep(500);
                        TwinCathelper.Client.Close();
                        break;
                    case "Ads State Lost":
                        retry = 0;
                        setState();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        int retry;
        bool setState()
        {
            log.Log(LogLevel.Info, $"{retry} Attempting to Start PLC ....");
            TwinCathelper.StartpPLC();
            Thread.Sleep(500);
            var info = TwinCathelper.GetTwincatState();
            if (info.AdsState != AdsState.Run)
            {
                if (retry < 5)
                {
                    retry++;
                    setState();
                }
                else
                {
                    log.Log(LogLevel.Info, $"PLC Failed...");
                    return false;
                }
            }
            log.Log(LogLevel.Info, $"PLC is running....");
            return true;
        }
    }
}

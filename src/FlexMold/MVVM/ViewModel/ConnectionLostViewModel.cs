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
        private static Logger log = LogManager.GetLogger("TwinCat Connection");
        private string _EventMessage;

        public string EventMessage
        {
            get { return _EventMessage; }
            set { _EventMessage = value; OnPropertyChanged(); }
        }

        private bool _Restore;

        public ConnectionLostViewModel(string _msg, ITwinCathelper twinCathelper)
        {
            EventMessage = _msg;
            this._TwinCathelper = twinCathelper;
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
                        _TwinCathelper.Client.Close();
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

        private int retry;
        private readonly ITwinCathelper _TwinCathelper;

        private bool setState()
        {
            log.Log(LogLevel.Info, $"{retry} Attempting to Start PLC ....");
            _TwinCathelper.StartpPLC();
            Thread.Sleep(500);
            var info = _TwinCathelper.GetTwincatState();
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
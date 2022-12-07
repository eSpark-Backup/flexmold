using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using NLog;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //Task _logTask;
        //CancellationTokenSource _cancelLogTask;
        public MainWindow()
        {
           

            InitializeComponent();
            logCtrl.ItemAdded += OnLogMessageItemAdded;

            Console.WriteLine();
        }
        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= NLog.LogLevel.Error)
                SystemSounds.Beep.Play();
        }
        
        //private void BackgroundSending_Checked(object sender, RoutedEventArgs e)
        //{
        //    _cancelLogTask = new CancellationTokenSource();
        //    var token = _cancelLogTask.Token;
        //    _logTask = new Task(SendLogs, token, token);
        //    _logTask.Start();
        //}
        //private void SendLogs(object obj)
        //{
        //    CancellationToken ct = (CancellationToken)obj;

        //    int counter = 0;
        //    Logger log = NLog.LogManager.GetLogger("task");

        //    log.Debug("Backgroundtask started.");

        //    while (!ct.WaitHandle.WaitOne(2000))
        //    {
        //        log.Trace(string.Format("Messageno {0} from backgroudtask.", counter++));
        //    }

        //    log.Debug("Backgroundtask stopped.");
        //}
        private void BackgroundSending_Unchecked(object sender, RoutedEventArgs e)
        {
            //if (_cancelLogTask != null)
            //    _cancelLogTask.Cancel();
        }
      

    }
}

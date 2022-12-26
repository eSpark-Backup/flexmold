using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.Utility;
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
using TwinCAT.Ads;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Machine SelectedMachine { get; set; }
        public MainWindow()
        {
          

            ShowMachineSlectionWindow();
            //SelectedMachine = _SelectedMachine;
            Logger log = LogManager.GetLogger("Main");
            InitializeComponent();
            logCtrl.ItemAdded += OnLogMessageItemAdded;
            log.Log(LogLevel.Info, $"Application Loaded Scussfully.....");
            log.Log(LogLevel.Info, $"Selected Machine Type : {SelectedMachine.MachineType} Name : {SelectedMachine.Name} ID : {SelectedMachine.ID}");
            Console.WriteLine();

            /// Test Area please comment
            //TwinCathelper tt = new TwinCathelper();
            /// Region closed 
        }
        void ShowMachineSlectionWindow()
        {
            SelectedMachine = new Machine();
            try
            {
                var vm = new SplashScreenViewModel();
                var splashscreen = new SplashMachineSlection
                {
                    DataContext = vm
                };
                vm.OnRequestClose += (s, e) => splashscreen.Close();
                splashscreen.ShowDialog();
                if (vm.SelectedMachine.Name == null)
                {
                    ShowMachineSlectionWindow();
                }
                else
                {
                    SelectedMachine = vm.SelectedMachine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OnLogMessageItemAdded(object o, EventArgs Args)
        {
            // Do what you want :)
            LogEventInfo logInfo = (NLogEvent)Args;
            if (logInfo.Level >= NLog.LogLevel.Error)
                SystemSounds.Beep.Play();
        }

    }
}

using FlexMold.Core;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwinCAT.Ads;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MotorStatusViewModel : ObservableObject
    {

        public ICommand MotorStepCommand { get; set; }
        public ICommand MotorReverseCommand { get; set; }
        public ICommand MotorForwardCommand { get; set; }
        public ICommand PlcConnectCommand { get; set; }
        private string _PlcConnectButtonContent;
        public string PlcConnectButtonContent
        {
            get { return _PlcConnectButtonContent; }
            set {
                _PlcConnectButtonContent = value;
                OnPropertyChanged();
            }
        }

        //private PlcHelper _plcHelper;
        public PlcTask plcHelper { get; set; }

        public MotorStatusViewModel()
        {

            PlcConnectButtonContent = "Connect";
            PlcConnectCommand = new RelayCommand(x => { PlcConnectButtonContent = plcHelper.connectPLC( PlcConnectButtonContent == "Connect"); });
            MotorReverseCommand = new RelayCommand(x => { plcHelper.setMotorDirection(); });
            MotorForwardCommand = new RelayCommand(x => { plcHelper.setMotorDirection(); });
            MotorStepCommand = new RelayCommand(x => { plcHelper.setMotorStep(); });
            plcHelper = new PlcTask
            {
                plcHostAvailablePorts = new UInt16[] { 851, 850 },
                plcHostAvailableIPs = new string[] { "192.168.18.57.1.1" },
            };
            

        }
    }
}

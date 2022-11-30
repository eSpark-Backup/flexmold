using FlexMold.Core;
using MahApps.Metro.Controls;
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
        private PlcHelper _plcHelper;
        public PlcHelper plcHelper
        {
            get { return _plcHelper; }
            set
            {
                _plcHelper = value;
                OnPropertyChanged();
            }
        }

        private int _motorStep;
        public int motorStep
        {
            get { return _motorStep; }
            set
            {
                _motorStep = value;
                OnPropertyChanged();
            }
        }
        private bool _motorDirection;

        public bool motorDirection
        {
            get { return _motorDirection; }
            set
            {
                _motorDirection = value;
                OnPropertyChanged();
            }
        }

        public MotorStatusViewModel()
        {
            _motorStep = 10;
            _motorDirection = false;
            plcHelper = new PlcHelper
            {
                plcHostAvailablePorts = new int[] { 851, 850 },
                plcHostAvailableIPs = new string[] { "192.168.1.1.1.1" }
            };
            MotorReverseCommand = new RelayCommand(x => { changeMotorDirection(); });
            MotorForwardCommand = new RelayCommand(x => { changeMotorDirection(); });
            PlcConnectCommand = new RelayCommand(x => { plcHelper.connectPLC(); });

        }
        public void changeMotorDirection()
        {
            MessageBox.Show("asdasd");
        }


    }
}

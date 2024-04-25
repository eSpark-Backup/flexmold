using FlexMold.Core;
//using FlexMold.MVVM.ViewModel.Home;
using FlexMold.Utility;
//using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
//using System.Windows;
using System.Windows.Forms;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    internal class ReadMotorViewModel : ObservableObject, IReadMotorViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand ReadMotorVCommand { get; set; }
        public bool DoMachineUserCanceled = false;
        private Logger log=LogManager.GetLogger("Main");
        private readonly ITwinCathelper _TwinCathelper;

        private object _currentView;

        //public object CurrentView
        //{
        //    get { return _currentView; }
        //    set
        //    {
        //        _currentView = value;
        //        OnPropertyChanged();
        //    }
        //}
        private int _NumMotorTo;

        public int NumMotorTo
        {
            get { return _NumMotorTo; }
            set
            {
                if (value <= TwinCathelper.DeviceCount)
                    _NumMotorTo = value;
                else
                    MessageBox.Show("Invalid value selected");

                OnPropertyChanged();
            }
        }
        private int _NumMotorFrom;

        public int NumMotorFrom
        {
            get { return _NumMotorFrom; }
            set
            {
                if (value <= TwinCathelper.DeviceCount)
                    _NumMotorFrom = value;
                else
                    MessageBox.Show("Invalid value selected");
                
                OnPropertyChanged();
            }
        }

        private string _RespondingCommand;

        public string RespondingCommand
        {
            get { return _RespondingCommand; }
            set
            {
                _RespondingCommand = value;
                OnPropertyChanged();
            }
        }

        private string _NonRespondingCommand;

        public string NonRespondingCommand
        {
            get { return _NonRespondingCommand; }
            set
            {
                _NonRespondingCommand = value;
                OnPropertyChanged();
            }
        }


        public ReadMotorViewModel(ITwinCathelper twinCathelper)
        {
            //ReadMotorVM = new HeaterView();
            //CurrentView = HeaterStatusVM;

            DoMachineUserCanceled = false;
            this._TwinCathelper = twinCathelper;
            if (_TwinCathelper.Client == null)
            {
                return;
            }
            log = LogManager.GetLogger("Read Motor");
            ReadMotorVCommand = new RelayCommand(o => {
                if (NumMotorFrom > TwinCathelper.DeviceCount)
                { MessageBox.Show("Invalid value selected Motor From"); return; }
                else if(NumMotorTo > TwinCathelper.DeviceCount)
                { MessageBox.Show("Invalid value selected Motor To"); return;}
                else if (NumMotorFrom  > NumMotorTo)
            { MessageBox.Show("Invalid value selected.Outside of bounds"); return;}

        ReadSelectedMotors(); });
        }

        //public IMotorStatusViewmodel MotorStatus { get { return App.AppHost.Services.GetRequiredService<IMotorStatusViewmodel>(); } }

        public void ReadSelectedMotors() {

            if (!CSVHelper.MachineEnabled && !DoMachineUserCanceled)
            {
                log.Log(LogLevel.Error, $"Machine status disconnected! ");
                DialogResult dr = MessageBox.Show("Machine link InActive, Please check connection!", "Machine not connected", MessageBoxButtons.RetryCancel);

                if (dr == DialogResult.Cancel)
                {
                    DoMachineUserCanceled = true;
                    return;
                }
                return;
            }


            log.Log(LogLevel.Info, "Reading Selected Motors");
            String responding = "", nonresponding="";
            for (int i = NumMotorFrom-1; i <= NumMotorTo-1; i++)
            { 
                try {
                    if (CSVHelper.MotorsStatusGlobal[i] == "Healthy")
                        responding += DateTime.Now + " >>> Motor " + i+" Healthy" + Environment.NewLine;
                    else
                        nonresponding += DateTime.Now + " >>> Motor " + i + " Not Responding:"+ CSVHelper.MotorsStatusGlobal[i] + Environment.NewLine;

                } catch { nonresponding += DateTime.Now + " >>> Motor " + i + " Not Responding" + Environment.NewLine; }

            }

            RespondingCommand += responding;
            NonRespondingCommand += nonresponding;


        }
    }
}
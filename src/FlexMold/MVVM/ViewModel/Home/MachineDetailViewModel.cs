using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads;
using NLog;
using System.Timers;
using FlexMold.Utility;
using System.Collections;
using System.Reactive.Linq;
using FlexMold.Core;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MachineDetailViewModel : ObservableObject
    {
        Logger log;
        public UInt16 ECAnSlaves { get; set; }
        public bool ECAbError { get; set; }
        public UInt32 ECAnErrId { get; set; }
        public List<ST_EcSlaveState> ECAdevStates { get => eCAdevStates; set { eCAdevStates = value; OnPropertyChanged(); } }

        private List<ST_EcSlaveState> eCAdevStates;
        byte[] ECAdevStatesB;

        public MachineDetailViewModel()
        {
            log = LogManager.GetLogger("Motor Status");
            ECAdevStates = new List<ST_EcSlaveState>();
            ECAdevStatesB = new byte[TwinCathelper.DeviceCount * sizeof(ElinkState) * 2];
            GetSlaveMachineState();

            var timer = new System.Timers.Timer(3000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
        }
        void GetSlaveMachineState()
        {
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().WriteValue(1);
            ECAbError = (bool)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbError}").FirstOrDefault().ReadAnyValue(typeof(bool));
            if (!ECAbError)
            {
                ushort index = 0;
                ECAdevStatesB = (byte[])TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAdevStates}").FirstOrDefault().ReadValue();
                for (int i = 0; i < ECAdevStatesB.Length;)
                {
                    var aa = new ST_EcSlaveState();
                    aa.SlaveName = $"Box_{index}";
                    aa.deviceState = (EreqState)ECAdevStatesB[i];
                    aa.linkState = (ElinkState)ECAdevStatesB[i + 1];
                    ECAdevStates.Add(aa);
                    i = i + 2;
                    index++;
                }
            }
            TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().WriteValue(0);


        }
      
    }
}

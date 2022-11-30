using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwinCAT.Ads;

namespace FlexMold.MVVM.ViewModel
{
    public class PlcHelper
    {
        public int[] plcHostAvailablePorts { get; set; }
        public string[] plcHostAvailableIPs { get; set; }
        public int plcHostSelectedPort { get; set; }
        public string plcHostSelectedIP { get; set; }
        private TcAdsClient adsClient;
        private int varHandle;

       public void connectPLC()
        {
            try
            {
                // Create instance of class TcAdsClient
                adsClient = new TcAdsClient();
                // PLC1 Port - TwinCAT 3=851
                adsClient.Connect(plcHostSelectedPort);
                varHandle = adsClient.CreateVariableHandle("MAIN.motorStatus");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}

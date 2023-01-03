using FlexMold.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;

namespace FlexMold.MVVM.ViewModel
{
    class PlcTask : ObservableObject
    {

        public UInt16[] plcHostAvailablePorts { get; set; }
        public string[] plcHostAvailableIPs { get; set; }
       
        public UInt16 plcHostSelectedPort { get; set; }

        private UInt16 _motorStep;

        public UInt16 motorStep
        {
            get { return _motorStep; }
            set {
                _motorStep = value;
                OnPropertyChanged();
            }
        }
        private bool _motorDirection;

        public bool motorDirection
        {
            get { return _motorDirection; }
            set {
                _motorDirection = value;
                OnPropertyChanged();
            }
        }

        private AdsClient adsClient;
        private AdsSession MyAdsSession;

        private int vhmotorStatus;
        private uint vhdir;
        private uint vhspeed;

        public string connectPLC(bool connect)
        {
            Logger log = LogManager.GetLogger("connectPLC");
            // Create instance of class TcAdsClient
            adsClient = new AdsClient();
            if (connect)
            {
                try
                {
                    // PLC1 Port - TwinCAT 3=851
                    adsClient.Connect(plcHostSelectedPort);
                    log.Log(LogLevel.Info, $"ADS Address : {adsClient.Address}");
                    log.Log(LogLevel.Info, $"ADS ID : {adsClient.Id}");
                    log.Log(LogLevel.Info, $"ADS IsLocal : {adsClient.IsLocal}");
                    log.Log(LogLevel.Info, $"ADS ClientAddress : {adsClient.ClientAddress}");
                    log.Log(LogLevel.Info, $"ADS IsConnected : {adsClient.IsConnected}");
                    log.Log(LogLevel.Info, $"ADS Timeout : {adsClient.Timeout}");

                    vhdir = adsClient.CreateVariableHandle("MAIN.dirM1");
                    vhspeed = adsClient.CreateVariableHandle("MAIN.speedM1");
                    getPlcVariables();
                    return "Diconnect";
                }
                catch (Exception err)
                {
                    log.Log(LogLevel.Error, err, $"Reding Failed");
                    return "Connect";
                }
            }
            else
            {
                try
                {
                    adsClient.Disconnect();
                    return "Connect";
                }
                catch (Exception err)
                {
                    log.Log(LogLevel.Error, err, $"Reding Failed");
                    return "Diconnect";
                }
            }
        }
        void getPlcVariables()
        {
            getMotorDirection();
            getMotorStep();
        }
        public void getMotorDirection()
        {
            Logger log = LogManager.GetLogger("getMotorDirection");
            try
            {
                if (adsClient != null)
                {
                    var aa=  adsClient.ReadAny(vhdir, typeof(UInt16));
                    motorDirection = Convert.ToBoolean(aa);
                }
            }
            catch (Exception err)
            {
                log.Log(LogLevel.Error, err, $"Reding Failed");
            }
        }
        public void getMotorStep()
        {
            Logger log = LogManager.GetLogger("getMotorStep");
            try
            {
                if (adsClient != null)
                {
                    var aa = adsClient.ReadAny(vhspeed, typeof(byte));
                    motorStep = Convert.ToUInt16(aa);
                }
            }
            catch (Exception err)
            {
                log.Log(LogLevel.Error, err, $"Reding Failed");

            }
        }
        public void setMotorDirection()
        {
            Logger log = LogManager.GetLogger("setMotorDirection");

            try
            {
                if (adsClient != null)
                {
                    var aa = Convert.ToByte(motorDirection);
                    adsClient.WriteAny(vhdir, aa);
                }
            }
            catch (Exception err)
            {
                log.Log(LogLevel.Error, err, $"Reding Failed");
            }
        }
        public void setMotorStep()
        {
            Logger log = LogManager.GetLogger("setMotorStep");

            try
            {
                if (adsClient != null)
                {
                    adsClient.WriteAny(vhspeed, motorStep);
                }
            }
            catch (Exception err)
            {
                log.Log(LogLevel.Error, err, $"Reding Failed");
            }
        }
    }
}

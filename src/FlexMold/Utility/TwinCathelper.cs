using FlexMold.MVVM.Model;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace FlexMold.Utility
{
    internal static class TwinCathelper
    {
        private static Logger _log = LogManager.GetLogger("TwinCat");
        public static string SAmsNetId { get; private set; }
        public static ushort[] SAmsPort { get; private set; }
        private static int _port { get; set; }
        public static AdsSession Session { get; private set; }
        public static AdsSession SessionSystemService { get; set; }
        public static AdsConnection Client { get; private set; }
        public static AdsConnection ClientSystemService { get; set; }
        public static ConnectionState MyConnectionStateOld { get; set; }
        public static ConnectionState MyConnectionStateNew { get; set; }
        public static ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        public static AdsState MyAdsState { get; set; }
        public static AmsRouterState MyAmsRouterState { get; set; }
        private static IDataTypeCollection<IDataType> _dataTypes { get; set; }
        public static ObservableCollection<Symbol> MySymbols { get; private set; }
        public static Int16 DeviceCount { get; private set; }
        public static EthercatDevice[] PLCMotors { get; private set; }
        public static AdsState CurrentAdsState { get; set; }
        public static ConnectionState CurrentConnectionState { get; set; }

        public static bool TwinCatConnect851()
        {
            _port = 851;
            _log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {_port}");
            Session = new AdsSession(TwinCAT.Ads.AmsNetId.Local, _port);
            Client = (AdsConnection)Session.Connect();
            if (Client.IsConnected)
            {
                _log.Log(LogLevel.Info, $"Connected to : {Client.Address}, Port : {_port}");
                if (Client.IsActive)
                {
                    if (GetPLCVariables())
                    {
                        InitializeVarables();
                        return true;
                    }
                }
            }
            return false;
        }
        public static void TwinCatDiconnectConnect851()
        {
            _log.Log(LogLevel.Info, $"DisConnecting to Local Host, Port : {_port}");
            if (Client.IsConnected)
                Client.Disconnect();
            if (Session.IsConnected)
                Session.Disconnect();
            Client = null;
            Session = null;
        }
        public static Symbol SymbolEthercatDevice { get; private set; }
        public static int structSize { get; private set; }
        public static byte[] buffer { get; private set; }

        public static bool InitializeMotors()
        {
            try
            {
                TwinCathelper.DeviceCount = (short)TwinCathelper.MySymbols.Where(a =>
                           a.InstancePath == TCVarName.DeviceCount).FirstOrDefault().ReadAnyValue(typeof(short));
                PLCMotors = new EthercatDevice[DeviceCount];
                SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == TCVarName.MainDevice).FirstOrDefault();
                structSize = (SymbolEthercatDevice.Size) / TwinCathelper.DeviceCount;
                buffer = new byte[structSize * TwinCathelper.DeviceCount];
                ReadAllEthercatDeviceData();
                if (SessionSystemService == null)
                {
                    SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
                    ClientSystemService = (AdsConnection)TwinCathelper.SessionSystemService.Connect();
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
                return false;
            }
        }

        public static int RegisterNotification(string _deviceStatusVariableName)
        {
            if (TwinCathelper.Client.State == ConnectionState.Connected)
            {
                var deviceStatusSymbol = TwinCathelper.MySymbols.Where(a => a.InstancePath == _deviceStatusVariableName).FirstOrDefault();
                return (int)TwinCathelper.Client.AddDeviceNotification(deviceStatusSymbol.IndexGroup,
                   deviceStatusSymbol.IndexOffset, (int)(deviceStatusSymbol.Size), NotificationSettings.Default, deviceStatusSymbol);
            }
            _log.Log(LogLevel.Error, $"TwinCAT Connection is {TwinCathelper.Client.State}");
            return -1;
        }

        private static void ReadAllEthercatDeviceData()
        {
            Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
            for (int i = 0; i < DeviceCount; i++)
            {
                var mtarray = new byte[structSize];
                Array.Copy(buffer, structSize * i, mtarray, 0, structSize);
                PLCMotors[i].Unpack(mtarray);
            }
        }

        private static void InitializeVarables()
        {
            SAmsNetId = (string)TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsNetId}").FirstOrDefault().ReadAnyValue(typeof(string));
            SAmsPort = (UInt16[])TwinCathelper.MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsPort}").FirstOrDefault().ReadAnyValue(typeof(UInt16[]));
        }

        public static void ReadEthercatDeviceData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            TwinCathelper.Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            PLCMotors[_motorNumber].Unpack(mtarray);
        }

        public static void WriteEthercatDeviceData()
        {
            for (int i = 0; i < PLCMotors.Length; i++)
            {
                PLCMotors[i].Pack().CopyTo(buffer, structSize * i);
            }
            TwinCathelper.Client.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
        }

        public static void WriteEthercatDeviceData(uint _motorNumber)
        {
            try
            {
               var mtarray = new byte[structSize];
                PLCMotors[_motorNumber].Pack().CopyTo(mtarray, 0);
                TwinCathelper.Client.Write(SymbolEthercatDevice.IndexGroup,
                    SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }
        }

        public static void StartpPLC()
        {
            _log.Log(LogLevel.Info, $"Resuming PLC ....");
            Client.WriteControl(new StateInfo(AdsState.Run, ClientSystemService.ReadState().DeviceState));
        }

        public static void StoppPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Stop, ClientSystemService.ReadState().DeviceState));
        }

        public static void ConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Config, ClientSystemService.ReadState().DeviceState));
        }

        public static void ReConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Reconfig, ClientSystemService.ReadState().DeviceState));
        }

        public static StateInfo GetTwincatState()
        {
            StateInfo info = new StateInfo();
            if (Client.IsConnected)
            {
                AdsErrorCode errorCode = TwinCathelper.Client.TryReadState(out info);
            }
            return info;
        }

        private static bool GetPLCVariables()
        {
            _log.Log(LogLevel.Info, $"Fetching PLC Variables");
            MySymbols = new ObservableCollection<Symbol>();
            _dataTypes = Session.SymbolServer.DataTypes;
            var symbols = Session.SymbolServer.Symbols;
            if (symbols.Count != 0)
            {
                ISymbolLoader loader = SymbolLoaderFactory.Create(Client, SymbolLoaderSettings.Default);

                foreach (var item in from item in symbols
                                     where item.InstanceName == "MAIN"
                                     select item)
                {
                    foreach (var itemsymbols in item.SubSymbols)
                    {
                        MySymbols.Add((Symbol)loader.Symbols[itemsymbols.InstancePath]);
                        _log.Log(LogLevel.Info, $"Symbol : {itemsymbols.InstancePath}");
                    }
                }
                return true;
            }
            _log.Log(LogLevel.Error, "No Symbol Found...");
            return false;
        }
    }
}
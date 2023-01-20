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
    static class TwinCathelper
    {
        static private Logger log;
        static public int port { get; private set; }
        static public AdsSession _session { get; private set; }
        static public AdsSession _sessionSystemService { get; set; }
        static public AdsConnection Client { get; private set; }
        static public AdsConnection _connectionSystemService { get; set; }
        static public ConnectionState MyConnectionStateOld { get; set; }
        static public ConnectionState MyConnectionStateNew { get; set; }
        static public ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        static public AdsState MyAdsState { get; set; }
        static public AmsRouterState MyAmsRouterState { get; set; }
        static public IDataTypeCollection<IDataType> dataTypes { get; private set; }
        static public ObservableCollection<Symbol> MySymbols { get; private set; }
        static public Int16 DeviceCount { get; private set; }
        public static EthercatDevice[] PLCMotors { get; private set; }

        static public void TwinCatConnect851()
        {
            log = LogManager.GetLogger("TwinCat");
            port = 851;
            log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {port}");
            _session = new AdsSession(TwinCAT.Ads.AmsNetId.Local, port);
            Client = (AdsConnection)_session.Connect();
            if (Client.IsConnected)
            {
                log.Log(LogLevel.Info, $"Connected to : {Client.Address}, Port : {port}");
            }
            else
            {
                log.Log(LogLevel.Info, $"Connection Status : {Client.ConnectionState}");
            }


            GetPLCVariables();
        }
        static public Symbol SymbolEthercatDevice { get; private set; }
        static public int structSize { get; private set; }
        static public byte[] buffer { get; private set; }
        static public void InitializeMotors()
        {
            TwinCathelper.DeviceCount = (short)TwinCathelper.MySymbols.Where(a =>
                            a.InstancePath == TCVarName.DeviceCount).FirstOrDefault().ReadAnyValue(typeof(short));
            PLCMotors = new EthercatDevice[DeviceCount];
            SymbolEthercatDevice = TwinCathelper.MySymbols.Where(a => a.InstancePath == TCVarName.MainDevice).FirstOrDefault();
            structSize = (SymbolEthercatDevice.Size) / TwinCathelper.DeviceCount;
            buffer = new byte[structSize * TwinCathelper.DeviceCount];
            ReadAllEthercatDeviceData();
        }
        static void ReadAllEthercatDeviceData()
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
        public static void ReadEthercatDeviceData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            TwinCathelper.Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            PLCMotors[_motorNumber].Unpack(mtarray);
        }

        public static void WriteAllEthercatDeviceData()
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
            var mtarray = new byte[structSize];
            PLCMotors[_motorNumber].Pack().CopyTo(mtarray, 0);
            TwinCathelper.Client.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
        }
        static public void StartpPLC()
        {
            _connectionSystemService.WriteControl(new StateInfo(AdsState.Run, _connectionSystemService.ReadState().DeviceState));
        }
        static public void StoppPLC()
        {
            _connectionSystemService.WriteControl(new StateInfo(AdsState.Stop, _connectionSystemService.ReadState().DeviceState));
        }
        static public void ConfigPLC()
        {
            _connectionSystemService.WriteControl(new StateInfo(AdsState.Config, _connectionSystemService.ReadState().DeviceState));
        }
        static public void ReConfigPLC()
        {
            _connectionSystemService.WriteControl(new StateInfo(AdsState.Reconfig, _connectionSystemService.ReadState().DeviceState));
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
        static void GetPLCVariables()
        {
            log.Log(LogLevel.Info, $"Fetching PLC Variables");
            MySymbols = new ObservableCollection<Symbol>();
            dataTypes = _session.SymbolServer.DataTypes;
            var symbols = _session.SymbolServer.Symbols;
            ISymbolLoader loader = SymbolLoaderFactory.Create(Client, SymbolLoaderSettings.Default);

            foreach (var item in from item in symbols
                                 where item.InstanceName == "MAIN"
                                 select item)
            {
                foreach (var itemsymbols in item.SubSymbols)
                {
                    MySymbols.Add((Symbol)loader.Symbols[itemsymbols.InstancePath]);
                    log.Log(LogLevel.Info, $"Symbol : {itemsymbols.InstancePath}");
                }
            }
        }


    }
}

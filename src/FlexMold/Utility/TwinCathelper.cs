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
        static public AdsSession _session { get; set; }
        static public AdsSession _sessionSystemService { get; set; }
        static public AdsConnection _connection { get; set; }
        static public AdsConnection _connectionSystemService { get; set; }
        static public ConnectionState MyConnectionStateOld { get; set; }
        static public ConnectionState MyConnectionStateNew { get; set; }
        static public ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        static public AdsState MyAdsState { get; set; }
        static public AmsRouterState MyAmsRouterState { get; set; }
        static public IDataTypeCollection<IDataType> dataTypes { get; private set; }
        static public ISymbolCollection<ISymbol> symbols { get; private set; }
        static public ObservableCollection<Symbol> MySymbols { get; private set; }
        static public Int16 DeviceCount { get;  set; }
        static public void TwinCatConnect851()
        {
            log = LogManager.GetLogger("TwinCat");
            port = 851;
            log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {port}");
            _session = new AdsSession(TwinCAT.Ads.AmsNetId.Local, port);
            _connection = (AdsConnection)_session.Connect();
            if (_connection.IsConnected)            {
                log.Log(LogLevel.Info, $"Connected to : {_connection.Address}, Port : {port}");
            }
            else 
            {
                log.Log(LogLevel.Info, $"Connection Status : {_connection.ConnectionState}");
            }


            GetPLCVariables();
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
            if (_connection.IsConnected)
            {
                AdsErrorCode errorCode = TwinCathelper._connection.TryReadState(out info);
            }
            return info;
        }
        static void GetPLCVariables()
        {
            log.Log(LogLevel.Info, $"Fetching PLC Variables");
            MySymbols = new ObservableCollection<Symbol>();
            dataTypes = _session.SymbolServer.DataTypes;
            symbols = _session.SymbolServer.Symbols;
            ISymbolLoader loader = SymbolLoaderFactory.Create(_connection, SymbolLoaderSettings.Default);

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

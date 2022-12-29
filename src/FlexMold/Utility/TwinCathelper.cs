using FlexMold.Core;
using FlexMold.MVVM.Model;
using NLog;
using NLog.Fluent;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using TwinCAT.ValueAccess;
using static FlexMold.MVVM.Model.PLCMotor;
using static System.Windows.Forms.AxHost;

namespace FlexMold.Utility
{
    static class TwinCathelper
    {
        static private Logger log;
        static public int port { get; private set; }
        static public AdsSession _session { get; set; }
        static public AdsConnection _connection { get; set; }
        static public ConnectionState MyConnectionStateOld { get; set; }
        static public ConnectionState MyConnectionStateNew { get; set; }
        static public ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        static public AdsState MyAdsState { get; set; }
        static public AmsRouterState MyAmsRouterState { get; set; }
        static public IDataTypeCollection<IDataType> dataTypes { get; private set; }
        static public ISymbolCollection<ISymbol> symbols { get; private set; }
        static public ObservableCollection<Symbol> MySymbols { get; private set; }
        static public Int16 DeviceCount { get; private set; }
        static public void TwinCatConnect()
        {
            log = LogManager.GetLogger("TwinCat");
            port = 851;
            log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {port}");
            _session = new AdsSession(AmsNetId.Local, port);
            _connection = (AdsConnection)_session.Connect();
            GetPLCVariables();
            DeviceCount = (Int16)MySymbols.Where(a => a.InstancePath == "MAIN.deviceCount").FirstOrDefault().ReadAnyValue(typeof(Int16));
        }
        static public void StartpPLC()
        {
            _connection.WriteControl(new StateInfo(AdsState.Run, _connection.ReadState().DeviceState));
        }
        static public void StoppPLC()
        {
            _connection.WriteControl(new StateInfo(AdsState.Stop, _connection.ReadState().DeviceState));
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

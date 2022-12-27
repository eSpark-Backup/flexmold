using FlexMold.Core;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using static System.Windows.Forms.AxHost;

namespace FlexMold.Utility
{
     class TwinCathelper :ObservableObject
    {
        private Logger log;

        public int port { get; private set; }
        public AdsSession _session { get; set; }
        public AdsConnection _connection { get; set; }
        public ConnectionState MyConnectionStateOld { get; set; }
        public ConnectionState MyConnectionStateNew { get; set; }
        public ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        public AdsState MyAdsState { get; set; }
        public AmsRouterState MyAmsRouterState { get; set; }
        public IDataTypeCollection<IDataType> dataTypes { get; private set; }
        public ISymbolCollection<ISymbol> symbols { get; private set; }
        public ObservableCollection<Symbol> MySymbols { get; private set; }
        public TwinCathelper()
        {
            log = LogManager.GetLogger("TwinCat");
            Connect();
            GetPLCVariables();
        }
        void Connect()
        {
            port = 851;
            log.Log(LogLevel.Info, $"Connecting to Local Host, Port : {port}");
            _session = new AdsSession(AmsNetId.Local, port);
            _session.ConnectionStateChanged += _session_ConnectionStateChanged;

            _connection = (AdsConnection)_session.Connect();
            _connection.ConnectionStateChanged += _connection_ConnectionStatusChanged;
            _connection.RouterStateChanged += _connection_RouterStateChanged;

            // Add the Notification event handler
            try
            {
                _connection.RegisterAdsStateChangedAsync(_connection_AdsStateChanged, CancellationToken.None);
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
            }
            RegisterNotificationsAsync();
        }

        public void Update()
        {
            if (_connection != null)
            {
                if (_connection.IsConnected)
                {
                    StateInfo info = new StateInfo();
                    AdsErrorCode errorCode = _connection.TryReadState(out info);

                    switch (info.AdsState)
                    {
                        case AdsState.Run:
                            break;
                        case AdsState.Stop:
                        case AdsState.Error:
                            break;
                        case AdsState.Config:
                            break;
                    }
                }
            }
            else
            {

            }
        }


        void _connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            MyAdsState = e.State.AdsState;
            log.Log(LogLevel.Info, $"AdsState : {MyAdsState}");
        }

        void _connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            MyAmsRouterState = e.State;
            log.Log(LogLevel.Info, $"Router State : {MyAmsRouterState}");
        }

        void _connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            log.Log(LogLevel.Info, $"Connection Status Old : {MyConnectionStateOld}");
            log.Log(LogLevel.Info, $"Connection Status New : {MyConnectionStateNew}");
            log.Log(LogLevel.Info, $"Connection Status Reason : {MyConnectionStateChangedReason}");
        }

        void _session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            log.Log(LogLevel.Info, $"Connection State Old : {MyConnectionStateOld}");
            log.Log(LogLevel.Info, $"Connection State New : {MyConnectionStateNew}");
            log.Log(LogLevel.Info, $"Connection State Reason : {MyConnectionStateChangedReason}");
        }
        private async Task RegisterNotificationsAsync()
        {
            CancellationToken cancel = CancellationToken.None;


            // Add the Notification event handler
            _connection.AdsNotification += Client_AdsNotification2;

            // Connect to target
            uint notificationHandle = 0;

            // Notification to a DINT Type (UINT32)
            // Check for change every 200 ms

            //byte[] notificationBuffer = new byte[sizeof(UInt32)];
            int size = sizeof(UInt32);

            ResultHandle result = await _connection.AddDeviceNotificationAsync("MAIN.speedM1", size, new NotificationSettings(AdsTransMode.OnChange, 200, 0), null, cancel);

            if (result.Succeeded)
            {
                notificationHandle = result.Handle;
                await Task.Delay(5000); // Wait asynchronously without blocking the UI Thread.
                                        // Unregister the Event / Handle
                ResultAds result2 = await _connection.DeleteDeviceNotificationAsync(notificationHandle, cancel);
            }
            _connection.AdsNotification -= Client_AdsNotification2;

        }

        private void Client_AdsNotification2(object sender, AdsNotificationEventArgs e)
        {
            // Or here we know about UDINT type --> can be marshalled as UINT32
            uint nCounter = BinaryPrimitives.ReadUInt32LittleEndian(e.Data.Span);

            // If Synchronization is needed (e.g. in Windows.Forms or WPF applications)
            // we could synchronize via SynchronizationContext into the UI Thread

            /*SynchronizationContext syncContext = SynchronizationContext.Current;
              _context.Post(status => someLabel.Text = nCounter.ToString(), null); // Non-blocking post */
        }

        void GetPLCVariables()
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

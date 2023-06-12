using FlexMold.MVVM.Model;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace FlexMold.Utility
{
    internal static partial class TwinCathelper
    {
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
        public static void InitializeNotifications()
        {
            Session.ConnectionStateChanged += Session_ConnectionStateChanged;
            ClientSystemService.ConnectionStateChanged += Connection_ConnectionStatusChanged;
            Client.RouterStateChanged += Connection_RouterStateChanged;
            Client.RegisterAdsStateChangedAsync(Connection_AdsStateChanged, CancellationToken.None);

            RegisterNotification($"MAIN.{TCSymbols.mPosition}");
            RegisterNotification($"MAIN.{TCSymbols.MotorDiagnostic}");
            Client.AdsNotification += Client_AdsNotification;
        }
        private static void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            try
            {
                // Or here we know about UDINT type --> can be marshalled as UINT32
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol != null)
                {
                    TCSymbols aa;
                    Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                    switch (aa)
                    {
                        case TCSymbols.deviceCount:
                            break;
                        case TCSymbols.device:
                            break;
                        case TCSymbols.mPosition:
                            for (int i = 0; i < PLCMotors.Length; i++)
                                PLCMotorsStates.mPostion[i] = BitConverter.ToSingle(e.Data.ToArray(), i * sizeof(float));
                            TMotor_PositionChanged?.Invoke(null, e);
                            break;
                        case TCSymbols.mTemperature:
                            break;
                        case TCSymbols.foe_dwPass:
                            break;
                        case TCSymbols.foe_nSlaveAddr:
                            break;
                        case TCSymbols.foe_eMode:
                            break;
                        case TCSymbols.foe_sPathName:
                            break;
                        case TCSymbols.foe_bLoad:
                            break;
                        case TCSymbols.foe_bBusy:
                            break;
                        case TCSymbols.foe_bError:
                            break;
                        case TCSymbols.foe_nErrID:
                            break;
                        case TCSymbols.foe_nBytesWritten:
                            break;
                        case TCSymbols.foe_nPercent:
                            break;
                        case TCSymbols.foe_sAmsNetId:
                            break;
                        case TCSymbols.foe_sAmsPort:
                            break;
                        case TCSymbols.ecs_bExecute:
                            break;
                        case TCSymbols.ecs_reqState:
                            break;
                        case TCSymbols.ecs_bBusy:
                            break;
                        case TCSymbols.ecs_bError:
                            break;
                        case TCSymbols.ecs_nErrId:
                            break;
                        case TCSymbols.ecs_currState:
                            break;
                        case TCSymbols.fbGetSlaveState:
                            break;
                        case TCSymbols.ecg_bExecute:
                            break;
                        case TCSymbols.state:
                            break;
                        case TCSymbols.ecg_bError:
                            break;
                        case TCSymbols.ecg_nErrId:
                            break;
                        case TCSymbols.ECAbExecute:
                            break;
                        case TCSymbols.ECAdevStates:
                            break;
                        case TCSymbols.ECAnSlaves:
                            break;
                        case TCSymbols.ECAbError:
                            break;
                        case TCSymbols.ECAnErrId:
                            break;
                        case TCSymbols.MotorDiagnostic:
                            Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStates.MotorDiagnistic, 0, e.Data.Length);
                            _log.Log(LogLevel.Info, $"MotorDiagnistic Updated : {String.Join(",", PLCMotorsStates.MotorDiagnistic)}");
                            TMotor_MotorDiagnosticChanged?.Invoke(null, e);
                            break;
                        case TCSymbols.DeviceStatus:
                            Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStates.DeviceStatus, 0, e.Data.Length);
                            _log.Log(LogLevel.Info, $"DeviceStatus Updated : {String.Join(",", PLCMotorsStates.DeviceStatus)}");
                            TMotor_DeviceStatusChanged?.Invoke(null, e);
                            break;
                        case TCSymbols.mDiag3:
                            break;
                        default:
                            break;
                    }
                }
            }

            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }

            TMotor_PositionChanged?.Invoke(null, e);
        }

        private static void Session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Session Connection Status : {MyConnectionStateNew}");
            TSession_ConnectionStateChanged?.Invoke(null, e);
        }
        private static void Connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Connection Status : {MyConnectionStateNew}");
            TConnection_ConnectionStatusChanged?.Invoke(null, e);
            TConnection_ConnectionStatusChanged?.Invoke(null, e);
        }
        private static void Connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            MyAmsRouterState = e.State;
            _log.Log(LogLevel.Info, $"Router State : {TwinCathelper.MyAmsRouterState}");
            TConnection_RouterStateChanged?.Invoke(null, e);
        }
        private static void Connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            MyAdsState = e.State.AdsState;
            switch (e.State.AdsState)
            {
                case AdsState.Run:
                    _log.Log(LogLevel.Info, $"AdsState : {MyAdsState}");
                    break;
                default:
                    _log.Log(LogLevel.Error, $"AdsState : {MyAdsState}");
                    break;
            }
            TConnection_AdsStateChanged?.Invoke(null, e);
        }

        public static bool InitializeMotors()
        {
            try
            {
                TwinCathelper.DeviceCount = (short)MySymbols.FirstOrDefault(a =>
                           a.InstancePath == TCVarName.DeviceCount).ReadAnyValue(typeof(short));
                PLCMotors = new EthercatDevice[DeviceCount];
                PLCMotorsStates = new DeviceStatuses((uint)DeviceCount);
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
            if (Client.State == ConnectionState.Connected)
            {
                var deviceStatusSymbol = MySymbols.Where(a => a.InstancePath == _deviceStatusVariableName).FirstOrDefault();
                return (int)Client.AddDeviceNotification(deviceStatusSymbol.IndexGroup,
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
        public static void WriteEthercatDeviceData(uint _motorNumber, DeviceSymbol deviceSymbol)
        {
            var aa = SymbolEthercatDevice
                      .SubSymbols[(int)_motorNumber]
                      .SubSymbols[(int)deviceSymbol].InstancePath;
            try
            {
                var mtarray = new byte[sizeof(uint)];
                Array.Copy(PLCMotors[_motorNumber].mTime.CastToArray(), mtarray, mtarray.Length);
                var handle = Client.CreateVariableHandle(aa);
                Client.Write(handle, mtarray.AsMemory());
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
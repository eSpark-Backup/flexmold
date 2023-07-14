using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace FlexMold.Utility
{
    public partial class TwinCathelper : ITwinCathelper
    {
        public void SetMachineStatusTimer()
        {
            ValidStateMachines = new ESlaveStateMachine[DeviceCount];
            ValidElinkStates = new ESlaveLink[DeviceCount];
            defaultStateMachines = new ESlaveStateMachine[DeviceCount];
            defaultElinkStates = new ESlaveLink[DeviceCount];

            Array.Fill(ValidStateMachines, ESlaveStateMachine.EC_DEVICE_STATE_OP);
            Array.Fill(defaultStateMachines, ESlaveStateMachine.UnAvailable);
            Array.Fill(ValidElinkStates, ESlaveLink.EC_LINK_STATE_OK);
            Array.Fill(defaultElinkStates, ESlaveLink.UnAvailable);

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Timer_Elapsed);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            GetSlaveMachineState();
        }
        private void GetSlaveMachineState()
        {
            try
            {
                var pp = MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().TryWriteValue(1, 200);

                ECAbError = (bool)MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbError}").FirstOrDefault().ReadAnyValue(typeof(bool));

                if (!ECAbError)
                {
                    var deviceandlinkstatusbytes = (byte[])MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAdevStates}").FirstOrDefault().ReadValue();

                    var oddIndexesValues = deviceandlinkstatusbytes.Where((value, index) => index % 2 != 0);
                    Slave_Link_Status = oddIndexesValues.Select(b => (ESlaveLink)Enum.ToObject(typeof(ESlaveLink), b)).ToArray();
                    var evenIndexesValues = deviceandlinkstatusbytes.Where((value, index) => index % 2 == 0);
                    Slave_StateMachine_Status = evenIndexesValues.Select(b => (ESlaveStateMachine)Enum.ToObject(typeof(ESlaveStateMachine), b)).ToArray();
                }
                else
                {
                    Slave_Link_Status = defaultElinkStates;
                    Slave_StateMachine_Status = defaultStateMachines;
                }
                MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.ECAbExecute}").FirstOrDefault().WriteValue(0);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }
        }
        public ESlaveLink[] Slave_Link_Status
        {
            get => _Slave_Link_Status;
            set
            {
                _Slave_Link_Status = value;
                TSlave_Link_Status_Changed.Invoke(null, null);
            }
        }
        public ESlaveStateMachine[] Slave_StateMachine_Status
        {
            get => _Slave_StateMachine_Status;
            set
            {
                _Slave_StateMachine_Status = value;
                TSlave_StateMachine_Status_Changed.Invoke(null, null);
            }
        }
        public void SetDefaultMode(Logger log)
        {
            if (Motor_Diagnostic_Abort == false &&
                Slave_Link_Abort == false &&
                Slave_StateMachine_Abort == false)
            {
                for (int i = 0; i < DeviceCount; i++)
                    PLCMotors[i].mMode = (int)EMode.Displacement;
                WriteEthercatDeviceData(DeviceSymbol.mMode);
                log.Log(LogLevel.Info, $"Configured to Default Device Mode : {EMode.Displacement}");
                TNF.ShowSuccess($"Default Device Mode : {EMode.Displacement}");
            }
        }
        public void SetAbortMode(Logger log)
        {
            if (Motor_Diagnostic_Abort == true ||
                Slave_Link_Abort == true ||
                Slave_StateMachine_Abort == true)
            {
                for (int i = 0; i < DeviceCount; i++)
                    PLCMotors[i].mRPM = 0;
                WriteEthercatDeviceData(DeviceSymbol.mRPM);

                for (int i = 0; i < DeviceCount; i++)
                    PLCMotors[i].mMode = (int)EMode.Abort;

                WriteEthercatDeviceData(DeviceSymbol.mMode);
                log.Log(LogLevel.Warn, $"Device Mode Changed to : {EMode.Abort}");
                log.Log(LogLevel.Warn, $"RPM values Changed to : 0");
                TNF.ShowError($"Device Mode: {EMode.Abort}");
            }
        }
        public bool TwinCatConnect851()
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
        public void TwinCatDiconnectConnect851()
        {
            _log.Log(LogLevel.Info, $"DisConnecting to Local Host, Port : {_port}");
            if (Client.IsConnected)
                Client.Disconnect();
            if (Session.IsConnected)
                Session.Disconnect();
            Client = null;
            Session = null;
        }
        public void InitializeNotifications()
        {
            Session.ConnectionStateChanged += Session_ConnectionStateChanged;
            ClientSystemService.ConnectionStateChanged += Connection_ConnectionStatusChanged;
            Client.RouterStateChanged += Connection_RouterStateChanged;
            Client.RegisterAdsStateChangedAsync(Connection_AdsStateChanged, CancellationToken.None);

            RegisterNotification($"MAIN.{TCSymbols.mPosition}");
            RegisterNotification($"MAIN.{TCSymbols.MotorDiagnostic}");
            RegisterNotification($"MAIN.{TCSymbols.DeviceStatus}");
            Client.AdsNotification += Client_AdsNotification;
        }
        private void Client_AdsNotification(object sender, AdsNotificationEventArgs e)
        {
            try
            {
                var _mysymbol = e.UserData as Symbol;
                if (_mysymbol == null)
                    return;

                TCSymbols aa;
                Enum.TryParse(_mysymbol.InstancePath.Replace("MAIN.", ""), out aa);
                switch (aa)
                {
                    case TCSymbols.mPosition:
                        for (int i = 0; i < PLCMotors.Length; i++)
                            PLCMotorsStatuses.mPostion[i] = BitConverter.ToSingle(e.Data.ToArray(), i * sizeof(float));
                        TMotor_PositionChanged?.Invoke(null, e);
                        break;
                    case TCSymbols.MotorDiagnostic:
                        Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStatuses.MotorDiagnistic, 0, e.Data.Length);
                        _log.Log(LogLevel.Info, $"MotorDiagnistic Updated : {String.Join(",", PLCMotorsStatuses.MotorDiagnistic)}");
                        TMotor_MotorDiagnosticChanged?.Invoke(null, e);
                        break;
                    case TCSymbols.DeviceStatus:
                        Buffer.BlockCopy(e.Data.ToArray(), 0, PLCMotorsStatuses.DeviceStatus, 0, e.Data.Length);
                        _log.Log(LogLevel.Info, $"DeviceStatus Updated : {String.Join(",", PLCMotorsStatuses.DeviceStatus)}");
                        TMotor_DeviceStatusChanged?.Invoke(null, e);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }

            TMotor_PositionChanged?.Invoke(null, e);
        }

        private void Session_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Session Connection Status : {MyConnectionStateNew}");
            TSession_ConnectionStateChanged?.Invoke(null, e);
        }
        private void Connection_ConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            MyConnectionStateOld = e.OldState;
            MyConnectionStateNew = e.NewState;
            MyConnectionStateChangedReason = e.Reason;
            _log.Log(LogLevel.Info, $"Connection Status : {MyConnectionStateNew}");
            TConnection_ConnectionStatusChanged?.Invoke(null, e);
            TConnection_ConnectionStatusChanged?.Invoke(null, e);
        }
        private void Connection_RouterStateChanged(object sender, AmsRouterNotificationEventArgs e)
        {
            MyAmsRouterState = e.State;
            _log.Log(LogLevel.Info, $"Router State : {MyAmsRouterState}");
            TConnection_RouterStateChanged?.Invoke(null, e);
        }
        private void Connection_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
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

        public bool InitializeSlaves(EMachineType emt)
        {
            try
            {
                switch (emt)
                {
                    case EMachineType.Motor:
                        DeviceCount = (short)MySymbols.FirstOrDefault(a =>
                           a.InstancePath == TCVarName.DeviceCount).ReadAnyValue(typeof(short));
                        PLCMotors = new EthercatDevice[DeviceCount];
                        PLCMotorsStatuses = new DeviceStatuses((uint)DeviceCount);
                        SymbolEthercatDevice = MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.device}").FirstOrDefault();
                        structSize = (SymbolEthercatDevice.Size) / DeviceCount;
                        buffer = new byte[structSize * DeviceCount];
                        ReadAllEthercatDeviceData();
                        if (SessionSystemService == null)
                        {
                            SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
                            ClientSystemService = (AdsConnection)SessionSystemService.Connect();
                        }
                        return true;
                    case EMachineType.ScanDevices:
                        DeviceCount = (short)MySymbols.FirstOrDefault(a =>
                           a.InstancePath == TCVarName.DeviceCount).ReadAnyValue(typeof(short));
                        if (MySymbols.Any(a => a.InstancePath == $"MAIN.{TCSymbols.SlaveCount}"))
                        {
                            if (SessionSystemService == null)
                            {
                                SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
                                ClientSystemService = (AdsConnection)SessionSystemService.Connect();
                            }
                            return true;
                        }
                        return false;
                    default:
                        return false;
                }

            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
                return false;
            }
        }

        public int RegisterNotification(string _deviceStatusVariableName)
        {
            if (Client.State == ConnectionState.Connected)
            {
                var deviceStatusSymbol = MySymbols.Where(a => a.InstancePath == _deviceStatusVariableName).FirstOrDefault();
                return (int)Client.AddDeviceNotification(deviceStatusSymbol.IndexGroup,
                   deviceStatusSymbol.IndexOffset, (int)(deviceStatusSymbol.Size), NotificationSettings.Default, deviceStatusSymbol);
            }
            _log.Log(LogLevel.Error, $"TwinCAT Connection is {Client.State}");
            return -1;
        }

        private void ReadAllEthercatDeviceData()
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

        private void InitializeVarables()
        {
            SAmsNetId = (string)MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsNetId}").FirstOrDefault().ReadAnyValue(typeof(string));
            SAmsPort = (UInt16[])MySymbols.Where(a => a.InstancePath == $"MAIN.{TCSymbols.foe_sAmsPort}").FirstOrDefault().ReadAnyValue(typeof(UInt16[]));
        }

        public void ReadEthercatDeviceData(uint _motorNumber)
        {
            var mtarray = new byte[structSize];
            Client.Read(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            PLCMotors[_motorNumber].Unpack(mtarray);
        }
        public void ReadEthercatDeviceData(DeviceSymbol deviceSymbol)
        {
            switch (deviceSymbol)
            {
                case DeviceSymbol.mDirection:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mDirection =Convert.ToBoolean(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float))[0]);
                    break;
                case DeviceSymbol.mRPM:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mRPM = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mDisplacement:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mDisplacement = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mTime:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mTime = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                case DeviceSymbol.mMode:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mMode = Convert.ToUInt32(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;    
                case DeviceSymbol.mRampDelay:
                    for (uint i = 0; i < DeviceCount; i++)
                        PLCMotors[i].mRampDelay = Convert.ToSingle(ReadEthercatDeviceData(i, deviceSymbol, sizeof(float)));
                    break;
                default:
                    break;
            }
        }
        public byte[] ReadEthercatDeviceData(uint _motorNumber, DeviceSymbol deviceSymbol, int dataLength)
        {
            var aa = SymbolEthercatDevice
                      .SubSymbols[(int)_motorNumber]
                      .SubSymbols[(int)deviceSymbol].InstancePath;
            var mtarray = new byte[dataLength];
            try
            {
                var handle = Client.CreateVariableHandle(aa);
                Client.Read(handle, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, $"Motor[{_motorNumber}] Variable : {deviceSymbol} " + ex.Message);
            }
            return mtarray;
        }

        public void WriteEthercatDeviceData()
        {
            for (int i = 0; i < PLCMotors.Length; i++)
            {
                PLCMotors[i].Pack().CopyTo(buffer, structSize * i);
            }
            Client.Write(SymbolEthercatDevice.IndexGroup,
                SymbolEthercatDevice.IndexOffset, buffer.AsMemory());
        }

        public void WriteEthercatDeviceData(uint _motorNumber)
        {
            try
            {
                var mtarray = new byte[structSize];
                PLCMotors[_motorNumber].Pack().CopyTo(mtarray, 0);
                Client.Write(SymbolEthercatDevice.IndexGroup,
                    SymbolEthercatDevice.IndexOffset + (uint)_motorNumber * (uint)structSize, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.Message);
            }
        }
        public void WriteEthercatDeviceData(DeviceSymbol deviceSymbol)
        {
            switch (deviceSymbol)
            {
                case DeviceSymbol.mDirection:
                    for (uint i = 0; i < DeviceCount; i++)
                        WriteEthercatDeviceData(i, deviceSymbol,new byte[] { PLCMotors[i].mDirection.CastToArray()[0] });
                    break;
                case DeviceSymbol.mRPM:
                    for (uint i = 0; i < DeviceCount; i++)
                        WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mRPM.CastToArray());
                    break;
                case DeviceSymbol.mDisplacement:
                    for (uint i = 0; i < DeviceCount; i++)
                        WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mDisplacement.CastToArray());
                    break;
                case DeviceSymbol.mTime:
                    for (uint i = 0; i < DeviceCount; i++)
                        WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mTime.CastToArray());
                    break;
                case DeviceSymbol.mMode:
                    for (uint i = 0; i < DeviceCount; i++)
                        WriteEthercatDeviceData(i, deviceSymbol, PLCMotors[i].mMode.CastToArray());
                    break;
                default:
                    break;
            }
        }

        public void WriteEthercatDeviceData(uint _motorNumber, DeviceSymbol deviceSymbol, byte[] data)
        {
            var aa = SymbolEthercatDevice
                      .SubSymbols[(int)_motorNumber]
                      .SubSymbols[(int)deviceSymbol].InstancePath;
            try
            {
                var mtarray = new byte[data.Length];
                Array.Copy(data, mtarray, mtarray.Length);
                var handle = Client.CreateVariableHandle(aa);
                Client.Write(handle, mtarray.AsMemory());
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, $"Motor[{_motorNumber}] Variable : {deviceSymbol} " + ex.Message);
            }
        }

        public void StartpPLC()
        {
            _log.Log(LogLevel.Info, $"Resuming PLC ....");
            Client.WriteControl(new StateInfo(AdsState.Run, ClientSystemService.ReadState().DeviceState));
        }

        public void StoppPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Stop, ClientSystemService.ReadState().DeviceState));
        }

        public void ConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Config, ClientSystemService.ReadState().DeviceState));
        }

        public void ReConfigPLC()
        {
            ClientSystemService.WriteControl(new StateInfo(AdsState.Reconfig, ClientSystemService.ReadState().DeviceState));
        }

        public StateInfo GetTwincatState()
        {
            StateInfo info = new StateInfo();
            if (Client.IsConnected)
            {
                AdsErrorCode errorCode = Client.TryReadState(out info);
            }
            return info;
        }

        private bool GetPLCVariables()
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
        public bool LoadSetting()
        {
            var log = LogManager.GetLogger("AppSettings");

            // Create an XmlSerializer instance for the Person class
            XmlSerializer serializer = new(typeof(AppSettings));
            try
            {
                // Read the complete contents of the text file
                string appSettings =
                    File.ReadAllText(AppSettings.SettingRoot + "AppSettings.xml");

                using StringReader reader = new(appSettings);
                // Deserialize the XML back into a Person object
                AppSettings = serializer.Deserialize(reader) as AppSettings;
                return true;
            }
            catch (Exception ex)
            {
                if (!File.Exists(AppSettings.SettingRoot + "AppSettings_Default.xml"))
                {
                    // Create a StringWriter to hold the XML
                    StringWriter stringWriter = new();
                    // Serialize the object to XML
                    serializer.Serialize(stringWriter, new AppSettings());

                    // Write the content to the text file
                    if (!Directory.Exists(AppSettings.SettingRoot))
                        Directory.CreateDirectory(AppSettings.SettingRoot);
                    File.WriteAllText(AppSettings.SettingRoot + "AppSettings_Default.xml", stringWriter.ToString());
                }
                var tmsg = $"AppSettings.xml : {ex.Message}";
                log.Log(LogLevel.Error, tmsg);
                MessageBox.Show(tmsg);
                return false;
            }
        }
        public bool WriteSetting()
        {
            var log = LogManager.GetLogger("AppSettings");

            // Create an XmlSerializer instance for the Person class
            XmlSerializer serializer = new(typeof(AppSettings));
            try
            {
                // Create a StringWriter to hold the XML
                StringWriter stringWriter = new();
                // Serialize the object to XML
                serializer.Serialize(stringWriter, AppSettings);

                // Write the content to the text file
                if (!Directory.Exists(AppSettings.SettingRoot))
                    Directory.CreateDirectory(AppSettings.SettingRoot);
                File.WriteAllText(AppSettings.SettingRoot + "AppSettings.xml", stringWriter.ToString());

                return true;
            }
            catch (Exception ex)
            {
                var tmsg = $"AppSettings.xml : {ex.Message}";
                log.Log(LogLevel.Error, tmsg);
                MessageBox.Show(tmsg);
                return false;
            }
        }
    }
}
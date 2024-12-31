using FlexMold.MVVM.Model;
using NLog;
using System;
using System.Collections.ObjectModel;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace FlexMold.Utility
{
    public partial class TwinCathelper
    {
        private  Logger _log = LogManager.GetLogger("TwinCat");
        public  AdsConnection Client { get; private set; }
        public  AdsConnection ClientSystemService { get; set; }
        public  AdsState CurrentAdsState { get; set; }
        public  ConnectionState CurrentConnectionState { get; set; }
        public  static Int16 DeviceCount { get; private set; }
        public  AdsState MyAdsState { get; set; }
        public  AmsRouterState MyAmsRouterState { get; set; }
        public  ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        public  ConnectionState MyConnectionStateNew { get; set; }
        public  ConnectionState MyConnectionStateOld { get; set; }
        public  ObservableCollection<Symbol> MySymbols { get; private set; }
        public  EthercatDevice[] PLCMotors { get; private set; }
        public  DeviceStatuses PLCMotorsStatuses { get; private set; }
        public  string SAmsNetId { get; private set; }
        public  ushort[] SAmsPort { get; private set; }
        public  AdsSession Session { get; private set; }
        public  AdsSession SessionSystemService { get; set; }
        private  IDataTypeCollection<IDataType> _dataTypes { get; set; }
        private  int _port { get; set; }
        public  Symbol SymbolEthercatDevice { get; private set; }
        public  int structSize { get; private set; }
        public  byte[] buffer { get; private set; }
        public  AppSettings AppSettings { get; set; } = new();
        private  ESlaveLink[] _Slave_Link_Status;
        private  ESlaveStateMachine[] _Slave_StateMachine_Status;
        public  bool ECAbError { get; set; }
        private  ESlaveLink[] defaultElinkStates;
        private  ESlaveStateMachine[] defaultStateMachines;
        public  ESlaveLink[] ValidElinkStates { get; set; }
        public  ESlaveStateMachine[] ValidStateMachines { get; set; }
        public  bool Slave_StateMachine_Abort { get; set; }
        public  bool Slave_Link_Abort { get; set; }
        public  bool Motor_Diagnostic_Abort { get; set; }

        public  event EventHandler TSession_ConnectionStateChanged;
        public  event EventHandler TConnection_ConnectionStatusChanged;
        public  event EventHandler TConnection_RouterStateChanged;
        public  event EventHandler TConnection_AdsStateChanged;

        public  event EventHandler TMotor_PositionChanged;
        public  event EventHandler TMotor_ModeChanged;
        public  event EventHandler TMotor_MotorDiagnosticChanged;
        public  event EventHandler TMotor_TemperatureChanged;
        public  event EventHandler TMotor_DeviceStatusChanged;
        public  event EventHandler TSlave_Link_Status_Changed;
        public  event EventHandler TSlave_StateMachine_Status_Changed;
        public enum DeviceSymbol
        {
            mDirection = 0,
            mRPM,
            mDisplacement,
            mTime,
            mRampDelay,
            mMode,
        }

    }
}
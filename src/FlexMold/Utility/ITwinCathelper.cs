using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel;
using NLog;
using System;
using System.Collections.ObjectModel;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.Utility
{
    public interface ITwinCathelper : IViewModel
    {
        AppSettings AppSettings { get; set; }
        byte[] buffer { get; }
        AdsConnection Client { get; }
        AdsConnection ClientSystemService { get; set; }
        AdsState CurrentAdsState { get; set; }
        ConnectionState CurrentConnectionState { get; set; }
        bool ECAbError { get; set; }
        bool Motor_Diagnostic_Abort { get; set; }
        AdsState MyAdsState { get; set; }
        AmsRouterState MyAmsRouterState { get; set; }
        ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        ConnectionState MyConnectionStateNew { get; set; }
        ConnectionState MyConnectionStateOld { get; set; }
        ObservableCollection<Symbol> MySymbols { get; }
        EthercatDevice[] PLCMotors { get; }
        DeviceStatuses PLCMotorsStatuses { get; }
        string SAmsNetId { get; }
        ushort[] SAmsPort { get; }
        AdsSession Session { get; }
        AdsSession SessionSystemService { get; set; }
        bool Slave_Link_Abort { get; set; }
        ESlaveLink[] Slave_Link_Status { get; set; }
        bool Slave_StateMachine_Abort { get; set; }
        ESlaveStateMachine[] Slave_StateMachine_Status { get; set; }
        int structSize { get; }
        Symbol SymbolEthercatDevice { get; }
        ESlaveLink[] ValidElinkStates { get; set; }
        ESlaveStateMachine[] ValidStateMachines { get; set; }

        event EventHandler TConnection_AdsStateChanged;
        event EventHandler TConnection_ConnectionStatusChanged;
        event EventHandler TConnection_RouterStateChanged;
        event EventHandler TMotor_DeviceStatusChanged;
        event EventHandler TMotor_ModeChanged;
        event EventHandler TMotor_MotorDiagnosticChanged;
        event EventHandler TMotor_PositionChanged;
        event EventHandler TMotor_TemperatureChanged;
        event EventHandler TSession_ConnectionStateChanged;
        event EventHandler TSlave_Link_Status_Changed;
        event EventHandler TSlave_StateMachine_Status_Changed;

        void ConfigPLC();
        StateInfo GetTwincatState();
        void InitializeNotifications();
        bool InitializeSlaves(EMachineType emt);
        bool LoadSetting();
        void ReadEthercatDeviceData(TwinCathelper.DeviceSymbol deviceSymbol);
        void ReadEthercatDeviceData(uint _motorNumber);
        byte[] ReadEthercatDeviceData(uint _motorNumber, TwinCathelper.DeviceSymbol deviceSymbol, int dataLength);
        void ReConfigPLC();
        int RegisterNotification(string _deviceStatusVariableName);
        void SetAbortMode(Logger log);
        void SetDefaultMode(Logger log);
        void SetMachineStatusTimer();
        void StartpPLC();
        void StoppPLC();
        bool TwinCatConnect851();
        void TwinCatDiconnectConnect851();
        void WriteEthercatDeviceData();
        void WriteEthercatDeviceData(TwinCathelper.DeviceSymbol deviceSymbol);
        void WriteEthercatDeviceData(uint _motorNumber);
        void WriteEthercatDeviceData(uint _motorNumber, TwinCathelper.DeviceSymbol deviceSymbol, byte[] data);
        bool WriteSetting();
    }
}
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
    internal static partial class TwinCathelper
    {
        private static Logger _log = LogManager.GetLogger("TwinCat");
        public static AdsConnection Client { get; private set; }
        public static AdsConnection ClientSystemService { get; set; }
        public static AdsState CurrentAdsState { get; set; }
        public static ConnectionState CurrentConnectionState { get; set; }
        public static Int16 DeviceCount { get; private set; }
        public static AdsState MyAdsState { get; set; }
        public static AmsRouterState MyAmsRouterState { get; set; }
        public static ConnectionStateChangedReason MyConnectionStateChangedReason { get; set; }
        public static ConnectionState MyConnectionStateNew { get; set; }
        public static ConnectionState MyConnectionStateOld { get; set; }
        public static ObservableCollection<Symbol> MySymbols { get; private set; }
        public static EthercatDevice[] PLCMotors { get; private set; }
        public static DeviceStatuses PLCMotorsStates { get; private set; }
        public static string SAmsNetId { get; private set; }
        public static ushort[] SAmsPort { get; private set; }
        public static AdsSession Session { get; private set; }
        public static AdsSession SessionSystemService { get; set; }
        private static IDataTypeCollection<IDataType> _dataTypes { get; set; }
        private static int _port { get; set; }
        public static Symbol SymbolEthercatDevice { get; private set; }
        public static int structSize { get; private set; }
        public static byte[] buffer { get; private set; }
        public static AppSettings AppSettings { get; set; } = new();

        public static event EventHandler TSession_ConnectionStateChanged;
        public static event EventHandler TConnection_ConnectionStatusChanged;
        public static event EventHandler TConnection_RouterStateChanged;
        public static event EventHandler TConnection_AdsStateChanged;

        public static event EventHandler TMotor_PositionChanged;
        public static event EventHandler TMotor_MotorDiagnosticChanged;
        public static event EventHandler TMotor_TemperatureChanged;
        public static event EventHandler TMotor_DeviceStatusChanged;
        public enum DeviceSymbol
        {
            mDirection=0,
            mRPM,
            mDisplacement,
            mTime,
            mMode,
        }
    }
}
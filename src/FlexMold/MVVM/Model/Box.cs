using System.Collections.Generic;
using System.Xml.Serialization;

namespace FlexMold.MVVM.Model
{
    [XmlRoot(ElementName = "AmsAddress")]
    public class AmsAddress
    {
        [XmlElement(ElementName = "AmsPort")]
        public int AmsPort { get; set; }

        [XmlElement(ElementName = "AmsPortTimeout")]
        public int AmsPortTimeout { get; set; }

        [XmlElement(ElementName = "AmsNetId")]
        public string AmsNetId { get; set; }
    }

    [XmlRoot(ElementName = "BoxDef")]
    public class BoxDef
    {
        [XmlElement(ElementName = "FieldbusAddress")]
        public int FieldbusAddress { get; set; }

        [XmlElement(ElementName = "AmsAddress")]
        public AmsAddress AmsAddress { get; set; }
    }

    [XmlRoot(ElementName = "Info")]
    public class Info
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "PhysAddr")]
        public int PhysAddr { get; set; }

        [XmlElement(ElementName = "AutoIncAddr")]
        public int AutoIncAddr { get; set; }

        [XmlElement(ElementName = "Physics")]
        public string Physics { get; set; }

        [XmlElement(ElementName = "VendorId")]
        public int VendorId { get; set; }

        [XmlElement(ElementName = "ProductCode")]
        public int ProductCode { get; set; }

        [XmlElement(ElementName = "RevisionNo")]
        public int RevisionNo { get; set; }

        [XmlElement(ElementName = "SerialNo")]
        public int SerialNo { get; set; }
    }

    [XmlRoot(ElementName = "Send")]
    public class Send
    {
        [XmlElement(ElementName = "BitStart")]
        public int BitStart { get; set; }

        [XmlElement(ElementName = "BitLength")]
        public int BitLength { get; set; }

        [XmlElement(ElementName = "Start")]
        public int Start { get; set; }

        [XmlElement(ElementName = "Length")]
        public int Length { get; set; }
    }

    [XmlRoot(ElementName = "Recv")]
    public class Recv
    {
        [XmlElement(ElementName = "BitStart")]
        public int BitStart { get; set; }

        [XmlElement(ElementName = "BitLength")]
        public int BitLength { get; set; }

        [XmlElement(ElementName = "Start")]
        public int Start { get; set; }

        [XmlElement(ElementName = "Length")]
        public int Length { get; set; }

        [XmlElement(ElementName = "StatusBitAddr")]
        public int StatusBitAddr { get; set; }
    }

    [XmlRoot(ElementName = "Sm2")]
    public class Sm2
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "DefaultSize")]
        public int DefaultSize { get; set; }

        [XmlElement(ElementName = "StartAddress")]
        public int StartAddress { get; set; }

        [XmlElement(ElementName = "ControlByte")]
        public int ControlByte { get; set; }

        [XmlElement(ElementName = "Enable")]
        public int Enable { get; set; }

        [XmlElement(ElementName = "Pdo")]
        public int Pdo { get; set; }
    }

    [XmlRoot(ElementName = "Sm3")]
    public class Sm3
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "DefaultSize")]
        public int DefaultSize { get; set; }

        [XmlElement(ElementName = "StartAddress")]
        public int StartAddress { get; set; }

        [XmlElement(ElementName = "ControlByte")]
        public int ControlByte { get; set; }

        [XmlElement(ElementName = "Enable")]
        public int Enable { get; set; }

        [XmlElement(ElementName = "Pdo")]
        public int Pdo { get; set; }
    }

    [XmlRoot(ElementName = "AdsInfo")]
    public class AdsInfo
    {
        [XmlElement(ElementName = "AmsAddress")]
        public AmsAddress AmsAddress { get; set; }

        [XmlElement(ElementName = "IndexGroup")]
        public int IndexGroup { get; set; }

        [XmlElement(ElementName = "IndexOffset")]
        public int IndexOffset { get; set; }

        [XmlElement(ElementName = "Length")]
        public int Length { get; set; }
    }

    [XmlRoot(ElementName = "Entry")]
    public class Entry
    {
        [XmlElement(ElementName = "Index")]
        public string Index { get; set; }

        [XmlElement(ElementName = "SubIndex")]
        public int SubIndex { get; set; }

        [XmlElement(ElementName = "BitLen")]
        public int BitLen { get; set; }

        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "DataType")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "AdsInfo")]
        public AdsInfo AdsInfo { get; set; }
    }

    [XmlRoot(ElementName = "TxPdo")]
    public class TxPdo
    {
        [XmlElement(ElementName = "Index")]
        public string Index { get; set; }

        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Entry")]
        public List<Entry> Entry { get; set; }

        [XmlAttribute(AttributeName = "Fixed")]
        public bool Fixed { get; set; }

        [XmlAttribute(AttributeName = "Mandatory")]
        public bool Mandatory { get; set; }

        [XmlAttribute(AttributeName = "Sm")]
        public int Sm { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "RxPdo")]
    public class RxPdo
    {
        [XmlElement(ElementName = "Index")]
        public string Index { get; set; }

        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Entry")]
        public List<Entry> Entry { get; set; }

        [XmlAttribute(AttributeName = "Fixed")]
        public bool Fixed { get; set; }

        [XmlAttribute(AttributeName = "Mandatory")]
        public bool Mandatory { get; set; }

        [XmlAttribute(AttributeName = "Sm")]
        public int Sm { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ProcessData")]
    public class ProcessData
    {
        [XmlElement(ElementName = "Send")]
        public Send Send { get; set; }

        [XmlElement(ElementName = "Recv")]
        public Recv Recv { get; set; }

        [XmlElement(ElementName = "Sm2")]
        public Sm2 Sm2 { get; set; }

        [XmlElement(ElementName = "Sm3")]
        public Sm3 Sm3 { get; set; }

        [XmlElement(ElementName = "TxPdo")]
        public TxPdo TxPdo { get; set; }

        [XmlElement(ElementName = "RxPdo")]
        public RxPdo RxPdo { get; set; }
    }

    [XmlRoot(ElementName = "Timeout")]
    public class Timeout
    {
        [XmlElement(ElementName = "ReturningRequest")]
        public int ReturningRequest { get; set; }

        [XmlElement(ElementName = "Response")]
        public int Response { get; set; }

        [XmlElement(ElementName = "I2P")]
        public int I2P { get; set; }

        [XmlElement(ElementName = "P2S2O")]
        public int P2S2O { get; set; }

        [XmlElement(ElementName = "Back2PI")]
        public int Back2PI { get; set; }

        [XmlElement(ElementName = "O2S")]
        public int O2S { get; set; }
    }

    [XmlRoot(ElementName = "BootStrap")]
    public class BootStrap
    {
        [XmlElement(ElementName = "Send")]
        public Send Send { get; set; }

        [XmlElement(ElementName = "Recv")]
        public Recv Recv { get; set; }
    }

    [XmlRoot(ElementName = "ChannelInfo")]
    public class ChannelInfo
    {
        [XmlElement(ElementName = "ProfileNo")]
        public int ProfileNo { get; set; }

        [XmlElement(ElementName = "AddInfo")]
        public int AddInfo { get; set; }
    }

    [XmlRoot(ElementName = "Profile")]
    public class Profile
    {
        [XmlElement(ElementName = "ChannelInfo")]
        public ChannelInfo ChannelInfo { get; set; }
    }

    [XmlRoot(ElementName = "CoE")]
    public class CoE
    {
        [XmlElement(ElementName = "CanOpenType")]
        public int CanOpenType { get; set; }

        [XmlElement(ElementName = "Profile")]
        public Profile Profile { get; set; }
    }

    [XmlRoot(ElementName = "Mailbox")]
    public class Mailbox
    {
        [XmlElement(ElementName = "Send")]
        public Send Send { get; set; }

        [XmlElement(ElementName = "Recv")]
        public Recv Recv { get; set; }

        [XmlElement(ElementName = "Timeout")]
        public Timeout Timeout { get; set; }

        [XmlElement(ElementName = "BootStrap")]
        public BootStrap BootStrap { get; set; }

        [XmlElement(ElementName = "Protocol")]
        public List<string> Protocol { get; set; }

        [XmlElement(ElementName = "CoE")]
        public CoE CoE { get; set; }

        [XmlAttribute(AttributeName = "DataLinkLayer")]
        public bool DataLinkLayer { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "InitCmd")]
    public class InitCmd
    {
        [XmlElement(ElementName = "Transition")]
        public List<string> Transition { get; set; }

        [XmlElement(ElementName = "Comment")]
        public string Comment { get; set; }

        [XmlElement(ElementName = "Requires")]
        public string Requires { get; set; }

        [XmlElement(ElementName = "Cmd")]
        public int Cmd { get; set; }

        [XmlElement(ElementName = "Adp")]
        public int Adp { get; set; }

        [XmlElement(ElementName = "Ado")]
        public int Ado { get; set; }

        [XmlElement(ElementName = "Data")]
        public string Data { get; set; }

        [XmlElement(ElementName = "Retries")]
        public int Retries { get; set; }

        [XmlElement(ElementName = "Timeout")]
        public int Timeout { get; set; }

        [XmlElement(ElementName = "Validate")]
        public Validate Validate { get; set; }

        [XmlElement(ElementName = "Cnt")]
        public int Cnt { get; set; }
    }

    [XmlRoot(ElementName = "Validate")]
    public class Validate
    {
        [XmlElement(ElementName = "Data")]
        public string Data { get; set; }

        [XmlElement(ElementName = "DataMask")]
        public string DataMask { get; set; }

        [XmlElement(ElementName = "Timeout")]
        public int Timeout { get; set; }
    }

    [XmlRoot(ElementName = "InitCmds")]
    public class InitCmds
    {
        [XmlElement(ElementName = "InitCmd")]
        public List<InitCmd> InitCmd { get; set; }
    }

    [XmlRoot(ElementName = "CycleTimeSync0")]
    public class CycleTimeSync0
    {
        [XmlAttribute(AttributeName = "Factor")]
        public int Factor { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "CycleTimeSync1")]
    public class CycleTimeSync1
    {
        [XmlAttribute(AttributeName = "Factor")]
        public int Factor { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "OpMode")]
    public class OpMode
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Desc")]
        public string Desc { get; set; }

        [XmlElement(ElementName = "AssignActivate")]
        public string AssignActivate { get; set; }

        [XmlElement(ElementName = "CycleTimeSync0")]
        public CycleTimeSync0 CycleTimeSync0 { get; set; }

        [XmlElement(ElementName = "ShiftTimeSync0")]
        public int ShiftTimeSync0 { get; set; }

        [XmlElement(ElementName = "CycleTimeSync1")]
        public CycleTimeSync1 CycleTimeSync1 { get; set; }

        [XmlElement(ElementName = "ShiftTimeSync1")]
        public int ShiftTimeSync1 { get; set; }

        [XmlAttribute(AttributeName = "Selected")]
        public bool Selected { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "DC")]
    public class DC
    {
        [XmlElement(ElementName = "PotentialReferenceClock")]
        public bool PotentialReferenceClock { get; set; }

        [XmlElement(ElementName = "TimeLoopControlOnly")]
        public bool TimeLoopControlOnly { get; set; }

        [XmlElement(ElementName = "OpMode")]
        public List<OpMode> OpMode { get; set; }
    }

    [XmlRoot(ElementName = "StateMachine")]
    public class StateMachine
    {
        [XmlElement(ElementName = "AutoRestoreStates")]
        public bool AutoRestoreStates { get; set; }

        [XmlElement(ElementName = "WaitForWcStateOkay")]
        public bool WaitForWcStateOkay { get; set; }

        [XmlElement(ElementName = "ReInitAfterCommError")]
        public bool ReInitAfterCommError { get; set; }

        [XmlElement(ElementName = "LogCommChanges")]
        public bool LogCommChanges { get; set; }

        [XmlElement(ElementName = "FinalState")]
        public string FinalState { get; set; }

        [XmlElement(ElementName = "Timeout")]
        public Timeout Timeout { get; set; }

        [XmlElement(ElementName = "CheckVendorId")]
        public string CheckVendorId { get; set; }

        [XmlElement(ElementName = "CheckProductCode")]
        public string CheckProductCode { get; set; }

        [XmlElement(ElementName = "CheckRevisionNo")]
        public object CheckRevisionNo { get; set; }

        [XmlElement(ElementName = "CheckSerialNo")]
        public object CheckSerialNo { get; set; }

        [XmlElement(ElementName = "CheckIdentification")]
        public object CheckIdentification { get; set; }
    }

    [XmlRoot(ElementName = "Divider0400")]
    public class Divider0400
    {
        [XmlAttribute(AttributeName = "Enabled")]
        public bool Enabled { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "PdiTime0410")]
    public class PdiTime0410
    {
        [XmlAttribute(AttributeName = "Enabled")]
        public bool Enabled { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "SmTime0420")]
    public class SmTime0420
    {
        [XmlAttribute(AttributeName = "Enabled")]
        public bool Enabled { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "Watchdog")]
    public class Watchdog
    {
        [XmlElement(ElementName = "Divider0400")]
        public Divider0400 Divider0400 { get; set; }

        [XmlElement(ElementName = "PdiTime0410")]
        public PdiTime0410 PdiTime0410 { get; set; }

        [XmlElement(ElementName = "SmTime0420")]
        public SmTime0420 SmTime0420 { get; set; }
    }

    [XmlRoot(ElementName = "PdoSettings")]
    public class PdoSettings
    {
        [XmlElement(ElementName = "PdoAssign")]
        public int PdoAssign { get; set; }

        [XmlElement(ElementName = "PdoConfig")]
        public int PdoConfig { get; set; }
    }

    [XmlRoot(ElementName = "InfoData")]
    public class InfoData
    {
        [XmlElement(ElementName = "State")]
        public bool State { get; set; }

        [XmlElement(ElementName = "AdsAddress")]
        public bool AdsAddress { get; set; }

        [XmlElement(ElementName = "Channels")]
        public bool Channels { get; set; }

        [XmlElement(ElementName = "DcTimes")]
        public bool DcTimes { get; set; }

        [XmlElement(ElementName = "ObjectId")]
        public bool ObjectId { get; set; }
    }

    [XmlRoot(ElementName = "Settings")]
    public class Settings
    {
        [XmlElement(ElementName = "StateMachine")]
        public StateMachine StateMachine { get; set; }

        [XmlElement(ElementName = "Watchdog")]
        public Watchdog Watchdog { get; set; }

        [XmlElement(ElementName = "PdoSettings")]
        public PdoSettings PdoSettings { get; set; }

        [XmlElement(ElementName = "InfoData")]
        public InfoData InfoData { get; set; }
    }

    [XmlRoot(ElementName = "TwinCAT")]
    public class TwinCAT
    {
        [XmlElement(ElementName = "ExtensionSupportFlags")]
        public int ExtensionSupportFlags { get; set; }
    }

    [XmlRoot(ElementName = "Slave")]
    public class Slave
    {
        [XmlElement(ElementName = "Info")]
        public Info Info { get; set; }

        [XmlElement(ElementName = "ProcessData")]
        public ProcessData ProcessData { get; set; }

        [XmlElement(ElementName = "Mailbox")]
        public Mailbox Mailbox { get; set; }

        [XmlElement(ElementName = "InitCmds")]
        public InitCmds InitCmds { get; set; }

        [XmlElement(ElementName = "DC")]
        public DC DC { get; set; }

        [XmlElement(ElementName = "Settings")]
        public Settings Settings { get; set; }

        [XmlElement(ElementName = "EsiFile")]
        public string EsiFile { get; set; }

        [XmlElement(ElementName = "TwinCAT")]
        public TwinCAT TwinCAT { get; set; }
    }

    [XmlRoot(ElementName = "EtherCAT")]
    public class EtherCAT
    {
        [XmlElement(ElementName = "Slave")]
        public Slave Slave { get; set; }

        [XmlElement(ElementName = "AmsAddress")]
        public AmsAddress AmsAddress { get; set; }
    }

    [XmlRoot(ElementName = "TreeItem")]
    public class TreeItem
    {
        [XmlElement(ElementName = "ItemName")]
        public string ItemName { get; set; }

        [XmlElement(ElementName = "PathName")]
        public string PathName { get; set; }

        [XmlElement(ElementName = "ItemType")]
        public int ItemType { get; set; }

        [XmlElement(ElementName = "ItemId")]
        public int ItemId { get; set; }

        [XmlElement(ElementName = "ObjectId")]
        public string ObjectId { get; set; }

        [XmlElement(ElementName = "ItemSubType")]
        public int ItemSubType { get; set; }

        [XmlElement(ElementName = "ItemSubTypeName")]
        public string ItemSubTypeName { get; set; }

        [XmlElement(ElementName = "ChildCount")]
        public int ChildCount { get; set; }

        [XmlElement(ElementName = "Disabled")]
        public bool Disabled { get; set; }

        [XmlElement(ElementName = "TreeImageData16x14")]
        public string TreeImageData16x14 { get; set; }

        [XmlElement(ElementName = "BoxDef")]
        public BoxDef BoxDef { get; set; }

        [XmlElement(ElementName = "EtherCAT")]
        public EtherCAT EtherCAT { get; set; }
    }
}
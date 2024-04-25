using EnvDTE;
using EnvDTE80;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TCatSysManagerLib;
using TwinCAT.Ads;
using TwinCAT.SystemManager;

namespace FlexMold.MVVM.ViewModel
{
    /// <summary>
    /// Demonstrates the generation + compilation of PLC projects.
    /// The method Execute() will be called by ScriptingContainer and will execute the actual script code.
    /// </summary>
    public class TcDeviceScanning
        : ScriptEarlyBound
    {
        public TcDeviceScanning(ITwinCathelper twinCathelper)
        {
            this._TwinCathelper = twinCathelper;
        }
        private ITcSysManager4 systemManager = null;
        private EnvDTE.Project project = null;

        /// <summary>
        /// Handler function Initializing the Script (Configuration preparations)
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>Usually used to to the open a prepared or new XAE configuration</remarks>
        protected override void OnInitialize(IContext context)
        {
            base.OnInitialize(context);
        }

        /// <summary>
        /// Handler function called after the Solution object has been created.
        /// </summary>
        protected override void OnSolutionCreated()
        {
            this.project = (EnvDTE.Project)CreateNewProject();
            this.systemManager = (ITcSysManager4)project.Object;
            base.OnSolutionCreated();
        }

        /// <summary>
        /// Cleaning up the XAE configuration after script execution.
        /// </summary>
        /// <param name="worker">The worker.</param>
        protected override void OnCleanUp(IWorker worker)
        {
            base.OnCleanUp(worker);
        }

        /// <summary>
        /// Insertion Mode for creating PLC projects.
        /// </summary>
        public enum CreatePlcMode
        {
            /// <summary>
            /// Copies a PLC Project
            /// </summary>
            Copy = 0,

            /// <summary>
            /// Moves a PLC Project
            /// </summary>
            Move = 1,

            /// <summary>
            /// References a PLC Project
            /// </summary>
            Reference = 2
        }

        private void browse(ITcSmTreeItem root, IWorker worker)
        {
            string name = root.Name;
            string pathName = root.PathName;
            TreeItemType itemType = (TreeItemType)root.ItemType;
            string subTypeName = root.ItemSubTypeName;

            worker.ProgressStatus = string.Format("Browsing node '{0}'", pathName);

            string xml = root.ProduceXml();

            // Iteration over each node and produce output for demo purposes
            foreach (ITcSmTreeItem3 child in root)
            {
                browse(child, worker);
            }
        }

        private void AddLibrary(string _LibraryName, ITcSmTreeItem plcProject, IWorker worker)
        {
            worker.ProgressStatus = string.Format("Adding Library '{0}' ...", _LibraryName);

            ITcSmTreeItem referencesItem = plcProject.LookupChild("References");
            ITcPlcLibraryManager libraryManager = (ITcPlcLibraryManager)referencesItem;
            libraryManager.AddLibrary(_LibraryName);
        }

        /// <summary>
        /// Handler function Executing the Script code.
        /// </summary>
        /// <param name="worker">The worker.</param>
        protected override void OnExecute(IWorker worker)
        {
            if (worker.CancellationPending)
                throw new Exception("Cancelled");
            worker.ProgressStatus = $"Connecting to TwinCAT Via ADS On Port {(int)AmsPort.SystemService}";
            _TwinCathelper.SessionSystemService = new AdsSession(AmsNetId.Local, (int)AmsPort.SystemService);
            _TwinCathelper.ClientSystemService = (AdsConnection)_TwinCathelper.SessionSystemService.Connect();
            worker.Progress = 8;

            worker.ProgressStatus = $"Configuring TwinCAT System into ConfigMode ....";
            _TwinCathelper.ReConfigPLC();
            //int timeoutmsec = int.Parse(System.Configuration.ConfigurationManager.AppSettings["AdsStateTimeout"].ToString()) * 1000;
            int timeoutmsec = CSVHelper.AdsStateTimeout * 1000;

            WaitForState(AdsState.Config, timeoutmsec);
            //WaitForState(AdsState.Config, 3000);
            worker.Progress = 10;

            /* =================================================================================================
             * Get references to PLC and IO nodes
             * ================================================================================================= */
            ITcSmTreeItem plcConfig = systemManager.LookupTreeItem("TIPC"); // Getting PLC Configuration item
            ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID"); // Getting IO-Configuration item

            /* =================================================================================================
             * Scans the Fieldbus interfaces and adds an EtherCAT Device.
             * ================================================================================================= */
            ScanDevicesAndBoxes(worker); //custom code
            worker.Progress = 15;
            if (AvaiableDevicesCount > 0 && AvaiableBoxesCount > 0)
            {
                /* =================================================================================================
                 * Attach an empty PLC Project using the "TwinCAT Project" template
                 * ================================================================================================= */
                worker.ProgressStatus = "Creating empty PLC Project ...";
                ITcSmTreeItem plcGenerated = plcConfig.CreateChild("PlcGenerated", 0, "", vsXaePlcStandardTemplateName);

                worker.ProgressStatus = "PLC Project created ...";
                worker.Progress = 20;

                /* =================================================================================================
                 * Get references to attached PLC Project and some of its childs
                 * ================================================================================================= */
                ITcSmTreeItem plcProjectRootItem = systemManager.LookupTreeItem("TIPC^PlcGenerated"); // Plc Project Root (XAE Base side)
                ITcSmTreeItem plcProjectItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project"); // PlcProject (PlcControl side) determined via LookupTreeItem
                ITcSmTreeItem plcPousItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^POUs");
                ITcSmTreeItem plcDutsItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^DUTs");
                ITcSmTreeItem plcProject = plcGenerated.LookupChild("PlcGenerated" + " Project");
                AddLibrary("Tc2_EtherCAT, * (Beckhoff Automation GmbH)", plcProject, worker);
                Console.WriteLine();

                worker.Progress = 30;

                /* =================================================================================================
                 * Creating POUs under folder "MyPous"
                 * ================================================================================================= */
                CreatePOUsST(plcPousItem, worker);
                worker.Progress = 40;

                /* =================================================================================================
                 * Adjust Visual Studio Solution configuration
                 * ================================================================================================= */
                SolutionBuild solutionBuild = solution.SolutionBuild;
                SolutionConfiguration activeConfig = solutionBuild.ActiveConfiguration;
                SolutionContexts contexts = activeConfig.SolutionContexts;

                // ATTENTION: VisualStudio has a bug so that the SolutionContexts Collections cannot be used. This
                // Bug is fixed by Microsoft in VS2012 and later!

                // Iterate over the active Configuration Solution Contexts and activate all Projects within the Solution
                // (Root and Nested projects)
                foreach (SolutionContext context in activeConfig.SolutionContexts)
                {
                    string projectName = context.ProjectName;
                    string configName = context.ConfigurationName;
                    string platform = context.PlatformName;

                    bool shouldBuild = context.ShouldBuild;

                    if (shouldBuild == false) // If not set
                    {
                        context.ShouldBuild = true; // ShouldBuild can be set.
                    }
                }

                /* =================================================================================================
                 * Compiling project using Automation Interface methods
                 * ================================================================================================= */
                ITcPlcProject iecProjectRoot = (ITcPlcProject)plcProjectRootItem;
                worker.ProgressStatus = "Compiling project ...";

                iecProjectRoot.CompileProject();

                ErrorItems errors = dte.ToolWindows.ErrorList.ErrorItems;
                worker.Progress = 55;
                iecProjectRoot.BootProjectAutostart = true;

                /* =================================================================================================
                 * Compiling project using Visual Studio DTE methods
                 * ================================================================================================= */
                errors = null;
                worker.ProgressStatus = "Compiling project (3) ...";
                dte.Solution.SolutionBuild.Build(true);
                waitForBuildAndCheckErrors(worker, out errors);
                worker.Progress = 60;
                if (errors.Count == 0)
                {
                    /* =================================================================================================
                     * Generating Boot Project
                     * ================================================================================================= */
                    worker.ProgressStatus = "Generating boot project ...";
                    iecProjectRoot.GenerateBootProject(true);
                    worker.Progress = 65;

                    /* =================================================================================================
                     * Prepare XML for PLC Login
                     * ================================================================================================= */
                    string xmlLogin = @"<TreeItem>
                                    <IECProjectDef>
                                        <OnlineSettings>
                                                <Commands>
                                                        <LoginCmd>true</LoginCmd>
                                                        <LogoutCmd>false</LogoutCmd>
                                                        <StartCmd>false</StartCmd>
                                                        <StopCmd>false</StopCmd>
                                                        <ResetColdCmd>false</ResetColdCmd>
                                                        <ResetOriginCmd>false</ResetOriginCmd>
                                                </Commands>
                                        </OnlineSettings>
                                    </IECProjectDef>
                                </TreeItem>";

                    /* =================================================================================================
                     * Prepare XML for PLC Start
                     * ================================================================================================= */
                    string xmlStart = @"<TreeItem>
                                    <IECProjectDef>
                                        <OnlineSettings>
                                                <Commands>
                                                        <LoginCmd>false</LoginCmd>
                                                        <LogoutCmd>false</LogoutCmd>
                                                        <StartCmd>true</StartCmd>
                                                        <StopCmd>false</StopCmd>
                                                        <ResetColdCmd>false</ResetColdCmd>
                                                        <ResetOriginCmd>false</ResetOriginCmd>
                                                </Commands>
                                        </OnlineSettings>
                                    </IECProjectDef>
                                </TreeItem>";

                    /* =================================================================================================
                     * Prepare XML for PLC Stop
                     * ================================================================================================= */
                    string xmlStop = @"<TreeItem>
                                    <IECProjectDef>
                                        <OnlineSettings>
                                                <Commands>
                                                        <LoginCmd>false</LoginCmd>
                                                        <LogoutCmd>false</LogoutCmd>
                                                        <StartCmd>false</StartCmd>
                                                        <StopCmd>true</StopCmd>
                                                        <ResetColdCmd>false</ResetColdCmd>
                                                        <ResetOriginCmd>false</ResetOriginCmd>
                                                </Commands>
                                        </OnlineSettings>
                                    </IECProjectDef>
                                </TreeItem>";

                    /* =================================================================================================
                     * Prepare XML for PLC Logout
                     * ================================================================================================= */
                    string xmlLogout = @"<TreeItem>
                                    <IECProjectDef>
                                        <OnlineSettings>
                                                <Commands>
                                                        <LoginCmd>false</LoginCmd>
                                                        <LogoutCmd>true</LogoutCmd>
                                                        <StartCmd>false</StartCmd>
                                                        <StopCmd>false</StopCmd>
                                                        <ResetColdCmd>false</ResetColdCmd>
                                                        <ResetOriginCmd>false</ResetOriginCmd>
                                                </Commands>
                                        </OnlineSettings>
                                    </IECProjectDef>
                                </TreeItem>";

                    /* =================================================================================================
                     * Activate configuration and restart TwinCAT
                     * ================================================================================================= */
                    worker.ProgressStatus = "Activating configuration ...";
                    systemManager.ActivateConfiguration();
                    worker.ProgressStatus = "Restarting TwinCAT ...";
                    systemManager.StartRestartTwinCAT();
                    worker.Progress = 70;

                    /* =================================================================================================
                     * Execute PLC Login, Start, Logout, ...
                     * ================================================================================================= */
                    System.Threading.Thread.Sleep(5000);
                    worker.Progress = 75;

                    worker.ProgressStatus = "Logging in to PLC runtime ...";
                    System.Threading.Thread.Sleep(5000);
                    plcProjectItem.ConsumeXml(xmlLogin);
                    worker.Progress = 80;

                    worker.ProgressStatus = "Stopping PLC runtime ...";
                    System.Threading.Thread.Sleep(5000);
                    plcProjectItem.ConsumeXml(xmlStop);
                    worker.Progress = 85;

                    worker.ProgressStatus = "Starting PLC runtime ...";
                    System.Threading.Thread.Sleep(5000);
                    plcProjectItem.ConsumeXml(xmlStart);
                    worker.Progress = 90;

                    worker.ProgressStatus = "Logging out of PLC runtime ...";
                    System.Threading.Thread.Sleep(5000);
                    plcProjectItem.ConsumeXml(xmlLogout);
                    worker.Progress = 95;
                    worker.IsBuildSucceeded = true;
                }
                else
                {
                    worker.ProgressStatus = "TwinCAT Project build Failed";
                    worker.Progress = 0;
                    worker.IsBuildSucceeded = false;
                }
            }
            else
            {
                worker.ProgressStatus = "No Device or Box Found";
                worker.IsBuildSucceeded = false;
                worker.Progress = 0;
            }
        }

        private bool WaitForState(AdsState state, int timeOutInMilliSeconds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds <= timeOutInMilliSeconds)
            {
                try
                {
                    if (_TwinCathelper.ClientSystemService.ReadState().AdsState == state)
                    {
                        return true;
                    }
                }
                catch (AdsErrorException)
                {
                    // This can happen while ADS changes state and we try to read it
                }
                finally
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }

            stopwatch.Stop();
            return false;
        }
        private bool waitForBuildAndCheckErrors(IWorker worker, out ErrorItems errorItems)
        {
            bool buildSucceeded = false;
            vsBuildState state = dte.Solution.SolutionBuild.BuildState;

            /* =================================================================================================
             * Wait for build process to finish
             * ================================================================================================= */
            while (state == vsBuildState.vsBuildStateInProgress)
            {
                System.Threading.Thread.Sleep(500);
                state = dte.Solution.SolutionBuild.BuildState;
            }
            buildSucceeded = (dte.Solution.SolutionBuild.LastBuildInfo == 0 && state == vsBuildState.vsBuildStateDone);

            /* =================================================================================================
             * Check for any errors. Please note that the ErrorList is not significant for a successful build!
             * Because the PLC Project can contain errors within not used types.
             * Relevant is only the "LastBuildInfo" object that contains the numer of failed project compilations!
             * ================================================================================================= */
            ErrorList errorList = dte.ToolWindows.ErrorList;
            errorItems = errorList.ErrorItems;

            int overallMessages = errorItems.Count;

            int errors = 0;
            int warnings = 0;
            int messages = 0;

            for (int i = 1; i <= overallMessages; i++) // Please note: List is starting from 1 !
            {
                ErrorItem item = errorItems.Item(i);

                if (item.ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelHigh)
                {
                    errors++;
                    worker.ProgressStatus = "Compiler error: " + item.Description;
                }
                else if (item.ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelMedium)
                {
                    warnings++;
                    worker.ProgressStatus = "Compiler warning: " + item.Description;
                }
                else if (item.ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelLow)
                {
                    messages++;
                    worker.ProgressStatus = "Compiler message: " + item.Description;
                }
            }
            return buildSucceeded;
        }

        private void CreatePOUsST(ITcSmTreeItem parent, IWorker worker)
        {
            /* =================================================================================================
             * Creating ST POUs using CreateChild()
             * ================================================================================================= */
            IECLanguageType language = IECLanguageType.ST;
            worker.ProgressStatus = "Generating ST POUs ...";
            ITcSmTreeItem program = AddPOUProgram("Main", parent, language, worker);
        }

        private string deviceName = "LAN9255_EtherCAT_Slave";

        private ITcSmTreeItem AddPOUProgram(string pouName, ITcSmTreeItem parent, IECLanguageType language, IWorker worker)
        {
            ITcSmTreeItem programPou = parent.LookupChild("Main");
            ITcPlcDeclaration pouDecl = programPou as ITcPlcDeclaration;
            ITcPlcImplementation pouImpl = programPou as ITcPlcImplementation;
            ITcXmlDocument pouDoc = programPou as ITcXmlDocument;
            string foe_sAmsNetId = $"foe_sAmsNetId: T_AmsNetId :=";
            string foe_sAmsPort = $"foe_sAmsPort: ARRAY[1..deviceCount] OF UINT :=[";
            string outStr = "Outputs process data mapping";
            string inpStr = "Inputs process data mapping";
            foe_sAmsNetId += $"'{FCAModel.MyBoxes[0].EtherCAT.AmsAddress.AmsNetId}';";
            foreach (var item in FCAModel.MyBoxes)
            {
                foe_sAmsPort += $"{item.EtherCAT.AmsAddress.AmsPort},";
            }
            string foe_Scrip =
                    "fbDownload : FB_EcFoeLoad ;\n" +
                    "foe_dwPass     : DWORD := 0; // 287454020\n" +
                    "foe_nSlaveAddr : UINT := 0;\n" +
                    "foe_eMode      : E_EcFoeMode := eFoeMode_Write;\n" +
                    "foe_sPathName : STRING := '';\n" +
                    "foe_bLoad      : BOOL;\n" +
                    "foe_bBusy      : BOOL;\n" +
                    "foe_bError     : BOOL;\n" +
                    "foe_nErrID     : UDINT;\n" +
                    "foe_nBytesWritten : UDINT;\n" +
                    "foe_nPercent      : UDINT;\n";

            string exstate = "fbSetSlaveState: FB_EcSetSlaveState;\r\n" +
                "ecs_bExecute   : BOOL; \r\n" +
                "ecs_reqState   : WORD; \r\n" +
                "ecs_bBusy     : BOOL;\r\n" +
                "ecs_bError    : BOOL;\r\n" +
                "ecs_nErrId    : UDINT;\r\n" +
                "ecs_currState : ST_EcSlaveState;";
            string getstate = "fbGetSlaveState : FB_EcGetSlaveState;\r\n" +
                "state           : ST_EcSlaveState;" +
                "ecg_bExecute : BOOL;" +
                "ecg_bError          : BOOL;" +
                "ecg_nErrId          : UDINT;";

            var fbGetSlaveState = "" +
         "fbGetAllSlaveStates : FB_EcGetAllSlaveStates;\n" +
         "    ECAbExecute            : BOOL;\n" +
         "    ECAdevStates           : ARRAY[1..deviceCount] OF ST_EcSlaveState;\n" +
         "    ECAnSlaves             : UINT := 0;\n" +
         "    ECAbError              : BOOL;\n" +
         "    ECAnErrId              : UDINT;";

            string decl1 =
                "PROGRAM MAIN \n" +
                "VAR CONSTANT \n" +
                $"deviceCount : WORD := {FCAModel.MyBoxes.Count}; \n" +
                "END_VAR \n" +
                "VAR \n" +
                 "{attribute 'TcLinkTo' := " + $"'{devices[0].PathName}^Inputs^DevState" + "'}\n" +
                 $"SlaveCount AT%I* : UINT; \n" +
                foe_sAmsNetId + "\n" + foe_sAmsPort.Trim(',') + "];\n" +
                foe_Scrip + "\n" + exstate + "\n" + getstate + "\n" + fbGetSlaveState +
                    "\nEND_VAR";
            string fbgetslaveIpl = "fbGetAllSlaveStates(sNetId:= foe_sAmsNetId, pStateBuf := ADR(ECAdevStates), cbBufLen:=SIZEOF(ECAdevStates), bExecute:=ECAbExecute);\r\nECAnSlaves := fbGetAllSlaveStates.nSlaves;\r\nECAbError := fbGetAllSlaveStates.bError;\r\nECAnErrId := fbGetAllSlaveStates.nErrId;";
            string implfoe = "fbDownload( bExecute := foe_bLoad,\r\n\t\tsNetId  := foe_sAmsNetId,\r\n\t\tnSlaveAddr := foe_nSlaveAddr,\r\n\t\tdwPass  := foe_dwPass,\r\n\t\tsPathName  := foe_sPathName, \r\n        bBusy => foe_bBusy,\r\n        bError => foe_bError,\r\n        nErrId => foe_nErrID,\r\n        cbLoad => foe_nBytesWritten,\r\n        nProgress => foe_nPercent );";
            string implexstate = "fbSetSlaveState(\r\n\tsNetId := foe_sAmsNetId,\r\n\tnSlaveAddr := foe_nSlaveAddr,\r\n\tbExecute := ecs_bExecute,\r\n\treqState := ecs_reqState, \r\n\tnErrId =>ecs_nErrId, \r\n\tbBusy=> ecs_bBusy, \r\n\tbError=>ecs_bError\r\n);";
            string implgetstate = "fbGetSlaveState(sNetId:= foe_sAmsNetId, nSlaveAddr:= foe_nSlaveAddr, bExecute:=ecg_bExecute);\r\nstate := fbGetSlaveState.state;\r\n ecg_bError := fbGetSlaveState.bError;\r\n ecg_nErrId := fbGetSlaveState.nErrId;";
            pouDecl.DeclarationText = decl1;
            pouImpl.ImplementationText = implfoe + "\n" + implexstate + "\n" + implgetstate + "\n" + fbgetslaveIpl;

            return programPou;
        }

        public override string Description
        {
            get { return "Demonstrates the creation of PLC projects, POUs etc., Setting of Solution Configurations."; }
        }

        public override string DetailedDescription
        {
            get
            {
                string test = @"Creation of a new PLC Project from Scratch. Adding Libraries, POUs of the different types and languages and referencing tasks.
Compilation of the project and Generating a Boot project.  Access and adjustment of Solution Configurations
";
                return test;
            }
        }

        public override string Keywords
        {
            get
            {
                return "Create PlcProject, POU Management, Interface / Code access, PlcImport/Export, Build Configuration";
            }
        }

        public override Version TwinCATVersion
        {
            get
            {
                return new Version(3, 1);
            }
        }

        public override string TwinCATBuild
        {
            get
            {
                return "4012";
            }
        }

        public override string Category
        {
            get
            {
                return "PLC";
            }
        }

        public int AvaiableBoxesCount { get; set; }
        public int AvaiableDevicesCount { get; set; }
        public List<ITcSmTreeItem> AvailableBoxes { get; set; }
        List<ITcSmTreeItem> devices;
        private readonly ITwinCathelper _TwinCathelper;

        public void ScanDevicesAndBoxes(IWorker worker)
        {
            FCAModel.MyBoxes = new List<TreeItem>();
            AvailableBoxes = new List<ITcSmTreeItem>();
            //AvaiableDevices = 1;
            //AvaiableBoxesCount = 2;
            if (AvaiableBoxesCount > 0 && AvaiableBoxesCount > 0)
            {
                worker.ProgressStatus = "! Warning .......";
                worker.ProgressStatus = "Simulated Version";
                worker.ProgressStatus = $"Dummy  {AvaiableDevicesCount} Divices and {AvaiableBoxesCount} Boxes added";
            }
            else
            {
                worker.ProgressStatus = $"Scanning Connected Devices and boxes....";
                ITcSmTreeItem ioDevicesItem = systemManager.LookupTreeItem("TIID");
                string scannedXml = ioDevicesItem.ProduceXml(false);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(scannedXml);
                XmlNodeList xmlDeviceList = xmlDoc.SelectNodes("TreeItem/DeviceGrpDef/FoundDevices/Device");
                devices = new List<ITcSmTreeItem>();
                int deviceCount = 0;
                foreach (XmlNode node in xmlDeviceList)
                {
                    int itemSubType = int.Parse(node.SelectSingleNode("ItemSubType").InnerText);
                    string typeName = node.SelectSingleNode("ItemSubTypeName").InnerText;
                    XmlNode xmlAddress = node.SelectSingleNode("AddressInfo");
                    ITcSmTreeItem device = ioDevicesItem.CreateChild(string.Format("Device_{0}", ++deviceCount), itemSubType, string.Empty, null);
                    string xml = string.Format("<TreeItem><DeviceDef>{0}</DeviceDef></TreeItem>", xmlAddress.OuterXml);
                    worker.ProgressStatus = $"xmlAddress : {xmlAddress.InnerText}, itemSubType : {itemSubType}";
                    device.ConsumeXml(xml);
                    devices.Add(device);
                }
                foreach (ITcSmTreeItem device in devices)
                {
                    string xml = "<TreeItem><DeviceDef><ScanBoxes>1</ScanBoxes></DeviceDef></TreeItem>";
                    try
                    {
                        device.ConsumeXml(xml);
                    }
                    catch (Exception ex)
                    {
                        worker.ProgressStatus = "Warning: " + ex.Message;
                    }
                    AvaiableDevicesCount = AvaiableDevicesCount + 1;

                    foreach (ITcSmTreeItem box in device)
                    {
                        if (box.PathName.Contains(deviceName))
                        {
                            var boxxml = box.ProduceXml();

                            XmlSerializer serializer = new XmlSerializer(typeof(TreeItem));
                            using (StringReader reader = new StringReader(boxxml))
                            {
                                var test = (TreeItem)serializer.Deserialize(reader);
                                FCAModel.MyBoxes.Add(test);
                            }
                            AvailableBoxes.Add(box);
                            AvaiableBoxesCount = AvaiableBoxesCount + 1;
                            worker.ProgressStatus = $"{box.PathName}";
                        }
                    }
                }
            }
        }
    }
}

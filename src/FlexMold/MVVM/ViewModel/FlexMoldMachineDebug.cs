using EnvDTE;
using EnvDTE80;
using FlexMold.Utility;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using TCatSysManagerLib;
using TwinCAT.Ads;
using TwinCAT.SystemManager;

namespace FlexMold.MVVM.ViewModel
{
    /// <summary>
    /// Demonstrates the generation + compilation of PLC projects.
    /// The method Execute() will be called by ScriptingContainer and will execute the actual script code.
    /// </summary>
    public class FlexMoldMachineDebug
        : ScriptEarlyBound
    {
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
            TwinCathelper.SessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
            TwinCathelper.ClientSystemService = (AdsConnection)TwinCathelper.SessionSystemService.Connect();
            worker.Progress = 8;
            worker.ProgressStatus = $"Configuring TwinCAT System into ConfigMode ....";
            TwinCathelper.ReConfigPLC();
            WaitForState(AdsState.Config, 3000);
            worker.Progress = 10;

            /* =================================================================================================
             * Get references to PLC and IO nodes
             * ================================================================================================= */
            ITcSmTreeItem plcConfig = systemManager.LookupTreeItem("TIPC"); // Getting PLC Configuration item
            ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID"); // Getting IO-Configuration item

            /* =================================================================================================
             * Scans the Fieldbus interfaces and adds an EtherCAT Device.
             * ================================================================================================= */
            worker.ProgressStatus = $"Scanning Devices ....";
            ScanDevicesAndBoxes(worker); //custom code
            worker.Progress = 15;

            /* =================================================================================================
             * Attach an empty PLC Project using the "TwinCAT Project" template
             * ================================================================================================= */
            worker.ProgressStatus = "Creating empty PLC Project ...";
            string plcProjectName = "PlcGenerated";
            ITcSmTreeItem plcProjectRoot = plcConfig.CreateChild(plcProjectName, 0, "", vsXaePlcStandardTemplateName);

            worker.ProgressStatus = "PLC Project created ...";
            worker.Progress = 20;

            /* =================================================================================================
             * Get references to attached PLC Project and some of its childs
             * ================================================================================================= */
            ITcSmTreeItem plcProjectRootItem = systemManager.LookupTreeItem("TIPC^PlcGenerated"); // Plc Project Root (XAE Base side)
            ITcSmTreeItem plcProjectItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project"); // PlcProject (PlcControl side) determined via LookupTreeItem
            ITcSmTreeItem plcPousItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^POUs");
            ITcSmTreeItem plcDutsItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^DUTs");
            ITcSmTreeItem plcProject = plcProjectRoot.LookupChild(plcProjectName + " Project");
            AddLibrary("Tc2_EtherCAT, * (Beckhoff Automation GmbH)", plcProject, worker);
            Console.WriteLine();

            /* =================================================================================================
             * Creating datatypes in \DUTs\
             * ================================================================================================= */

            ITcSmTreeItem plcDutStruct = CreateDut(plcDutsItem, TreeItemType.PlcDutStruct, "EthercatDevice", null, worker);
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
        }

        private bool WaitForState(AdsState state, int timeOutInMilliSeconds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds <= timeOutInMilliSeconds)
            {
                try
                {
                    if (TwinCathelper.ClientSystemService.ReadState().AdsState == state)
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

        private ITcSmTreeItem CreateDut(ITcSmTreeItem parent, TreeItemType treeItemType, string typeName, string baseType, IWorker worker)
        {
            string declarationCode = @"TYPE EthercatDevice :
STRUCT
    mDirection AT %Q*: BOOL;
    mSpeed AT %Q*: DWORD;
    mTime AT %Q*: DWORD;
    mMode AT %Q*: BOOL;
END_STRUCT
END_TYPE";

            /* =================================================================================================
             * Creating the DUT using CreateChild()
             * ================================================================================================= */
            worker.ProgressStatus = string.Format("Creating Duts '{0}' ...", typeName);
            ITcSmTreeItem item = parent.CreateChild(typeName, treeItemType.AsInt32(), "", declarationCode);
            return item;
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

        private ITcSmTreeItem AddPOUProgram(string pouName, ITcSmTreeItem parent, IECLanguageType language, IWorker worker)
        {
            ITcSmTreeItem programPou = parent.LookupChild("Main");
            ITcPlcDeclaration pouDecl = programPou as ITcPlcDeclaration;
            ITcPlcImplementation pouImpl = programPou as ITcPlcImplementation;
            ITcXmlDocument pouDoc = programPou as ITcXmlDocument;
            string deviceAtrib = "";
            string mPositionAtrib = "";
            string mTemperatureAtrib = "";
            string Diag1Atrib = "";
            string Diag2Atrib = "";
            string Diag3Atrib = "";

            string deviceName = "LAN9255_EtherCAT_Slave";
            string outStr = "Outputs process data mapping";
            string inpStr = "Inputs process data mapping";
            for (int j = 1; j <= AvaiableDevicesCount; j++)
            {
                for (int i = 1; i <= AvaiableBoxesCount; i++)
                {
                    deviceAtrib += $"" +
                        $"[{i}].mDirection := TIID^Device_{j}^Box {i} ({deviceName})^{outStr}^Direction;\n" +
                        $"[{i}].mSpeed := TIID^Device_{j}^Box {i} ({deviceName})^{outStr}^Speed;\n" +
                        $"[{i}].mTime := TIID^Device_{j}^Box {i} ({deviceName})^{outStr}^Time;\n" +
                        $"[{i}].mMode := TIID^Device_{j}^Box {i} ({deviceName})^{outStr}^Mode;";

                    mPositionAtrib += $"" +
                        $"[{i}]:= TIID^Device_{j}^Box {i} ({deviceName})^{inpStr}^Position;";

                    mTemperatureAtrib += $"" +
                        $"[{i}]:= TIID^Device_{j}^Box {i} ({deviceName})^{inpStr}^Temperature;";

                    Diag1Atrib += $"" +
                        $"[{i}]:= TIID^Device_{j}^Box {i} ({deviceName})^{inpStr}^Diag1;";

                    Diag2Atrib += $"" +
                        $"[{i}]:= TIID^Device_{j}^Box {i} ({deviceName})^{inpStr}^Diag2;";

                    Diag2Atrib += $"" +
                        $"[{i}]:= TIID^Device_{j}^Box {i} ({deviceName})^{inpStr}^Diag3;";
                }
            }

            string foe_Scrip =
                    "fbDownload : FB_EcFoeLoad ;\n" +
                    "dwPass     : DWORD := 287454020; // 0x11223344\n" +
                    "sNetId     : T_AmsNetId := '192.168.18.191.3.1' ;\n" +
                    "nSlaveAddr : UINT := 1001;\n" +
                    "eMode      : E_EcFoeMode := eFoeMode_Write;\n" +
                    "sPathName : STRING := 'D:\\LAN9255Updated.bin';\n" +
                    "bLoad      : BOOL;\n" +
                    "bBusy      : BOOL;\n" +
                    "bError     : BOOL;\n" +
                    "nErrID     : UDINT;\n" +
                    "nBytesWritten : UDINT;\n" +
                    "nPercent      : UDINT;\n";

            string decl1 =
                "PROGRAM MAIN \n" +
                "VAR CONSTANT \n" +
                $"deviceCount : WORD := {AvaiableBoxesCount}; \n" +
                "END_VAR \n" +
                "VAR \n" +

                "{attribute 'TcLinkTo' := '" + deviceAtrib +
                    "'}\n" +
                "device : ARRAY [1..deviceCount] OF EthercatDevice ; \n" +

                "{attribute 'TcLinkTo' := '" + mPositionAtrib +
                    "'}\n" +
                    "mPosition AT %I* : ARRAY [1..deviceCount] OF DWORD ;\n" +

                "{attribute 'TcLinkTo' := '" + mTemperatureAtrib +
                    "'}\n" +
                    "mTemperature AT %I* : ARRAY [1..deviceCount] OF DWORD ;\n" +

                "{attribute 'TcLinkTo' := '" + Diag1Atrib +
                    "'}\n" +
                    "mDiag1 AT %I*: ARRAY [1..deviceCount] OF DWORD ;\n" +

                "{attribute 'TcLinkTo' := '" + Diag2Atrib +
                    "'}\n" +
                    "mDiag2 AT %I*: ARRAY [1..deviceCount] OF DWORD ;\n" +

                "{attribute 'TcLinkTo' := '" + Diag3Atrib +
                    "'}\n" +
                    "mDiag3 AT %I*: ARRAY [1..deviceCount] OF DWORD ;\n" +
                    foe_Scrip +
                "END_VAR"
                ;
            string implfoe = "fbDownload( bExecute := bLoad,\r\n\t\tsNetId  := sNetId,\r\n\t\tnSlaveAddr := nSlaveAddr,\r\n\t\tdwPass  := dwPass,\r\n\t\tsPathName  := sPathName, \r\n        bBusy => bBusy,\r\n        bError => bError,\r\n        nErrId => nErrID,\r\n        cbLoad => nBytesWritten,\r\n        nProgress => nPercent );";

            pouDecl.DeclarationText = decl1;
            pouImpl.ImplementationText = implfoe;

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

        public void ScanDevicesAndBoxes(IWorker worker)
        {
            AvaiableBoxesCount = 2;
            AvaiableDevicesCount = 1;
            ITcSmTreeItem ioDevicesItem = systemManager.LookupTreeItem("TIID");
            string scannedXml = ioDevicesItem.ProduceXml(false);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(scannedXml);
            XmlNodeList xmlDeviceList = xmlDoc.SelectNodes("TreeItem/DeviceGrpDef/FoundDevices/Device");
            List<ITcSmTreeItem> devices = new List<ITcSmTreeItem>();
            int deviceCount = 0;
            foreach (XmlNode node in xmlDeviceList)
            {
                int itemSubType = int.Parse(node.SelectSingleNode("ItemSubType").InnerText);
                string typeName = node.SelectSingleNode("ItemSubTypeName").InnerText;
                XmlNode xmlAddress = node.SelectSingleNode("AddressInfo");
                ITcSmTreeItem device = ioDevicesItem.CreateChild(string.Format("Device_{0}", ++deviceCount), itemSubType, string.Empty, null);
                string xml = string.Format("<TreeItem><DeviceDef>{0}</DeviceDef></TreeItem>", xmlAddress.OuterXml);
                worker.ProgressStatus = $"xmlAddress : {xmlAddress}, itemSubType : {itemSubType}";
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
                    if (box.Name.Contains("sample"))
                    {
                        AvaiableBoxesCount = AvaiableBoxesCount + 1;
                        worker.ProgressStatus = $"Box Added Name : {box.Name}";
                    }
                }
            }
        }
    }
}
using EnvDTE;
using EnvDTE80;
using FlexMold.Utility;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using TCatSysManagerLib;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.SystemManager;

namespace FlexMold.MVVM.ViewModel
{
    /// <summary>
    /// Demonstrates the generation + compilation of PLC projects. 
    /// The method Execute() will be called by ScriptingContainer and will execute the actual script code.
    /// </summary>
    public class FlexMoldMachine1
        : ScriptEarlyBound
    {
        ITcSysManager4 systemManager = null;
        EnvDTE.Project project = null;

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

        /// <summary>
        /// Handler function Executing the Script code.
        /// </summary>
        /// <param name="worker">The worker.</param>
        protected override void OnExecute(IWorker worker)
        {

            if (worker.CancellationPending)
                throw new Exception("Cancelled");
            worker.ProgressStatus = $"Connecting to TwinCAT Via ADS On Port {(int)AmsPort.SystemService}";
            TwinCathelper._sessionSystemService = new AdsSession(TwinCAT.Ads.AmsNetId.Local, (int)AmsPort.SystemService);
            TwinCathelper._connectionSystemService = (AdsConnection)TwinCathelper._sessionSystemService.Connect();
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
            ScanDevicesAndBoxes(worker); //custom code
            worker.Progress = 15;
            if (AvaiableDevices > 0 && AvaiableBoxesCount > 0)
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
            else
            {
                worker.ProgressStatus = "No Divice & Box Found";
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
                    if (TwinCathelper._connectionSystemService.ReadState().AdsState == state)
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

        string deviceName = "LAN9255";
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
            string outStr = "Outputs process data mapping";
            string inpStr = "Inputs process data mapping";
            for (int j = 1; j <= AvaiableDevices; j++)
            {
                for (int i = 1; i <= AvailableBoxes.Count; i++)
                {
                    deviceAtrib += $"" +
                  $"[{i}].mDirection := {AvailableBoxes[i - 1].PathName}^{outStr}^Direction;\n" +
                  $"[{i}].mSpeed := {AvailableBoxes[i - 1].PathName}^{outStr}^Speed;\n" +
                  $"[{i}].mTime := {AvailableBoxes[i - 1].PathName}^{outStr}^Time;\n" +
                  $"[{i}].mMode := {AvailableBoxes[i - 1].PathName}^{outStr}^Mode;\n";

                    // "TIID^Device_1^Box 1 (LAN9255_EtherCAT_Slave)"
                    deviceAtrib += $"" +
                        $"[{i}].mDirection := {AvailableBoxes[i - 1].PathName}^{outStr}^Direction;\n" +
                        $"[{i}].mSpeed := {AvailableBoxes[i - 1].PathName}^{outStr}^Speed;\n" +
                        $"[{i}].mTime := {AvailableBoxes[i - 1].PathName}^{outStr}^Time;\n" +
                        $"[{i}].mMode := {AvailableBoxes[i - 1].PathName}^{outStr}^Mode;";

                    mPositionAtrib += $"" +
                        $"[{i}]:= {AvailableBoxes[i - 1].PathName}^{inpStr}^Position;";

                    mTemperatureAtrib += $"" +
                        $"[{i}]:= {AvailableBoxes[i - 1].PathName}^{inpStr}^Temperature;";

                    Diag1Atrib += $"" +
                        $"[{i}]:= {AvailableBoxes[i - 1].PathName}^{inpStr}^Diag1;";

                    Diag2Atrib += $"" +
                        $"[{i}]:= {AvailableBoxes[i - 1].PathName}^{inpStr}^Diag2;";

                    Diag3Atrib += $"" +
                        $"[{i}]:= {AvailableBoxes[i-1].PathName}^{inpStr}^Diag3;";
                }

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
                    "END_VAR"
                    ;
                pouDecl.DeclarationText = decl1;
                string impl1 = "";
            }
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
        public int AvaiableDevices { get; set; }
        public List<ITcSmTreeItem> AvailableBoxes { get; set; }
        public void ScanDevicesAndBoxes(IWorker worker)
        {
            AvailableBoxes = new List<ITcSmTreeItem>();
            //AvaiableDevices = 1;
            //AvaiableBoxesCount = 2;
            if (AvaiableBoxesCount > 0 && AvaiableBoxesCount > 0)
            {
                worker.ProgressStatus = "! Warning .......";
                worker.ProgressStatus = "Simulated Version";
                worker.ProgressStatus = $"Dummy  {AvaiableDevices} Divices and {AvaiableBoxesCount} Boxes added";
            }
            else
            {
                worker.ProgressStatus = $"Scanning Connected Devices and boxes....";
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
                    AvaiableDevices = AvaiableDevices + 1;

                    foreach (ITcSmTreeItem box in device)
                    {
                        if (box.PathName.Contains(deviceName))
                        {
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
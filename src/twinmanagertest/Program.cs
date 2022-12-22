using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TCatSysManagerLib;
using TwinCAT.SystemManager;

namespace twinmanagertest
{
    internal class Program
    {
        static ITcSysManager15 sysMan = null;

        IWorker _worker = null;
        ConfigurationFactory _factory = null;
        Script _runningScript = null;

        static void Main(string[] args)
        {
            //MessageFilter.Register();

            Console.WriteLine("Hello, World!");
            string visualStudioVersion = "VisualStudio.DTE.15.0";
            //string visualStudioVersion = "TcXaeShell.DTE.15.0";
            string ams = "127.0.0.1.1.1";
            string fullName1 = @"D:\smhraza\Projects\FLEXMOLD\FlexMold TwinCAT PLC\src\FlexMold TwinCAT PLC\FlexMold TwinCAT PLC.sln";
            Console.WriteLine();
            dowork();
            //CloseRunnigInstancesOfVs(fullName1);

            //OpenNewInstanceOfVs(visualStudioVersion, fullName1);
        }
        private static async void dowork()
        {
            string visualStudioVersion = "VisualStudio.DTE.15.0";

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await GeneratedPLCNewSolWithProject(visualStudioVersion, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public static Version CurrentTwinCATVersion
        {
            get
            {
                string ret = null;

                string path = string.Empty;

                if (TcEnvironment.Is64BitProcess)
                    path = "Software\\Wow6432Node\\Beckhoff\\TwinCAT3";
                else
                    path = "Software\\Beckhoff\\TwinCAT3";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    ret = (string)key.GetValue("CurrentVersion");
                }

                if (ret == null) throw new ApplicationException("Could not determine actual TwinCAT Version!");

                return new Version(ret);
            }
        }
        public static string TwinCATInstallDir
        {
            get
            {
                string ret = null;

                Version tcVer = CurrentTwinCATVersion;

                string path;

                if (TcEnvironment.Is64BitProcess)
                    path = "Software\\Wow6432Node\\Beckhoff\\TwinCAT3";
                else
                    path = "Software\\Beckhoff\\TwinCAT3";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format("{0}\\{1}", path, tcVer.ToString(2))))
                {
                    ret = (string)key.GetValue("InstallDir");
                }
                return ret;
            }
        }
        public static Version TwinCATVersion
        {
            get
            {
                return new Version(3, 1);
            }
        }
        public static string vsXaeTemplateName30 = "TwinCAT Project.tsp";
        public static string vsXaeTemplateName = "TwinCAT Project.tsproj";
        public static string VsXaeTemplatePath
        {
            get
            {
                string path = Path.Combine(TwinCATInstallDir, @"Components\Base\PrjTemplate", vsXaeTemplateName);

                if (!File.Exists(path))
                {
                    // Try out the Twincat30 "*.tsProj"
                    string path2 = Path.Combine(TwinCATInstallDir, @"Components\Base\PrjTemplate", vsXaeTemplateName30);
                    if (File.Exists(path2))
                        return path2;
                }
                return path;
            }
        }

        private static bool DeleteDirectory(string target_dir)
        {
            if (string.IsNullOrEmpty(target_dir)) throw new ArgumentOutOfRangeException("target_dir");

            if (!Directory.Exists(target_dir)) return false;

            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            Directory.Delete(target_dir, true);
            return true;
        }
        public static string vsXaePlcStandardTemplateName = "Standard PLC Template.plcproj";
        static DTE2 dte;
        static async Task GeneratedPLCNewSolWithProject(string _visualStudioVersion, CancellationToken token)
        {
            var appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
            string projectName = "Myproj";
            string templatePath = "";
            string ScriptRootFolder = "output";
            string ScriptName = "Mysol";
            Project project = null;
            string solutionPath = Path.Combine(appPath, ScriptRootFolder, string.Format("{0}.sln", ScriptName));
            MessageFilter.Register();
            try
            {
                CloseRunnigInstancesOfVs(solutionPath);
                DeleteDirectory(appPath + ScriptRootFolder);
                Directory.CreateDirectory(appPath + ScriptRootFolder);

                Type t = Type.GetTypeFromProgID(_visualStudioVersion);
                dte = (DTE)Activator.CreateInstance(t, true) as DTE2;
                dte.MainWindow.WindowState = 0;
                dte.MainWindow.Visible = true;
                dte.SuppressUI = true;
                dte.UserControl = true;

                Solution sol = dte.Solution;
                sol.Create(ScriptRootFolder, ScriptName);
                sol.SaveAs(solutionPath);
                System.Threading.Thread.Sleep(5000);
                FileVersionInfo sysSvcInfo = FileVersionInfo.GetVersionInfo(TwinCATInstallDir + @"System\TCATSysSrv.exe");
                Version sysSvcVersion = new Version(sysSvcInfo.ProductMajorPart, sysSvcInfo.ProductMinorPart);
                if (sysSvcVersion >= TwinCATVersion)
                {
                    Console.WriteLine(string.Format("Creating project {0} from template ...", projectName, templatePath));
                    bool exists = File.Exists(VsXaeTemplatePath);
                    project = sol.AddFromTemplate(VsXaeTemplatePath, appPath + ScriptRootFolder, ScriptName, false);
                    System.Threading.Thread.Sleep(5000);
                    Project pro = sol.Projects.Item(1);
                    var systemManager = (ITcSysManager15)pro.Object;

                    Console.WriteLine("Getting PLC Configuration item");
                    ITcSmTreeItem plcConfig = systemManager.LookupTreeItem("TIPC"); // Getting PLC Configuration item
                    Console.WriteLine("Getting IO-Configuration item");
                    ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID"); // Getting IO-Configuration item
                    Console.WriteLine("Creating empty PLC Project ...");
                    ITcSmTreeItem plcGenerated = plcConfig.CreateChild("PlcGenerated", 0, "", vsXaePlcStandardTemplateName);
                    Console.WriteLine("PLC Project created ...");
                    System.Threading.Thread.Sleep(5000);

                    /* =================================================================================================
             * Get references to attached PLC Project and some of its childs
             * ================================================================================================= */
                    ITcSmTreeItem plcProjectRootItem = systemManager.LookupTreeItem("TIPC^PlcGenerated"); // Plc Project Root (XAE Base side)
                    ITcSmTreeItem plcProjectItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project"); // PlcProject (PlcControl side) determined via LookupTreeItem
                    ITcSmTreeItem plcInstancesItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Instance"); // Instances
                    ITcSmTreeItem plcPousItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^POUs");
                    ITcSmTreeItem plcDutsItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^DUTs");
                    ITcSmTreeItem plcGlvsItem = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^GVLs");
                    ITcSmTreeItem plcPousItemMain = systemManager.LookupTreeItem("TIPC^PlcGenerated^PlcGenerated Project^POUs^Main");

                    Console.WriteLine("Updating ST POUs ...");
                    IECLanguageType language = IECLanguageType.ST;

                    ITcSmTreeItem program = UpdatePOUProgram("MAIN", plcPousItem, language);
                    Console.WriteLine("ST POUs Updated");
                    plcPousItem = program;

                    /* =================================================================================================
                     * Adjust Visual Studio Solution configuration
                     * ================================================================================================= */
                    SolutionBuild solutionBuild = sol.SolutionBuild;
                    SolutionConfiguration activeConfig = solutionBuild.ActiveConfiguration;
                    SolutionContexts contexts = activeConfig.SolutionContexts;

                    // ATTENTION: VisualStudio has a bug so that the SolutionContexts Collections cannot be used. This
                    // Bug is fixed by Microsoft in VS2012 and later!

                    // Iterate over the active Configuration Solution Contexts and activate all Projects within the Solution
                    // (Root and Nested projects)
                    foreach (SolutionContext context in activeConfig.SolutionContexts)
                    {
                        string MyprojectName = context.ProjectName;
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
                    Console.WriteLine("Compiling project (1) ...");

                    iecProjectRoot.CompileProject();

                    ErrorItems errors = dte.ToolWindows.ErrorList.ErrorItems;
                    iecProjectRoot.BootProjectAutostart = true;

                    /* =================================================================================================
            * Compiling project using Visual Studio DTE methods 
            * ================================================================================================= */
                    errors = null;
                    Console.WriteLine("Compiling project (3) ...");
                    dte.Solution.SolutionBuild.Build(true);
                    waitForBuildAndCheckErrors(out errors);
                    Console.WriteLine("Build Suceded");
                }
                else
                {
                    throw new Exception("System requirements not met! You need a higher TwinCAT Build to execute this script!");
                }
                //Console.WriteLine("Closing Soltiuon ...");
                //sol.Close();
                //Console.WriteLine("Closing VS ...");
                //dte.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                MessageFilter.Revoke();
                Console.WriteLine("Revoke");
            }
            Console.WriteLine("");

        }
        private static bool waitForBuildAndCheckErrors(out ErrorItems errorItems)
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
                    Console.WriteLine("Compiler error: " + item.Description);
                }
                else if (item.ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelMedium)
                {
                    warnings++;
                    Console.WriteLine("Compiler warning: " + item.Description);
                }
                else if (item.ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelLow)
                {
                    messages++;
                    Console.WriteLine("Compiler message: " + item.Description);
                }

            }
            return buildSucceeded;
        }

        static ITcSmTreeItem UpdatePOUProgram(string pouName, ITcSmTreeItem parent, IECLanguageType language)
        {
            foreach (var item in parent)
            {
                ITcPlcPou pofdu = item as ITcPlcPou;
                Console.WriteLine();
            }
            parent.DeleteChild(pouName);
            /* =================================================================================================
             * Create Program POU using CreateChild()
             * ================================================================================================= */
            string fname = @"C:\Users\123\Downloads\ChristianRaun-FirmwareDevelopment-main (1)\ChristianRaun-FirmwareDevelopment-main\TwinCAT Projects\Motor controlling Daisy Chain\Motor controlling Daisy Chain\Untitled1\POUs\MAIN.zip";
            ITcSmTreeItem programPou = parent.ImportChild(fname, "", true, "");
            Console.WriteLine(string.Format("POU Program '{0}' added ...", pouName));

            System.Threading.Thread.Sleep(2000);
            /* =================================================================================================
             * Accessing declaration and implementation area 
             * ================================================================================================= */
            ITcPlcPou pou = programPou as ITcPlcPou;
            ITcPlcDeclaration pouDecl = programPou as ITcPlcDeclaration;
            ITcPlcImplementation pouImpl = programPou as ITcPlcImplementation;
            ITcXmlDocument pouDoc = programPou as ITcXmlDocument;
            string decl1 = pouDecl.DeclarationText;
            string impl1 = pouImpl.ImplementationText;

            pouDecl.DeclarationText += (Environment.NewLine + "(* Added by Script *)");

            string oldDocumentXml = pouDoc.DocumentXml; // Accessing POUs Code Document

            /* =================================================================================================
             * Modifying implementation area, based on IEC language.
             * Please note: Direct access to implementation text is only provided for ST !
             * ================================================================================================= */
            if (language == IECLanguageType.ST)
            {
                var decl1info = decl1.Replace("\t", "").Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in decl1info)
                {
                    if (item.Contains("{attribute 'TcLinkTo' := 'TIID^Device"))
                    {
                        pouDecl.DeclarationText += $"{{attribute 'TcLinkTo' := 'TIID^Device {5} (EtherCAT)^Box {2} (sample_app)^Outputs process data mapping^Trigger'}}" + Environment.NewLine;
                    }
                    else
                    {
                        pouDecl.DeclarationText += item + Environment.NewLine;
                    }
                }

                pouImpl.ImplementationText = "i := i + 1; (* This code is added by script *)";
                pouImpl.ImplementationText += (Environment.NewLine + "(* Added by Script *)");

            }
            return programPou;
        }
        public static void CloseRunnigInstancesOfVs(string fullName1)
        {
            Dictionary<string, DTE> runningIDEInstances = new Dictionary<string, DTE>();
            Dictionary<string, List<object>> runningObjects = ROTAccess.GetRunningObjectTable();

            IEnumerable<KeyValuePair<string, List<object>>> objTable = runningObjects.Where<KeyValuePair<string, List<object>>>((entry) => { ROTDteInfo info = null; return ROTDteInfo.TryParse(entry.Key, out info); });

            Dictionary<ROTDteInfo, DTE> ret = new Dictionary<ROTDteInfo, DTE>();

            foreach (KeyValuePair<string, List<object>> entry in objTable)
            {
                string displayName = entry.Key;
                Debug.Assert(entry.Value.Count == 1);

                if (entry.Value.Count > 0)
                {
                    DTE dte = (DTE)entry.Value[0];
                    string solutionPath = dte.Solution.FullName;

                    ROTDteInfo info = ROTDteInfo.Parse(entry.Key);
                    info.SolutionPath = solutionPath;
                    ret.Add(info, dte);
                }
            }

            // Get opened instance of solution
            var openInstances = ret.Where(a => a.Key.SolutionPath == fullName1).ToList();
            // Close All opened VS instance of desired solution 
            foreach (var item in openInstances)
            {
                item.Value.DTE.Solution.Close();
                item.Value.DTE.Quit();
            }
            Console.WriteLine();
        }
        public static void ScanDevicesAndBoxes()
        {
            ITcSmTreeItem ioDevicesItem = sysMan.LookupTreeItem("TIID");
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
                    Console.WriteLine("Warning: {0}", ex.Message);
                }
                foreach (ITcSmTreeItem box in device)
                {
                    Console.WriteLine(box.Name);
                }
            }
            Console.WriteLine();
        }
        static void OpenNewInstanceOfVs(string _visualStudioVersion, string fullName1)
        {
            // Visual studio update could change default version of vs when MORE THAN ONE visual studio same version is installed like (vs2017 enterprises and vs2017 community)
            Type t = Type.GetTypeFromProgID(_visualStudioVersion);
            var dte = (DTE)Activator.CreateInstance(t, true) as DTE2;
            dte.SuppressUI = false;
            dte.MainWindow.Visible = true;
            Solution sol = dte.Solution;
            try
            {
                sol.Open(fullName1);
                System.Threading.Thread.Sleep(5000); // paused because it keep runs next without loading project properly 
                Project pro = sol.Projects.Item(1);
                //sol.SolutionBuild.Clean(true);
                //sol.SolutionBuild.Build(true);
                sysMan = (ITcSysManager15)pro.Object;
                ITcConfigManager configManager = (ITcConfigManager)sysMan.ConfigurationManager;
                if (configManager.ActiveTargetPlatform == "TwinCAT RT (x86)")
                    configManager.ActiveTargetPlatform = "TwinCAT RT (x64)";
                ErrorItems errors = dte.ToolWindows.ErrorList.ErrorItems;
                for (int i = 1; i < errors.Count; i++)
                {
                    ErrorItem item = errors.Item(i);
                    Console.WriteLine($"Level : {item.ErrorLevel} : {item.Description} File: {item.FileName} Line #: {item.Line}");
                }
                //sysMan.SetTargetNetId(ams);
                sysMan.ActivateConfiguration();
                ITcSmTreeItem plc = sysMan.LookupTreeItem("TIPC");
                foreach (ITcSmTreeItem plcProject in plc)
                {
                    ITcProjectRoot projectRoot = (ITcProjectRoot)plcProject;
                    Console.WriteLine("projectRoot " + projectRoot.ToString());
                    ITcSmTreeItem nestedProject = projectRoot.NestedProject;
                    Console.WriteLine("nestedProject " + nestedProject.Name);
                    ITcSmTreeItem plcPousItem = sysMan.LookupTreeItem($"{nestedProject.PathName}^POUs");
                    string pp = @"MAIN.TcPOU";
                    plcPousItem.CreateChild("MAIN.TcPOU", 602, null, pp);
                    foreach (var item in plcPousItem)
                    {
                        ITcPlcPou fbPou = (ITcPlcPou)item;
                        ITcPlcDeclaration fbPouDecl = (ITcPlcDeclaration)fbPou;
                        ITcPlcImplementation fbPouImpl = (ITcPlcImplementation)fbPou;
                        string decl = fbPouDecl.DeclarationText;
                        string impl = fbPouImpl.ImplementationText;
                        string implXml = fbPouImpl.ImplementationXml;
                    }


                    ITcSmTreeItem projectInstance = plcProject.get_Child(1);
                    Console.WriteLine("projectInstance " + projectInstance.Name);
                }
                sol.SolutionBuild.Build();
                //ScanDevicesAndBoxes();

                sysMan.StartRestartTwinCAT();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                MessageFilter.Revoke();

                sol.Close();
                dte.Quit();
            }
        }
    }
}

using EnvDTE;
using EnvDTE80;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using TCatSysManagerLib;
using Process = System.Diagnostics.Process;

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
            
            
            //CloseRunnigInstancesOfVs(fullName1);
            //OpenNewInstanceOfVs(visualStudioVersion, fullName1);
        }
        internal static void CloseRunnigInstancesOfVs(string fullName1)
        {
            //Access COM Running Obects Table
            Dictionary<string, List<object>> rot = ROTAccess.GetRunningObjectTable();
            var dteTable = ROTAccess.GetRunningDTETable();
            // Get opened instance of solution
            var openInstances = dteTable.Where(a => a.Key.SolutionPath == fullName1).ToList();
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
                    Console.WriteLine("projectRoot "+projectRoot.ToString());
                    ITcSmTreeItem nestedProject = projectRoot.NestedProject;
                    Console.WriteLine("nestedProject " +nestedProject.Name);
                    ITcSmTreeItem plcPousItem = sysMan.LookupTreeItem($"{nestedProject.PathName}^POUs");
                    string pp = @"C:\Users\123\Downloads\ChristianRaun-FirmwareDevelopment-main (1)\ChristianRaun-FirmwareDevelopment-main\TwinCAT Projects\Motor controlling Daisy Chain\Motor controlling Daisy Chain\Untitled1\POUs\MAIN.TcPOU";
                    plcPousItem.CreateChild("NameOfImportedPOU", 58, null, pp);
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
                    Console.WriteLine("projectInstance "+ projectInstance.Name);
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

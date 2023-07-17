using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Diagnostics;

namespace FlexMold.E2ETest
{
    public class SpalshScreenTest
    {

        [Test]
        public void CloseButton()
        {
            AppiumOptions options = new AppiumOptions();
            foreach (var process in Process.GetProcessesByName("FlexMold"))
            {
                process.Kill();
            }
            string appDir = @"D:\smhraza\Projects\FLEXMOLD\flexmold\src\FlexMold\bin\Debug\net6.0-windows\";
            Directory.SetCurrentDirectory(appDir);
            Process.Start(appDir + "FlexMold.exe");

            options.AddAdditionalCapability("app", appDir + "FlexMold.dll");
            options.AddAdditionalCapability("appWorkingDir", appDir);

            WindowsDriver<WindowsElement> driver = new WindowsDriver<WindowsElement>
                (new Uri("http://localhost:4723/"), options);
            var aa = driver.FindElementByAccessibilityId("TileClick");
            driver.FindElementByName("TwinCat Config").Click();
            var quest = driver.FindElementByName("Question");
            quest.FindElementByName("No").Click();

            var FoeView = driver.FindElementByClassName("FoeView");
            FoeView.FindElementByName("..").Click(); ;
            var open = driver.FindElementByName("Open");
            var sv = open.FindElementByName("Shell Folder View");
            var iv = sv.FindElementByName("Items View");
            var children = iv.FindElementsByXPath("//ListItem");

            AppiumWebElement ff = null; 
            foreach (var child in children)
            {
                try
                {
                    ff = child.FindElementByName("LAN9255_018.bin");

                }
                catch (Exception)
                {
                    ff = null;
                }
            }
            ff.Click();
            driver.FindElementByAccessibilityId("Close").Click();

            driver.Close();
        }
    }
}
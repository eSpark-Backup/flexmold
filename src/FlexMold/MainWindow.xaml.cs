using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            string selectedPath = @"C:\Users\123\Desktop\Projects";


            DirectoryInfo di = new DirectoryInfo(selectedPath);
            var laserFilesPath = di.GetFiles("*.las", SearchOption.AllDirectories);
            var diPanels = new List<string>();
            var diProjects = new List<string>();

            foreach (var item in laserFilesPath)
            {
                diPanels.Add(item.Directory.ToString());
                diProjects.Add(item.Directory.Parent.ToString());

            }
            diPanels = diPanels.Distinct().ToList();
            diProjects = diProjects.Distinct().ToList();
            foreach (var projectItem in diProjects)
            {
                //new DirectoryInfo(projectItem).GetDirectories
                //foreach (var item in)
                //{

                //}
            }
            InitializeComponent();
            Console.WriteLine();
        }

        private void btProjectBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                Console.WriteLine();
            }
        }
    }
}

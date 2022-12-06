using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using Sylvan.Data.Csv;
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

            InitializeComponent();
            Console.WriteLine();
            //var csv = @"C:\Users\123\Downloads\Sylvan-main\Sylvan-main\source\Sylvan.Data.Csv.Tests\Data\Quote.csv";
            var csv = @"D:\smhraza\Projects\FLEXMOLD\flexmolddata\FlexMain\Projects\M&M\S3.2\S3.2.csv";


            using (var reader = new StreamReader(csv))
            {
                var options = new CsvDataReaderOptions
                {
                    HasHeaders = false,
                    BufferSize = 0x10000,
                };
                var csvr = CsvDataReader.Create(reader, options);
                var data = new List<double>();
                while (csvr.Read())
                {
                    for (int i = 0; i < csvr.FieldCount; i++)
                    {
                        data.Add(csvr.GetDouble(i));
                    }
                }
                Console.WriteLine();
            }
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

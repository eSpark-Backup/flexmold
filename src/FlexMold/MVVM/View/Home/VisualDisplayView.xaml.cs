using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TwinCAT.Ads;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for VisualDisplayView.xaml
    /// </summary>
    public partial class VisualDisplayView : UserControl
    {
        public VisualDisplayView()
        {
            InitializeComponent();
            //try
            //{
            //    // Create instance of class TcAdsClient
            //    adsClient = new TcAdsClient();
            //    // PLC1 Port - TwinCAT 3=851
            //    adsClient.Connect(851);

            //    varHandle = adsClient.CreateVariableHandle("MAIN.motorStatus");
            //}
            //catch (Exception err)
            //{
            //    MessageBox.Show(err.Message);
            //}
        }
        //private TcAdsClient adsClient;
        //private int varHandle;
        //private void read(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        AdsStream adsStream = new AdsStream(30);
        //        AdsBinaryReader reader = new AdsBinaryReader(adsStream);
        //        adsClient.Read(varHandle, adsStream);
        //        textBox1.Text = reader.ReadPlcAnsiString(30);
        //    }
        //    catch (Exception err)
        //    {
        //        MessageBox.Show(err.Message);
        //    }
        //}

        //private void write(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        AdsStream adsStream = new AdsStream(30);
        //        AdsBinaryWriter writer = new AdsBinaryWriter(adsStream);
        //        writer.WritePlcAnsiString(textBox1.Text, 30);
        //        adsClient.Write(varHandle, adsStream);
        //    }
        //    catch (Exception err)
        //    {
        //        MessageBox.Show(err.Message);
        //    }
        //}
    }
}


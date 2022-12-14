using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for SambaConfigWindow.xaml
    /// </summary>
    /// 
   
    public partial class SambaConfigWindow : MetroWindow
    {
        public MainWindow main { get; set; }
        public Projector CurrentProjector { get; set; }
        public bool SaveData { get; set; }
        public SambaConfigWindow(MainWindow _main, Projector _Porjector)
        {
            main = _main;
            main.IsEnabled = false;
            main.Effect = new BlurEffect();
            CurrentProjector = new Projector();
            CurrentProjector = _Porjector;
            InitializeComponent();
            SaveData = false;
            tb_ip.Text = CurrentProjector.IpAddres;
            pbox_pwd.Password = CurrentProjector.Password;
            tb_username.Text = CurrentProjector.UserName;
            tb_sharename.Text = CurrentProjector.PlotFolderName;
        }
        private void MetroWindow_Closing(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData = true;
            this.Close();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveData)
            {
                main.Effect = null;
                main.IsEnabled = true;
                CurrentProjector.IpAddres = tb_ip.Text;
                CurrentProjector.Password = pbox_pwd.Password;
                CurrentProjector.UserName = tb_username.Text;
                CurrentProjector.PlotFolderName = tb_sharename.Text;
            }
            else
            {
                main.Effect = null;
                main.IsEnabled = true;
            }
        }
    }
}

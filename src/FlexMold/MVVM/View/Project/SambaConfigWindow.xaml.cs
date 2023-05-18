using FlexMold.MVVM.Model;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Media.Effects;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for SambaConfigWindow.xaml
    /// </summary>
    ///

    public partial class SambaConfigWindow : MetroWindow
    {
        public MainWindow main { get; set; }
        public LaserProjector CurrentProjector { get; set; }
        public bool SaveData { get; set; }

        public SambaConfigWindow(MainWindow _main, LaserProjector _Porjector)
        {
            main = _main;
            main.IsEnabled = false;
            main.Effect = new BlurEffect();
            CurrentProjector = new LaserProjector();
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
            if (tb_ip.Text != "" & pbox_pwd.Password != "" & tb_username.Text != "" & tb_sharename.Text != "")
            {
                SaveData = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please Enter Values in all fields first \n otherwise close or discard window");
            }
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
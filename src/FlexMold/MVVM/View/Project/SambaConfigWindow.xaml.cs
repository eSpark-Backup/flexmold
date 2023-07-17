using FlexMold.MVVM.ViewModel.Project;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for SambaConfigWindow.xaml
    /// </summary>
    ///

    public partial class SambaConfigWindow : UserControl
    {
        ILaserProjectorViewModel _LaserProjectorViewModel = App.AppHost.Services.GetRequiredService<ILaserProjectorViewModel>();

        public SambaConfigWindow()
        {
            DataContext = _LaserProjectorViewModel;
            InitializeComponent();
            _LaserProjectorViewModel.SaveData = false;
            tb_ip.Text = _LaserProjectorViewModel.MyPorjector.IpAddres;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (tb_ip.Text != "" & pbox_pwd.Password != "" & tb_username.Text != "" & tb_sharename.Text != "")
            {
                _LaserProjectorViewModel.SaveData = true;
            }
            else
            {
                MessageBox.Show("Please Enter Values in all fields first \n otherwise close or discard window");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_LaserProjectorViewModel.SaveData)
            {
                _LaserProjectorViewModel.MyPorjector.IpAddres = tb_ip.Text;
                _LaserProjectorViewModel.MyPorjector.Password = pbox_pwd.Password;
                _LaserProjectorViewModel.MyPorjector.UserName = tb_username.Text;
                _LaserProjectorViewModel.MyPorjector.PlotFolderName = tb_sharename.Text;
            }
        }
    }
}
using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.Project;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for LaserProjSelectionView.xaml
    /// </summary>
    public partial class PriveledgeUpView : UserControl
    {        
        IPriveledgeUpViewModel _IPriveledgeUpViewModel = App.AppHost.Services.GetRequiredService<IPriveledgeUpViewModel>();
        private Logger log = LogManager.GetLogger("MachineSettings");

        public PriveledgeUpView()
        {
            DataContext = _IPriveledgeUpViewModel;
            InitializeComponent();
        }

        private void UpdateSettingsDB()
        {
            //File.ReadAllLines(CSVHelper.GetCurrentDirectoryPath() + "\\FlexMoldUser.connect", Encoding.UTF8);

            //CREATE TABLE "FM_Settings"(
            //    "FM_SaltKey"    TEXT,
            //    "FM_UserName"   TEXT,
            //    "FM_Dept"   TEXT
            //)

            Dictionary<string, string> dct = new Dictionary<string, string>();
            dct.Add("FM_SaltKey", LP_ZLaserIPSelect.Text);

            FlexMoldDB.UpdateData("FM_Settings", null, dct);

            MessageBox.Show("Admin password is now updated successfully!", "Password Changed to \""+ LP_ZLaserIPSelect.Text+"\"", MessageBoxButton.OK);
            log.Log(LogLevel.Info, $"Updated Admin Password Successcully!");
        }

        private void btnAdminLogin_Click(object sender, RoutedEventArgs e)
        {
            FlexMoldDB.InitializeFlexMoldDB();

            if (FlexMoldDB.checkAdminPriv(LP_ZLaserIPSelect.Text))
                {
                    MessageBox.Show("Now you are in Admin mode \r\n Be Carefull when Updating values as it may effect on performance!", "Priveledge Mode Enabled!", MessageBoxButton.OK);
                    FlexMoldDB.IsEditingEnabled = true;
                    //OnPropertyChanged();
                    //UpdateSettingsDB();
                }
                else
                {
                    MessageBox.Show("Please try again!", "Incorrect Password", MessageBoxButton.OK);
                    FlexMoldDB.IsEditingEnabled = false;
                }
        }

        private void btnChangePasswd_Click(object sender, RoutedEventArgs e)
        {
            if (FlexMoldDB.IsEditingEnabled)
            {
                UpdateSettingsDB();
            }
            else
            {
                MessageBox.Show("Please login as administrator before changing password!", "Please login", MessageBoxButton.OK);
                FlexMoldDB.IsEditingEnabled = false;
            }
        }
    }
}
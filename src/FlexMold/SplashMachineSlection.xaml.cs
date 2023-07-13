using FlexMold.MVVM.ViewModel;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Forms;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        ISplashScreenViewModel _SplashScreenViewModel = App.AppHost.Services.GetRequiredService<ISplashScreenViewModel>();
        public SplashMachineSlection()
        {
            _SplashScreenViewModel.OnRequestClose += (s, e) => this.Visibility = Visibility.Collapsed;
            DataContext = _SplashScreenViewModel;
            InitializeComponent();
        }
    }
}
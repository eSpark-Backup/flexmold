using FlexMold.MVVM.ViewModel;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        ISplashScreenViewModel _SplashScreenViewModel { get { return App.AppHost.Services.GetRequiredService<ISplashScreenViewModel>(); } }
        public SplashMachineSlection()
        {
            _SplashScreenViewModel.OnRequestClose += (s, e) => this.Close();
            _SplashScreenViewModel.OnRequestCollapsed += (s, e) => this.Visibility = System.Windows.Visibility.Collapsed;
            DataContext = _SplashScreenViewModel;
            InitializeComponent();
        }
    }
}
using FlexMold.MVVM.ViewModel;
using MahApps.Metro.Controls;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        private readonly SplashScreenViewModel _splashScreenViewModel;
        public SplashMachineSlection(SplashScreenViewModel splashScreenViewModel)
        {
            InitializeComponent();
            this._splashScreenViewModel = splashScreenViewModel;
            this._splashScreenViewModel.OnRequestClose += (s, e) => this.Close();
        }
    }
}
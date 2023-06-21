using FlexMold.MVVM.ViewModel;
using MahApps.Metro.Controls;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        public SplashMachineSlection()
        {
            InitializeComponent();
            var vm = DataContext as SplashScreenViewModel;
            vm.OnRequestClose += (s, e) => this.Close();
        }
    }
}
using FlexMold.MVVM.ViewModel.Home;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for MotorStatusView.xaml
    /// </summary>
    public partial class MotorStatusView : UserControl
    {
        public MotorStatusView(IMotorStatusViewModel motorStatusViewModel)
        {
            DataContext = motorStatusViewModel;
            InitializeComponent();
        }
    }
}
using FlexMold.MVVM.ViewModel.MachineTest;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.MachineTest
{
    /// <summary>
    /// Interaction logic for EmergencyStatusView.xaml
    /// </summary>
    public partial class EmergencyStatusView : UserControl
    {
        public EmergencyStatusView(IEmergencyStatusViewModel EmergencyStatusViewModel)
        {
            DataContext = EmergencyStatusViewModel;
            InitializeComponent();
        }
    }
}
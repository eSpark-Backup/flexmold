using FlexMold.MVVM.ViewModel.Home;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for MachineDetailView.xaml
    /// </summary>
    public partial class MachineDetailView : UserControl
    {
        public MachineDetailView(IMachineDetailViewModel iMachineDetailViewModel)
        {
            DataContext = iMachineDetailViewModel;
            InitializeComponent();
        }
    }
}
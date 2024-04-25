using FlexMold.MVVM.ViewModel.MachineTest;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.MachineTest
{
    /// <summary>
    /// Interaction logic for MotorStatusView.xaml
    /// </summary>
    public partial class ResetView : UserControl
    {
        private IResetViewModel _ResetViewModel { get { return App.AppHost.Services.GetRequiredService<IResetViewModel>(); } }
        public ResetView()
        {
            DataContext = _ResetViewModel;
            InitializeComponent();
        }
    }
}
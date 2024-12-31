using FlexMold.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class MachineTestView : UserControl
    {
        private IMachineTestViewModel _MachineTestViewModel { get { return App.AppHost.Services.GetRequiredService<IMachineTestViewModel>(); } }
        public MachineTestView()
        {
            DataContext = _MachineTestViewModel;
            InitializeComponent();
        }
    }
}
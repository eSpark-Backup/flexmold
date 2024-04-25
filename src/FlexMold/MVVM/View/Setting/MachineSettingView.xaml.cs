using FlexMold.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Setting
{
    /// <summary>
    /// Interaction logic for MachineTestView.xaml
    /// </summary>
    public partial class MachineSettingView : UserControl
    {
        private IMachineSettingViewModel _MachineSettingViewModel { get { return App.AppHost.Services.GetRequiredService<IMachineSettingViewModel>(); } }

        public MachineSettingView()
        {
            DataContext = _MachineSettingViewModel;
            InitializeComponent();
        }
    }
}
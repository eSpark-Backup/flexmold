using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.MachineTest;
using FlexMold.MVVM.ViewModel.Setting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Setting
{
    /// <summary>
    /// Interaction logic for MotorSettingView.xaml
    /// </summary>
    public partial class MotorSettingView : UserControl
    {
        public MotorSettingView(IMotorSettingViewModel motorSettingViewModel)
        {
            DataContext = motorSettingViewModel;
            InitializeComponent();
        }
    }
}
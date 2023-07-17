using FlexMold.MVVM.ViewModel.Setting;
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
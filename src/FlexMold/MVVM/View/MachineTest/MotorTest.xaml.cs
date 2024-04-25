using FlexMold.MVVM.ViewModel.MachineTest;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.MachineTest
{
    /// <summary>
    /// Interaction logic for MotorTest.xaml
    /// </summary>
    public partial class MotorTest : UserControl
    {
        public MotorTest(IMotorTestViewModel motorTestViewModel)
        {
            DataContext = motorTestViewModel;
            InitializeComponent();
        }
    }
}

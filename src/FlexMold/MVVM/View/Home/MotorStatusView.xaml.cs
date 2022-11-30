using FlexMold.MVVM.ViewModel.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for MotorStatusView.xaml
    /// </summary>
    public partial class MotorStatusView : UserControl
    {
        public MotorStatusView()
        {

            InitializeComponent();
        }
        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            var vm = this.DataContext as MotorStatusViewModel;
            vm.plcHelper.setMotorStep();
        }


    }
}

using FlexMold.MVVM.ViewModel.MachineTest;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.MachineTest
{
    /// <summary>
    /// Interaction logic for EmergencyStatusView.xaml
    /// </summary>
    public partial class ReadMotorView : UserControl
    {
        private IReadMotorViewModel _ReadMotorViewModel { get { return App.AppHost.Services.GetRequiredService<IReadMotorViewModel>(); } }
        public ReadMotorView()
        {
            DataContext = _ReadMotorViewModel;
            InitializeComponent();
        }
    }
}
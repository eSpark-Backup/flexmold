using FlexMold.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class SettingView : UserControl
    {
        private ISettingViewModel _SettingViewModel { get { return App.AppHost.Services.GetRequiredService<ISettingViewModel>(); } }
        public SettingView()
        {
            DataContext = _SettingViewModel;
            InitializeComponent();
        }
    }
}
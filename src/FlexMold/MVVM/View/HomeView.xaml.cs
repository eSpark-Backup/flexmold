using FlexMold.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private IHomeViewModel _HomeViewModel { get { return App.AppHost.Services.GetRequiredService<IHomeViewModel>(); } }
        public HomeView()
        {
            DataContext = _HomeViewModel;
            InitializeComponent();
        }
    }
}
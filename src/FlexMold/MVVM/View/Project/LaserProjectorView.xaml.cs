using FlexMold.MVVM.ViewModel.Project;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for LaserProjectorView.xaml
    /// </summary>
    public partial class LaserProjectorView : UserControl
    {
        private ILaserProjectorViewModel _laserProjectorViewModel { get { return App.AppHost.Services.GetRequiredService<ILaserProjectorViewModel>(); } }

        public LaserProjectorView()
        {
            DataContext = _laserProjectorViewModel;
            InitializeComponent();
        }
    }
}
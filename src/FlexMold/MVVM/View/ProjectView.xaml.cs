using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.Project;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for Project.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        private IProjectViewModel _ProjectViewModel { get { return App.AppHost.Services.GetRequiredService<IProjectViewModel>(); } }
        public ProjectView()
        {
            DataContext = _ProjectViewModel;
            InitializeComponent();
        }
    }
}
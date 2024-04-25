using FlexMold.MVVM.ViewModel.Project;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace FlexMold.MVVM.View
{
    /// <summary>
    /// Interaction logic for Project.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        private IProjectViewModel _ProjectViewModel { get { return App.AppHost.Services.GetRequiredService<IProjectViewModel>(); } }

        private RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
        public ProjectView()
        {
            DataContext = _ProjectViewModel;
            InitializeComponent();
        }

        private void HandleStop(object sender, RoutedEventArgs e)
        {
            ProjectViewModel.UserStop = true;          
        }
        private void HandleResume(object sender, RoutedEventArgs e)
        {
            ProjectViewModel.UserStop = true;
        }

        private void HandleShape(object sender, RoutedEventArgs e)
        {
            
        }
        private void HandleHome(object sender, RoutedEventArgs e)
        {
            
        }

        private void ProjectsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CSVHelper.IsNewProjSelected = true;
            _ProjectViewModel.selectedListRootDir = ((ListView)sender).SelectedValue.ToString();
            
        }
    }
}
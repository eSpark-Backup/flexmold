using DataGridExtensions;
using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for ProjectGridView.xaml
    /// </summary>
    public partial class ProjectGridView : UserControl
    {
        public ProjectGridView()
        {
            InitializeComponent();
            // Get a reference to the tasks collection.
        }

        private void UngroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks != null)
            {
                cvTasks.GroupDescriptions.Clear();
            }
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks != null && cvTasks.CanGroup == true)
            {
                cvTasks.GroupDescriptions.Clear();
                cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                //cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("Complete"));
            }
        }
    }
}

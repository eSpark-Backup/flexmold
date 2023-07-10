using FlexMold.MVVM.ViewModel.SplashScreen;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlexMold.MVVM.View.SplashScreen
{
    /// <summary>
    /// Interaction logic for ProjectGridView.xaml
    /// </summary>
    public partial class FoeGridView : UserControl, IFoe
    {
        public FoeGridView()
        {
            InitializeComponent();
            (DataContext as FoeViewModel).View = this as IFoe;
            Console.WriteLine();
            // Get a reference to the tasks collection.
        }

        public void RefreshDataGrid()
        {
            ((CollectionViewSource)this.Resources["devices"]).View.Refresh();
        }
    }
}
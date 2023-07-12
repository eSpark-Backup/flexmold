using FlexMold.MVVM.Model;
using FlexMold.MVVM.ViewModel.SplashScreen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace FlexMold.MVVM.View.SplashScreen
{
    /// <summary>
    /// Interaction logic for Foe.xaml
    /// </summary>
    public partial class FoeView : UserControl, IFoe
    {
        public FoeView(FoeViewModel foeViewModel)
        {
            this.DataContext = foeViewModel;
            InitializeComponent();
            var aa = (FoeViewModel)mygrid.DataContext;
            if (this.DataContext != null)
                foreach (var item in (this.DataContext as FoeViewModel).MyDevicesAndBoxes)
                {
                    item.View = this as IFoe;
                }
        }
       
        public void RefreshDataGrid()
        {
            this.Dispatcher.Invoke(() =>
            {
                ((CollectionViewSource)this.Resources["devices"]).View.Refresh();
            });

        }
    }
}

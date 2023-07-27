using FlexMold.MVVM.ViewModel.SplashScreen;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlexMold.MVVM.View.SplashScreen
{
    /// <summary>
    /// Interaction logic for Foe.xaml
    /// </summary>
    public partial class FoeView : UserControl, IFoe
    {
        public FoeView(IFoeViewModel foeViewModel)
        {
            this.DataContext = foeViewModel;
            InitializeComponent();
            //if (foeViewModel != null)
            //    foreach (var item in foeViewModel.MyDevicesAndBoxes)
            //    {
            //        item.View = this as IFoe;
            //    }
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

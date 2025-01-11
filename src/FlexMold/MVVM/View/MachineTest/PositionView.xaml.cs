using FlexMold.MVVM.ViewModel.MachineTest;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlexMold.MVVM.View.MachineTest
{
    /// <summary>
    /// Interaction logic for MachineDetailView.xaml
    /// </summary>
    public partial class PositionView : UserControl
    {
        private IPositionViewModel _PositionViewModel { get { return App.AppHost.Services.GetRequiredService<IPositionViewModel>(); } }
        public PositionView()
        {
            DataContext = _PositionViewModel;
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Color colorDefault = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD5D5D5");
            this.Moveinput.Background = new SolidColorBrush(colorDefault); 
            this.BrowseButton.Background= new SolidColorBrush(Colors.Green);
            PositionViewModel.blnOptionBrowse = true;
        }

        private void Moveinput_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double?> e)
        {
            Color colorDefault = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD5D5D5");
            this.BrowseButton.Background = new SolidColorBrush(colorDefault);
            this.Moveinput.Background = new SolidColorBrush(Colors.Green);
            PositionViewModel.blnOptionBrowse = false;

            if (this.Moveinput.Value==0)
                this.Moveinput.Background = new SolidColorBrush(colorDefault);

        }
    }
}

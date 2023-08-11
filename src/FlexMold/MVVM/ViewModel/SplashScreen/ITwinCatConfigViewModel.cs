using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace FlexMold.MVVM.ViewModel.SplashScreen
{
    public interface ITwinCatConfigViewModel: IViewModel
    {
        bool CanClose { get; set; }
        ObservableCollection<TabItem> TabList { get; set; }
    }
}
using FlexMold.MVVM.ViewModel.Project;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for FoeSettingDiaglog.xaml
    /// </summary>
    public partial class CSVDiaglogView : UserControl
    {
        public CSVDiaglogView(ICSVDiaglogViewModel cSVDiaglogViewModel)
        {
            DataContext = cSVDiaglogViewModel;
            InitializeComponent();
        }
    }
}
using FlexMold.Utility;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Setting
{
    /// <summary>
    /// Interaction logic for LaserSettingView.xaml
    /// </summary>
    public partial class LaserProjectorSettingView : UserControl
    {
        public LaserProjectorSettingView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TelnetHelper.Instance.SendMessage = this.txtSendmsg.Text;
        }
    }
}

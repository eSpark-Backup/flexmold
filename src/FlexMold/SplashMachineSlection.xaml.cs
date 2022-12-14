using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Utilities;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        public SplashMachineSlection()
        {
            InitializeComponent();

            string[] MachineList = new string[] { "Machine1", "Machine2", "Machine3", "Machine4" };

            var rn = new Random();
            foreach (var item in MachineList)
            {
                PackIconFontAwesome icon = new PackIconFontAwesome()
                { Height = 40, Width = 40, Kind = (PackIconFontAwesomeKind)rn.Next(1, 100) };
                myw.Children.Add(new Tile()
                {
                    Style = Resources["LargeTileStyle"] as Style,
                    Title = item,
                    Content = icon
                });
            }
        }
    }
}

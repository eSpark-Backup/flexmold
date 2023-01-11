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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for MotorStatusView.xaml
    /// </summary>
    public partial class MotorStatusView : UserControl
    {
        public MotorStatusView()
        {

            InitializeComponent();
            //int myGridColumns = 20;
            //int myGridRows = 20;

            //for (int i = 0; i < myGridColumns; i++)
            //{
            //    ColumnDefinition colDef1 = new ColumnDefinition();
            //    myGrid.ColumnDefinitions.Add(colDef1);
            //}
            //for (int i = 0; i < myGridRows; i++)
            //{
            //    RowDefinition colDef1 = new RowDefinition();
            //    myGrid.RowDefinitions.Add(colDef1);
            //}
            //int mcount = 0;
            //for (int i = 0; i < myGridColumns; i++)
            //{
            //    for (int j = 0; j < myGridRows; j++)
            //    {
            //        var myEllipse = new Ellipse();
            //        if (i==10&j==12)
            //        {
            //            myEllipse.Fill = System.Windows.Media.Brushes.Red;
            //        }
            //        else
            //        {
            //            myEllipse.Fill =Brushes.Transparent;
            //        }
            //        myEllipse.HorizontalAlignment = HorizontalAlignment.Center;
            //        myEllipse.VerticalAlignment = VerticalAlignment.Center;
            //        myEllipse.Width = 25;
            //        myEllipse.Height = 25;
            //        myGrid.Children.Add(myEllipse);
            //        Grid.SetRow(myEllipse,j);
            //        Grid.SetColumn(myEllipse,i);

            //        TextBlock tm = new TextBlock();
            //        tm.Text = (mcount).ToString();
            //        tm.TextAlignment = TextAlignment.Center;
            //        tm.VerticalAlignment = VerticalAlignment.Center;
            //        tm.HorizontalAlignment = HorizontalAlignment.Center;
            //        myGrid.Children.Add(tm);
            //        Grid.SetRow(tm, j);
            //        Grid.SetColumn(tm, i);
            //        mcount++;
            //    }
            //}

        }

    }
}

using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlexMold.MVVM.ViewModel.Home
{
    class MotorStatusViewmodel : ObservableObject
    {
        public ObservableCollection<MotorStatusControl> MyMotors { get; set; }

        public MotorStatusViewmodel()
        {
            int myGridColumns = 20;
            int myGridRows = 20;
            MyMotors = new ObservableCollection<MotorStatusControl>() { };
            int mcount = 0;
            for (int i = 0; i < myGridColumns; i++)
            {
                for (int j = 0; j < myGridRows; j++)
                {
                    MyMotors.Add(new MotorStatusControl() { MotorId = mcount, FillColor = Brushes.Transparent });
                    mcount++;
                }
            }
        }
        public class MotorStatusControl
        {
            public int MotorId { get; set; }
            public Brush FillColor { get; set; }

        }
    }
}
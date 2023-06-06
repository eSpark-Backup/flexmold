using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Project
{
    internal class CSVDiaglogViewModel : ObservableObject
    {
        private int _SelectedMotorDirPValue;

        public int SelectedMotorDirPValue
        {
            get { return _SelectedMotorDirPValue; }
            set
            {
                _SelectedMotorDirPValue = value;
                OnPropertyChanged();
            }
        }
    }
}

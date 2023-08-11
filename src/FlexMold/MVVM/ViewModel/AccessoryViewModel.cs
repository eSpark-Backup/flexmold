using FlexMold.Core;
using FlexMold.MVVM.View.Accessory;

namespace FlexMold.MVVM.ViewModel
{
    internal class AccessoryViewModel : ObservableObject
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        public RelayCommand HeaterStatusCommand { get; set; }

        public RelayCommand LaserStatusCommand { get; set; }

        public HeaterView HeaterStatusVM { get; set; }
        public LaserView LaserStatusVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public AccessoryViewModel()
        {
            HeaterStatusVM = new HeaterView();
            LaserStatusVM = new LaserView();

            CurrentView = HeaterStatusVM;

            HeaterStatusCommand = new RelayCommand(o => { CurrentView = HeaterStatusVM; });
            LaserStatusCommand = new RelayCommand(o => { CurrentView = LaserStatusVM; });
        }
    }
}
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

        public HeaterStatusView HeaterStatusVM { get; set; }
        public LaserStatusView LaserStatusVM { get; set; }

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
            HeaterStatusVM = new HeaterStatusView();
            LaserStatusVM = new LaserStatusView();

            CurrentView = HeaterStatusVM;

            HeaterStatusCommand = new RelayCommand(o => { CurrentView = HeaterStatusVM; });
            LaserStatusCommand = new RelayCommand(o => { CurrentView = LaserStatusVM; });
        }
    }
}
using FlexMold.MVVM.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlexMold.Core
{
    public class ObservableObject : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
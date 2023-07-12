using FlexMold.Core;
using FlexMold.MVVM.View.SplashScreen;
using FlexMold.Utility;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FlexMold.MVVM.ViewModel.SplashScreen
{
    public class TwinCatConfigViewModel : ObservableObject, ITwinCatConfigViewModel
    {
        public ObservableCollection<TabItem> TabList { get; set; }
        private bool _canClose;
        public bool CanClose
        {
            get
            {
                return _canClose;
            }
            set
            {
                _canClose = value;
                OnPropertyChanged();
            }
        }
        private void Update_Status(object sender, bool e)
        {
            CanClose = e;
        }
        FoeViewModel foeViewModel;
        public TwinCatConfigViewModel(FoeView foeView, IFoeViewModel foeViewModel)
        {
            CanClose = true;
            foeViewModel.TUpdate_Status += Update_Status;
            this.TabList = new ObservableCollection<TabItem>
            {
                new TabItem(){Content= foeView},
            };
        }
    }
}

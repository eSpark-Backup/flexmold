using FlexMold.Core;
using FlexMold.MVVM.View.SplashScreen;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
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
        private readonly IFoeViewModel _FoeViewModel; /*{ get { return App.AppHost.Services.GetRequiredService<IFoeViewModel>(); } }*/
        private FoeView _FoeView { get { return App.AppHost.Services.GetRequiredService<FoeView>(); } }

        public TwinCatConfigViewModel(IFoeViewModel foeViewModel)
        {
            _FoeViewModel = foeViewModel;
            CanClose = true;
            _FoeViewModel.TUpdate_Status += Update_Status;
            this.TabList = new ObservableCollection<TabItem>
            {
                new TabItem(){Content= _FoeView},
            };
        }
    }
}

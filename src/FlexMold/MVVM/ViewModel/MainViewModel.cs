using FlexMold.Core;
using FlexMold.Utility;
using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace FlexMold.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly Notifier _notifier;

        private string _MotorMaxData;
        public string MotorMaxData
        {
            get { return _MotorMaxData; }
            set
            {
                _MotorMaxData = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 0,
                    offsetY: 0);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(4),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;

                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 250;
            });

            _MotorMaxData = CSVHelper.MaxDisplacement.ToString();
        }

        public void OnUnloaded()
        {
            _notifier.Dispose();
        }

        public void ShowInformation(string message)
        {
            _notifier.ShowInformation(message);
        }
    }
}
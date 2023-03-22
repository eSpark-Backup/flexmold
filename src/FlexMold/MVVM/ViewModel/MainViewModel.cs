using FlexMold.Core;
using FlexMold.MVVM.Model;
using FlexMold.Utility;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TwinCAT.Ads;
using TwinCAT;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace FlexMold.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly Notifier _notifier;
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

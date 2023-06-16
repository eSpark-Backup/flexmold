using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications.Core;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications;
using ToastNotifications.Position;
using System.Windows;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using Humanizer;

namespace FlexMold.MVVM.ViewModel
{
    public static class TNF
    {
        private static readonly Notifier _Notifier;

        static TNF()
        {
            _messageCounter = 5;
            _Notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomLeft,
                    offsetX: 0,
                    offsetY: 0);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(6),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(_messageCounter));

                cfg.Dispatcher = Application.Current.Dispatcher;

                cfg.DisplayOptions.TopMost = false;
                cfg.DisplayOptions.Width = 200;
            });
        }

        private static string _lastMessage = "";
        private static int _messageCounter;

        private static void RememberMessage(string message)
        {
            if (_messageCounter % 3 == 0)
            {
                _lastMessage = message.Humanize();
            }
        }

        public static void ShowInformation(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _Notifier.ShowInformation(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public static void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _Notifier.ShowError(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public static void ShowSuccess(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _Notifier.ShowSuccess(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public static void ShowWarning(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _Notifier.ShowWarning(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        private static MessageOptions CreateOptions()
        {
            return new MessageOptions
            {
                FreezeOnMouseEnter = FreezeOnMouseEnter.GetValueOrDefault(),
                ShowCloseButton = ShowCloseButton.GetValueOrDefault(),
                Tag = ++_messageCounter % 2
            };
        }

        public static bool? FreezeOnMouseEnter { get; set; } = false;
        public static bool? ShowCloseButton { get; set; } = true;
    }
}
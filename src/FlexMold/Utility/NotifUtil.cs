using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace FlexMold.Utility
{

    public static class ToastNotificationUtil
    {
        static readonly Notifier _notifier;
        static ToastNotificationUtil()
        {
            _messageCounter = 5;
            _notifier = new Notifier(cfg =>
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
                _lastMessage = message;
            }
        }
        public static void ShowInformation(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifier.ShowInformation(message, CreateOptions());
                RememberMessage(message);
            });
        }
        public static void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifier.ShowError(message, CreateOptions());
                RememberMessage(message);
            });
        }
        public static void ShowSuccess(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifier.ShowSuccess(message, CreateOptions());
                RememberMessage(message);
            });
        }
        public static void ShowWarning(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifier.ShowWarning(message, CreateOptions());
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

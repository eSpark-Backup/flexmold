using Humanizer;
using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace FlexMold.Utility
{
    public class ToastNotificationUtil
    {
        public Notifier Notifier { get; private set; }

        public ToastNotificationUtil()
        {
            _messageCounter = 5;
            Notifier = new Notifier(cfg =>
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

        private string _lastMessage = "";
        private int _messageCounter;

        private void RememberMessage(string message)
        {
            if (_messageCounter % 3 == 0)
            {
                _lastMessage = message.Humanize();
            }
        }

        public void ShowInformation(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Notifier.ShowInformation(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Notifier.ShowError(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public void ShowSuccess(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Notifier.ShowSuccess(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        public void ShowWarning(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Notifier.ShowWarning(message.Humanize(), CreateOptions());
                RememberMessage(message);
            });
        }

        private MessageOptions CreateOptions()
        {
            return new MessageOptions
            {
                FreezeOnMouseEnter = FreezeOnMouseEnter.GetValueOrDefault(),
                ShowCloseButton = ShowCloseButton.GetValueOrDefault(),
                Tag = ++_messageCounter % 2
            };
        }

        public bool? FreezeOnMouseEnter { get; set; } = false;
        public bool? ShowCloseButton { get; set; } = true;
    }
}
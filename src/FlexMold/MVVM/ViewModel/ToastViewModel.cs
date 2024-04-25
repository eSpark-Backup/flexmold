using NLog;
using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace FlexMold.MVVM.ViewModel
{
    public static class TNF
    {
        private static readonly Notifier _Notifier;
        public static bool show;
        static TNF()
        {
            _messageCounter = 5;
            _Notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 0,
                    offsetY: 0);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(4),
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
            try { 
            if (show)
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Notifier.ShowInformation(message, CreateOptions());
                    RememberMessage(message);
                }));
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Project");
                log.Log(LogLevel.Error, "MainWindow-MetroClosing" + ex.Message);
            }
        }

        public static void ShowError(string message)
        {
            try{
                if (show)
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Notifier.ShowError(message, CreateOptions());
                    RememberMessage(message);
                }));
        }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Project");
        log.Log(LogLevel.Error, "MainWindow-MetroClosing" + ex.Message);
            }
}

public static void ShowSuccess(string message)
        {
            try { 
            if (show)
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Notifier.ShowSuccess(message, CreateOptions());
                    RememberMessage(message);
                }));
}
            catch (Exception ex)
{
    var log = LogManager.GetLogger("Project");
    log.Log(LogLevel.Error, "MainWindow-MetroClosing" + ex.Message);
}
        }

        public static void ShowWarning(string message)
        {
            try { 
            if (show)
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _Notifier.ShowWarning(message, CreateOptions());
                    RememberMessage(message);
                }));
}
            catch (Exception ex)
{
    var log = LogManager.GetLogger("Project");
    log.Log(LogLevel.Error, "MainWindow-MetroClosing" + ex.Message);
}
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

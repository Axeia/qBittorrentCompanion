using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Notification;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Reactive;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public partial class NotificationsViewModel : ViewModelBase
    {
        public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

        public ReactiveCommand<Unit, Unit> ShowTestNotificationCommand
            => ReactiveCommand.Create(ShowTestNotification);

        public void ShowTestNotification()
        {
            Manager
                .CreateMessage()
                .Accent(new SolidColorBrush(ThemeColors.SystemAccentLight1))
                .HasMessage(Resources.Resources.NotificationsViewModel_TestMessage)
                .Dismiss().WithDelay(TimeSpan.FromSeconds(10))
                .Queue();
        }

        private bool _notificationNativeUpdateAvailable = Design.IsDesignMode || ConfigService.NotificationNativeUpdateAvailable;

        public bool NotificationNativeUpdateAvailable
        {
            get => ConfigService.NotificationNativeUpdateAvailable;
            set
            {
                if (value != _notificationNativeUpdateAvailable)
                {
                    _notificationNativeUpdateAvailable = value;
                    this.RaisePropertyChanged(nameof(NotificationNativeUpdateAvailable));
                    ConfigService.NotificationNativeUpdateAvailable = value;
                }
            }
        }
    }
}
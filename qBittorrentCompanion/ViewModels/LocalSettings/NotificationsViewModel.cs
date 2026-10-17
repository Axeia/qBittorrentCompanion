using Avalonia.Controls;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Reactive;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public partial class NotificationsViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> ShowTestNotificationCommand
            => ReactiveCommand.Create(ShowTestNotification);

        public void ShowTestNotification()
        {
            // Route through your central NotificationService or trigger a UI action
            NotificationService.NotifyTest();
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
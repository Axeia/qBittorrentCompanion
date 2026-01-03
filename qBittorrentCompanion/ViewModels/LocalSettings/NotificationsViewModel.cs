using Avalonia.Controls;
using qBittorrentCompanion.Services;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public partial class NotificationsViewModel : ViewModelBase
    {
        private bool _notificationNativeDisconnected = Design.IsDesignMode
            ? false
            : ConfigService.NotificationNativeDisconnected;

        public bool NotificationNativeDisconnected
        {
            get => _notificationNativeDisconnected;
            set
            {
                if (_notificationNativeDisconnected != value)
                {
                    _notificationNativeDisconnected = value;
                    ConfigService.NotificationNativeDisconnected = value;
                    this.RaisePropertyChanged(nameof(NotificationNativeDisconnected));
                }
            }
        }
    }
}
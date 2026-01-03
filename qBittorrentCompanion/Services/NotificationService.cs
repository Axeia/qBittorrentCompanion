using Avalonia.Controls;
using ReactiveUI;
using System;

namespace qBittorrentCompanion.Services
{
    // Class to centralize access to NotificationNativeSuspended settings
    public partial class NotificationService : ReactiveObject
    {
        private static readonly Lazy<NotificationService> _instance = new(() => new());
        public static NotificationService Instance => _instance.Value;

        private bool _notificationNativeSuspended = !Design.IsDesignMode && ConfigService.NotificationNativeSuspended;

        public bool NotificationNativeSuspended
        {
            get => _notificationNativeSuspended;
            set
            {
                if (_notificationNativeSuspended != value)
                {
                    _notificationNativeSuspended = value;
                    ConfigService.NotificationNativeSuspended = value;
                    this.RaisePropertyChanged(nameof(NotificationNativeSuspended));
                }
            }
        }
    }
}

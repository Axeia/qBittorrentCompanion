using Avalonia.Media;
using Avalonia.Notification;
using qBittorrentCompanion.Helpers;
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
                .HasMessage("This is what a notification would look like, although it'd be in the main window. Self destructing in 10 seconds.")
                .Dismiss().WithDelay(TimeSpan.FromSeconds(10))
                .Queue();
        }
    }
}
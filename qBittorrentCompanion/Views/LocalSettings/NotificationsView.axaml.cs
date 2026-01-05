using Avalonia.Controls;
using Avalonia.Labs.Notifications;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System.Diagnostics;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class NotificationsView : UserControl
    {
        public NotificationsView()
        {
            InitializeComponent();
            DataContext = new NotificationsViewModel();
        }

        private void TestNotification_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            NotificationService.Instance.NotifyNativelyTorrentAdded("Leap-16.0-offline-installer-x86_64-Build171.1.install.iso");
        }
    }
}
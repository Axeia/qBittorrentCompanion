using Avalonia.Controls;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels.LocalSettings;

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
            NotificationService.Instance.NotifyNativelyTest();
        }
    }
}
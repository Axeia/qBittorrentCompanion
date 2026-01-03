using Avalonia.Controls;
using Avalonia.Labs.Notifications;
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
            if (NativeNotificationManager.Current is { } manager)
            {
                INativeNotification? inn = manager.CreateNotification("Test");
                if (inn is INativeNotification innn)
                {
                    innn.Message = "Leap-16.0-offline-installer-x86_64-Build171.1.install.iso";
                    innn.Title = "Download completed";
                    innn.Tag = "qBittorrent Companion";
                    innn.Icon = App.Current?.CurrentModeWindowIconBitmap;
                    innn.Show();
                }
                else
                {
                    Debug.WriteLine("No native notification");
                }
            }
            else
            {
                Debug.WriteLine("No native notification manager");
            }
        }
    }
}
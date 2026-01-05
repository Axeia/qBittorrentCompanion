using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Labs.Notifications;
using qBittorrentCompanion.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace qBittorrentCompanion.Services
{
    // Class to centralize access to NotificationNativeSuspended settings
    public partial class NotificationService : ReactiveObject
    {
        private static readonly Lazy<NotificationService> _instance = new(() => new());
        public static NotificationService Instance => _instance.Value;

        private bool _notificationNativeSuspended = !Design.IsDesignMode && ConfigService.NotificationNativeSuspended;

        /// <summary>
        /// Used by the system tray icon and local settings to indicate 
        /// if native notifications have been suspended.
        /// 
        /// When toggled to true if there's any notifications queued up (<see cref="_nativeNotificationQueue"/>)
        /// they'll all be displayed and removed from the queue.
        /// </summary>
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

                    if (value == false)
                        foreach(INativeNotification inn in _nativeNotificationQueue)
                        {
                            inn.Show();
                            _nativeNotificationQueue.Remove(inn);
                        }
                }
            }
        }

        private bool _notificationInAppDownloadCompleted = !Design.IsDesignMode && ConfigService.NotificationInAppDownloadCompleted;
        public bool NotificationInAppDownloadCompleted
        {
            get => _notificationInAppDownloadCompleted;
            set
            {
                if (value != _notificationInAppDownloadCompleted)
                {
                    _notificationInAppDownloadCompleted = value;
                    ConfigService.NotificationInAppDownloadCompleted = value;
                    this.RaisePropertyChanged(nameof(NotificationInAppDownloadCompleted));
                }
            }
        }

        private bool _notificationNativeDownloadCompleted = !Design.IsDesignMode && ConfigService.NotificationNativeDownloadCompleted;
        public bool NotificationNativeDownloadCompleted
        {
            get => _notificationInAppDownloadCompleted;
            set
            {
                if (value != _notificationNativeDownloadCompleted)
                {
                    _notificationNativeDownloadCompleted = value;
                    ConfigService.NotificationNativeDownloadCompleted = value;
                    this.RaisePropertyChanged(nameof(NotificationNativeDownloadCompleted));
                }
            }
        }

        private List<INativeNotification> _nativeNotificationQueue = [];

        public void NotifyNativelyDisconnected()
        {
            INativeNotification? inn = GetNotification(
                "Connection", 
                "Download completed", 
                "A problem occured trying to contact the qbittorrent web API"
            );

            ShowOrQueueUpNativeNotification(inn);
        }

        public void NotifyNativelyTorrentAdded(string torrentName)
        {
            INativeNotification? inn = GetNotification(
                "Torrents", 
                "Download completed", 
                torrentName
            );

            ShowOrQueueUpNativeNotification(inn);
        }

        private static INativeNotification? GetNotification(string category, string title, string message)
        {
            if (NativeNotificationManager.Current is { } manager)
            {
                INativeNotification? inn = manager.CreateNotification(category);
                if (inn is INativeNotification innn)
                {
                    innn.Message = message;
                    innn.Title = title;
                    innn.Tag = "qBittorrent Companion";
                    innn.Icon = App.Current?.CurrentModeWindowIconBitmap;
                    innn.Show();
                }
                else
                {
                    Debug.WriteLine($"No native notification for {title}");
                }

                return inn;
            }
            else
            {
                Debug.WriteLine("No native notification manager");
                return null;
            }
        }

        private void ShowOrQueueUpNativeNotification(INativeNotification? inn)
        {
            if (inn is null)
                return;

            if (NotificationNativeSuspended)
                _nativeNotificationQueue.Add(inn);
            else
                inn.Show();
        }

        internal void NotifyDisconnected()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (mw.ShowInTaskbar 
                    && mw.WindowState != WindowState.Minimized
                    && NotificationInAppDownloadCompleted)
                {
                    // Use in app notification
                }
                else if (NotificationNativeDownloadCompleted)
                {
                    NotifyNativelyDisconnected();
                }
            }
        }
    }
}

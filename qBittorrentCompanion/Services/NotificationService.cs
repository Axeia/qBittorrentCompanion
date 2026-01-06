using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Labs.Notifications;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
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
            get => _notificationNativeDownloadCompleted;
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

        private bool _notificationInAppTorrentAdded = Design.IsDesignMode || ConfigService.NotificationInAppTorrentAdded;
        public bool NotificationInAppTorrentAdded
        {
            get => _notificationInAppTorrentAdded;
            set
            {
                if (value != _notificationInAppTorrentAdded)
                {
                    _notificationInAppTorrentAdded = value;
                    ConfigService.NotificationInAppTorrentAdded = value;
                    this.RaisePropertyChanged(nameof(NotificationInAppTorrentAdded));
                }
            }
        }

        private bool _notificationNativeTorrentAdded = !Design.IsDesignMode && ConfigService.NotificationNativeTorrentAdded;
        public bool NotificationNativeTorrentAdded
        {
            get => _notificationNativeTorrentAdded;
            set
            {
                if (value != _notificationNativeTorrentAdded)
                {
                    _notificationNativeTorrentAdded = value;
                    ConfigService.NotificationNativeTorrentAdded = value;
                    this.RaisePropertyChanged(nameof(NotificationNativeTorrentAdded));
                }
            }
        }

        private List<INativeNotification> _nativeNotificationQueue = [];

        public static void NotifyNativelyTest()
        {
            INativeNotification? inn = GetNotification(
                "Test",
                "Title",
                "Description of what happened"
            );

            inn?.Show();
        }

        private void NotifyNativelyDisconnected()
        {
            INativeNotification? inn = GetNotification(
                "Connection", 
                "Too many retries - disconnected", 
                "A problem occured trying to contact the qbittorrent web API"
            );

            ShowOrQueueUpNativeNotification(inn);
        }

        private void NotifyNativelyTorrentAdded(string torrentName)
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

        public void NotifyDisconnected()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (MainWindowIsActive(mw) && NotificationInAppDownloadCompleted)
                {
                    // Use in app notification
                }
                else if (NotificationNativeDownloadCompleted)
                {
                    NotifyNativelyDisconnected();
                }
            }
        }

        public void NotifyTorrentCompleted(TorrentInfoViewModel tivm)
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (MainWindowIsActive(mw) && NotificationInAppDownloadCompleted)
                {
                    // Use in app notification
                }
                else if (NotificationNativeDownloadCompleted)
                {
                    NotifyNativelyTorrentCompleted(tivm);
                }
            }
        }
           
        private static void NotifyNativelyTorrentCompleted(TorrentInfoViewModel tivm)
        {
            GetNotification("Torrents", "Download completed", tivm.Name);
        }


        public void NotifyTorrentAdded(TorrentPartialInfo tpi)
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (MainWindowIsActive(mw) && NotificationInAppTorrentAdded)
                {
                    // Use in app notification
                }
                else if (NotificationNativeTorrentAdded)
                {
                    NotifyNativelyTorrentAdded(tpi);
                }
            }
        }

        private static void NotifyNativelyTorrentAdded(TorrentPartialInfo tpi)
        {
            GetNotification("Torrents", "Torrent added", tpi.Name);
        }

        private static bool MainWindowIsActive(MainWindow mw)
        {
            return mw.ShowInTaskbar && mw.WindowState != WindowState.Minimized;
        }
    }
}

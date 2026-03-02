using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Labs.Notifications;
using Avalonia.Notification;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        private void NotifyNatively(string category, string title, string message)
        {
            INativeNotification? inn = GetNotification(
                category,
                title,
                message
            );

            ShowOrQueueUpNativeNotification(inn);
        }

        private static void NotifyInApp(string message)
        {
            GetMainWindowViewModel()?
                .NotificationManager
                .CreateMessage()
                .HasMessage(message)
                .Dismiss().WithButton(Resources.Resources.Global_Close, button => { })
                .Queue();
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

        private static MainWindow? GetMainWindow()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mw)
                return mw;

            return null;
        }

        private static MainWindowViewModel? GetMainWindowViewModel()
        {
            if (GetMainWindow() is MainWindow mw && mw.DataContext is MainWindowViewModel mwvm)
                return mwvm;

            return null;
        }

        public void NotifyDisconnected()
        {
            if (GetMainWindow() is MainWindow mw)
            {
                if (MainWindowIsActive(mw) && NotificationInAppDownloadCompleted)
                {
                    // no need, log in should pop up
                }
                else if (NotificationNativeDownloadCompleted)
                {
                    NotifyNatively( 
                        "Connection",
                        Resources.Resources.NotificationService_TooManyRetriesDisconnected,
                        Resources.Resources.NotificationService_ProblemContactingApi
                    );
                }
            }
        }


        public void NotifyTorrentCompleted(TorrentInfoViewModel tivm)
        {
            if (GetMainWindow() is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (MainWindowIsActive(mw) && NotificationInAppDownloadCompleted)
                {
                    NotifyInApp(Resources.Resources.NotificationService_DownloadCompleted + " " + tivm.Name);
                }
                else if (NotificationNativeDownloadCompleted)
                {
                    NotifyNatively("Torrents", Resources.Resources.NotificationService_DownloadCompleted, tivm.Name);
                }
            }
        }

        public void NotifyTorrentAdded(TorrentPartialInfo tpi)
        {
            if (GetMainWindow() is MainWindow mw)
            {
                // If displayed - use in app notification if it's enabled.
                if (MainWindowIsActive(mw) && NotificationInAppTorrentAdded)
                {
                    NotifyInApp(Resources.Resources.NotificationService_TorrentsAdded + " " + tpi.Name);
                }
                else if (NotificationNativeTorrentAdded)
                {
                    NotifyNatively("Torrents", Resources.Resources.NotificationService_TorrentsAdded, tpi.Name);
                }
            }
        }

        public void NotifyUpdateAvailable(string newVersion, string url)
        {
            if (GetMainWindow() is MainWindow mw)
            {
                if (!mw.IsActive && ConfigService.NotificationNativeUpdateAvailable)
                {
                    NotifyNatively(
                        "qBittorrent Companion",
                        Resources.Resources.NotificationService_QbcUpdateAvailable,
                        string.Format(Resources.Resources.NotificationService_UpdateAvailableCurNext, UpdateService.ZeroPaddedVersionString, newVersion)
                    );
                }
                var res = GetMainWindowViewModel()?
                    .NotificationManager
                    .CreateMessage()
                    .HasHeader(string.Format(Resources.Resources.NotificationService_UpdateAvailable, newVersion))
                    .HasMessage(string.Format(Resources.Resources.NotificationService_UpdateAvailableCurNext, UpdateService.ZeroPaddedVersionString))
                    .Dismiss().WithButton(Resources.Resources.Global_Close, button => { })
                    .Queue();

                /// NotificationManger does not allow setting tooltips on buttons, do it manually
                AddUpdateSplitButton(res, newVersion, url);
            }
            else
            {
                Debug.WriteLine("Not a main window");
            }
        }

        private void AddUpdateSplitButton(INotificationMessage? msg, string newVersion, string url)
        {
            if (msg is null) return;

            foreach (var btnObj in msg.Buttons)
            {
                if (btnObj is INotificationMessageButton nmb)
                {
                    if (btnObj is Control avObj)
                    {
                        ToolTip.SetTip(avObj, Resources.Resources.NotificationService_CloseToolTip);
                    }
                }
            }

            var splitButton = new SplitButton()
            {
                Content = Resources.Resources.NotificationService_Install,
                Command = ReactiveCommand.Create(() => { /* Main Install Logic */ })
            };

            // Create the dropdown menu
            var flyout = new MenuFlyout();

            var githubMenuItem = new MenuItem { Header = Resources.Resources.NotificationService_GitHub };
            ToolTip.SetTip(githubMenuItem, Resources.Resources.NotificationService_GitHubToolTip);
            githubMenuItem.Click += (s, e) => { if (TopLevel.GetTopLevel(GetMainWindow()) is TopLevel topLevel) { _ = topLevel.Launcher.LaunchUriAsync(new Uri(url)); } };
            githubMenuItem.Icon = new SymbolIcon() { Symbol = Symbol.WindowNew };
            flyout.Items.Add(githubMenuItem);

            flyout.Items.Add(new Separator());

            var ignoreForSessionMenuItem = new MenuItem { Header = Resources.Resources.NotificationService_IgnoreForSession };
            ToolTip.SetTip(ignoreForSessionMenuItem, Resources.Resources.NotificationService_IgnoreForSessionToolTip);
            ignoreForSessionMenuItem.Click += (s, e) => { UpdateService.Instance.StopTimer(); GetMainWindowViewModel()?.NotificationManager.Dismiss(msg); };
            ignoreForSessionMenuItem.Icon = new SymbolIcon() { Symbol = Symbol.GlobeClock };
            flyout.Items.Add(ignoreForSessionMenuItem);

            var ignorePermanentlyMenuItem = new MenuItem { Header = Resources.Resources.NotificationService_IgnorePermanently };
            ToolTip.SetTip(ignorePermanentlyMenuItem, Resources.Resources.NotificationService_IgnorePermanentlyToolTip);
            ignorePermanentlyMenuItem.Click += (s, e) => { IgnoreUpdateVersion(newVersion); GetMainWindowViewModel()?.NotificationManager.Dismiss(msg); };
            ignorePermanentlyMenuItem.Icon = new SymbolIcon() { Symbol = Symbol.GlobeOff };
            flyout.Items.Add(ignorePermanentlyMenuItem);

            var neverEverUpdateMenuItem = new MenuItem { Header = Resources.Resources.NotificationService_NeverEverUpdate };
            ToolTip.SetTip(neverEverUpdateMenuItem, Resources.Resources.NotificationService_NeverEverUpdateToolTip);
            neverEverUpdateMenuItem.Click += (s, e) => { UpdateService.Instance.CheckForUpdates = false; };
            neverEverUpdateMenuItem.Icon = new SymbolIcon() { Symbol = Symbol.GlobeProhibited };
            flyout.Items.Add(neverEverUpdateMenuItem);

            // Fill the flyout

            splitButton.Flyout = flyout;
            msg.Buttons.Add(splitButton);
        }

        private void IgnoreUpdateVersion(string newVersion)
        {
            if (ConfigService.IgnoredUpdateVersions.Contains(newVersion)) return;

            ConfigService.IgnoredUpdateVersions = [.. ConfigService.IgnoredUpdateVersions, .. new string[] { newVersion }];
        }

        private static bool MainWindowIsActive(MainWindow mw)
        {
            return mw.ShowInTaskbar && mw.WindowState != WindowState.Minimized;
        }
    }
}

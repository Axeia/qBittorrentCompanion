using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    public sealed class DirectoryMonitorService : IDisposable
    {
        private static readonly Lazy<DirectoryMonitorService> _instance = new(() => new DirectoryMonitorService());
        public static string DotTorrentFilter 
            => "*.torrent";
        private static string RenameToExtensionPostfix
            => ConfigService.DotTorrentRenameExtensionPostfix;
        public static DirectoryMonitorService Instance => _instance.Value;

        private readonly List<DebouncedFileWatcher> _activeWatchers = [];

        private DirectoryMonitorService() { }

        public void Initialize()
        {
            DisposeWatchers();

            foreach (var dir in ConfigService.MonitoredDirectories)
            {
                if (!Directory.Exists(dir.PathToMonitor))
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<DirectoryMonitorService>(),
                        "Config contains invalid to monitor directory",
                        $"Cannot add {dir.PathToMonitor} as a monitored directory, it does not exist"
                    );

                    continue;
                }

                if (dir.Action == MonitoredDirectoryAction.Move && !Directory.Exists(dir.PathToMoveTo))
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<DirectoryMonitorService>(),
                        "Config contains invalid move to directory",
                        $"Cannot monitor {dir.PathToMonitor} as its configured to copy added {DotTorrentFilter} files  to {dir.PathToMonitor} but that directory no longer exists."
                    );

                    continue;
                }
                
                var watcher = new DebouncedFileWatcher(dir.PathToMonitor, DotTorrentFilter);
                watcher.ChangesReadyAsync += () => OnDirectoryChangedAsync(dir);
                _activeWatchers.Add(watcher);
            }
        }

        private async Task OnDirectoryChangedAsync(MonitoredDirectory dir)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                switch (dir.Action)
                {
                    case MonitoredDirectoryAction.ChangeExtension:
                        AddDotTorrentsToDownloadsAndChangeExtension(dir.PathToMonitor, RenameToExtensionPostfix);
                        break;
                    case MonitoredDirectoryAction.Move:
                        AddDotTorrentsToDownloadsAndMoveFiles(dir.PathToMonitor, dir.PathToMoveTo);
                        break;
                    case MonitoredDirectoryAction.Delete:
                        AddDotTorrentsToDownloadAndDeleteFiles(dir.PathToMonitor);
                        break;
                }
            });
        }

        private void AddDotTorrentsToDownloadsAndChangeExtension(string pathToMonitor, string renameToExtensionPostfix)
        {
            var dotTorrentFiles = Directory.GetFiles(pathToMonitor, DirectoryMonitorService.DotTorrentFilter);
            foreach (var dotTorrentfile in dotTorrentFiles)
            {
                //AddTorrentsRequest addTorrentsRequest = new()
                //{
                //    DownloadFolder = string.tex,
                //    Cookie = string.Empty,
                //    Rename = string.Empty,
                //    Category = category,
                //    Tags = [],
                //    SkipHashChecking = SkipHashCheckCheckBox.IsChecked == true,
                //    CreateRootFolder = false,
                //    AutomaticTorrentManagement = AutoTMMComboBox.SelectedIndex == 1,
                //    RatioLimit = 0.0,
                //    SeedingTimeLimit = TimeSpan.FromSeconds(3600),
                //    //stop condition?
                //    ContentLayout = (TorrentContentLayout)ContentLayoutComboBox.SelectedIndex,
                //    SequentialDownload = SequentialDownloadCheckBox.IsChecked == true,
                //    FirstLastPiecePrioritized = FirstLastPrioCheckBox.IsChecked == true,
                //    DownloadLimit = (int?)dlLimitNumericUpDown.Value,
                //    UploadLimit = (int?)upLimitNumericUpDown.Value
                //};

                //QBittorrentService.AddTorrentsAsync()
            }
        }

        private void AddDotTorrentsToDownloadsAndMoveFiles(string pathToMonitor, string pathToMoveTo)
        {
            //throw new NotImplementedException();
        }

        private void AddDotTorrentsToDownloadAndDeleteFiles(string pathToMonitor)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            DisposeWatchers();
        }

        private void DisposeWatchers()
        {
            foreach (var watcher in _activeWatchers)
                watcher.Dispose();

            _activeWatchers.Clear();
        }
    }
}

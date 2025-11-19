using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Extensions;
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
                        $"Cannot add \n{dir.PathToMonitor}\nas a monitored directory, it does not exist"
                    );

                    continue;
                }

                if (dir.Action == MonitoredDirectoryAction.Move && !Directory.Exists(dir.PathToMoveTo))
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<DirectoryMonitorService>(),
                        "Config contains invalid move to directory",
                        $"{dir.PathToMoveTo} \ndoes not exist and thus \n{dir.PathToMonitor} \nwill not be monitored for {DotTorrentFilter} files."
                    );

                    continue;
                }
                
                // Add watcher
                var watcher = new DebouncedFileWatcher(dir.PathToMonitor, DotTorrentFilter);
                watcher.ChangesReadyAsync += () => OnDirectoryChangedAsync(dir);
                _activeWatchers.Add(watcher);

                // Run the same logic once as files might already be present
                _ = OnDirectoryChangedAsync(dir);
            }
        }

        private async Task OnDirectoryChangedAsync(MonitoredDirectory dir)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                switch (dir.Action)
                {
                    case MonitoredDirectoryAction.ChangeExtension:
                        _ = AddDotTorrentsToDownloadsAndChangeExtensionAsync(dir.PathToMonitor, RenameToExtensionPostfix, dir.Optionals);
                        break;
                    case MonitoredDirectoryAction.Move:
                        _ = AddDotTorrentsToDownloadsAndMoveFilesAsync(dir.PathToMonitor, dir.PathToMoveTo, dir.Optionals);
                        break;
                    case MonitoredDirectoryAction.Delete:
                        _ = AddDotTorrentsToDownloadAndDeleteFilesAsync(dir.PathToMonitor, dir.Optionals);
                        break;
                }
            });
        }

        private static async Task AddDotTorrentsToDownloadsAndChangeExtensionAsync(string pathToMonitor, string renameToExtensionPostfix, AddTorrentRequestBaseDto? dto)
        {
            AddTorrentsRequest addTorrentsRequest = PrepRequest(pathToMonitor, dto);

            try
            {
                // Send to qBittorrent
                await QBittorrentService.AddTorrentsAsync(addTorrentsRequest);

                // Must have succeeded - rename the files.
                foreach (var torrentFilePath in addTorrentsRequest.TorrentFiles)
                    File.Move(torrentFilePath, torrentFilePath + renameToExtensionPostfix);

                ReportAddedTorrents(addTorrentsRequest, pathToMonitor, $"added them to downloads and renamed the files to {DotTorrentFilter}.{RenameToExtensionPostfix}");
            }
            catch (Exception ex)
            {
                ReportFailedToAddTorrents(addTorrentsRequest, pathToMonitor, ex);
            }
        }

        private static async Task AddDotTorrentsToDownloadsAndMoveFilesAsync(string pathToMonitor, string pathToMoveTo, AddTorrentRequestBaseDto? dto)
        {
            AddTorrentsRequest addTorrentsRequest = PrepRequest(pathToMonitor, dto);

            try
            {
                await QBittorrentService.AddTorrentsAsync(addTorrentsRequest);

                // Must have succeeded - move the files.
                foreach (var torrentFilePath in addTorrentsRequest.TorrentFiles)
                    if(Path.GetFileName(torrentFilePath) is string fileNameWithExtension)
                        File.Move(torrentFilePath, Path.Combine(pathToMoveTo, fileNameWithExtension));

                ReportAddedTorrents(addTorrentsRequest, pathToMonitor, $"added them to downloads and moved the files to {pathToMoveTo}");
            }
            catch (Exception ex)
            {
                ReportFailedToAddTorrents(addTorrentsRequest, pathToMonitor, ex);
            }
        }

        private static async Task AddDotTorrentsToDownloadAndDeleteFilesAsync(string pathToMonitor, AddTorrentRequestBaseDto? dto)
        {
            AddTorrentsRequest addTorrentRequest = PrepRequest(pathToMonitor, dto);
            try
            {
                await QBittorrentService.AddTorrentsAsync(addTorrentRequest);

                int deletedFileCount = 0;
                foreach (var torrentFilePath in addTorrentRequest.TorrentFiles)
                    try
                    {
                        File.Delete(torrentFilePath);
                        deletedFileCount++;
                    }
                    catch(Exception e)
                    {
                        AppLoggerService.AddLogMessage(
                            Splat.LogLevel.Warn,
                            GetFullTypeName<DirectoryMonitorService>(),
                            $"Could not delete {DotTorrentFilter} file",
                            $"File was successfully added to downloads but could not delete it: \n{torrentFilePath}\n{e.Message}"
                        );
                    }

                if (deletedFileCount < addTorrentRequest.TorrentFiles.Count)
                    ReportAddedTorrents(
                        addTorrentRequest, 
                        pathToMonitor,
                        $"Only managed to delete [{deletedFileCount}/{addTorrentRequest.TorrentFiles.Count}] files during cleanup afterwards",
                        Splat.LogLevel.Warn
                    );
                else
                    ReportAddedTorrents(
                        addTorrentRequest,
                        pathToMonitor,
                        $"Deleted all {deletedFileCount} files successfully afterwards."
                    );

            }
            catch (Exception ex)
            {
                ReportFailedToAddTorrents(addTorrentRequest, pathToMonitor, ex);
            }
        }

        private static AddTorrentsRequest PrepRequest(string pathToMonitor, AddTorrentRequestBaseDto? dto)
        {
            string[] torrentFilePaths = GetFilesFromDirectory(pathToMonitor);
            // Get .torrent files and prepare the request
            AddTorrentsRequest addTorrentsRequest = dto?.ToAddTorrentRequest() ?? new AddTorrentsRequest();
            addTorrentsRequest.BatchAddFiles(torrentFilePaths);

            return addTorrentsRequest;
        }

        private static void ReportAddedTorrents(AddTorrentsRequest atr, string monitoredPath, string successMessage, Splat.LogLevel logLevel = Splat.LogLevel.Info)
        {
            int dotTorrentFileCount = atr.TorrentFiles.Count;
            string directoryName = new DirectoryInfo(monitoredPath).Name;

            AppLoggerService.AddLogMessage(
                logLevel,
                GetFullTypeName<DirectoryMonitorService>(),
                $"Added {dotTorrentFileCount} downloads",
                $"Found {dotTorrentFileCount} {DotTorrentFilter} files in \n{monitoredPath}. \n" +
                successMessage,
                directoryName
            );

        }

        private static void ReportFailedToAddTorrents(AddTorrentsRequest atr, string monitoredPath, Exception ex)
        {
            string directoryName = new DirectoryInfo(monitoredPath).Name;

            AppLoggerService.AddLogMessage(
                Splat.LogLevel.Error,
                GetFullTypeName<DirectoryMonitorService>(),
                "Failed to add downloads",
                $"Found {atr.TorrentFiles.Count} {DotTorrentFilter} files in \n{monitoredPath}. However QBC failed to add them to the downloads.\n{ex.Message}\n{ex.Message}",
                directoryName
            );
        }

        private static string[] GetFilesFromDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, DotTorrentFilter);
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

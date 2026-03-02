using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using qBittorrentCompanion.Views;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    public class UpdateService : ReactiveObject
    {
        private static readonly HttpClient _httpClient = new();

        static UpdateService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "qBittorrentCompanion-App");
        }

        // Versions are parsed as ints which gets rid of zero prefixes
        // This adds them back in for consistent formatting in line with what
        // has been used on github so far.
        public static string ZeroPaddedVersionString =>
            Assembly.GetEntryAssembly()?.GetName().Version is Version v
                ? $"{v.Major}.{v.Minor:D2}.{v.Build:D2}.{v.Revision:D4}"
                : "";

        private static readonly Lazy<UpdateService> _instance = new(() => new());
        public static UpdateService Instance => _instance.Value;

        private bool _checkForUpdates = Design.IsDesignMode || ConfigService.CheckForQbcUpdates;
        public bool CheckForUpdates
        { 
            get => _checkForUpdates;
            set
            {
                Debug.WriteLine($"bool {value}");
                if (value != _checkForUpdates)
                {
                    _checkForUpdates = value;
                    ConfigService.CheckForQbcUpdates = value;
                    Debug.WriteLine("write config");
                    if (value)
                        StartTimer();
                    else
                        StopTimer();
                }
            }
        }

        private CancellationTokenSource? _updateCheckCts;
        public void StartTimer()
        {
            StopTimer();
            _updateCheckCts = new CancellationTokenSource();
            _ = CheckUpdatesLoopAsync(_updateCheckCts.Token);
        }

        public void StopTimer()
        {
            if (_updateCheckCts != null)
            {
                _updateCheckCts.Cancel();
                _updateCheckCts.Dispose();
                _updateCheckCts = null;

                AppLoggerService.AddLogMessage(LogLevel.Info, "UpdateService", "Update timer requested to stop.");
            }
        }

        private async Task CheckUpdatesLoopAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            try
            {
                // Check right now
                await StartCheckingForUpdates();
                // Keep checking based on the timer
                while (await timer.WaitForNextTickAsync(ct))
                    await StartCheckingForUpdates();
            }
            catch (OperationCanceledException)
            {
                // This is expected when the app closes/timer is cancelled
                AppLoggerService.AddLogMessage(LogLevel.Info, "UpdateService", "Update timer stopped.");
            }
        }

        private async Task StartCheckingForUpdates()
        {
            if (!UpdateService.Instance.CheckForUpdates) return;

            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                GetFullTypeName<MainWindow>(),
                Resources.Resources.MainWindow_CheckingForUpdates
            );

            var (isUpdateAvailable, latestVersion, downloadUrl) = await CheckForUpdatesAsync();

            if (isUpdateAvailable)
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    GetFullTypeName<MainWindow>(),
                    String.Format(Resources.Resources.MainWindow_UpdateFound, latestVersion)
                );

                // Needs to be on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    NotificationService.Instance.NotifyUpdateAvailable(latestVersion, downloadUrl);
                });
            }
            else
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    GetFullTypeName<MainWindow>(),
                    Resources.Resources.MainWindow_AlreadyUpToDate
                );
            }
        }

        private const string GitHubApiUrl = "https://api.github.com/repos/Axeia/qBittorrentCompanion/releases";

        public static async Task<(bool isUpdateAvailable, string latestVersion, string downloadUrl)> CheckForUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "qBittorrentCompanion-App");

                var response = await client.GetStringAsync(GitHubApiUrl);
                using var json = JsonDocument.Parse(response);

                if (json.RootElement.ValueKind != JsonValueKind.Array)
                    return (false, string.Empty, string.Empty);

                Version? currentVersion = Version.Parse("1.0"); //Assembly.GetEntryAssembly()?.GetName().Version;
                Version? highestVersion = currentVersion;
                string? bestTag = null;
                string? bestUrl = null;

                foreach (var release in json.RootElement.EnumerateArray())
                {
                    if (release.GetProperty("draft").GetBoolean()) continue;

                    string? remoteTag = release.GetProperty("tag_name").GetString();
                    // Convert old format to new format
                    if (remoteTag?.Length > 4) remoteTag = remoteTag.ReplaceLastOccurrence(".", "");
                    string? htmlUrl = release.GetProperty("html_url").GetString() ?? "";

                    if (Version.TryParse(remoteTag?.TrimStart('v'), out var parsedVersion))
                    {
                        // Is this version higher than anything we've seen so far?
                        if (parsedVersion > highestVersion)
                        {
                            // Double check it's not on the ignore list
                            if (ConfigService.IgnoredUpdateVersions.Contains(remoteTag)) continue;

                            highestVersion = parsedVersion;
                            bestTag = remoteTag;
                            bestUrl = htmlUrl;
                        }
                    }
                }

                // If highestVersion is still currentVersion, we found nothing new
                if (bestTag != null && highestVersion > currentVersion)
                {
                    return (true, bestTag, bestUrl ?? "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
            }

            return (false, string.Empty, string.Empty);
        }
    }
}

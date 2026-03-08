using Avalonia.Controls;
using qBittorrentCompanion.Helpers;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace qBittorrentCompanion.Services
{    
    public class UpdateService : ReactiveObject
    {
        public static string CurrentVersion =>
            Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "0.0.0";

        private static bool? _isVelopackInstalled = null;

        public static bool IsVelopackInstalled()
        {
            if (_isVelopackInstalled.HasValue)
                return _isVelopackInstalled.Value;

#if WINDOWS
            var assemblyDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
    
            // Check if parent exists and contains 'Update.exe'
            if (assemblyDir.Parent?.FullName is string parentPath)
            {
                string updateExe = Path.Combine(parentPath, "Update.exe");
                _isVelopackInstalled = File.Exists(updateExe);
            }
            else
            {
                _isVelopackInstalled = false;
            }
#else
            _isVelopackInstalled = false;
#endif

            return _isVelopackInstalled.Value;
        }

        private static readonly Lazy<UpdateService> _instance = new(() => new());
        public static UpdateService Instance => _instance.Value;

        /// <summary>
        /// Only want to show one notification for updates. 
        /// This is used to track which version is being shown to prevent showing another update for it.
        /// </summary>
        private string? _trackedUpdate = null;
        /// <summary>
        /// Clears <see cref="_trackedUpdate"/> so that <br/>
        /// <see cref="VelopackUpdateCheck"/><br/> and<br/> 
        /// <see cref="RegularUpdateCheck"/><br/>
        /// will show a notification again when an update is found.
        /// </summary>
        public void ClearTrackedUpdate()
        {
            _trackedUpdate = null;
        }

        private bool _checkForUpdates = Design.IsDesignMode || ConfigService.CheckForQbcUpdates;
        public bool CheckForUpdates
        { 
            get => _checkForUpdates;
            set
            {
                if (value != _checkForUpdates)
                {
                    _checkForUpdates = value;
                    ConfigService.CheckForQbcUpdates = value;
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
            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                TypeNameHelper.GetFullTypeName<UpdateService>(),
                Resources.Resources.UpdateService_StartingTimer
            );
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

                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    TypeNameHelper.GetFullTypeName<UpdateService>(), 
                    Resources.Resources.UpdateService_RequestUpdateTimerToStop
                );
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
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    TypeNameHelper.GetFullTypeName<UpdateService>(), 
                    Resources.Resources.UpdateService_UpdateTimerStopped
                );
            }
        }

        private async Task StartCheckingForUpdates()
        {
            if (!this.CheckForUpdates) return; // Use local property

            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                TypeNameHelper.GetFullTypeName<UpdateService>(),
                Resources.Resources.MainWindow_CheckingForUpdates
            );

            if (IsVelopackInstalled())
                VelopackUpdateCheck();
            else
                await RegularUpdateCheck();
        }



        private async void VelopackUpdateCheck()
        {
            try
            {
                var mgr = new UpdateManager(new GithubSource("https://github.com/Axeia/qBittorrentCompanion", null, false));
                var updateInfo = await mgr.CheckForUpdatesAsync();

                if (updateInfo != null)
                {
                    var versionString = updateInfo.TargetFullRelease.Version.ToNormalizedString();

                    // Prevent showing another notification if it's already being displayed.
                    if (versionString == _trackedUpdate)
                        return;

                    AppLoggerService.AddLogMessage(
                        LogLevel.Info,
                        TypeNameHelper.GetFullTypeName<UpdateService>(),
                        string.Format(Resources.Resources.UpdateService_UpdateFound, versionString)
                    );

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        NotificationService.Instance.NotifyUpdateAvailable(versionString, "", null);
                    });
                }
                else
                {
                    AppLoggerService.AddLogMessage(
                        LogLevel.Info,
                        TypeNameHelper.GetFullTypeName<UpdateService>(),
                        Resources.Resources.UpdateService_NoUpdateAvailable
                    );
                }
            }
            catch (Exception ex)
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Error,
                    TypeNameHelper.GetFullTypeName<UpdateService>(),
                    $"Velopack Check Failed",
                    ex.Message
                );
            }
        }

        private const string GitHubApiUrl = "https://api.github.com/repos/Axeia/qBittorrentCompanion/releases";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task RegularUpdateCheck()
        {
            string response = "";
            Version currentVersion = Assembly.GetEntryAssembly()?.GetName().Version!;
            Version latestVersion = currentVersion!;
            string? latestUrl = null;
            string? latestDownloadUrl = null;

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "qBittorrentCompanion-App");

                response = await client.GetStringAsync(GitHubApiUrl);
                using var json = JsonDocument.Parse(response);

                if (json.RootElement.ValueKind != JsonValueKind.Array)
                {
                    Debug.WriteLine(string.Format(Resources.Resources.UpdateService_UnexpectedResultFrom, GitHubApiUrl));
                    return;
                }

                foreach (var release in json.RootElement.EnumerateArray())
                {
                    if (release.GetProperty("draft").GetBoolean()) continue;

                    string remoteTag = release.GetProperty("tag_name").GetString()!; // e.g., "v0.0.1-Alpha"
                    string cleanTag = remoteTag.TrimStart('v'); // "0.0.1-Alpha"

                    // Split by '-' and take the first part to get "0.0.1" for numeric comparison
                    string numericPart = cleanTag.Split('-')[0];

                    // Skip anything using the old version numbering
                    // As they obviously wouldn't be an update
                    // but the higher numbers would be seen as one.
                    if (remoteTag.Count(c => c == '.') > 2)
                        continue;

                    if (Version.TryParse(numericPart, out var parsedVersion))
                    {
                        if (parsedVersion > latestVersion)
                        {
                            // Blacklisted - not interested
                            if (ConfigService.IgnoredUpdateVersions.Contains(remoteTag)) 
                                continue;
  
                            // Create the filename string of what we'd expect to find.
                            string platform = OperatingSystem.IsWindows() ? "win" : "linux";
                            string extension = OperatingSystem.IsWindows() ? ".zip" : ".tar.gz";
                            string fileNameToFind = $"qBittorrentCompanion-v{cleanTag}-{platform}-x64{extension}";

                            latestVersion = parsedVersion;
                            latestUrl = release.GetProperty("html_url").GetString() ?? "";

                            if (release.TryGetProperty("assets", out var assets))
                            {
                                foreach (var asset in assets.EnumerateArray())
                                {
                                    string assetName = asset.GetProperty("name").GetString() ?? "";

                                    // 3. MATCH DYNAMIC FILENAME
                                    if (assetName.Equals(fileNameToFind, StringComparison.OrdinalIgnoreCase))
                                    {
                                        latestDownloadUrl = asset.GetProperty("browser_download_url").GetString();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(Resources.Resources.UpdateService_RegularUpdateCheckFailed, ex.Message));
            }

            // Already showing an notification for this update, don't display another
            if (latestVersion.ToString() == _trackedUpdate)
                return;


            if (latestVersion > currentVersion)
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    TypeNameHelper.GetFullTypeName<UpdateService>(),
                    string.Format(Resources.Resources.UpdateService_UpdateFound, latestVersion)
                );
                NotificationService.Instance.NotifyUpdateAvailable(latestVersion.ToString(), latestUrl!, latestDownloadUrl);
            }
            else
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    TypeNameHelper.GetFullTypeName<UpdateService>(),
                    Resources.Resources.UpdateService_NoUpdateAvailable,
                    response
                );
            }
        }

        public static void ApplyWindowsUpdate(string downloadedZipFile)
        {
            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                TypeNameHelper.GetFullTypeName<UpdateService>(),
                Resources.Resources.UpdateService_LaunchingPowershellUpdater
            );
            string downloadedZipPath = Path.Combine(Path.GetTempPath(), downloadedZipFile);
            Debug.WriteLine("Call updated script with: " + downloadedZipPath);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "qBittorrentCompanion.Resources.UpdateScript.ps1";
            string scriptPath = Path.Combine(Path.GetTempPath(), "qbc_updater.ps1");

            // Write script to temp folder
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
            using (StreamReader reader = new(stream))
            {
                File.WriteAllText(scriptPath, reader.ReadToEnd());
            }

            // Prepare launch arguments
            string installDir = AppContext.BaseDirectory;
            int currentPid = Environment.ProcessId;

            string quotedScript = $"\"{scriptPath}\"";
            string quotedInstall = $"\"{installDir.TrimEnd('\\')}\"";
            string quotedZip = $"\"{downloadedZipPath}\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                // We use -Command to invoke the script file with its parameters
                Arguments = $"-ExecutionPolicy Bypass -Command \"& {quotedScript} -ParentPid {currentPid} -InstallPath {quotedInstall} -ZipPath {quotedZip}\"",
                CreateNoWindow = false,
                UseShellExecute = false
            };

            // Launch and close window
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        public static void ApplyLinuxUpdate(string downloadedZipFile)
        {
            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                TypeNameHelper.GetFullTypeName<UpdateService>(),
                "Launching Bash updater..."
            );

            string downloadedZipPath = Path.Combine(Path.GetTempPath(), downloadedZipFile);
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "qBittorrentCompanion.Resources.UpdateScript.sh";
            string scriptPath = Path.Combine(Path.GetTempPath(), "qbc_updater.sh");

            // Write script to temp folder
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
            using (StreamReader reader = new(stream))
            {
                File.WriteAllText(scriptPath, reader.ReadToEnd().Replace("\r\n", "\n"));
            }

            // Set execute permissions on the script itself
            try { Process.Start("chmod", $"+x \"{scriptPath}\"").WaitForExit(); }
            catch (Exception ex) { Debug.WriteLine("Failed to chmod script: " + ex.Message); }

            // Prepare launch arguments
            string installDir = AppContext.BaseDirectory;
            int currentPid = Environment.ProcessId;

            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\" {currentPid} \"{installDir}\" \"{downloadedZipPath}\"",
                CreateNoWindow = false,
                UseShellExecute = false
            };

            // Launch and close window
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}

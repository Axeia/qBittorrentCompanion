using AutoPropertyChangedGenerator;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public partial class LocalSearchPluginService : SearchPluginServiceBase
    {
        private static readonly Lazy<LocalSearchPluginService> _instance =
            new(() => new LocalSearchPluginService());
        public static LocalSearchPluginService Instance => _instance.Value;
        
        private string[] _nonSearchPluginPythonFileNames = [];
        public string[] NonSearchPluginPythonFileNames => _nonSearchPluginPythonFileNames;
        public Action<string[]>? NonPluginPythonFilesChanged { get; set; }

        public ObservableCollection<LocalSearchPluginViewModel> SearchPlugins { get; } = [
            new LocalSearchPluginViewModel(new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = DefaultCategories }, ""),
            new LocalSearchPluginViewModel(new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = DefaultCategories }, "")
        ];

        private readonly FileSystemWatcher? _pluginWatcher;
        private readonly DispatcherTimer _debounceTimer;
        /// <summary>
        /// /nova3/
        /// </summary>
        public static readonly string WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "nova3");
        /// <summary>
        /// /nova3/engines
        /// </summary>
        public static string SearchEngineDirectory => Path.Combine(WorkingDirectory, "engines");

        private LocalSearchPluginService()
        {
            _debounceTimer = new DispatcherTimer{ Interval = TimeSpan.FromMilliseconds(200) };
            _debounceTimer.Tick += async (sender, e) =>
            {
                _debounceTimer.Stop();
                await UpdateSearchPluginsAsync();
            };

            _pluginWatcher = new FileSystemWatcher(SearchEngineDirectory, "*.py")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            // Some of these can fire in quick succession, the debounce timer prevents things from going awry.
            _pluginWatcher.Created += PluginWatcher_Changed;
            _pluginWatcher.Deleted += PluginWatcher_Changed;
            _pluginWatcher.Changed += PluginWatcher_Changed;
            _pluginWatcher.Renamed += PluginWatcher_Changed;

            _ = InitializeAsync();
        }

        private void PluginWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Reset the timer on every change event
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public async Task InitializeAsync()
        {
            await UpdateSearchPluginsAsync();
        }

        public async Task UpdateSearchPluginsAsync()
        {
            while(SearchPlugins.Count > 2)
                SearchPlugins.RemoveAt(SearchPlugins.Count-1);

            List<LocalSearchPluginViewModel> nova2SearchPlugins = await PythonSearchBridge.GetSearchPluginsThroughNova2();
            List<string> DisabledSearchPluginNames = [.. ConfigService.DisabledLocalSearchPlugins];

            foreach (var searchPlugin in nova2SearchPlugins)
            {
                string filePath = Path.Combine(SearchEngineDirectory, searchPlugin.FileName);
                if (File.Exists(filePath))
                {
                    Version version = GetVersionFromSearchPluginFile(filePath);

                    // Update on UI thread to avoid cross-thread collection exceptions
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        searchPlugin.Version = version;
                        searchPlugin.IsEnabled = !DisabledSearchPluginNames.Contains(searchPlugin.Name);
                        SearchPlugins.Add(searchPlugin);
                    });
                }
                else
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Warn,
                        GetFullTypeName<LocalSearchPluginService>(),
                        $"{searchPlugin.Name}.py does not exist"
                    );
                }
            }

            var pythonFileNames = Directory.GetFiles(SearchEngineDirectory)
                .Where(f => Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f));
            var searchPluginFileNames = SearchPlugins.Skip(2).Select(sp => sp.FileName);
            var nonPluginPythonFileNames = pythonFileNames.Where(pfn=> !searchPluginFileNames.Contains(pfn)).ToArray();
            
            if(!nonPluginPythonFileNames.SequenceEqual(_nonSearchPluginPythonFileNames))
            {
                _nonSearchPluginPythonFileNames = nonPluginPythonFileNames;
                Debug.WriteLine("Invoke the invoker");
                NonPluginPythonFilesChanged?.Invoke(nonPluginPythonFileNames);
            }
        }

        private static Version GetVersionFromSearchPluginFile(string file)
        {
            var versionRegex = SearchPluginVersionRegex();
            var lines = File.ReadLines(file).Take(20);
            string version = "???";

            foreach (var line in lines)
            {
                var match = versionRegex.Match(line);
                if (match.Success)
                {
                    version = match.Groups[1].Value;
                    break;
                }
            }

            return new(version);
        }

        [GeneratedRegex(@"#\s*VERSION:\s*(\S+)")]
        private static partial Regex SearchPluginVersionRegex();

        public IEnumerable<string> PluginFilesAll
            => SearchPlugins.Select(sp=>sp.Name+".py");
        public IEnumerable<string> PluginFilesEnabled
            => SearchPlugins.Where(sp=>sp.IsEnabled).Select(sp => sp.Name + ".py");

        public void ClearOutNonPluginPyFiles()
        {
            foreach(string nonSearchPluginPythonFileName in _nonSearchPluginPythonFileNames)
            {
                string filePath = Path.Combine(SearchEngineDirectory, nonSearchPluginPythonFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}

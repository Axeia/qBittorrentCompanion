using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public partial class LocalSearchPluginService : IDisposable
    {
        private static readonly Lazy<LocalSearchPluginService> _instance =
            new(() => new LocalSearchPluginService());
        public static LocalSearchPluginService Instance => _instance.Value;
        
        private string[] _nonSearchPluginPythonFileNames = [];
        public string[] NonSearchPluginPythonFileNames => _nonSearchPluginPythonFileNames;
        public Action<string[]>? NonPluginPythonFilesChanged { get; set; }

        public ObservableCollection<LocalSearchPluginViewModel> SearchPlugins { get; } = [
            new LocalSearchPluginViewModel(new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = RemoteSearchPluginService.DefaultCategories }, ""),
            new LocalSearchPluginViewModel(new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = RemoteSearchPluginService.DefaultCategories }, "")
        ];

        private readonly DebouncedFileWatcher _debouncedWatcher;
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
            _debouncedWatcher = new DebouncedFileWatcher(SearchEngineDirectory, "*.py");
            _debouncedWatcher.ChangesReadyAsync += UpdateSearchPluginsAsync;

            _ = InitializeAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _debouncedWatcher.ChangesReadyAsync -= UpdateSearchPluginsAsync;
            _debouncedWatcher.Dispose();
        }

        public async Task InitializeAsync()
        {
            await UpdateSearchPluginsAsync();
        }

        public async Task UpdateSearchPluginsAsync()
        {
            while(SearchPlugins.Count > 2)
                SearchPlugins.RemoveAt(SearchPlugins.Count-1);

            List<SearchPlugin> nova2SearchPlugins = await PythonSearchBridge.GetSearchPluginsThroughNova2();
            List<LocalSearchPluginViewModel> localSearchPluginViewModels = [];
            List<string> DisabledSearchPluginNames = [.. ConfigService.DisabledLocalSearchPlugins];

            foreach (var searchPlugin in nova2SearchPlugins)
            {
                string fileName = searchPlugin.AdditionalData.Keys.First();
                string filePath = Path.Combine(SearchEngineDirectory, fileName);
                if (File.Exists(filePath))
                {
                    Version version = GetVersionFromSearchPluginFile(filePath);

                    // Update on UI thread to avoid cross-thread collection exceptions
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        searchPlugin.Version = version;
                        searchPlugin.IsEnabled = !DisabledSearchPluginNames.Contains(fileName);
                        SearchPlugins.Add(new LocalSearchPluginViewModel(searchPlugin, fileName));
                    });
                }
                else
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Warn,
                        GetFullTypeName<LocalSearchPluginService>(),
                        $"{fileName} does not exist"
                    );
                }
            }

            // Get the file names of all .py files in the search engine directory
            var pythonFileNames = Directory.GetFiles(SearchEngineDirectory)
                .Where(f => Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f));
            // Get file names of all search engine plugins
            var searchPluginFileNames = SearchPlugins.Skip(2).Select(sp => sp.FileName); // Skip 'All' and 'Enabled'
            // Create list of .py files that are not a search engine plugin
            var nonPluginPythonFileNames = pythonFileNames.Where(pfn=> !searchPluginFileNames.Contains(pfn)).ToArray();
            
            if(!nonPluginPythonFileNames.SequenceEqual(_nonSearchPluginPythonFileNames))
            {
                _nonSearchPluginPythonFileNames = nonPluginPythonFileNames;
                NonPluginPythonFilesChanged?.Invoke(nonPluginPythonFileNames);
            }
        }

        /// <summary>
        /// Checks the first 20 lines of the plugin file (from the filepath provided)
        /// and returns the version in it, or "???" if it couldn't be found.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static Version GetVersionFromSearchPluginFile(string filePath)
        {
            var versionRegex = SearchPluginVersionRegex();
            var lines = File.ReadLines(filePath).Take(20);

            foreach (var line in lines)
            {
                var match = versionRegex.Match(line);
                if (match.Success)
                    return new(match.Groups[1].Value);
            }

            return new("???");
        }

        [GeneratedRegex(@"#\s*VERSION:\s*(\S+)")]
        private static partial Regex SearchPluginVersionRegex();

        public IEnumerable<string> PluginFilesAll
            => SearchPlugins.Select(sp=>sp.Name+".py");
        public IEnumerable<string> PluginFilesEnabled
            => SearchPlugins.Where(sp=>sp.IsEnabled).Select(sp => sp.Name + ".py");

        /// <summary>
        /// If nova3 could not interact with it as a plugin it's not considered a plugin.
        /// Calling this method will delete such files
        /// </summary>
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

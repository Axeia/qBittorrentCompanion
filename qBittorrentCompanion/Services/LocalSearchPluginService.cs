using Avalonia.Threading;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
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
        public static string SearchEnginePath => Path.Combine("nova3", "engines");

        private LocalSearchPluginService()
        {
            // Observe folder for changes?
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            await UpdateSearchPluginsAsync();
        }

        public async Task UpdateSearchPluginsAsync()
        {
            List<SearchPlugin> nova2SearchPlugins = await PythonSearchBridge.GetSearchPluginsThroughNova2();

            foreach (var searchPlugin in nova2SearchPlugins)
            {
                string filePath = Path.Combine(SearchEnginePath, searchPlugin.Name + ".py");
                if (File.Exists(filePath))
                { 
                    Version version = GetVersionFromSearchPluginFile(filePath);

                    // Update on UI thread to avoid cross-thread collection exceptions
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        searchPlugin.Version = version;
                        SearchPlugins.Add(searchPlugin);
                    });
                }
                else
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Warn,
                        GetFullTypeName<LocalSearchPluginService>(),
                        $"nova2.py found {searchPlugin} but {searchPlugin.Name}.py does not exist"
                    );
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

        public IEnumerable<string> PluginFilesAll
            => SearchPlugins.Select(sp=>sp.Name+".py");
        public IEnumerable<string> PluginFilesEnabled
            => SearchPlugins.Where(sp=>sp.IsEnabled).Select(sp => sp.Name + ".py");

        [GeneratedRegex(@"#\s*VERSION:\s*(\S+)")]
        private static partial Regex SearchPluginVersionRegex();
    }
}

using Avalonia.Threading;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
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

        // Event that classes can subscribe to for notifications
        public event EventHandler? SearchPluginsUpdated;

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
            // Get the latest plugins from QBittorrent
            var pluginFiles = Directory.GetFiles(SearchEnginePath, "*.py");

            var versionRegex = SearchPluginVersionRegex();
            foreach (var file in pluginFiles)
            {
                var lines = File.ReadLines(file).Take(20);
                string version = "???";
                string className = Path.GetFileNameWithoutExtension(file);

                foreach (var line in lines)
                {
                    var match = versionRegex.Match(line);
                    if (match.Success)
                    {
                        version = match.Groups[1].Value;
                        break;
                    }
                }

                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchPlugins.Add(new SearchPlugin
                    {
                        FullName = className,
                        Name = className,
                        Version = new Version(version),
                        IsEnabled = true, // Default to enabled, can be toggled later
                        Categories = [new SearchPluginCategory(SearchPlugin.All, "All categories")]
                    });


                    // Notify subscribers
                    SearchPluginsUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
        }

        public IEnumerable<string> PluginFilesAll
            => SearchPlugins.Select(sp=>sp.Name+".py");
        public IEnumerable<string> PluginFilesEnabled
            => SearchPlugins.Where(sp=>sp.IsEnabled).Select(sp => sp.Name + ".py");

        [GeneratedRegex(@"#\s*VERSION:\s*(\S+)")]
        private static partial Regex SearchPluginVersionRegex();
    }
}

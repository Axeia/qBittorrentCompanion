using ReactiveUI;
using RssPlugins;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace qBittorrentCompanion.ViewModels
{
    public class RssPluginsViewModel : ViewModelBase
    {
        public RssPluginsViewModel()
        {
            Plugins.Add(new FossRssPlugin.FossRssPlugin(""));
            Plugins.Add(new SeriesRssPlugin.SeriesRssPlugin(""));

            //TODO fetch from SERVICE which one should be selected
            SelectedPlugin = Plugins.First();
        }

        private ObservableCollection<RssPluginBase> _plugins = [];
        public ObservableCollection<RssPluginBase> Plugins
        {
            get => _plugins;
            set => this.RaiseAndSetIfChanged(ref _plugins, value);
        }

        private RssPluginBase _selectedPlugin;
        public RssPluginBase SelectedPlugin
        {
            get => _selectedPlugin;
            set => this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
        }

        private void LoadPlugins()
        {
            Debug.WriteLine("Reloading plugins...");
            var pluginAssemblies = Directory.GetFiles("RssPlugins", "*.dll");
            Debug.WriteLine($"Found {pluginAssemblies.Count()} .dll files");

            foreach (var assemblyPath in pluginAssemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var pluginTypes = assembly.GetTypes().Where(t => typeof(RssPluginBase).IsAssignableFrom(t) && !t.IsInterface);
                foreach (var pluginType in pluginTypes)
                {
                    var pluginInstance = Activator.CreateInstance(pluginType, new object[] { "" }) as RssPluginBase;
                    if (pluginInstance != null)
                    {
                        // Add plugin to your collection or process it
                        Plugins.Add(pluginInstance);
                        Debug.WriteLine(pluginInstance.RuleTitle);
                    }
                }
            }

            Debug.WriteLine("Plugins reloaded");
        }
    }
}

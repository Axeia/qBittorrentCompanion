﻿using Avalonia.Controls;
using qBittorrentCompanion.Services;
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

            if (!Design.IsDesignMode)
                _selectedPlugin = Plugins.FirstOrDefault(
                    p => p.Name == ConfigService.LastSelectedRssPlugin,
                    Plugins.First()
                );
            else
                _selectedPlugin = Plugins.First();
        }

        private ObservableCollection<RssRulePluginBase> _plugins = [];
        public ObservableCollection<RssRulePluginBase> Plugins
        {
            get => _plugins;
            set => this.RaiseAndSetIfChanged(ref _plugins, value);
        }

        private RssRulePluginBase _selectedPlugin;
        public RssRulePluginBase SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                if (_selectedPlugin != null 
                    && value.Target == string.Empty 
                    && _selectedPlugin.Target != string.Empty)
                    value.RevalidateOn(_selectedPlugin.Target);

                this.RaiseAndSetIfChanged(ref _selectedPlugin!, value);
                if (!Design.IsDesignMode)
                    ConfigService.LastSelectedRssPlugin = _selectedPlugin.Name;
            }
        }

        private void LoadPlugins()
        {
            Debug.WriteLine("Reloading plugins...");
            var pluginAssemblies = Directory.GetFiles("RssPlugins", "*.dll");
            Debug.WriteLine($"Found {pluginAssemblies.Count()} .dll files");

            foreach (var assemblyPath in pluginAssemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var pluginTypes = assembly.GetTypes().Where(t => typeof(RssRulePluginBase).IsAssignableFrom(t) && !t.IsInterface);
                foreach (var pluginType in pluginTypes)
                {
                    var pluginInstance = Activator.CreateInstance(pluginType, new object[] { "" }) as RssRulePluginBase;
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class SearchPluginServiceBase
    {
        public static List<SearchPluginCategory> DefaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];

        // Observable collection that all views can bind to
        public ObservableCollection<SearchPlugin> SearchPlugins { get; } = [
            new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = DefaultCategories },
            new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = DefaultCategories }
        ];
    }
}

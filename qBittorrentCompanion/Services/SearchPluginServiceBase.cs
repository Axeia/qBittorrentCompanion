using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class SearchPluginServiceBase
    {
        public static bool UseRemoteSearch
        {
            get
            {
                // MainWindowViewModel is the gatekeeper and has it cached in a variable 
                // which should avoid unnecessary calls to ConfigService (which uses disk access)
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.DataContext is MainWindowViewModel mwvm)
                    return mwvm.UseRemoteSearch;

                // If we are in design mode, it doesn't really matter as long as we 
                // avoid accessing ConfigService (errors out the preview)
                return Design.IsDesignMode;
            }
            set
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.DataContext is MainWindowViewModel mwvm)
                    mwvm.UseRemoteSearch = value;
            }
        }

        public static List<SearchPluginCategory> DefaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];

        // Observable collection that all views can bind to
        public ObservableCollection<SearchPlugin> SearchPlugins { get; } = [
            new SearchPlugin() { FullName = "Only enabled", Name = SearchPlugin.Enabled, Categories = DefaultCategories },
            new SearchPlugin() { FullName = "All plugins", Name = SearchPlugin.All, Categories = DefaultCategories }
        ];
    }
}

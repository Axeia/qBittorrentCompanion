using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchPluginViewModel(SearchPlugin searchPlugin) : ReactiveObject
    {

        protected readonly SearchPlugin _searchPlugin = searchPlugin;
        public SearchPlugin SearchPlugin => _searchPlugin;

        public bool IsEnabled
        {
            get => _searchPlugin.IsEnabled;
            set
            {
                if(value != _searchPlugin.IsEnabled)
                {
                    IsProcessing = true;

                    _searchPlugin.IsEnabled = value;
                    this.RaisePropertyChanged(nameof(IsEnabled));

                    try
                    {
                        if (value)
                            QBittorrentService.EnableSearchPluginAsync(Name);
                        else
                            QBittorrentService.DisableSearchPluginAsync(Name);
                    }
                    catch (Exception)
                    {
                        AppLoggerService.AddLogMessage(
                            LogLevel.Warn,
                            GetFullTypeName<RemoteSearchPluginsViewModel>(),
                            $"Couldn't change enabled state for {Name}"
                        );
                        // Revert
                        _searchPlugin.IsEnabled = !value;
                    }
                    finally
                    {
                        IsProcessing = false;
                    }
                }
            }
        }

        [RaiseChange]
        private bool _isProcessing = false;

        public string FullName 
            => SearchPlugin.FullName ?? _searchPlugin.Name;
        public string Name 
            => _searchPlugin.Name;
        public IReadOnlyList<SearchPluginCategory> Categories 
            => _searchPlugin.Categories;
        public Uri Url 
            => _searchPlugin.Url;
        public string UrlShortened
            => _searchPlugin.Url == null ? string.Empty : _searchPlugin.Url.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? _searchPlugin.Url.Host[4..]
                : _searchPlugin.Url.Host;
        public Version Version 
            => _searchPlugin.Version;
    }
}

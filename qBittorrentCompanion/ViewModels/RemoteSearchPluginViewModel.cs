using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchPluginViewModel(SearchPlugin searchPlugin) : ReactiveObject
    {
        [AutoProxyPropertyChanged(nameof(SearchPlugin.IsEnabled))]
        protected readonly SearchPlugin _searchPlugin = searchPlugin;
        public SearchPlugin SearchPlugin => _searchPlugin;

        public string FullName 
            => _searchPlugin.FullName;
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

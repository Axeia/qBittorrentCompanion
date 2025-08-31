using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using ReactiveUI;
using System;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// LocalSearchPluginViewModel only differs from RemoteSearchPluginViewModel in two aspects.
    /// 1) It has a FileName property
    /// 2) Its version can be set after the fact
    /// <param name="searchPlugin"></param>
    /// <param name="fileName"></param>
    public partial class LocalSearchPluginViewModel(SearchPlugin searchPlugin, string fileName) : RemoteSearchPluginViewModel(searchPlugin)
    {
        [AutoPropertyChanged]
        private string _fileName = fileName;

        public new Version Version
        {
            get => _searchPlugin.Version;
            set
            {
                if (value != _searchPlugin.Version)
                {
                    _searchPlugin.Version = value;
                    this.RaisePropertyChanged(nameof(Version));
                }
            }
        }
    }
}

using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class SearchPluginViewModel : INotifyPropertyChanged
    {
        private SearchPlugin _searchPlugin;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SearchPluginViewModel(SearchPlugin searchPlugin)
        {
            _searchPlugin = searchPlugin;
        }

        public bool IsEnabled 
        { 
            get => _searchPlugin.IsEnabled; 
            set
            {
                if (value !=  _searchPlugin.IsEnabled)
                {
                    _searchPlugin.IsEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                    if (value == true)
                        _ = QBittorrentService.QBittorrentClient.EnableSearchPluginAsync(Name);
                    else
                        _ = QBittorrentService.QBittorrentClient.DisableSearchPluginAsync(Name);
                }
            }
        }
        public string FullName{ get => _searchPlugin.FullName; }
        public string Name { get => _searchPlugin.Name; }
        public IReadOnlyList<SearchPluginCategory> Categories { get => _searchPlugin.Categories; }
        public Uri Url { get => _searchPlugin.Url; }
        public Version Version { get => _searchPlugin.Version; }
    }
}

using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class SearchPluginViewModel(SearchPlugin searchPlugin) : INotifyPropertyChanged
    {
        private readonly SearchPlugin _searchPlugin = searchPlugin;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                        _ = QBittorrentService.EnableSearchPluginAsync(Name);
                    else
                        _ = QBittorrentService.DisableSearchPluginAsync(Name);
                }
            }
        }
        public string FullName 
            => _searchPlugin.FullName;
        public string Name 
            => _searchPlugin.Name;
        public IReadOnlyList<SearchPluginCategory> Categories 
            => _searchPlugin.Categories;
        public Uri Url 
            => _searchPlugin.Url;
        public Version Version 
            => _searchPlugin.Version;
    }
}

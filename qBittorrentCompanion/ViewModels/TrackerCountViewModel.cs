using Newtonsoft.Json.Linq;
using qBittorrentCompanion.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace qBittorrentCompanion.ViewModels
{
    public class TrackerCountViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TrackerCountViewModel(string url, int count)
        {
            _url = url;
            _count = count;
            SetDisplayUrl();
        }

        private string _displayUrl = "";
        public string DisplayUrl
        {
            get => _displayUrl;
        }

        private string _url = "";
        public string Url
        {
            get => _url;
            set 
            {
                if (value != _url)
                {
                    _url = value;
                    OnPropertyChanged();
                    SetDisplayUrl();
                    OnPropertyChanged(nameof(DisplayUrl));
                }
            }
        }

        private void SetDisplayUrl()
        {
            if (_url == "All" || _url == "Trackerless")
                _displayUrl = _url;
            else
            {
                Uri uri = new Uri(_url);
                _displayUrl = uri.Host;
            }
        }

        private int _count = 0;
        public int Count
        {
            get => _count;
            set
            {
                if (value != _count) 
                {
                    _count = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}

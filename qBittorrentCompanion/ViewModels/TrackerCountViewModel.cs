using RaiseChangeGenerator;
using ReactiveUI;
using System;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TrackerCountViewModel : ReactiveObject
    {

        [RaiseChange]
        private int _count = 0;

        public TrackerCountViewModel(string url, int count)
        {
            _url = url;
            _count = count;
            SetDisplayUrl();
        }

        private string _displayUrl = "";
        public string DisplayUrl => _displayUrl;

        private string _url = "";
        public string Url
        {
            get => _url;
            set 
            {
                if (value != _url)
                {
                    _url = value;
                    this.RaisePropertyChanged(nameof(Url));
                    SetDisplayUrl();
                    this.RaisePropertyChanged(nameof(DisplayUrl));
                }
            }
        }

        private void SetDisplayUrl()
        {
            if (_url == "All" || _url == "Trackerless")
                _displayUrl = _url;
            else
            {
                Uri uri = new(_url);
                _displayUrl = uri.Host;
            }
        }
    }
}

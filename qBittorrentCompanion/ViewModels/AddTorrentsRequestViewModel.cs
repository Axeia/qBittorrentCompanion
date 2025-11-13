using QBittorrent.Client;
using System;
using System.Collections.ObjectModel;

namespace qBittorrentCompanion.ViewModels
{
    public partial class AddTorrentsRequestViewModel : ViewModelBase
    {
        private AddTorrentsRequest _addTorrentsRequest;

        public AddTorrentsRequestViewModel(AddTorrentsRequest addTorrentsRequest)
        {
            _addTorrentsRequest = addTorrentsRequest;

            foreach (string filePath in addTorrentsRequest.TorrentFiles)
                TorrentFiles.Add(filePath);
            foreach (Uri uri in addTorrentsRequest.TorrentUrls)
                TorrentUrls.Add(uri);
        }

        public ObservableCollection<string> TorrentFiles = [];
        public ObservableCollection<Uri> TorrentUrls = [];

        public string? DownloadFolder = null;
    }
}

using Avalonia;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class TorrentTrackerViewModel : INotifyPropertyChanged
    {
        private TorrentTracker _torrentTracker;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public TorrentTrackerViewModel(TorrentTracker torrentTracker)
        {
            _torrentTracker = torrentTracker;
        }

        public string Message
        {
            get => _torrentTracker.Message;
            set
            {
                if (value != _torrentTracker.Message)
                {
                    _torrentTracker.Message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        public int? CompletedDownloads
        {
            get => _torrentTracker.CompletedDownloads;
            set
            {
                if (value != _torrentTracker.CompletedDownloads)
                {
                    _torrentTracker.CompletedDownloads = value;
                    OnPropertyChanged(nameof(CompletedDownloads));
                }
            }
        }

        public int? Leeches
        {
            get => _torrentTracker.Leeches;
            set
            {
                if (value != _torrentTracker.Leeches)
                {
                    _torrentTracker.Leeches = value;
                    OnPropertyChanged(nameof(Leeches));
                }
            }
        }

        public int? Peers
        {
            get => _torrentTracker.Peers;
            set
            {
                if (value != _torrentTracker.Peers)
                {
                    _torrentTracker.Peers = value;
                    OnPropertyChanged(nameof(Peers));
                }
            }
        }

        public int? Seeds
        {
            get => _torrentTracker.Seeds;
            set
            {
                if (value != _torrentTracker.Seeds)
                {
                    _torrentTracker.Seeds = value;
                    OnPropertyChanged(nameof(Seeds));
                }
            }
        }

        public string Status
        {
            get => _torrentTracker.Status;
            set
            {
                if (value != _torrentTracker.Status)
                {
                    _torrentTracker.Status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public int? Tier
        {
            get => _torrentTracker.Tier;
            set
            {
                if (value != _torrentTracker.Tier)
                {
                    _torrentTracker.Tier = value;
                    OnPropertyChanged(nameof(Tier));
                }
            }
        }

        public System.Uri Url
        {
            get => _torrentTracker.Url;
            set
            {
                if (value != _torrentTracker.Url)
                {
                    _torrentTracker.Url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        public void Update(TorrentTracker torrentTracker)
        {
            Message = torrentTracker.Message;
            CompletedDownloads = torrentTracker.CompletedDownloads;
            Leeches = torrentTracker.Leeches;
            Peers = torrentTracker.Peers;
            Seeds = torrentTracker.Seeds;
            Status = torrentTracker.Status;
            Tier = torrentTracker.Tier;
            Url = torrentTracker.Url;
        }
    }


}

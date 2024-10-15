using QBittorrent.Client;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class EditTorrentTrackerViewModel : TorrentTrackerViewModel
    {
        private bool _isNew = false;
        public bool IsNew
        {
            get => _isNew;
            set
            {
                if(value != _isNew)
                {
                    _isNew = value;
                    OnPropertyChanged(nameof(IsNew));
                }
            }
        }
        public EditTorrentTrackerViewModel(TorrentTracker torrentTracker, bool isNew = false) : base(torrentTracker)
        {
            _newTier = torrentTracker.Tier ?? default;
            _newUrl = torrentTracker.Url.ToString();
            _isNew = isNew;
        }

        private bool _deleteMe = false;
        public bool DeleteMe
        {
            get => _deleteMe;
            set
            {
                if (value != _deleteMe)
                {
                    _deleteMe = value;
                    OnPropertyChanged(nameof(DeleteMe));
                }
            }
        }

        private int _newTier = 0;
        public int NewTier
        {
            get => _newTier;
            set
            {
                if (value != _newTier)
                {
                    _newTier = value;
                    OnPropertyChanged(nameof(NewTier));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        private string _newUrl = "";
        public string NewUrl
        {
            get => _newUrl;
            set
            {
                if (value != _newUrl)
                {
                    _newUrl = value;
                    OnPropertyChanged(nameof(Url));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        private bool _isModified = false;
        public bool IsModified
        {
            get => NewUrl != Url.ToString() || NewTier != Tier;
        }

        private bool _isInProgress = true;
        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if(value != _isInProgress)
                {
                    _isInProgress = value;
                    OnPropertyChanged(nameof(IsInProgress));
                }
            }
        }
        

        public new void Update(TorrentTracker torrentTracker)
        {
            base.Update(torrentTracker);
        }
    }
}

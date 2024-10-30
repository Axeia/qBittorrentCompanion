using QBittorrent.Client;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using qBittorrentCompanion.Validators;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class TorrentTrackerViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private TorrentTracker _torrentTracker;

        public class ErrorMessage
        {
            private string _message;
            public string Message
            {
                get => _message;
                private set => _message = value;
            }
            public ErrorMessage(string message)
            {
                _message = message;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
                    OnPropertyChanged(nameof(IsEditable));
                }
            }
        }

        public bool IsEditable => Tier > -1;

        [Required]
        [ValidTrackerUrl]
        public Uri Url
        {
            get => _torrentTracker.Url;
            set
            {
                if (value != _torrentTracker.Url)
                {
                    _torrentTracker.Url = value;
                    OnPropertyChanged(nameof(Url));
                    ValidateProperty(value, nameof(Url)); // Re-validate on change.
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


        // Error handling
        private readonly Dictionary<string, List<string>> _errors = new();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private readonly ObservableCollection<ErrorMessage> _urlErrors = new();
        public ObservableCollection<ErrorMessage> UrlErrors => _urlErrors;

        public bool HasErrors => _errors.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            return _errors.ContainsKey(propertyName) 
                ? _errors[propertyName] 
                : Enumerable.Empty<string>();
        }

        private void ValidateProperty(object value, string propertyName)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, results))
                _errors[propertyName] = results.Select(r => r.ErrorMessage).ToList();
            else
                _errors.Remove(propertyName);

            OnErrorsChanged(propertyName);

            if (propertyName == nameof(Url))
            {
                UrlErrors.Clear();
                if (_errors.TryGetValue(nameof(Url), out var errors))
                    errors.ToList().ForEach(e => UrlErrors.Add(new ErrorMessage(e)));
            }

            OnPropertyChanged(nameof(HasErrors));
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
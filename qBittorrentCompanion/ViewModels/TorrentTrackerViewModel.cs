using QBittorrent.Client;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using qBittorrentCompanion.Validators;
using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public partial class TorrentTrackerViewModel(TorrentTracker torrentTracker) : ViewModelBase, INotifyDataErrorInfo
    {
        [AutoProxyPropertyChanged(nameof(TorrentTracker.Message))]
        [AutoProxyPropertyChanged(nameof(TorrentTracker.CompletedDownloads))]
        [AutoProxyPropertyChanged(nameof(TorrentTracker.Leeches))]
        [AutoProxyPropertyChanged(nameof(TorrentTracker.Peers))]
        [AutoProxyPropertyChanged(nameof(TorrentTracker.Seeds))]
        [AutoProxyPropertyChanged(nameof(TorrentTracker.Status))]
        private readonly TorrentTracker _torrentTracker = torrentTracker;

        public class ErrorMessage(string message)
        {
            private string _message = message;
            public string Message
            {
                get => _message;
                private set => _message = value;
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
                    this.RaisePropertyChanged(nameof(Tier));
                    this.RaisePropertyChanged(nameof(IsEditable));
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
                    this.RaisePropertyChanged(nameof(Url));
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
        private readonly Dictionary<string, List<string>> _errors = [];
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private readonly ObservableCollection<ErrorMessage> _urlErrors = [];
        public ObservableCollection<ErrorMessage> UrlErrors => _urlErrors;

        public bool HasErrors => _errors.Count != 0;

        public IEnumerable GetErrors(string propertyName) => 
            _errors.TryGetValue(propertyName, out List<string>? value)
                ? value 
                : Enumerable.Empty<string>();

        private void ValidateProperty(object value, string propertyName)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(value, context, results))
                _errors[propertyName] = [.. results.Select(r => r.ErrorMessage)];
            else
                _errors.Remove(propertyName);

            OnErrorsChanged(propertyName);

            if (propertyName == nameof(Url))
            {
                UrlErrors.Clear();
                if (_errors.TryGetValue(nameof(Url), out var errors))
                    errors.ToList().ForEach(e => UrlErrors.Add(new ErrorMessage(e)));
            }

            this.RaisePropertyChanged(nameof(HasErrors));
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
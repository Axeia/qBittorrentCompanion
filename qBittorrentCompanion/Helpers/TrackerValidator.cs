using qBittorrentCompanion.Validators;
using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers
{
    public partial class TrackerValidator : ReactiveObject
    {
        public TrackerValidator(string url, int tier)
        {
            Url = url;
            _tier = tier;
        }

        [RaiseChange]
        private int _tier;
        [RaiseChange]
        private bool _isValid = true;
        [RaiseChange]
        private string _errorMessage = string.Empty;
        [RaiseChange]
        private bool _isSharedTier = false;
        [RaiseChange]
        private bool _isTierJump = false;

        private string _url = string.Empty;
        public string Url
        {
            get => _url;
            set
            {
                if (value != _url)
                {
                    ErrorMessage = ValidTrackerUrlAttribute.GetTrackerValidationText(value);
                    IsValid = ErrorMessage == string.Empty;
                    _url = value;
                    this.RaisePropertyChanged(nameof(Url));
                }
            }
        }
    }
}

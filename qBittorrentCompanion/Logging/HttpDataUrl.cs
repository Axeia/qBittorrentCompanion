using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Logging
{
    public partial class HttpDataUrl(string url, LinkDocInfo linkDocInfo) : ReactiveObject
    {
        private readonly string _url = url;
        public string Url => _url;

        private readonly LinkDocInfo _linkDocInfo = linkDocInfo;
        public LinkDocInfo LinkDocInfo => _linkDocInfo;

        [RaiseChange]
        private bool _isChecked = true;

        private int _count = 1;
        public int Count => _count;
        public void IncreaseCount()
        {
            _count++;
            this.RaisePropertyChanged(nameof(Count));
        }
    }
}

using RaiseChangeGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.Helpers.Preferences
{
    public partial class CustomHttpHeader(string header) : ReactiveObject
    {
        [RaiseChange]
        private string _header = header;
    }
}

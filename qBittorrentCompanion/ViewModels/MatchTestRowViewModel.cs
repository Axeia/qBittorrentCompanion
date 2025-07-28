using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public partial class MatchTestRowViewModel : RssRuleIsMatchViewModel
    {
        [AutoPropertyChanged]
        private string _matchTest = "";
    }
}

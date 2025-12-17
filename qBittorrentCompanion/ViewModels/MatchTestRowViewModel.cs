using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class MatchTestRowViewModel : RssRuleIsMatchViewModel
    {
        [RaiseChange]
        private string _matchTest = "";
    }
}

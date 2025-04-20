using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public class MatchTestRowViewModel : RssRuleIsMatchViewModel
    {
        private string _matchTest = "";
        public string MatchTest
        {
            get => _matchTest;
            set => this.RaiseAndSetIfChanged(ref _matchTest, value);
        }
    }
}

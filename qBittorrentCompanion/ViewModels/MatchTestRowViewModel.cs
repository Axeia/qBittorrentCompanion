namespace qBittorrentCompanion.ViewModels
{
    public class MatchTestRowViewModel : RssRuleIsMatchViewModel
    {
        private string _matchTest = "";
        public string MatchTest
        {
            get => _matchTest;
            set
            {
                if (value != _matchTest)
                {
                    _matchTest = value;
                    OnPropertyChanged(nameof(MatchTest));
                }
            }
        }
    }
}

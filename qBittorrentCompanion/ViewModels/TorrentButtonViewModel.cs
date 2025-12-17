using RaiseChangeGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentButtonViewModel : ViewModelBase
    {
        public static string[] Actions => ["Download now", "Add paused"];

        [RaiseChange]
        private string _selectedAction = Actions[0];
    }
}
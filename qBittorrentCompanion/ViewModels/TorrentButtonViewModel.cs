using AutoPropertyChangedGenerator;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentButtonViewModel : ViewModelBase
    {
        public static string[] Actions => ["Download now", "Add paused"];

        [AutoPropertyChanged]
        private string _selectedAction = Actions[0];
    }
}
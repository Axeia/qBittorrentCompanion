using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentButtonViewModel : ViewModelBase
    {
        public static string[] Actions => ["Download now", "Add paused"];

        private string _selectedAction = Actions[0];
        public string SelectedAction
        {
            get => _selectedAction;
            set => this.RaiseAndSetIfChanged(ref _selectedAction, value);
        }
    }
}
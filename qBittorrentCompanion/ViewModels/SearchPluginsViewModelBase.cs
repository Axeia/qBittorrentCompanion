using AutoPropertyChangedGenerator;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public abstract partial class SearchPluginsViewModelBase : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> UninstallSearchPluginCommand { get; set; }
        public bool IsPopulating { get; set; } = false;

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        // Surpressing non-nullable warnings, enheriting class should set it.
        // This is regarding UninstalSearchPluginCommand and ToggleEnabledSearchPluginCommand
#pragma warning disable CS8618 
        public SearchPluginsViewModelBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(Initialise);
        }

        [AutoPropertyChanged]
        private ObservableCollection<RemoteSearchPluginViewModel> _searchPlugins = [];
        [AutoPropertyChanged]
        private RemoteSearchPluginViewModel? _selectedSearchPlugin = null;

        protected abstract Task FetchDataAsync();

        public Task Initialise()
        {
            return FetchDataAsync();
        }
    }
}

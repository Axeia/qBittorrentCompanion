using AutoPropertyChangedGenerator;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public abstract partial class SearchPluginsViewModelBase : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> UninstallSearchPluginCommand { get; }
        public bool IsPopulating { get; set; } = false;

        protected abstract Task<Unit> UninstallSearchPluginAsync(Unit unit);

        public ReactiveCommand<bool, Unit> ToggleEnabledSearchPluginCommand { get; }
        protected abstract Task<Unit> ToggleEnabledSearchPluginAsync(bool enable);

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public SearchPluginsViewModelBase()
        {
            UninstallSearchPluginCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(UninstallSearchPluginAsync);
            ToggleEnabledSearchPluginCommand = ReactiveCommand.CreateFromTask<bool, Unit>(ToggleEnabledSearchPluginAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(Initialise);
        }

        [AutoPropertyChanged]
        private ObservableCollection<SearchPluginViewModel> _searchPlugins = [];
        [AutoPropertyChanged]
        private SearchPluginViewModel? _selectedSearchPlugin = null;

        protected abstract Task FetchDataAsync();

        public Task Initialise()
        {
            return FetchDataAsync();
        }
    }
}

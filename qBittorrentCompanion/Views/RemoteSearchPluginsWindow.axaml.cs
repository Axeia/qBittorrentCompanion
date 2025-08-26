using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class RemoteSearchPluginsWindow : EscIcoWindow
    {
        private TypeToSelectDataGridHelper<SearchPluginViewModel>? _searchHelper;

        public RemoteSearchPluginsWindow()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                this.DataContext = new RemoteSearchPluginsViewModel();
            else
                this.DataContext = ConfigService.UseRemoteSearch
                    ? new RemoteSearchPluginsViewModel()
                    : new LocalSearchPluginsViewModel();

            Loaded += SearchPluginsWindow_Loaded;
        }

        private void SearchPluginsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase searchPluginsViewModel)
            {
                searchPluginsViewModel.Initialise();
            }

            _searchHelper = new TypeToSelectDataGridHelper<SearchPluginViewModel>(SearchPluginsDataGrid, "Name");
        }
    }
}

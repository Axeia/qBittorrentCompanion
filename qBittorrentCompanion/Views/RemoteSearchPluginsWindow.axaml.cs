using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class RemoteSearchPluginsWindow : EscIcoWindow
    {
        private TypeToSelectDataGridHelper<RemoteSearchPluginViewModel>? _searchHelper;

        public RemoteSearchPluginsWindow()
        {
            InitializeComponent();

            this.DataContext = new RemoteSearchPluginsViewModel();
            Loaded += SearchPluginsWindow_Loaded;
        }

        private void SearchPluginsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase searchPluginsViewModel)
            {
                searchPluginsViewModel.Initialise();
            }

            _searchHelper = new TypeToSelectDataGridHelper<RemoteSearchPluginViewModel>(SearchPluginsDataGrid, "Name");
        }
    }
}

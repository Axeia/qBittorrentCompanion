using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class SearchPluginsWindow : Window
    {
        private TypeToSelectDataGridHelper<SearchPluginViewModel>? _searchHelper;

        public SearchPluginsWindow()
        {
            InitializeComponent();

            this.DataContext = new SearchPluginsViewModel();
            Loaded += SearchPluginsWindow_Loaded;
        }

        private void SearchPluginsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModel searchPluginsViewModel)
            {
                searchPluginsViewModel.Initialise();
            }

            _searchHelper = new TypeToSelectDataGridHelper<SearchPluginViewModel>(SearchPluginsDataGrid, "Name");
        }
    }
}

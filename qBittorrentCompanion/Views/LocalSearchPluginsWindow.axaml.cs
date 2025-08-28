using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class LocalSearchPluginsWindow : EscIcoWindow
    {
        private TypeToSelectDataGridHelper<SearchPluginViewModel>? _searchHelper;

        public LocalSearchPluginsWindow()
        {
            InitializeComponent();
            LocalSearchPluginsViewModel dc = new();

            if (Design.IsDesignMode)
            {
                dc.SearchPlugins.Add(
                    new SearchPluginViewModel(new QBittorrent.Client.SearchPlugin()
                    {
                        Name = "Preview plugin",
                        Version = new System.Version("1.0.0"),
                        Url = new System.Uri("https://github.com/Axeia/qBittorrentCompanion")
                    })
                );

                dc.GitPublicSearchPlugins.Add(
                    new GitSearchPluginViewModel("Torrent Search", "Axeia", "2.0.0", "19/Sept 2025", "https://github.com/Axeia/qBittorrentCompanion/", "", "Qbt 4.4.x / Python 3.9")
                );
            }

            this.DataContext = dc;

            Loaded += SearchPluginsWindow_Loaded;
        }

        private void SearchPluginsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _searchHelper = new TypeToSelectDataGridHelper<SearchPluginViewModel>(SearchPluginsDataGrid, "Name");
        }

        private void GithubTabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is LocalSearchPluginsViewModel lspvm)
                GithubPluginsDataGrid.ItemsSource = GithubTabStrip.SelectedIndex == 0 
                    ? lspvm.GitPublicSearchPlugins
                    : lspvm.GitPrivateSearchPlugins;
        }

        private void LaunchDownloadUriButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is LocalSearchPluginsViewModel lspvm 
                && TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(lspvm.SelectedGitSearchPlugin!.DownloadUri);
        }

        private void LaunchInfoUrlButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is LocalSearchPluginsViewModel lspvm
                && TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(lspvm.SelectedGitSearchPlugin!.InfoUri!);
        }

        private void LaunchWikiButton_Click(object? sender, RoutedEventArgs e)
        {
            if ( TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(new System.Uri(LocalSearchPluginsViewModel.SearchPluginWikiLink));
        }
    }
}

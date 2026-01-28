using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class SearchPluginsControl : UserControl
    {
        public SearchPluginsControl()
        {
            InitializeComponent();
            this.DataContextChanged += ViewModel_Changed;
            ViewModel_Changed(null, EventArgs.Empty);

            if (Design.IsDesignMode)
            {
                //var dc = new RemoteSearchPluginsViewModel();
                var dc = new LocalSearchPluginsViewModel();
                dc.SearchPlugins.Add(
                    new RemoteSearchPluginViewModel(new SearchPlugin()
                    {
                        Name = "Preview plugin",
                        Version = new Version("1.0.0"),
                        Url = new Uri("https://github.com/Axeia/qBittorrentCompanion")
                    })
                );

                dc.GitPublicSearchPlugins.Add(
                    new GitSearchPluginViewModel("Torrent Search", "Axeia", "2.0.0", "19/Sept 2025", "https://github.com/Axeia/qBittorrentCompanion/", "https://github.com/Axeia/qBittorrentCompanion/", "Qbt 4.4.x / Python 3.9")
                );

                dc.SelectedGitHubSearchPlugin = dc.GitPublicSearchPlugins[0];

                DataContext = dc;
            }
        }

        private void ViewModel_Changed(object? sender, EventArgs e)
        {
            if (this.DataContext is SearchPluginsViewModelBase sp)
            {
                sp
                    .WhenAnyValue(sp => sp.ShowGitHubSearchPluginDetailLabelText)
                    .ObserveOn(AvaloniaScheduler.Instance)
                    .Subscribe(_ =>
                    {
                        // Trigger remeasuring of the label/icons grid so it resizes as expected
                        Grid.SetIsSharedSizeScope(SearchPluginDetailsStackPanel, false);
                        Grid.SetIsSharedSizeScope(SearchPluginDetailsStackPanel, true);
                    });
            }
        }

        private void LaunchDownloadUriButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase spvmb
                && TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(spvmb.SelectedGitHubSearchPlugin!.DownloadUri);
        }

        private void LaunchInfoUrlButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase spvmb
                && TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(spvmb.SelectedGitHubSearchPlugin!.InfoUri!);
        }

        private void LaunchWikiButton_Click(object? sender, RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(new Uri(SearchPluginsViewModelBase.SearchPluginWikiLink));
        }

        private void OpenSearchPluginDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase spvmb
                && spvmb.SelectedSearchPlugin is LocalSearchPluginViewModel searchPluginVm)
            {
                PlatformAgnosticLauncher.LaunchDirectoryAndSelectFile(searchPluginVm.FileName);
            }
            else if (TopLevel.GetTopLevel(this) is TopLevel topLevel)
            {
                topLevel.Launcher.LaunchDirectoryInfoAsync(new System.IO.DirectoryInfo(LocalSearchPluginService.SearchEngineDirectory));
            }
        }

        private void PublicPrivateComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SearchPluginsViewModelBase lspvm)
                GithubPluginsDataGrid.ItemsSource = PublicPrivateComboBox.SelectedIndex == 0
                    ? lspvm.GitPublicSearchPlugins
                    : lspvm.GitPrivateSearchPlugins;
        }

        private void AddUriToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                HeaderTemplateContentPresenter
                    .GetVisualDescendants()
                    .OfType<TextBox>()
                    .Where(tb => tb.Name == "AddSearchPluginUriTextBox")
                    .First()
                    ?.Focus();
            }, DispatcherPriority.Input);
        }
    }
}
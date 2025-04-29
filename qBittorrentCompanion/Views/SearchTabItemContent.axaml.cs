using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views;

public partial class SearchTabItemContent : RssRulePluginUserControl
{
    public SearchTabItemContent()
    {
        this.DataContext = new SearchViewModel();
        InitializeComponent();
        //this.DataContext = new SearchViewModel();
        Loaded += SearchTabItemContent_Loaded;
    }

    private void SearchTabItemContent_Loaded(object? sender, RoutedEventArgs e)
    {
        CreateRuleButton.GenerateRssRuleSplitButton.Click += GenerateRssRuleSplitButton_Click;
    }

    private void SearchPluginButton_Click(object? sender, RoutedEventArgs e)
    {
        var searchPluginsWindow = new SearchPluginsWindow();

        var mw = this.FindAncestorOfType<MainWindow>();
        if (mw != null)
            searchPluginsWindow.ShowDialog(mw);
    }

    private void SearchToggleButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel)
        {
            searchViewModel.EndSearch();
        }
    }

    private void SearchToggleButton_Unchecked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel)
        {
            searchViewModel.StartSearch();
        }
    }

    private void SearchQueryTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            SearchToggleButton.IsChecked = !SearchToggleButton.IsChecked;
    }

    private void SearchResultDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Control control
            && control.DataContext is SearchResult searchResult
            && this.FindAncestorOfType<MainWindow>() is MainWindow mainWindow)
        {
            mainWindow.AddTorrent(searchResult.FileUrl, searchResult.FileName);
        }
    }

    private void CopyNameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel 
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult)
        {
            _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(selectedSearchResult.FileName);
        }
    }

    private void CopyLinkMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel 
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult)
        {
            _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(selectedSearchResult.FileUrl.ToString());
        }
    }

    private void CopyDownloadLinkMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult)
        {
            _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(selectedSearchResult.FileUrl.ToString());
        }
    }

    private void CopyDescriptionPageUrlMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult)
        {
            _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(selectedSearchResult.DescriptionUrl.ToString());
        }
    }

    private void DownloadMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel 
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult
            && this.FindAncestorOfType<MainWindow>() is MainWindow mainWindow)
        {
            mainWindow.AddTorrent(selectedSearchResult.FileUrl, selectedSearchResult.FileName);
        }
    }

    private void OpenDescriptionPageMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SearchViewModel searchViewModel
            && searchViewModel.SelectedSearchResult is SearchResult selectedSearchResult
            && this.FindAncestorOfType<MainWindow>() is MainWindow mainWindow)
        {
            TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(selectedSearchResult.DescriptionUrl); 
        }
    }

    private void Expander_Expanded(object? sender, RoutedEventArgs e)
    {
        var height = ExpanderContentDockPanel.MinHeight + 36;
        RightGrid.RowDefinitions[2].Height = new GridLength(height);
        RightGrid.RowDefinitions[2].MinHeight = height;
        RightGrid.RowDefinitions[2].MaxHeight = double.PositiveInfinity;
    }

    private void Expander_Collapsed(object? sender, RoutedEventArgs e)
    {
        RightGrid.RowDefinitions[2].Height = new GridLength(36);
        RightGrid.RowDefinitions[2].MinHeight = 36;
        RightGrid.RowDefinitions[2].MaxHeight = 36;
    }
}
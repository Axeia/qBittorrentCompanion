using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views;

public partial class SearchTabItemContent : UserControl
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
        SetBindings();
    }

    /// <summary>
    /// Clear previous bindings if any to avoid conflicts
    /// </summary>
    /// <param name="comboBox"></param>
    private void ClearComboBoxValues(ComboBox comboBox)
    {
        comboBox.ClearValue(ComboBox.ItemsSourceProperty);
        comboBox.ClearValue(ComboBox.SelectedItemProperty);
        comboBox.ClearValue(Control.DataContextProperty);
    }

    private void SetBindings()
    {
        if (DataContext is SearchViewModel searchVm)
        {
            SearchQueryTextBox.ClearValue(Control.DataContextProperty);
            SearchQueryTextBox.DataContext = searchVm;
            SearchQueryTextBox.Bind(TextBox.TextProperty, new Binding
            {
                Path = "SearchQuery",
                Mode = BindingMode.TwoWay,
                Source = searchVm
            });

            ClearComboBoxValues(SearchPluginsComboBox);
            SearchPluginsComboBox.DataContext = searchVm;
            SearchPluginsComboBox.ItemsSource = searchVm.SearchPlugins;
            SearchPluginsComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
            {
                Path = "SelectedSearchPlugin",
                Mode = BindingMode.TwoWay,
                Source = searchVm
            });
            SearchPluginsComboBox.DisplayMemberBinding = new Binding(nameof(SearchPlugin.FullName));


            ClearComboBoxValues(SearchPluginCategoriesComboBox);
            SearchPluginCategoriesComboBox.DataContext = searchVm;
            SearchPluginCategoriesComboBox.ItemsSource = searchVm.PluginCategories;
            SearchPluginCategoriesComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
            {
                Path = "SelectedSearchPluginCategory",
                Mode = BindingMode.TwoWay,
                Source = searchVm
            });
            SearchPluginCategoriesComboBox.DisplayMemberBinding = new Binding(nameof(SearchPluginCategory.Name));
        }
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
}
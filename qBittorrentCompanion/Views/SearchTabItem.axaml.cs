using Avalonia.Controls;
using Avalonia.VisualTree;
using System;

namespace qBittorrentCompanion.Views;

public partial class SearchTabItem : TabItem
{
    public SearchTabItem()
    {
        InitializeComponent();
        SearchTabItemContent.Loaded += SearchTabItemContent_Loaded;
    }

    private void SearchTabItemContent_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DataContext = SearchTabItemContent.DataContext;
    }

    //Invisible without this
    protected override Type StyleKeyOverride => typeof(TabItem);

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.FindAncestorOfType<SearchView>() is SearchView searchView)
        {
            searchView.CloseTab(this);
        }
    }
}
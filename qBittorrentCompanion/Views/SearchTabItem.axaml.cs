using Avalonia.Controls;
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
}
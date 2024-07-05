using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;
using Avalonia;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using QBittorrent.Client;

namespace qBittorrentCompanion.Views
{
    public partial class SearchView : UserControl
    {
        //^\[([^\]]+)\] (.*)(?= \- [0-9]{1,3}) \- ([0-9]{1,3}).*\[([A-Z-0-9]{8})\]\.(?:mkv|MKV)
        public SearchView()
        {
            InitializeComponent();
            this.Loaded += SearchView_Loaded;
            this.DataContext = new SearchViewModel();

            // Subscribe to resize actions
            this.AttachedToVisualTree += (sender, e) =>
            {
                if (this.GetVisualRoot() is Window window)
                {
                    window.GetObservable(Window.BoundsProperty).Subscribe(Resize);
                }
            };
        }

        private void SearchView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel searchVm)
            {
                searchVm.Initialise();
            }
        }

        // Callback function for resize actions
        private void Resize(Rect size)
        {
            FiltersGrid.IsVisible = size.Width < 1220;
        }
    }
}

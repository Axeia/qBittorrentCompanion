using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace qBittorrentCompanion.Views
{
    public partial class SearchPluginsWindow : Window
    {
        private DispatcherTimer _keyPressTimer = new DispatcherTimer();
        private StringBuilder _keyPresses = new StringBuilder();

        public SearchPluginsWindow()
        {
            InitializeComponent();

            this.DataContext = new SearchPluginsViewModel();
            Loaded += SearchPluginsWindow_Loaded;

            _keyPressTimer.Interval = TimeSpan.FromMilliseconds(500); // Set the delay to 500 milliseconds
            _keyPressTimer.Tick += KeyPressTimer_Tick;
        }

        // Resets the typing logic
        private void KeyPressTimer_Tick(object? sender, EventArgs e)
        {
            _keyPressTimer.Stop();
            _keyPresses.Clear();
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            // Get the pressed key
            var key = e.Key;

            // Convert the key to a string
            var keyString = key.ToString();

            // Append the key to _keyPresses
            _keyPresses.Append(keyString);

            // Restart the timer
            _keyPressTimer.Stop();
            _keyPressTimer.Start();

            PerformSearch(_keyPresses.ToString());
        }

        private int _lastMatchedIndex = -1;

        private void PerformSearch(string toMatch)
        {
            Debug.WriteLine($"Performing search with keys: {toMatch}");

            // Convert ItemsSource to a list
            var items = new List<SearchPluginViewModel>(SearchPluginsDataGrid.ItemsSource.Cast<SearchPluginViewModel>());

            // Perform the search operation with keyPresses
            // Loop through the items in the DataGrid
            SearchPluginViewModel? matchedItem = null;
            for (int i = 0; i < items.Count; i++)
            {
                // Calculate the index of the current item, starting from the item after the last matched item
                int index = (_lastMatchedIndex + 1 + i) % items.Count;

                var item = items[index];
                // Check if the Name starts with the pressed key
                if (item.Name.StartsWith(toMatch, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"Found match: {item.Name}");

                    // Select the item
                    SearchPluginsDataGrid.SelectedItem = item;

                    // Scroll the DataGrid to the selected item
                    SearchPluginsDataGrid.ScrollIntoView(item, SearchPluginsDataGrid.Columns[0]);

                    matchedItem = item;
                    _lastMatchedIndex = index;

                    // Stop the loop
                    break;
                }
            }

            // If the same key is pressed again and no new match is found, select the next item
            if (toMatch.Length == 1 && matchedItem == null && items.Count > 0)
            {
                int nextIndex = (_lastMatchedIndex + 1) % items.Count;

                // Select the next item
                SearchPluginsDataGrid.SelectedItem = items[nextIndex];

                // Scroll the DataGrid to the selected item
                SearchPluginsDataGrid.ScrollIntoView(SearchPluginsDataGrid.SelectedItem, SearchPluginsDataGrid.Columns[0]);
            }
        }

        private void SearchPluginsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if(DataContext is SearchPluginsViewModel searchPluginsViewModel)
            {
                searchPluginsViewModel.Initialise();
            }            
        }
    }
}

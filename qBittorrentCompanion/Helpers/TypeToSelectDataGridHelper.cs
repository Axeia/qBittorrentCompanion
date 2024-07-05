using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Helpers
{
    public class TypeToSelectDataGridHelper<T>
    {
        private StringBuilder _keyPresses = new StringBuilder();
        private DateTime _lastKeyPressTime;
        private readonly TimeSpan _keyPressInterval = TimeSpan.FromMilliseconds(500);
        private readonly DataGrid _dataGrid;
        private readonly string _propertyName;

        public TypeToSelectDataGridHelper(DataGrid dataGrid, string propertyName)
        {
            _dataGrid = dataGrid;
            _propertyName = propertyName;
            _dataGrid.KeyDown += DataGrid_KeyDown;
        }

        private async void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            var keyString = key.ToString();
            if (string.IsNullOrEmpty(keyString))
                return;

            var currentTime = DateTime.Now;

            // If the time between key presses is more than the interval, reset the key presses
            if ((currentTime - _lastKeyPressTime) > _keyPressInterval)
            {
                _keyPresses.Clear();
            }

            // Append the key to _keyPresses
            _keyPresses.Append(keyString);
            _lastKeyPressTime = currentTime;

            // Perform search immediately
            await PerformSearchAsync(_keyPresses.ToString());
        }

        private void KeySequenceNotFoundReset()
        {
            if (_keyPresses.Length > 0)
            {
                char backup = _keyPresses[0];
                _keyPresses.Clear();
                _keyPresses.Append(backup);
            }
        }

        private async Task PerformSearchAsync(string toMatch)
        {
            List<T>? items = null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                items = _dataGrid.ItemsSource.Cast<T>().ToList();
            });

            if (items != null)
            {
                await Task.Run(() => Search(items, toMatch));
            }
        }

        private void Search(List<T> items, string toMatch)
        {
            Debug.WriteLine($"Performing search with keys: {toMatch}");

            int startAt = _dataGrid.SelectedIndex == -1 ? 0 : _dataGrid.SelectedIndex;
            int endAt = items.Count;

            // Look at entries after the current item for a match
            T? itemToSelect = SearchRange(items, toMatch, startAt + 1, items.Count);
            // Match not found, look at entries before the current item
            if (itemToSelect is null && startAt > 0)
                itemToSelect = SearchRange(items, toMatch, 0, startAt);

            // Match still not found, check if the key is a repeating sequence 
            if (itemToSelect is null && toMatch.Distinct().Count() == 1)
            {
                KeySequenceNotFoundReset(); // Altered _keyPresses
                string toMatchInstead = _keyPresses.ToString();
                // Attempt to find next entry with key instead
                itemToSelect = SearchRange(items, toMatchInstead, startAt + 1, items.Count);
                // Match still not found, last attempt at doing so
                if (itemToSelect is null && startAt > 0)
                    itemToSelect = SearchRange(items, toMatchInstead, 0, startAt);
            }

            if (itemToSelect is not null)
                UpdateSelection(itemToSelect);
        }

        private T? SearchRange(List<T> items, string toMatch, int start, int end)
        {
            for (int current = start; current < end; current++)
            {
                var item = items[current];
                var propertyValue = item?.GetType().GetProperty(_propertyName)?.GetValue(item, null)?.ToString();
                if (propertyValue != null && propertyValue.StartsWith(toMatch, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return default;
        }

        private void UpdateSelection(T item)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _dataGrid.SelectedItem = item;
                _dataGrid.ScrollIntoView(item, _dataGrid.Columns[0]);
            });
        }
    }
}

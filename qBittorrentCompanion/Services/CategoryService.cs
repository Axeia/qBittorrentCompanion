using Avalonia.Threading;
using AvaloniaEdit.Utils;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized RSS feed service
    public class CategoryService
    {
        private static readonly Lazy<CategoryService> _instance =
            new(() => new CategoryService());
        public static CategoryService Instance => _instance.Value;

        // Observable collection that all views can bind to
        public ObservableCollection<Category> Categories { get; } = [];

        // Event that classes can subscribe to for notifications
        public event EventHandler? CategoriesUpdated;

        public void AddCategories(IEnumerable<Category> categories)
        {
            Categories.AddRange(
                categories
                .Where(category => !Categories.Any(c => c.Name == category.Name))
            );
            CategoriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private CategoryService()
        {

        }

        public async Task InitializeAsync()
        {
            await UpdateCategoriesAsync();
        }

        public async Task UpdateCategoriesAsync()
        {
            try
            {
                // Get the latest feeds from QBittorrent
                var result = await QBittorrentService.QBittorrentClient.GetCategoriesAsync();

                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Categories.Clear();
                    foreach (KeyValuePair<string, Category> stringCat in result)
                        Categories.Add(stringCat.Value);

                    // Notify subscribers
                    CategoriesUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
            catch (Exception ex)
            {
                // Log exception
                Debug.WriteLine($"Error updating RSS feeds: {ex.Message}");
            }
        }
    }
}

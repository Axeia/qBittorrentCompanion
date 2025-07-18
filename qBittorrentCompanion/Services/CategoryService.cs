﻿using Avalonia.Threading;
using AvaloniaEdit.Utils;
using QBittorrent.Client;
using ReactiveUI;
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

        public void ChangeCategory(string oldCatName, Category newCat)
        {
            if (Categories.FirstOrDefault(c => c.Name == oldCatName) is Category cat)
            {
                cat.Name = newCat.Name;
                cat.SavePath = newCat.SavePath;
                cat.AdditionalData = newCat.AdditionalData;

                CategoriesUpdated?.Invoke(this, EventArgs.Empty);
            }
            else // Category doesn't exist - add  it
                Categories.Add(newCat);
            
        }

        public async Task UpdateCategoriesAsync()
        {
            var categories = await QBittorrentService.GetCategoriesAsync();

            if (categories != null)
            {
                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Categories.AddRange(categories.Values);
                    // Notify subscribers
                    CategoriesUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}

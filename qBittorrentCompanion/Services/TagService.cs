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
    public class TagService
    {
        private static readonly Lazy<TagService> _instance =
            new(() => new TagService());
        public static TagService Instance => _instance.Value;

        // Observable collection that all views can bind to
        public ObservableCollection<string> Tags { get; } = [];

        // Event that classes can subscribe to for notifications
        public event EventHandler? TagsUpdated;

        public void AddTags(IEnumerable<string> moreTags)
        {
            Tags.AddRange(moreTags.Where(t => !Tags.Contains(t)));
            TagsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private TagService()
        {

        }

        public async Task InitializeAsync()
        {
            await UpdateTagsAsync();
        }

        public async Task UpdateTagsAsync()
        {
            try
            {
                // Get the latest feeds from QBittorrent
                var result = await QBittorrentService.QBittorrentClient.GetTagsAsync();

                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Tags.Clear();
                    foreach (string tag in result)
                        Tags.Add(tag);

                    // Notify subscribers
                    TagsUpdated?.Invoke(this, EventArgs.Empty);
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

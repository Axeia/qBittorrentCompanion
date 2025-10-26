using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    // Centralized RSS tags service
    public class TagService
    {
        private static readonly Lazy<TagService> _instance =
            new(() => new TagService());
        public static TagService Instance => _instance.Value;

        // Observable collection that all views can bind to
        public ObservableCollection<string> Tags { get; } = Design.IsDesignMode ? ["Test tag"] : [];
        public static ObservableCollection<string> SharedTags => Instance.Tags;

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
            // Get the latest tags from QBittorrent
            var tags = await QBittorrentService.GetTagsAsync();
            
            if (tags != null)
            { 
                // Update on UI thread to avoid cross-thread collection exceptions
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Tags.Clear();
                    foreach (string tag in tags)
                        Tags.Add(tag);

                    // Notify subscribers
                    TagsUpdated?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}

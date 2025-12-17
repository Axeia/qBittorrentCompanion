using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public partial class RssFeedViewModel(RssFeed rssFeed) : ViewModelBase
    {
        [RaiseChangeProxy(nameof(RssFeed.Name))]
        [RaiseChangeProxy(nameof(RssFeed.HasError))]
        [RaiseChangeProxy(nameof(RssFeed.IsLoading))]
        [RaiseChangeProxy(nameof(RssFeed.LastBuildDate))]
        [RaiseChangeProxy(nameof(RssFeed.Title))]
        [RaiseChangeProxy(nameof(RssFeed.Uid))]
        [RaiseChangeProxy(nameof(RssFeed.Url))]
        private readonly RssFeed _rssFeed = rssFeed;

        public IList<RssArticle> Articles
        {
            get => _rssFeed.Articles;
            set
            {
                if (value != _rssFeed.Articles)
                {
                    _rssFeed.Articles = value;
                    this.RaisePropertyChanged(nameof(Articles));
                    this.RaisePropertyChanged(nameof(ReadArticleCount));
                }
            }
        }

        public int ReadArticleCount
        {
            get => _rssFeed.Articles.Count(a => !a.IsRead);
        }

        public void Update(RssFeed rssFeed)
        {
            Name = rssFeed.Name;
            Articles = rssFeed.Articles;
            HasError = rssFeed.HasError;
            IsLoading = rssFeed.IsLoading;
            LastBuildDate = rssFeed.LastBuildDate;
            Title = rssFeed.Title;
            Uid = rssFeed.Uid;
            Url = rssFeed.Url;
        }

        public RssFeedViewModel GetCopy()
        {
            return new RssFeedViewModel(new RssFeed
            {
                Name = this.Name,
                Articles = this.Articles,
                HasError = this.HasError,
                IsLoading = this.IsLoading,
                LastBuildDate = this.LastBuildDate,
                Title = this.Title,
                Uid = this.Uid,
                Url = this.Url
            });
        }

        public async Task Rename(string newName)
        {
            IsLoading = true;
            await QBittorrentService.MoveRssItemAsync(Name, newName);
            IsLoading = false;
        }
    }
}

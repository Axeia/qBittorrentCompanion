using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public partial class RssFeedViewModel(RssFeed rssFeed) : INotifyPropertyChanged
    {
        /// <summary>
        /// RssFeed enherits from RssItem - RssItem has a Name property.
        /// </summary>
        public string Name
        {
            get => _rssFeed.Name;
            set
            {
                if (value != _rssFeed.Name)
                {
                    _rssFeed.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// RssFeed enherits from RssItem - RssItem has a Name property.
        /// </summary>

        [AutoProxyPropertyChanged(nameof(RssFeed.HasError))]
        [AutoProxyPropertyChanged(nameof(RssFeed.IsLoading))]
        [AutoProxyPropertyChanged(nameof(RssFeed.LastBuildDate))]
        [AutoProxyPropertyChanged(nameof(RssFeed.Title))]
        [AutoProxyPropertyChanged(nameof(RssFeed.Uid))]
        [AutoProxyPropertyChanged(nameof(RssFeed.Url))]
        private readonly RssFeed _rssFeed = rssFeed;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IList<RssArticle> Articles
        {
            get => _rssFeed.Articles;
            set
            {
                if (value != _rssFeed.Articles)
                {
                    _rssFeed.Articles = value;                    
                    OnPropertyChanged(nameof(Articles));
                    OnPropertyChanged(nameof(ReadArticleCount));
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
            return new RssFeedViewModel(new RssFeed{
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

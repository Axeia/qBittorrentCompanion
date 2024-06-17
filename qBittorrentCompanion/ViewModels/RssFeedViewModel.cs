using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class RssFeedViewModel : INotifyPropertyChanged
    {
        private RssFeed _rssFeed;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RssFeedViewModel(RssFeed rssFeed)
        {
            _rssFeed = rssFeed;
        }

        /// <summary>
        /// RssFeed enherits from RssItem - RssItem has a Name property.
        /// </summary>
        public string Name
        {
            get => _rssFeed.Name;
            set
            {
                if(value != _rssFeed.Name)
                {
                    _rssFeed.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
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

        public bool? HasError
        {
            get => _rssFeed.HasError;
            set
            {
                if (value != _rssFeed.HasError)
                {
                    _rssFeed.HasError = value;
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool? IsLoading
        {
            get => _rssFeed.IsLoading;
            set
            {
                if (value != _rssFeed.IsLoading)
                {
                    _rssFeed.IsLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public DateTimeOffset? LastBuildDate
        {
            get => _rssFeed.LastBuildDate;
            set
            {
                if (value != _rssFeed.LastBuildDate)
                {
                    _rssFeed.LastBuildDate = value;
                    OnPropertyChanged(nameof(LastBuildDate));
                }
            }
        }

        public string Title
        {
            get => _rssFeed.Title;
            set
            {
                if (value != _rssFeed.Title)
                {
                    _rssFeed.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public Guid Uid
        {
            get => _rssFeed.Uid;
            set
            {
                if (value != _rssFeed.Uid)
                {
                    _rssFeed.Uid = value;
                    OnPropertyChanged(nameof(Uid));
                }
            }
        }

        public Uri Url
        {
            get => _rssFeed.Url;
            set
            {
                if (value != _rssFeed.Url)
                {
                    _rssFeed.Url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
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

            try
            {
                Debug.WriteLine($"Renaming from {Name} to {newName}");
                await QBittorrentService.QBittorrentClient.MoveRssItemAsync(
                    Name, newName
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.ViewModels
{
    public class RssArticleViewModel : INotifyPropertyChanged
    {
        private readonly RssArticle _rssArticle;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RssArticleViewModel(RssArticle rssArticle)
        {
            _rssArticle = rssArticle;
            IsMatch = _rssArticle.Title.Contains("[");
        }

        private bool _isMatch = false;
        public bool IsMatch
        {
            get => _isMatch;
            set
            {
                if (_isMatch != value)
                {
                    _isMatch = value;
                    OnPropertyChanged(nameof(IsMatch));
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="RssArticle.Id"/>
        /// </summary>
        public string Id => _rssArticle.Id;

        /// <summary>
        /// <inheritdoc cref="RssArticle.Date"/>
        /// </summary>
        public DateTimeOffset Date => _rssArticle.Date;

        /// <summary>
        /// <inheritdoc cref="RssArticle.Title"/>
        /// </summary>
        public string Title => _rssArticle.Title;

        /// <summary>
        /// <inheritdoc cref="RssArticle.Author"/>
        /// </summary>
        public string Author => _rssArticle.Author;

        /// <summary>
        /// <inheritdoc cref="RssArticle.Description"/>
        /// </summary>
        public string Description => _rssArticle.Description;

        /// <summary>
        /// <inheritdoc cref="RssArticle.TorrentUri"/>
        /// </summary>
        public Uri TorrentUri => _rssArticle.TorrentUri;

        /// <summary>
        /// <inheritdoc cref="RssArticle.Link"/>
        /// </summary>
        public Uri Link => _rssArticle.Link;

        /// <summary>
        /// <inheritdoc cref="RssArticle.IsRead"/>
        /// </summary>
        public bool IsRead => _rssArticle.IsRead;

        /// <summary>
        /// <inheritdoc cref="RssArticle.AdditionalData"/>
        /// </summary>
        public IDictionary<string, JToken> AdditionalData => _rssArticle.AdditionalData;
    }
}


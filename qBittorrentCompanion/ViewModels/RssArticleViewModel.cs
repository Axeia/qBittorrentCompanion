using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RssArticleViewModel : RssRuleIsMatchViewModel
    {
        private readonly RssArticle _rssArticle;

        // Not all torrents have a season and episode
        public int? Season { get; private set; } = null;
        public int? Episode { get; private set; } = null;

        [GeneratedRegex(@"\bs0?(\d{1,4})[ -_.]?e(0?\d{1,4})(?:\D|\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex TypicalSeasonEpisodePattern();

        [GeneratedRegex(@"\b(\d{1,4})x(0?\d{1,4})(?:\D|\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex XBasedSeasonEpisodePattern();

        public bool HasSeasonAndEpisode => Season != null && Episode != null;

        public RssArticleViewModel(RssArticle rssArticle)
        {
            _rssArticle = rssArticle ?? throw new ArgumentNullException(nameof(rssArticle));

            // Populate season and episode if present based on _rssArticle.Title
            ExtractSeasonAndEpisode(_rssArticle.Title);
        }

        /// <summary>
        /// Attempts to extract season and episode numbers from the article title.
        /// Uses qBittorrent's known patterns (SXXEXX or XXxXX).
        /// Based on:
        /// https://github.com/qbittorrent/qBittorrent/blob/83799f4f07fb2f568ca2c2d1c3f4e5f58fa6b557/src/base/rss/rss_autodownloadrule.cpp#L311-L361
        /// </summary>
        /// <param name="title">The title of the RSS article.</param>
        private void ExtractSeasonAndEpisode(string title)
        {
            Regex xBasedSeasonEpisodePattern = XBasedSeasonEpisodePattern();

            if (string.IsNullOrEmpty(title))
            {
                Season = null;
                Episode = null;
                return;
            }

            Regex typicalSeasonEpisodePattern = TypicalSeasonEpisodePattern();
            Match matcher = typicalSeasonEpisodePattern.Match(title);
            bool matched = matcher.Success;

            if (!matched)
            {
                matcher = xBasedSeasonEpisodePattern.Match(title);
                matched = matcher.Success;
            }

            if (matched)
            {
                Season = int.Parse(matcher.Groups[1].Value);
                Episode = int.Parse(matcher.Groups[2].Value);
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


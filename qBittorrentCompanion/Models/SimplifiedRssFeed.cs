using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Models
{
    public class SimplifiedRssFeed
    {
        private Uri _uri;
        public Uri Url { get => _uri; }

        private string _title;
        public string Title { get => _title; }

        public SimplifiedRssFeed(Uri uri, string title)
        {
            _uri = uri;
            _title = title;
        }
    }
}

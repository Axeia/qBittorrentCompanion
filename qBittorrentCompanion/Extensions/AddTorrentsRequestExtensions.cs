using QBittorrent.Client;
using System;
using System.Collections.Generic;

namespace qBittorrentCompanion.Extensions
{
    public static class AddTorrentsRequestExtensions
    {
        public static void BatchAddFiles(this AddTorrentsRequest addTorrentsRequest, IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
                addTorrentsRequest.TorrentFiles.Add(filePath);
        }
        public static void BatchAddUrls(this AddTorrentsRequest addTorrentsRequest, IEnumerable<Uri> urls)
        {
            foreach (var url in urls)
                addTorrentsRequest.TorrentUrls.Add(url);
        }
    }
}

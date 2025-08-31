using QBittorrent.Client;
using System.Collections.Generic;

namespace qBittorrentCompanion.Services
{
    // Centralized search plugin service
    public class SearchPluginServiceBase
    {
        public static List<SearchPluginCategory> DefaultCategories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Helpers
{
    public static class LinkChecker
    {
        public async static Task<bool> IsTorrentLink(string url)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.Headers.TryGetValues("Content-Type", out var values))
                    {
                        var firstHeader = values.FirstOrDefault();
                        if (firstHeader is string contentType && contentType == "application/x-bittorrent")
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}

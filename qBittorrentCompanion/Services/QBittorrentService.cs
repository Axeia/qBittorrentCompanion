using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    public class QBittorrentService
    {
        private static QBittorrentClient? _qBittorrentClient;
        /// <summary>
        /// Only usable after having used Authenticate or AutoAuthenticate
        /// </summary>
        public static QBittorrentClient QBittorrentClient { get => _qBittorrentClient!; }

        public static string Address = "";

         public static async Task<bool> AutoAthenticate()
        {
            var SecureStorage = new SecureStorage();
            try
            {
                (string username, string password, string url, string port) = SecureStorage.LoadData();
                //Debug.WriteLine($"username: {username}, password: {password}, url: {url}, port: {port}");
                await Authenticate(username, password, url, port);
                return true;
            }
            catch (NoLoginDataException)
            {
                return false;
            }

        }

        public static async Task Authenticate(string username, string password, string url, string port)
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            Address = $"http://{url}:{port}";
            _qBittorrentClient = new QBittorrentClient(new Uri(Address));
            await _qBittorrentClient.LoginAsync(username, password);
        }
    }
}

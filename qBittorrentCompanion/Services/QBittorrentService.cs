using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
                return await Authenticate(username, password, url, port);
            }
            catch (NoLoginDataException)
            {
                return false;
            }

        }

        public static async Task<bool> Authenticate(string username, string password, string url, string port)
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            Address = $"http://{url}:{port}";
            _qBittorrentClient = new QBittorrentClient(new Uri(Address));
            await _qBittorrentClient.LoginAsync(username, password);

            // qbittorrent-net-client does not have a IsLoggedIn() method so 
            // make a request and see if it fails
            try
            {
                await QBittorrentService.QBittorrentClient.GetApiVersionAsync();
                return true;
            }
            catch (QBittorrentClientRequestException e)
            {
                //if (e.StatusCode == HttpStatusCode.Forbidden)
                return false;
            }
        }
    }
}

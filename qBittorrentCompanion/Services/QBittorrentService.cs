using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
                await QBittorrentClient.GetApiVersionAsync();
                return true;
            }
            catch (QBittorrentClientRequestException e)
            {
                //if (e.StatusCode == HttpStatusCode.Forbidden)
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Do not use if it can be avoided. Ideally everything is handled through qbittorrent-net-client.
        /// However! If it falls short this allows getting access to its HttpClient through reflection.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static HttpClient GetHttpClient()
        {
            var qbittorrentClientType = typeof(QBittorrentClient);

            var clientField = qbittorrentClientType.GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance);

            if(clientField == null)
            {
                Debug.WriteLine("Could not find the _client field in QBittorrentClient.");
                throw new Exception("Could not find the _client field in QBittorrentClient.");
            }

            if(clientField.GetValue(QBittorrentService.QBittorrentClient) is HttpClient client)
                return client;

            Debug.WriteLine("Could not get HttpClient from qbittorrent-net-client");
            throw new Exception("Could not get HttpClient from qbittorrent-net-client");

        }

        /// <summary>
        /// Do not use if it can be avoided. Ideally everything is handled through qbittorrent-net-client.
        /// However! If it falls short this allows getting access to its Uri through reflection.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Uri GetUrl()
        {
            var qbittorrentClientType = typeof(QBittorrentClient);

            var uriField = qbittorrentClientType.GetField("_uri", BindingFlags.NonPublic | BindingFlags.Instance);
            if (uriField == null)
            {
                Debug.WriteLine("Could not find the _uri field in QBittorrentClient.");
                throw new Exception("Could not find the _uri field in QBittorrentClient.");
            }

            var uri = uriField.GetValue(QBittorrentService.QBittorrentClient) as Uri;
            if (uri == null)
            {
                Debug.WriteLine("The _uri field is null.");
                throw new Exception("The _uri field is null.");
            }

            return uri;
        }


    }
}

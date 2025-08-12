using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Logging;
using qBittorrentCompanion.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    public static partial class QBittorrentService
    {
        private static readonly int retryCount = 3;
        public static int RetryCount => retryCount;

        private static readonly int retryDelay = 1;

        public static event Action<HttpData>? NetworkRequestSent;

        // Add ThreadLocal to track current attempt across the call stack
        private static readonly ThreadLocal<int> currentAttempt = new(() => 1);

        public partial class LoggingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
        {
            // Add these static properties for testing
            public static bool TestMode { get; set; } = false;
            public static double FailureRate { get; set; } = 0.5; // 50% failure rate
            public static int ForceFailOnAttempt { get; set; } = 0; // 0 = disabled, 1+ = fail on specific attempt
            private static readonly Random random = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpData httpData = new(request.RequestUri!)
                {
                    IsPost = request.Method == HttpMethod.Post,
                    Request = await FormatRequestAsync(request),
                    RequestSend = DateTime.Now,
                    ConnectionAttempt = currentAttempt.Value // Get the current attempt number
                };

                NetworkRequestSent?.Invoke(httpData);

                // Test mode: Force failures
                if (TestMode)
                {
                    // Fail on specific attempt number
                    if (ForceFailOnAttempt > 0 && currentAttempt.Value == ForceFailOnAttempt)
                    {
                        httpData.RequestReceived = DateTime.Now;
                        httpData.HttpStatusCode = -1; // Mark as connection failure
                        throw new HttpRequestException($"Forced failure on attempt {currentAttempt.Value}");
                    }

                    // Random failure based on failure rate
                    if (ForceFailOnAttempt == 0 && random.NextDouble() < FailureRate)
                    {
                        httpData.RequestReceived = DateTime.Now;
                        httpData.HttpStatusCode = -1; // Mark as connection failure
                        throw new HttpRequestException($"Random failure on attempt {currentAttempt.Value}");
                    }
                }

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                httpData.HttpStatusCode = (int)response.StatusCode;
                httpData.RequestReceived = DateTime.Now;

                string responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                httpData.Response = responseText;

                return response;
            }

            private static async Task<string> FormatRequestAsync(HttpRequestMessage request)
            {
                var requestBuilder = new System.Text.StringBuilder();

                // Add request line
                requestBuilder.AppendLine($"{request.Method} {request.RequestUri?.PathAndQuery} HTTP/{request.Version}");
                requestBuilder.AppendLine($"Host: {request.RequestUri?.Authority}");

                // Add headers
                foreach (var header in request.Headers)
                {
                    requestBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                // Add content headers and body if present
                if (request.Content != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        requestBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                    }

                    requestBuilder.AppendLine(); // Empty line before body

                    try
                    {
                        string content = await request.Content.ReadAsStringAsync();

                        // Mask password as a security precaution
                        if (!string.IsNullOrEmpty(content))
                        {
                            content = _passwordUrlQueryRegex().Replace(content, m => new string('*', 8));
                            requestBuilder.AppendLine(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        requestBuilder.AppendLine($"[Error reading request body: {ex.Message}]");
                    }
                }

                return requestBuilder.ToString().TrimEnd();
            }

            [GeneratedRegex(@"(?<=\bpassword=)[^&\s]*", RegexOptions.IgnoreCase, "en-US")]
            private static partial Regex _passwordUrlQueryRegex();
        }

        public static async Task<T?> RetryAsync<T>(Func<Task<T>> action, Func<Exception, bool>? isRetryable = null)
        {
            TimeSpan tsDelay = TimeSpan.FromSeconds(retryDelay);
            isRetryable ??= (ex => ex is HttpRequestException);

            int attempt = 0;
            while (true)
            {
                try
                {
                    attempt++;
                    currentAttempt.Value = attempt; // Set current attempt for this thread
                    return await action();
                }
                catch (Exception ex) when (isRetryable(ex))
                {
                    if (attempt >= retryCount)
                    {
                        Console.WriteLine($"Max retry attempts reached. Last error: {ex.Message}");
                        return default;
                    }

                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {tsDelay.TotalSeconds}s...");
                    await Task.Delay(tsDelay);
                }
            }
        }

        public static async Task<T?> RunWithEventsAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await RetryAsync(action, ex =>
                {
                    // Retry on network-level failures
                    return ex is HttpRequestException || ex is TaskCanceledException;
                });

                return result;
            }
            catch (QBittorrentClientRequestException)
            {
                // Response already logged in LoggingHandler, so no need to log here again
                return default;
            }
            catch (Exception)
            {
                // Non-HTTP errors (already logged if from LoggingHandler)
                return default;
            }
            finally
            {
                currentAttempt.Value = 1; // Reset for next call
            }
        }

        public static async Task RunWithEventsAsync(Func<Task> action)
        {
            try
            {
                await RetryAsync(async () =>
                {
                    await action();
                    return true; // dummy return value to satisfy RetryAsync
                }, ex =>
                {
                    return ex is HttpRequestException || ex is TaskCanceledException;
                });
            }
            catch (QBittorrentClientRequestException)
            {
                // Already logged
            }
            catch (Exception)
            {
                // Already logged
            }
            finally
            {
                currentAttempt.Value = 1; // Reset for next call
            }
        }

        public static Task<PartialData?> GetPartialDataAsync(int ridIncrement)
            => RunWithEventsAsync(() => QBittorrentClient.GetPartialDataAsync(ridIncrement));

        public static Task<IReadOnlyDictionary<string, Category>?> GetCategoriesAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetCategoriesAsync(token));

        public static Task<IReadOnlyList<string>?> GetTagsAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTagsAsync(token));

        public static Task<RssFolder?> GetRssItemsAsync(bool withData = false, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetRssItemsAsync(withData, token));

        public static Task<IReadOnlyList<SearchPlugin>?> GetSearchPluginsAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetSearchPluginsAsync(token));

        public static Task<PeerAddResult?> AddTorrentPeersAsync(string hash, IEnumerable<string> peers, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddTorrentPeersAsync(hash, peers, token));

        public static Task<IReadOnlyList<TorrentTracker>?> GetTorrentTrackersAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTorrentTrackersAsync(hash, token));

        public static Task AddTrackersAsync(string hash, IEnumerable<Uri> trackers, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddTrackersAsync(hash, trackers, token));

        public static Task DeleteTrackersAsync(string hash, IEnumerable<Uri> trackers, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTrackersAsync(hash, trackers, token));

        public static Task<IReadOnlyList<NetInterface>?> GetNetworkInterfacesAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetNetworkInterfacesAsync(token));

        public static Task<IReadOnlyList<string>?> GetNetworkInterfaceAddressesAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetNetworkInterfaceAddressesAsync(token));

        public static Task<Preferences?> GetPreferencesAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetPreferencesAsync(token));

        public static Task SetPreferencesAsync(ExtendedPreferences extPrefs, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetPreferencesAsync(extPrefs, token));

        public static Task<IReadOnlyList<TorrentContent>?> GetTorrentContentsAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTorrentContentsAsync(hash, token));

        public static Task RenameFileAsync(string hash, string oldName, string newName, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.RenameFileAsync(hash, oldName, newName, token));

        public static Task RenameFolderAsync(string hash, string oldName, string newName, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.RenameFolderAsync(hash, oldName, newName, token));

        public static Task<IReadOnlyDictionary<string, RssAutoDownloadingRule>?> GetRssAutoDownloadingRulesAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetRssAutoDownloadingRulesAsync(token));

        public static Task SetRssAutoDownloadingRuleAsync(string name, RssAutoDownloadingRule rule, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetRssAutoDownloadingRuleAsync(name, rule, token));

        public static Task DeleteRssAutoDownloadingRuleAsync(string title, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteRssAutoDownloadingRuleAsync(title, token));

        public static Task DeleteTagAsync(string tag, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTagAsync(tag, token));

        public static Task DeleteTagsAsync(IEnumerable<string> tags, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTagsAsync(tags, token));

        public static Task DeleteAsync(IEnumerable<string> hashes, bool deleteDownloadedData, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteAsync(hashes, deleteDownloadedData, token));

        public static Task CreateTagAsync(string tag, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.CreateTagAsync(tag, token));

        public static Task RenameRssAutoDownloadingRuleAsync(string oldTitle, string title, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.RenameRssAutoDownloadingRuleAsync(oldTitle, title, token));

        public static Task MarkRssItemAsReadAsync(string itemPath, string? articleId = null, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.MarkRssItemAsReadAsync(itemPath, articleId, token));

        public static Task AddRssFeedAsync(Uri url, string? path = null, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddRssFeedAsync(url, path, token));

        public static Task DeleteRssItemAsync(string path, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteRssItemAsync(path, token));

        public static Task MoveRssItemAsync(string path, string newPath, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.MoveRssItemAsync(path, newPath, token));

        public static Task UninstallSearchPluginAsync(string name, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.UninstallSearchPluginAsync(name, token));

        public static Task<IReadOnlyList<Uri>?> GetTorrentWebSeedsAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTorrentWebSeedsAsync(hash, token));

        public static Task EnableSearchPluginAsync(string name, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.EnableSearchPluginAsync(name, token));

        public static Task DisableSearchPluginAsync(string name, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DisableSearchPluginAsync(name, token));

        public static Task<int> StartSearchAsync(string pattern, IEnumerable<string> plugins, string category = "all", CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.StartSearchAsync(pattern, plugins, category, token));

        public static Task<SearchResults?> GetSearchResultsAsync(int id, int offset = 0, int limit = 0, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetSearchResultsAsync(id, offset, limit, token));

        public static Task StopSearchAsync(int id, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.StopSearchAsync(id, token));

        public static Task SetGlobalDownloadLimitAsync(long limit, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetGlobalDownloadLimitAsync(limit, token));

        public static Task SetGlobalUploadLimitAsync(long limit, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetGlobalUploadLimitAsync(limit, token));

        public static Task SetFilePriorityAsync(string hash, int fileId, TorrentContentPriority priority, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetFilePriorityAsync(hash, fileId, priority, token));

        public static Task PauseAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.PauseAsync(hash, token));

        public static Task PauseAsync(IEnumerable<string> hashes, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.PauseAsync(hashes, token));

        public static Task ResumeAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ResumeAsync(hash, token));

        public static Task ResumeAsync(IEnumerable<string> hashes, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ResumeAsync(hashes, token));

        public static Task SetForceStartAsync(string hash, bool enabled, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetForceStartAsync(hash, enabled, token));

        public static Task ChangeTorrentPriorityAsync(string hash, TorrentPriorityChange change, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ChangeTorrentPriorityAsync(hash, change, token));

        public static Task ChangeTorrentPriorityAsync(IEnumerable<string> hashes, TorrentPriorityChange change, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ChangeTorrentPriorityAsync(hashes, change, token));

        public static Task AddCategoryAsync(string category, string savePath, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddCategoryAsync(category, savePath, token));

        public static Task DeleteCategoryAsync(string category, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteCategoryAsync(category, token));

        public static Task DeleteCategoriesAsync(IEnumerable<string> categories, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteCategoriesAsync(categories, token));

        public static Task SetTorrentCategoryAsync(string hash, string category, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetTorrentCategoryAsync(hash, category, token));

        public static Task DeleteTorrentTagAsync(string hash, string tag, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTorrentTagAsync(hash, tag, token));

        public static Task AddTorrentTagAsync(string hash, string tag, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddTorrentTagAsync(hash, tag, token));

        public static Task DeleteTorrentTagsAsync(string hash, IEnumerable<string>? tags, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTorrentTagsAsync(hash, tags, token));

        public static Task SetAutomaticTorrentManagementAsync(string hash, bool enabled, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetAutomaticTorrentManagementAsync(hash, enabled, token));

        public static Task ToggleFirstLastPiecePrioritizedAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ToggleFirstLastPiecePrioritizedAsync(token));

        public static Task ToggleSequentialDownloadAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ToggleSequentialDownloadAsync(token));

        public static Task SetSuperSeedingAsync(bool enabled, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetSuperSeedingAsync(enabled, token));

        public static Task SetTorrentDownloadLimitAsync(string hash, long limit, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetTorrentDownloadLimitAsync(hash, limit, token));

        public static Task SetTorrentUploadLimitAsync(string hash, long limit, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetTorrentUploadLimitAsync(hash, limit, token));

        public static Task SetShareLimitsAsync(string hash, double limit, TimeSpan seedingTime, TimeSpan inactiveSeedingTime, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetShareLimitsAsync(hash, limit, seedingTime, inactiveSeedingTime, token));

        public static Task RecheckAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.RecheckAsync(hash, token));

        public static Task ReannounceAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ReannounceAsync(hash, token));

        public static Task SetLocationAsync(string newLocation, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.SetLocationAsync(newLocation, token));

        public static Task RenameAsync(string hash, string newName, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.RenameAsync(hash, newName, token));

        public static Task<PeerPartialData?> GetPeerPartialDataAsync(string hash, int responseId = 0, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetPeerPartialDataAsync(hash, responseId, token));

        public static Task<IReadOnlyList<TorrentPieceState>?> GetTorrentPiecesStatesAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTorrentPiecesStatesAsync(hash, token));

        public static Task<TorrentProperties?> GetTorrentPropertiesAsync(string hash, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.GetTorrentPropertiesAsync(hash, token));

        public static Task DeleteTrackerAsync(string hash, Uri trackerUrl, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.DeleteTrackerAsync(hash, trackerUrl, token));

        public static Task AddTorrentsAsync(AddTorrentsRequest request, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.AddTorrentsAsync(request, token));

        public static Task ToggleAlternativeSpeedLimitsAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.ToggleAlternativeSpeedLimitsAsync(token));

        public static Task LogoutAsync(CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.LogoutAsync(token));

        public static Task EditCategoryAsync(string category, string savePath, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.EditCategoryAsync(category, savePath, token));

        public static Task EditTrackerAsync(string hash, Uri trackerUrl, Uri newTrackerUrl, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.EditTrackerAsync(hash, trackerUrl, newTrackerUrl, token));

        public static Task BanPeerAsync(string peer, CancellationToken token = default)
            => RunWithEventsAsync(() => QBittorrentClient.BanPeerAsync(peer, token));

        private static QBittorrentClient? _qBittorrentClient;
        /// <summary>
        /// Only usable after having used Authenticate or AutoAuthenticate
        /// </summary>
        public static QBittorrentClient QBittorrentClient => _qBittorrentClient!;
        public static string Address { get => address; set => address = value; }

        private static string address = "";

        public static async Task<bool> AutoAthenticate()
        {
            _ = new SecureStorage();
            try
            {
                (string username, string password, string url, string port) = SecureStorage.LoadData();
                return await Authenticate(username, password, url, port);
            }
            catch (NoLoginDataException)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="url"></param>
        /// <param name="port"></param>
        /// <returns>True on successful login, false if not</returns>
        public static async Task<bool> Authenticate(string username, string password, string url, string port)
        {
            Address = $"http://{url}:{port}";

            var baseUri = new Uri(Address);

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            var loggingHandler = new LoggingHandler(handler);

            // Force API v2 to avoid version probing
            _qBittorrentClient = new QBittorrentClient(baseUri, ApiLevel.V2, loggingHandler, disposeHandler: true);

            try
            {
                await _qBittorrentClient.LoginAsync(username, password);
                return true; 
            }
            catch (QBittorrentClientRequestException e)
            {
                AppLoggerService.AddLogMessage(LogLevel.Error, GetFullTypeName(typeof(QBittorrentService)), $"Failed to authenticate {e.StatusCode}", e.Message);
                return false;
            }
            catch (Exception e)
            {
                AppLoggerService.AddLogMessage(LogLevel.Error, GetFullTypeName(typeof(QBittorrentService)), $"Failed to authenticate", e.Message);
                Debug.WriteLine($"Login error: {e.Message}");
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
        public static void EnableTestingMode(double failureRate = 0.15)
        {
            LoggingHandler.TestMode = true;
            LoggingHandler.FailureRate = failureRate;
            LoggingHandler.ForceFailOnAttempt = 0; // Use random failures
        }

        public static void ForceFailOnAttempt(int attemptNumber)
        {
            LoggingHandler.TestMode = true;
            LoggingHandler.ForceFailOnAttempt = attemptNumber;
        }

        public static void DisableTestingMode()
        {
            LoggingHandler.TestMode = false;
            LoggingHandler.ForceFailOnAttempt = 0;
        }
    }
}

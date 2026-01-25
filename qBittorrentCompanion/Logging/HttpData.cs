using Newtonsoft.Json.Linq;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using System;
using System.Diagnostics;

namespace qBittorrentCompanion.Logging
{
    public class LinkDocInfo(string? shortDecription, Uri? url)
    {
        private readonly string? _shortDescription = shortDecription;
        public string? ShortDescription => _shortDescription;

        private readonly Uri? _url = url;
        public Uri? Url => _url;
    }

    public partial class HttpData(Uri url, int httpStatusCode = -1, bool isVisible = true) : ReactiveObject
    {
        private Uri _url = url;
        public Uri Url { get => _url; set => _url = value; }

        private readonly LinkDocInfo _linkDocInfo = GetLinkDocInfo(url.AbsolutePath.ToString());
        public LinkDocInfo LinkDocInfo => _linkDocInfo;

        public bool HasLinkDocInfo => _linkDocInfo.ShortDescription is not null && _linkDocInfo.Url is not null;

        public static LinkDocInfo GetLinkDocInfo(string url)
        {
            return url switch
            {
                "/api/v2/auth/login" => new LinkDocInfo(
                  "Authentication » Login",
                  new Uri("https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)#login")
                ),
                "/api/v2/sync/maindata" => new LinkDocInfo(
                  "Sync » Get main data",
                  new Uri("https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)#get-main-data")
                ),
                "/version/api" => new LinkDocInfo(
                  "Application » Get API version [obsolete]",
                  new Uri("https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-v3.1.x)")
                ),
                "/api/v2/app/webapiVersion" => new LinkDocInfo(
                  "Application » Get API version",
                  new Uri("https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-5.0)#get-api-version")
                ),
                _ => new LinkDocInfo(null, null),
            };
        }

        [RaiseChange]
        private bool _isPost = false;

        public string ConnectionType => IsPost ? "POST" : "GET";

        private DateTime _requestSend = DateTime.Now;

        public DateTime RequestSend
        {
            get => _requestSend;
            set
            {
                if (_requestSend != value)
                {
                    _requestSend = value;
                    this.RaisePropertyChanged(nameof(RequestSend));
                    this.RaisePropertyChanged(nameof(RequestDurationMilliseconds));
                    this.RaisePropertyChanged(nameof(RequestTime));
                }
            }
        }

        public String RequestTime =>
            _requestSend.ToString("HH:mm:ss:ff");

        private DateTime _requestReceived = DateTime.Now;
        public DateTime RequestReceived
        {
            get => _requestReceived;
            set
            {
                if (value != _requestReceived)
                {
                    _requestReceived = value;
                    this.RaisePropertyChanged(nameof(RequestReceived));
                    this.RaisePropertyChanged(nameof(RequestDurationMilliseconds));
                    this.RaisePropertyChanged(nameof(RequestTime));
                }
            }
        }

        public int RequestDurationMilliseconds =>
            (int)(_requestReceived - _requestSend).TotalMilliseconds;
        public string UrlPath => Url.PathAndQuery;

        private int _httpStatusCode = httpStatusCode;
        public int HttpStatusCode
        {
            get => _httpStatusCode;
            set
            {
                if (value != _httpStatusCode)
                {
                    _httpStatusCode = value;
                    this.RaisePropertyChanged(nameof(HttpStatusCode));
                    this.RaisePropertyChanged(nameof(IsGoodStatusCode));
                    this.RaisePropertyChanged(nameof(IsBadStatusCode));
                    this.RaisePropertyChanged(nameof(IsConnectionFailure));
                    this.RaisePropertyChanged(nameof(IsConnectedButBadStatusCode));
                }
            }
        }

        public bool IsGoodStatusCode => _httpStatusCode >= 200 && _httpStatusCode < 300;
        public bool IsBadStatusCode => IsConnectedButBadStatusCode || IsConnectionFailure;
        public bool IsConnectedButBadStatusCode => _httpStatusCode >= 400 && _httpStatusCode < 600;
        public bool IsConnectionFailure => _httpStatusCode == -1;

        [RaiseChange]
        private string _request = string.Empty;

        private string _response = string.Empty;
        public string Response
        {
            get => _response;
            set
            {
                try
                {
                    var parsedJson = JToken.Parse(value);
                    value = parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("What was received could not be parsed as JSON");
                    Debug.WriteLine(e.Message);
                }
                this.RaiseAndSetIfChanged(ref _response, value);
            }
        }

        [RaiseChange]
        private int _connectionAttempt = 1;

        public string ConnectionAttemptAndTotal =>
            $"{ConnectionAttempt}/{QBittorrentService.retryDelays.Length}";

        [RaiseChange]
        private bool _isVisible = isVisible;
    }
}

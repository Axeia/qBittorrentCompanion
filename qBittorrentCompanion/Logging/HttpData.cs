using AutoPropertyChangedGenerator;
using Newtonsoft.Json.Linq;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Diagnostics;

namespace qBittorrentCompanion.Logging
{

    public partial class HttpData(Uri url, int httpStatusCode = -1, bool isVisible = true) : ReactiveObject
    {
        [AutoPropertyChanged]
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

        private Uri _url = url;
        public Uri Url { get => _url; set => _url = value; }
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

        [AutoPropertyChanged]
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

        [AutoPropertyChanged]
        private int _connectionAttempt = 1;

        public string ConnectionAttemptAndTotal =>
            $"{ConnectionAttempt}/{QBittorrentService.RetryCount}";

        [AutoPropertyChanged]
        private bool _isVisible = isVisible;
    }
}

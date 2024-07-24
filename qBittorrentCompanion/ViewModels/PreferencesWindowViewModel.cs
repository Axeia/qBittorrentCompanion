using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public class PreferencesWindowViewModel : INotifyPropertyChanged
    {
        public PreferencesWindowViewModel()
        {
            _ = FetchData();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private TabItem? _selectedTabItem;

        public TabItem? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                if (_selectedTabItem != value)
                {
                    _selectedTabItem = value;
                    OnPropertyChanged(nameof(SelectedTabItem));

                    if (_selectedTabItem != null)
                        SelectedTabText = ((TextBlock)((StackPanel)_selectedTabItem.Header!).Children[1]).Text!;
                    else
                        SelectedTabText = "?";
                }
            }
        }

        private string _selectedTabText = "";
        public string SelectedTabText
        {
            get => _selectedTabText;
            set
            {
                if (_selectedTabText != value)
                {
                    _selectedTabText = value;
                    OnPropertyChanged(nameof(SelectedTabText));
                }
            }
        }


        private async Task FetchData()
        {
            var prefs = await QBittorrentService.QBittorrentClient.GetPreferencesAsync();
            Locale = prefs.Locale;
            SavePath = prefs.SavePath;
            TempPathEnabled = prefs.TempPathEnabled;
            TempPath = prefs.TempPath;
            ScanDirectories = prefs.ScanDirectories;
            ExportDirectory = prefs.ExportDirectory;
            ExportDirectoryForFinished = prefs.ExportDirectoryForFinished;
            MailNotificationEnabled = prefs.MailNotificationEnabled;
            MailNotificationEmailAddress = prefs.MailNotificationEmailAddress;
            MailNotificationSmtpServer = prefs.MailNotificationSmtpServer;
            MailNotificationSslEnabled = prefs.MailNotificationSslEnabled;
            MailNotificationAuthenticationEnabled = prefs.MailNotificationAuthenticationEnabled;
            MailNotificationUsername = prefs.MailNotificationUsername;
            MailNotificationPassword = prefs.MailNotificationPassword;
            AutorunEnabled = prefs.AutorunEnabled;
            AutorunProgram = prefs.AutorunProgram;
            PreallocateAll = prefs.PreallocateAll;
            QueueingEnabled = prefs.QueueingEnabled;
            MaxActiveDownloads = prefs.MaxActiveDownloads;
            MaxActiveTorrents = prefs.MaxActiveTorrents;
            MaxActiveUploads = prefs.MaxActiveUploads;
            DoNotCountSlowTorrents = prefs.DoNotCountSlowTorrents;
            MaxRatioEnabled = prefs.MaxRatioEnabled;
            MaxRatio = prefs.MaxRatio;
            MaxRatioAction = prefs.MaxRatioAction;
            MaxSeedingTime = prefs.MaxSeedingTime;
            MaxSeedingTimeEnabled = prefs.MaxSeedingTimeEnabled;
            MaxInactiveSeedingTime = prefs.MaxInactiveSeedingTime;
            MaxInactiveSeedingTimeEnabled = prefs.MaxInactiveSeedingTimeEnabled;
            AppendExtensionToIncompleteFiles = prefs.AppendExtensionToIncompleteFiles;
            ListenPort = prefs.ListenPort;
            UpnpEnabled = prefs.UpnpEnabled;
            RandomPort = prefs.RandomPort;
            DownloadLimit = prefs.DownloadLimit;
            UploadLimit = prefs.UploadLimit;
            MaxConnections = prefs.MaxConnections;

            MaxConnectionsPerTorrent = prefs.MaxConnectionsPerTorrent;
            MaxUploads = prefs.MaxUploads;
            MaxUploadsPerTorrent = prefs.MaxUploadsPerTorrent;
            EnableUTP = prefs.EnableUTP;
            LimitUTPRate = prefs.LimitUTPRate;
            LimitTcpOverhead = prefs.LimitTcpOverhead;
            AlternativeDownloadLimit = prefs.AlternativeDownloadLimit;
            AlternativeUploadLimit = prefs.AlternativeUploadLimit;
            SchedulerEnabled = prefs.SchedulerEnabled;
            ScheduleFromHour = prefs.ScheduleFromHour;
            ScheduleFromMinute = prefs.ScheduleFromMinute;
            ScheduleToHour = prefs.ScheduleToHour;
            ScheduleToMinute = prefs.ScheduleToMinute;
            SchedulerDays = prefs.SchedulerDays;
            DHT = prefs.DHT;
            DHTSameAsBT = prefs.DHTSameAsBT;
            DHTPort = prefs.DHTPort;
            PeerExchange = prefs.PeerExchange;
            LocalPeerDiscovery = prefs.LocalPeerDiscovery;
            Encryption = prefs.Encryption;
            AnonymousMode = prefs.AnonymousMode;
            ProxyType = prefs.ProxyType;
            ProxyAddress = prefs.ProxyAddress;

            ProxyPort = prefs.ProxyPort;
            ProxyPeerConnections = prefs.ProxyPeerConnections;
            ForceProxy = prefs.ForceProxy;
            ProxyTorrentsOnly = prefs.ProxyTorrentsOnly;
            ProxyAuthenticationEnabled = prefs.ProxyAuthenticationEnabled;
            ProxyUsername = prefs.ProxyUsername;
            ProxyPassword = prefs.ProxyPassword;
            ProxyHostnameLookup = prefs.ProxyHostnameLookup;
            ProxyBittorrent = prefs.ProxyBittorrent;
            ProxyMisc = prefs.ProxyMisc;
            ProxyRss = prefs.ProxyRss;
            IpFilterEnabled = prefs.IpFilterEnabled;
            IpFilterPath = prefs.IpFilterPath;
            IpFilterTrackers = prefs.IpFilterTrackers;
            WebUIAddress = prefs.WebUIAddress;
            WebUIPort = prefs.WebUIPort;
            WebUIDomain = prefs.WebUIDomain;
            WebUIUpnp = prefs.WebUIUpnp;
            WebUIUsername = prefs.WebUIUsername;
            WebUIPassword = prefs.WebUIPassword;
            WebUIPasswordHash = prefs.WebUIPasswordHash;
            WebUIHttps = prefs.WebUIHttps;
            WebUISslKey = prefs.WebUISslKey;
            WebUISslCertificate = prefs.WebUISslCertificate;
            WebUIClickjackingProtection = prefs.WebUIClickjackingProtection;
            WebUICsrfProtection = prefs.WebUICsrfProtection;
            WebUISecureCookie = prefs.WebUISecureCookie;
            WebUIMaxAuthenticationFailures = prefs.WebUIMaxAuthenticationFailures;
            WebUIBanDuration = prefs.WebUIBanDuration;
            WebUICustomHttpHeadersEnabled = prefs.WebUICustomHttpHeadersEnabled;
            WebUICustomHttpHeaders = prefs.WebUICustomHttpHeaders;

            BypassLocalAuthentication = prefs.BypassLocalAuthentication;
            BypassAuthenticationSubnetWhitelistEnabled = prefs.BypassAuthenticationSubnetWhitelistEnabled;
            BypassAuthenticationSubnetWhitelist = prefs.BypassAuthenticationSubnetWhitelist;
            DynamicDnsEnabled = prefs.DynamicDnsEnabled;
            DynamicDnsService = prefs.DynamicDnsService;
            DynamicDnsUsername = prefs.DynamicDnsUsername;
            DynamicDnsPassword = prefs.DynamicDnsPassword;
            DynamicDnsDomain = prefs.DynamicDnsDomain;
            RssRefreshInterval = prefs.RssRefreshInterval;
            RssMaxArticlesPerFeed = prefs.RssMaxArticlesPerFeed;
            RssProcessingEnabled = prefs.RssProcessingEnabled;
            RssAutoDownloadingEnabled = prefs.RssAutoDownloadingEnabled;
            RssDownloadRepackProperEpisodes = prefs.RssDownloadRepackProperEpisodes;
            RssSmartEpisodeFilters = prefs.RssSmartEpisodeFilters;
            AdditionalTrackersEnabled = prefs.AdditionalTrackersEnabled;
            AdditinalTrackers = prefs.AdditinalTrackers;
            BannedIpAddresses = prefs.BannedIpAddresses;
            BittorrentProtocol = prefs.BittorrentProtocol;
            CreateTorrentSubfolder = prefs.CreateTorrentSubfolder;
            AddTorrentPaused = prefs.AddTorrentPaused;
            TorrentFileAutoDeleteMode = prefs.TorrentFileAutoDeleteMode;
            AutoTMMEnabledByDefault = prefs.AutoTMMEnabledByDefault;
            AutoTMMRetainedWhenCategoryChanges = prefs.AutoTMMRetainedWhenCategoryChanges;
            AutoTMMRetainedWhenDefaultSavePathChanges = prefs.AutoTMMRetainedWhenDefaultSavePathChanges;
            AutoTMMRetainedWhenCategorySavePathChanges = prefs.AutoTMMRetainedWhenCategorySavePathChanges;
            MailNotificationSender = prefs.MailNotificationSender;
            LimitLAN = prefs.LimitLAN;
            SlowTorrentDownloadRateThreshold = prefs.SlowTorrentDownloadRateThreshold;
            SlowTorrentUploadRateThreshold = prefs.SlowTorrentUploadRateThreshold;
            SlowTorrentInactiveTime = prefs.SlowTorrentInactiveTime;
            AlternativeWebUIEnabled = prefs.AlternativeWebUIEnabled;
            AlternativeWebUIPath = prefs.AlternativeWebUIPath;
            WebUIHostHeaderValidation = prefs.WebUIHostHeaderValidation;
            WebUISslKeyPath = prefs.WebUISslKeyPath;

            WebUISslCertificatePath = prefs.WebUISslCertificatePath;
        }

        // Properties

        private string _locale;
        public string Locale
        {
            get => _locale;
            set
            {
                if (_locale != value)
                {
                    _locale = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _savePath;
        public string? SavePath
        {
            get { Debug.WriteLine("SavePath requested"); return _savePath; }
            set
            {
                Debug.WriteLine("SavePath set");
                if (_savePath != value)
                {
                    _savePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _tempPathEnabled;
        public bool? TempPathEnabled
        {
            get => _tempPathEnabled;
            set
            {
                if (_tempPathEnabled != value)
                {
                    _tempPathEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _tempPath;
        public string TempPath
        {
            get => _tempPath;
            set
            {
                if (_tempPath != value)
                {
                    _tempPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private IDictionary<string, SaveLocation> _scanDirectories;
        public IDictionary<string, SaveLocation> ScanDirectories
        {
            get => _scanDirectories;
            set
            {
                if (_scanDirectories != value)
                {
                    _scanDirectories = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _exportDirectory;
        public string ExportDirectory
        {
            get => _exportDirectory;
            set
            {
                if (_exportDirectory != value)
                {
                    _exportDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _exportDirectoryForFinished;
        public string ExportDirectoryForFinished
        {
            get => _exportDirectoryForFinished;
            set
            {
                if (_exportDirectoryForFinished != value)
                {
                    _exportDirectoryForFinished = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _mailNotificationEnabled;
        public bool? MailNotificationEnabled
        {
            get => _mailNotificationEnabled;
            set
            {
                if (_mailNotificationEnabled != value)
                {
                    _mailNotificationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _mailNotificationEmailAddress;
        public string MailNotificationEmailAddress
        {
            get => _mailNotificationEmailAddress;
            set
            {
                if (_mailNotificationEmailAddress != value)
                {
                    _mailNotificationEmailAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _mailNotificationSmtpServer;
        public string MailNotificationSmtpServer
        {
            get => _mailNotificationSmtpServer;
            set
            {
                if (_mailNotificationSmtpServer != value)
                {
                    _mailNotificationSmtpServer = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _mailNotificationSslEnabled;
        public bool? MailNotificationSslEnabled
        {
            get => _mailNotificationSslEnabled;
            set
            {
                if (_mailNotificationSslEnabled != value)
                {
                    _mailNotificationSslEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _mailNotificationAuthenticationEnabled;
        public bool? MailNotificationAuthenticationEnabled
        {
            get => _mailNotificationAuthenticationEnabled;
            set
            {
                if (_mailNotificationAuthenticationEnabled != value)
                {
                    _mailNotificationAuthenticationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _mailNotificationUsername;
        public string MailNotificationUsername
        {
            get => _mailNotificationUsername;
            set
            {
                if (_mailNotificationUsername != value)
                {
                    _mailNotificationUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _mailNotificationPassword;
        public string MailNotificationPassword
        {
            get => _mailNotificationPassword;
            set
            {
                if (_mailNotificationPassword != value)
                {
                    _mailNotificationPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _autorunEnabled;
        public bool? AutorunEnabled
        {
            get => _autorunEnabled;
            set
            {
                if (_autorunEnabled != value)
                {
                    _autorunEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _autorunProgram;
        public string AutorunProgram
        {
            get => _autorunProgram;
            set
            {
                if (_autorunProgram != value)
                {
                    _autorunProgram = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _preallocateAll;
        public bool? PreallocateAll
        {
            get => _preallocateAll;
            set
            {
                if (_preallocateAll != value)
                {
                    _preallocateAll = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _queueingEnabled;
        public bool? QueueingEnabled
        {
            get => _queueingEnabled;
            set
            {
                if (_queueingEnabled != value)
                {
                    _queueingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxActiveDownloads;
        public int? MaxActiveDownloads
        {
            get => _maxActiveDownloads;
            set
            {
                if (_maxActiveDownloads != value)
                {
                    _maxActiveDownloads = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxActiveTorrents;
        public int? MaxActiveTorrents
        {
            get => _maxActiveTorrents;
            set
            {
                if (_maxActiveTorrents != value)
                {
                    _maxActiveTorrents = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxActiveUploads;
        public int? MaxActiveUploads
        {
            get => _maxActiveUploads;
            set
            {
                if (_maxActiveUploads != value)
                {
                    _maxActiveUploads = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _doNotCountSlowTorrents;
        public bool? DoNotCountSlowTorrents
        {
            get => _doNotCountSlowTorrents;
            set
            {
                if (_doNotCountSlowTorrents != value)
                {
                    _doNotCountSlowTorrents = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _maxRatioEnabled;
        public bool? MaxRatioEnabled
        {
            get => _maxRatioEnabled;
            set
            {
                if (_maxRatioEnabled != value)
                {
                    _maxRatioEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _maxRatio;
        public double? MaxRatio
        {
            get => _maxRatio;
            set
            {
                if (_maxRatio != value)
                {
                    _maxRatio = value;
                    OnPropertyChanged();
                }
            }
        }

        private MaxRatioAction? _maxRatioAction;
        public MaxRatioAction? MaxRatioAction
        {
            get => _maxRatioAction;
            set
            {
                if (_maxRatioAction != value)
                {
                    _maxRatioAction = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxSeedingTime;
        public int? MaxSeedingTime
        {
            get => _maxSeedingTime;
            set
            {
                if (_maxSeedingTime != value)
                {
                    _maxSeedingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _maxSeedingTimeEnabled;
        public bool? MaxSeedingTimeEnabled
        {
            get => _maxSeedingTimeEnabled;
            set
            {
                if (_maxSeedingTimeEnabled != value)
                {
                    _maxSeedingTimeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxInactiveSeedingTime;
        public int? MaxInactiveSeedingTime
        {
            get => _maxInactiveSeedingTime;
            set
            {
                if (_maxInactiveSeedingTime != value)
                {
                    _maxInactiveSeedingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _maxInactiveSeedingTimeEnabled;
        public bool? MaxInactiveSeedingTimeEnabled
        {
            get => _maxInactiveSeedingTimeEnabled;
            set
            {
                if (_maxInactiveSeedingTimeEnabled != value)
                {
                    _maxInactiveSeedingTimeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _appendExtensionToIncompleteFiles;
        public bool? AppendExtensionToIncompleteFiles
        {
            get => _appendExtensionToIncompleteFiles;
            set
            {
                if (_appendExtensionToIncompleteFiles != value)
                {
                    _appendExtensionToIncompleteFiles = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _listenPort;
        public int? ListenPort
        {
            get => _listenPort;
            set
            {
                if (_listenPort != value)
                {
                    _listenPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _upnpEnabled;
        public bool? UpnpEnabled
        {
            get => _upnpEnabled;
            set
            {
                if (_upnpEnabled != value)
                {
                    _upnpEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _randomPort;
        public bool? RandomPort
        {
            get => _randomPort;
            set
            {
                if (_randomPort != value)
                {
                    _randomPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _downloadLimit;
        public int? DownloadLimit
        {
            get => _downloadLimit;
            set
            {
                if (_downloadLimit != value)
                {
                    _downloadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _uploadLimit;
        public int? UploadLimit
        {
            get => _uploadLimit;
            set
            {
                if (_uploadLimit != value)
                {
                    _uploadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxConnections;
        public int? MaxConnections
        {
            get => _maxConnections;
            set
            {
                if (_maxConnections != value)
                {
                    _maxConnections = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxConnectionsPerTorrent;
        public int? MaxConnectionsPerTorrent
        {
            get => _maxConnectionsPerTorrent;
            set
            {
                if (_maxConnectionsPerTorrent != value)
                {
                    _maxConnectionsPerTorrent = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxUploads;
        public int? MaxUploads
        {
            get => _maxUploads;
            set
            {
                if (_maxUploads != value)
                {
                    _maxUploads = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _maxUploadsPerTorrent;
        public int? MaxUploadsPerTorrent
        {
            get => _maxUploadsPerTorrent;
            set
            {
                if (_maxUploadsPerTorrent != value)
                {
                    _maxUploadsPerTorrent = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _enableUTP;
        public bool? EnableUTP
        {
            get => _enableUTP;
            set
            {
                if (_enableUTP != value)
                {
                    _enableUTP = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _limitUTPRate;
        public bool? LimitUTPRate
        {
            get => _limitUTPRate;
            set
            {
                if (_limitUTPRate != value)
                {
                    _limitUTPRate = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _limitTcpOverhead;
        public bool? LimitTcpOverhead
        {
            get => _limitTcpOverhead;
            set
            {
                if (_limitTcpOverhead != value)
                {
                    _limitTcpOverhead = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _alternativeDownloadLimit;
        public int? AlternativeDownloadLimit
        {
            get => _alternativeDownloadLimit;
            set
            {
                if (_alternativeDownloadLimit != value)
                {
                    _alternativeDownloadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _alternativeUploadLimit;
        public int? AlternativeUploadLimit
        {
            get => _alternativeUploadLimit;
            set
            {
                if (_alternativeUploadLimit != value)
                {
                    _alternativeUploadLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _schedulerEnabled;
        public bool? SchedulerEnabled
        {
            get => _schedulerEnabled;
            set
            {
                if (_schedulerEnabled != value)
                {
                    _schedulerEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _scheduleFromHour;
        public int? ScheduleFromHour
        {
            get => _scheduleFromHour;
            set
            {
                if (_scheduleFromHour != value)
                {
                    _scheduleFromHour = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _scheduleFromMinute;
        public int? ScheduleFromMinute
        {
            get => _scheduleFromMinute;
            set
            {
                if (_scheduleFromMinute != value)
                {
                    _scheduleFromMinute = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _scheduleToHour;
        public int? ScheduleToHour
        {
            get => _scheduleToHour;
            set
            {
                if (_scheduleToHour != value)
                {
                    _scheduleToHour = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _scheduleToMinute;
        public int? ScheduleToMinute
        {
            get => _scheduleToMinute;
            set
            {
                if (_scheduleToMinute != value)
                {
                    _scheduleToMinute = value;
                    OnPropertyChanged();
                }
            }
        }

        private SchedulerDay? _schedulerDays;
        public SchedulerDay? SchedulerDays
        {
            get => _schedulerDays;
            set
            {
                if (_schedulerDays != value)
                {
                    _schedulerDays = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _dht;
        public bool? DHT
        {
            get => _dht;
            set
            {
                if (_dht != value)
                {
                    _dht = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _dhtSameAsBT;
        public bool? DHTSameAsBT
        {
            get => _dhtSameAsBT;
            set
            {
                if (_dhtSameAsBT != value)
                {
                    _dhtSameAsBT = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _dhtPort;
        public int? DHTPort
        {
            get => _dhtPort;
            set
            {
                if (_dhtPort != value)
                {
                    _dhtPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _peerExchange;
        public bool? PeerExchange
        {
            get => _peerExchange;
            set
            {
                if (_peerExchange != value)
                {
                    _peerExchange = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _localPeerDiscovery;
        public bool? LocalPeerDiscovery
        {
            get => _localPeerDiscovery;
            set
            {
                if (_localPeerDiscovery != value)
                {
                    _localPeerDiscovery = value;
                    OnPropertyChanged();
                }
            }
        }

        private Encryption? _encryption;
        public Encryption? Encryption
        {
            get => _encryption;
            set
            {
                if (_encryption != value)
                {
                    _encryption = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _anonymousMode;
        public bool? AnonymousMode
        {
            get => _anonymousMode;
            set
            {
                if (_anonymousMode != value)
                {
                    _anonymousMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private ProxyType? _proxyType;
        public ProxyType? ProxyType
        {
            get => _proxyType;
            set
            {
                if (_proxyType != value)
                {
                    _proxyType = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _proxyAddress;
        public string ProxyAddress
        {
            get => _proxyAddress;
            set
            {
                if (_proxyAddress != value)
                {
                    _proxyAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _proxyPort;
        public int? ProxyPort
        {
            get => _proxyPort;
            set
            {
                if (_proxyPort != value)
                {
                    _proxyPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyPeerConnections;
        public bool? ProxyPeerConnections
        {
            get => _proxyPeerConnections;
            set
            {
                if (_proxyPeerConnections != value)
                {
                    _proxyPeerConnections = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _forceProxy;
        public bool? ForceProxy
        {
            get => _forceProxy;
            set
            {
                if (_forceProxy != value)
                {
                    _forceProxy = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyTorrentsOnly;
        public bool? ProxyTorrentsOnly
        {
            get => _proxyTorrentsOnly;
            set
            {
                if (_proxyTorrentsOnly != value)
                {
                    _proxyTorrentsOnly = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool? _proxyAuthenticationEnabled;
        public bool? ProxyAuthenticationEnabled
        {
            get => _proxyAuthenticationEnabled;
            set
            {
                if (_proxyAuthenticationEnabled != value)
                {
                    _proxyAuthenticationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _proxyUsername;
        public string ProxyUsername
        {
            get => _proxyUsername;
            set
            {
                if (_proxyUsername != value)
                {
                    _proxyUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _proxyPassword;
        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                if (_proxyPassword != value)
                {
                    _proxyPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyHostnameLookup;
        public bool? ProxyHostnameLookup
        {
            get => _proxyHostnameLookup;
            set
            {
                if (_proxyHostnameLookup != value)
                {
                    _proxyHostnameLookup = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyBittorrent;
        public bool? ProxyBittorrent
        {
            get => _proxyBittorrent;
            set
            {
                if (_proxyBittorrent != value)
                {
                    _proxyBittorrent = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyMisc;
        public bool? ProxyMisc
        {
            get => _proxyMisc;
            set
            {
                if (_proxyMisc != value)
                {
                    _proxyMisc = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _proxyRss;
        public bool? ProxyRss
        {
            get => _proxyRss;
            set
            {
                if (_proxyRss != value)
                {
                    _proxyRss = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _ipFilterEnabled;
        public bool? IpFilterEnabled
        {
            get => _ipFilterEnabled;
            set
            {
                if (_ipFilterEnabled != value)
                {
                    _ipFilterEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ipFilterPath;
        public string IpFilterPath
        {
            get => _ipFilterPath;
            set
            {
                if (_ipFilterPath != value)
                {
                    _ipFilterPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _ipFilterTrackers;
        public bool? IpFilterTrackers
        {
            get => _ipFilterTrackers;
            set
            {
                if (_ipFilterTrackers != value)
                {
                    _ipFilterTrackers = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUIAddress;
        public string WebUIAddress
        {
            get => _webUIAddress;
            set
            {
                if (_webUIAddress != value)
                {
                    _webUIAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _webUIPort;
        public int? WebUIPort
        {
            get => _webUIPort;
            set
            {
                if (_webUIPort != value)
                {
                    _webUIPort = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUIDomain;
        public string WebUIDomain
        {
            get => _webUIDomain;
            set
            {
                if (_webUIDomain != value)
                {
                    _webUIDomain = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUIUpnp;
        public bool? WebUIUpnp
        {
            get => _webUIUpnp;
            set
            {
                if (_webUIUpnp != value)
                {
                    _webUIUpnp = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUIUsername;
        public string WebUIUsername
        {
            get => _webUIUsername;
            set
            {
                if (_webUIUsername != value)
                {
                    _webUIUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUIPassword;
        public string WebUIPassword
        {
            get => _webUIPassword;
            set
            {
                if (_webUIPassword != value)
                {
                    _webUIPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUIPasswordHash;
        public string WebUIPasswordHash
        {
            get => _webUIPasswordHash;
            set
            {
                if (_webUIPasswordHash != value)
                {
                    _webUIPasswordHash = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUIHttps;
        public bool? WebUIHttps
        {
            get => _webUIHttps;
            set
            {
                if (_webUIHttps != value)
                {
                    _webUIHttps = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUISslKey;
        public string WebUISslKey
        {
            get => _webUISslKey;
            set
            {
                if (_webUISslKey != value)
                {
                    _webUISslKey = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUISslCertificate;
        public string WebUISslCertificate
        {
            get => _webUISslCertificate;
            set
            {
                if (_webUISslCertificate != value)
                {
                    _webUISslCertificate = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUIClickjackingProtection;
        public bool? WebUIClickjackingProtection
        {
            get => _webUIClickjackingProtection;
            set
            {
                if (_webUIClickjackingProtection != value)
                {
                    _webUIClickjackingProtection = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUICsrfProtection;
        public bool? WebUICsrfProtection
        {
            get => _webUICsrfProtection;
            set
            {
                if (_webUICsrfProtection != value)
                {
                    _webUICsrfProtection = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUISecureCookie;
        public bool? WebUISecureCookie
        {
            get => _webUISecureCookie;
            set
            {
                if (_webUISecureCookie != value)
                {
                    _webUISecureCookie = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _webUIMaxAuthenticationFailures;
        public int? WebUIMaxAuthenticationFailures
        {
            get => _webUIMaxAuthenticationFailures;
            set
            {
                if (_webUIMaxAuthenticationFailures != value)
                {
                    _webUIMaxAuthenticationFailures = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _webUIBanDuration;
        public int? WebUIBanDuration
        {
            get => _webUIBanDuration;
            set
            {
                if (_webUIBanDuration != value)
                {
                    _webUIBanDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUICustomHttpHeadersEnabled;
        public bool? WebUICustomHttpHeadersEnabled
        {
            get => _webUICustomHttpHeadersEnabled;
            set
            {
                if (_webUICustomHttpHeadersEnabled != value)
                {
                    _webUICustomHttpHeadersEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private IList<string> _webUICustomHttpHeaders;
        public IList<string> WebUICustomHttpHeaders
        {
            get => _webUICustomHttpHeaders;
            set
            {
                if (_webUICustomHttpHeaders != value)
                {
                    _webUICustomHttpHeaders = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _bypassLocalAuthentication;
        public bool? BypassLocalAuthentication
        {
            get => _bypassLocalAuthentication;
            set
            {
                if (_bypassLocalAuthentication != value)
                {
                    _bypassLocalAuthentication = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _bypassAuthenticationSubnetWhitelistEnabled;
        public bool? BypassAuthenticationSubnetWhitelistEnabled
        {
            get => _bypassAuthenticationSubnetWhitelistEnabled;
            set
            {
                if (_bypassAuthenticationSubnetWhitelistEnabled != value)
                {
                    _bypassAuthenticationSubnetWhitelistEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private IList<string> _bypassAuthenticationSubnetWhitelist;
        public IList<string> BypassAuthenticationSubnetWhitelist
        {
            get => _bypassAuthenticationSubnetWhitelist;
            set
            {
                if (_bypassAuthenticationSubnetWhitelist != value)
                {
                    _bypassAuthenticationSubnetWhitelist = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _dynamicDnsEnabled;
        public bool? DynamicDnsEnabled
        {
            get => _dynamicDnsEnabled;
            set
            {
                if (_dynamicDnsEnabled != value)
                {
                    _dynamicDnsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private DynamicDnsService? _dynamicDnsService;
        public DynamicDnsService? DynamicDnsService
        {
            get => _dynamicDnsService;
            set
            {
                if (_dynamicDnsService != value)
                {
                    _dynamicDnsService = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dynamicDnsUsername;
        public string DynamicDnsUsername
        {
            get => _dynamicDnsUsername;
            set
            {
                if (_dynamicDnsUsername != value)
                {
                    _dynamicDnsUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dynamicDnsPassword;
        public string DynamicDnsPassword
        {
            get => _dynamicDnsPassword;
            set
            {
                if (_dynamicDnsPassword != value)
                {
                    _dynamicDnsPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dynamicDnsDomain;
        public string DynamicDnsDomain
        {
            get => _dynamicDnsDomain;
            set
            {
                if (_dynamicDnsDomain != value)
                {
                    _dynamicDnsDomain = value;
                    OnPropertyChanged();
                }
            }
        }

        private uint? _rssRefreshInterval;
        public uint? RssRefreshInterval
        {
            get => _rssRefreshInterval;
            set
            {
                if (_rssRefreshInterval != value)
                {
                    _rssRefreshInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _rssMaxArticlesPerFeed;
        public int? RssMaxArticlesPerFeed
        {
            get => _rssMaxArticlesPerFeed;
            set
            {
                if (_rssMaxArticlesPerFeed != value)
                {
                    _rssMaxArticlesPerFeed = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _rssProcessingEnabled;
        public bool? RssProcessingEnabled
        {
            get => _rssProcessingEnabled;
            set
            {
                if (_rssProcessingEnabled != value)
                {
                    _rssProcessingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _rssAutoDownloadingEnabled;
        public bool? RssAutoDownloadingEnabled
        {
            get => _rssAutoDownloadingEnabled;
            set
            {
                if (_rssAutoDownloadingEnabled != value)
                {
                    _rssAutoDownloadingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _rssDownloadRepackProperEpisodes;
        public bool? RssDownloadRepackProperEpisodes
        {
            get => _rssDownloadRepackProperEpisodes;
            set
            {
                if (_rssDownloadRepackProperEpisodes != value)
                {
                    _rssDownloadRepackProperEpisodes = value;
                    OnPropertyChanged();
                }
            }
        }

        private IList<string> _rssSmartEpisodeFilters;
        public IList<string> RssSmartEpisodeFilters
        {
            get => _rssSmartEpisodeFilters;
            set
            {
                if (_rssSmartEpisodeFilters != value)
                {
                    _rssSmartEpisodeFilters = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _additionalTrackersEnabled;
        public bool? AdditionalTrackersEnabled
        {
            get => _additionalTrackersEnabled;
            set
            {
                if (_additionalTrackersEnabled != value)
                {
                    _additionalTrackersEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private IList<string> _additinalTrackers;
        public IList<string> AdditinalTrackers
        {
            get => _additinalTrackers;
            set
            {
                if (_additinalTrackers != value)
                {
                    _additinalTrackers = value;
                    OnPropertyChanged();
                }
            }
        }

        private IList<string> _bannedIpAddresses;
        public IList<string> BannedIpAddresses
        {
            get => _bannedIpAddresses;
            set
            {
                if (_bannedIpAddresses != value)
                {
                    _bannedIpAddresses = value;
                    OnPropertyChanged();
                }
            }
        }

        private BittorrentProtocol? _bittorrentProtocol;
        public BittorrentProtocol? BittorrentProtocol
        {
            get => _bittorrentProtocol;
            set
            {
                if (_bittorrentProtocol != value)
                {
                    _bittorrentProtocol = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _createTorrentSubfolder;
        public bool? CreateTorrentSubfolder
        {
            get => _createTorrentSubfolder;
            set
            {
                if (_createTorrentSubfolder != value)
                {
                    _createTorrentSubfolder = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _addTorrentPaused;
        public bool? AddTorrentPaused
        {
            get => _addTorrentPaused;
            set
            {
                if (_addTorrentPaused != value)
                {
                    _addTorrentPaused = value;
                    OnPropertyChanged();
                }
            }
        }

        private TorrentFileAutoDeleteMode? _torrentFileAutoDeleteMode;
        public TorrentFileAutoDeleteMode? TorrentFileAutoDeleteMode
        {
            get => _torrentFileAutoDeleteMode;
            set
            {
                if (_torrentFileAutoDeleteMode != value)
                {
                    _torrentFileAutoDeleteMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _autoTMMEnabledByDefault;
        public bool? AutoTMMEnabledByDefault
        {
            get => _autoTMMEnabledByDefault;
            set
            {
                if (_autoTMMEnabledByDefault != value)
                {
                    _autoTMMEnabledByDefault = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _autoTMMRetainedWhenCategoryChanges;
        public bool? AutoTMMRetainedWhenCategoryChanges
        {
            get => _autoTMMRetainedWhenCategoryChanges;
            set
            {
                if (_autoTMMRetainedWhenCategoryChanges != value)
                {
                    _autoTMMRetainedWhenCategoryChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _autoTMMRetainedWhenDefaultSavePathChanges;
        public bool? AutoTMMRetainedWhenDefaultSavePathChanges
        {
            get => _autoTMMRetainedWhenDefaultSavePathChanges;
            set
            {
                if (_autoTMMRetainedWhenDefaultSavePathChanges != value)
                {
                    _autoTMMRetainedWhenDefaultSavePathChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _autoTMMRetainedWhenCategorySavePathChanges;
        public bool? AutoTMMRetainedWhenCategorySavePathChanges
        {
            get => _autoTMMRetainedWhenCategorySavePathChanges;
            set
            {
                if (_autoTMMRetainedWhenCategorySavePathChanges != value)
                {
                    _autoTMMRetainedWhenCategorySavePathChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _mailNotificationSender;
        public string MailNotificationSender
        {
            get => _mailNotificationSender;
            set
            {
                if (_mailNotificationSender != value)
                {
                    _mailNotificationSender = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _limitLAN;
        public bool? LimitLAN
        {
            get => _limitLAN;
            set
            {
                if (_limitLAN != value)
                {
                    _limitLAN = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _slowTorrentDownloadRateThreshold;
        public int? SlowTorrentDownloadRateThreshold
        {
            get => _slowTorrentDownloadRateThreshold;
            set
            {
                if (_slowTorrentDownloadRateThreshold != value)
                {
                    _slowTorrentDownloadRateThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _slowTorrentUploadRateThreshold;
        public int? SlowTorrentUploadRateThreshold
        {
            get => _slowTorrentUploadRateThreshold;
            set
            {
                if (_slowTorrentUploadRateThreshold != value)
                {
                    _slowTorrentUploadRateThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _slowTorrentInactiveTime;
        public int? SlowTorrentInactiveTime
        {
            get => _slowTorrentInactiveTime;
            set
            {
                if (_slowTorrentInactiveTime != value)
                {
                    _slowTorrentInactiveTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _alternativeWebUIEnabled;
        public bool? AlternativeWebUIEnabled
        {
            get => _alternativeWebUIEnabled;
            set
            {
                if (_alternativeWebUIEnabled != value)
                {
                    _alternativeWebUIEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _alternativeWebUIPath;
        public string AlternativeWebUIPath
        {
            get => _alternativeWebUIPath;
            set
            {
                if (_alternativeWebUIPath != value)
                {
                    _alternativeWebUIPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _webUIHostHeaderValidation;
        public bool? WebUIHostHeaderValidation
        {
            get => _webUIHostHeaderValidation;
            set
            {
                if (_webUIHostHeaderValidation != value)
                {
                    _webUIHostHeaderValidation = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _webUISslKeyPath;
        public string WebUISslKeyPath
        {
            get => _webUISslKeyPath;
            set
            {
                if (_webUISslKeyPath != value)
                {
                    _webUISslKeyPath = value;
                    OnPropertyChanged();
                }
            }
        }

        // New properties
        private string _webUISslCertificatePath;
        public string WebUISslCertificatePath
        {
            get => _webUISslCertificatePath;
            set
            {
                if (_webUISslCertificatePath != value)
                {
                    _webUISslCertificatePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _webUISessionTimeout;
        public int? WebUISessionTimeout
        {
            get => _webUISessionTimeout;
            set
            {
                if (_webUISessionTimeout != value)
                {
                    _webUISessionTimeout = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentNetworkInterface;
        public string CurrentNetworkInterface
        {
            get => _currentNetworkInterface;
            set
            {
                if (_currentNetworkInterface != value)
                {
                    _currentNetworkInterface = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentInterfaceAddress;
        public string CurrentInterfaceAddress
        {
            get => _currentInterfaceAddress;
            set
            {
                if (_currentInterfaceAddress != value)
                {
                    _currentInterfaceAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _listenOnIPv6Address;
        public bool? ListenOnIPv6Address
        {
            get => _listenOnIPv6Address;
            set
            {
                if (_listenOnIPv6Address != value)
                {
                    _listenOnIPv6Address = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _saveResumeDataInterval;
        public int? SaveResumeDataInterval
        {
            get => _saveResumeDataInterval;
            set
            {
                if (_saveResumeDataInterval != value)
                {
                    _saveResumeDataInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _recheckCompletedTorrents;
        public bool? RecheckCompletedTorrents
        {
            get => _recheckCompletedTorrents;
            set
            {
                if (_recheckCompletedTorrents != value)
                {
                    _recheckCompletedTorrents = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _resolvePeerCountries;
        public bool? ResolvePeerCountries
        {
            get => _resolvePeerCountries;
            set
            {
                if (_resolvePeerCountries != value)
                {
                    _resolvePeerCountries = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentAsynchronousIOThreads;
        public int? LibtorrentAsynchronousIOThreads
        {
            get => _libtorrentAsynchronousIOThreads;
            set
            {
                if (_libtorrentAsynchronousIOThreads != value)
                {
                    _libtorrentAsynchronousIOThreads = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentFilePoolSize;
        public int? LibtorrentFilePoolSize
        {
            get => _libtorrentFilePoolSize;
            set
            {
                if (_libtorrentFilePoolSize != value)
                {
                    _libtorrentFilePoolSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentOutstandingMemoryWhenCheckingTorrent;
        public int? LibtorrentOutstandingMemoryWhenCheckingTorrent
        {
            get => _libtorrentOutstandingMemoryWhenCheckingTorrent;
            set
            {
                if (_libtorrentOutstandingMemoryWhenCheckingTorrent != value)
                {
                    _libtorrentOutstandingMemoryWhenCheckingTorrent = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentDiskCache;
        public int? LibtorrentDiskCache
        {
            get => _libtorrentDiskCache;
            set
            {
                if (_libtorrentDiskCache != value)
                {
                    _libtorrentDiskCache = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentDiskCacheExpiryInterval;
        public int? LibtorrentDiskCacheExpiryInterval
        {
            get => _libtorrentDiskCacheExpiryInterval;
            set
            {
                if (_libtorrentDiskCacheExpiryInterval != value)
                {
                    _libtorrentDiskCacheExpiryInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _libtorrentUseOSCache;
        public bool? LibtorrentUseOSCache
        {
            get => _libtorrentUseOSCache;
            set
            {
                if (_libtorrentUseOSCache != value)
                {
                    _libtorrentUseOSCache = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _libtorrentCoalesceReadsAndWrites;
        public bool? LibtorrentCoalesceReadsAndWrites
        {
            get => _libtorrentCoalesceReadsAndWrites;
            set
            {
                if (_libtorrentCoalesceReadsAndWrites != value)
                {
                    _libtorrentCoalesceReadsAndWrites = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _libtorrentPieceExtentAffinity;
        public bool? LibtorrentPieceExtentAffinity
        {
            get => _libtorrentPieceExtentAffinity;
            set
            {
                if (_libtorrentPieceExtentAffinity != value)
                {
                    _libtorrentPieceExtentAffinity = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool? _libtorrentSendUploadPieceSuggestions;
        public bool? LibtorrentSendUploadPieceSuggestions
        {
            get => _libtorrentSendUploadPieceSuggestions;
            set
            {
                if (_libtorrentSendUploadPieceSuggestions != value)
                {
                    _libtorrentSendUploadPieceSuggestions = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentSendBufferWatermark;
        public int? LibtorrentSendBufferWatermark
        {
            get => _libtorrentSendBufferWatermark;
            set
            {
                if (_libtorrentSendBufferWatermark != value)
                {
                    _libtorrentSendBufferWatermark = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentSendBufferLowWatermark;
        public int? LibtorrentSendBufferLowWatermark
        {
            get => _libtorrentSendBufferLowWatermark;
            set
            {
                if (_libtorrentSendBufferLowWatermark != value)
                {
                    _libtorrentSendBufferLowWatermark = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentSendBufferWatermarkFactor;
        public int? LibtorrentSendBufferWatermarkFactor
        {
            get => _libtorrentSendBufferWatermarkFactor;
            set
            {
                if (_libtorrentSendBufferWatermarkFactor != value)
                {
                    _libtorrentSendBufferWatermarkFactor = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentSocketBacklogSize;
        public int? LibtorrentSocketBacklogSize
        {
            get => _libtorrentSocketBacklogSize;
            set
            {
                if (_libtorrentSocketBacklogSize != value)
                {
                    _libtorrentSocketBacklogSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentOutgoingPortsMin;
        public int? LibtorrentOutgoingPortsMin
        {
            get => _libtorrentOutgoingPortsMin;
            set
            {
                if (_libtorrentOutgoingPortsMin != value)
                {
                    _libtorrentOutgoingPortsMin = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _libtorrentOutgoingPortsMax;
        public int? LibtorrentOutgoingPortsMax
        {
            get => _libtorrentOutgoingPortsMax;
            set
            {
                if (_libtorrentOutgoingPortsMax != value)
                {
                    _libtorrentOutgoingPortsMax = value;
                    OnPropertyChanged();
                }
            }
        }

        private UtpTcpMixedModeAlgorithm? _libtorrentUtpTcpMixedModeAlgorithm;
        public UtpTcpMixedModeAlgorithm? LibtorrentUtpTcpMixedModeAlgorithm
        {
            get => _libtorrentUtpTcpMixedModeAlgorithm;
            set
            {
                if (_libtorrentUtpTcpMixedModeAlgorithm != value)
                {
                    _libtorrentUtpTcpMixedModeAlgorithm = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentAllowMultipleConnectionsFromSameIp property
        private bool? _libtorrentAllowMultipleConnectionsFromSameIp;
        /// <summary>
        /// <inheritdoc cref="QBittorrent.Client.Preferences.LibtorrentAllowMultipleConnectionsFromSameIp"/>
        /// </summary>
        public bool? LibtorrentAllowMultipleConnectionsFromSameIp
        {
            get => _libtorrentAllowMultipleConnectionsFromSameIp;
            set
            {
                if (_libtorrentAllowMultipleConnectionsFromSameIp != value)
                {
                    _libtorrentAllowMultipleConnectionsFromSameIp = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentEnableEmbeddedTracker property
        private bool? _libtorrentEnableEmbeddedTracker;
        public bool? LibtorrentEnableEmbeddedTracker
        {
            get => _libtorrentEnableEmbeddedTracker;
            set
            {
                if (_libtorrentEnableEmbeddedTracker != value)
                {
                    _libtorrentEnableEmbeddedTracker = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentEmbeddedTrackerPort property
        private int? _libtorrentEmbeddedTrackerPort;
        public int? LibtorrentEmbeddedTrackerPort
        {
            get => _libtorrentEmbeddedTrackerPort;
            set
            {
                if (_libtorrentEmbeddedTrackerPort != value)
                {
                    _libtorrentEmbeddedTrackerPort = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentUploadSlotsBehavior property
        private ChokingAlgorithm? _libtorrentUploadSlotsBehavior;
        public ChokingAlgorithm? LibtorrentUploadSlotsBehavior
        {
            get => _libtorrentUploadSlotsBehavior;
            set
            {
                if (_libtorrentUploadSlotsBehavior != value)
                {
                    _libtorrentUploadSlotsBehavior = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentUploadChokingAlgorithm property
        private SeedChokingAlgorithm? _libtorrentUploadChokingAlgorithm;
        public SeedChokingAlgorithm? LibtorrentUploadChokingAlgorithm
        {
            get => _libtorrentUploadChokingAlgorithm;
            set
            {
                if (_libtorrentUploadChokingAlgorithm != value)
                {
                    _libtorrentUploadChokingAlgorithm = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentStrictSuperSeeding property
        private bool? _libtorrentStrictSuperSeeding;
        public bool? LibtorrentStrictSuperSeeding
        {
            get => _libtorrentStrictSuperSeeding;
            set
            {
                if (_libtorrentStrictSuperSeeding != value)
                {
                    _libtorrentStrictSuperSeeding = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentAnnounceToAllTrackers property
        private bool? _libtorrentAnnounceToAllTrackers;
        public bool? LibtorrentAnnounceToAllTrackers
        {
            get => _libtorrentAnnounceToAllTrackers;
            set
            {
                if (_libtorrentAnnounceToAllTrackers != value)
                {
                    _libtorrentAnnounceToAllTrackers = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentAnnounceToAllTiers property
        private bool? _libtorrentAnnounceToAllTiers;
        public bool? LibtorrentAnnounceToAllTiers
        {
            get => _libtorrentAnnounceToAllTiers;
            set
            {
                if (_libtorrentAnnounceToAllTiers != value)
                {
                    _libtorrentAnnounceToAllTiers = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentAnnounceIp property
        private string _libtorrentAnnounceIp;
        public string LibtorrentAnnounceIp
        {
            get => _libtorrentAnnounceIp;
            set
            {
                if (_libtorrentAnnounceIp != value)
                {
                    _libtorrentAnnounceIp = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentStopTrackerTimeout property
        private int? _libtorrentStopTrackerTimeout;
        public int? LibtorrentStopTrackerTimeout
        {
            get => _libtorrentStopTrackerTimeout;
            set
            {
                if (_libtorrentStopTrackerTimeout != value)
                {
                    _libtorrentStopTrackerTimeout = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for LibtorrentMaxConcurrentHttpAnnounces property
        private int? _libtorrentMaxConcurrentHttpAnnounces;
        public int? LibtorrentMaxConcurrentHttpAnnounces
        {
            get => _libtorrentMaxConcurrentHttpAnnounces;
            set
            {
                if (_libtorrentMaxConcurrentHttpAnnounces != value)
                {
                    _libtorrentMaxConcurrentHttpAnnounces = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for TorrentContentLayout property
        private TorrentContentLayout? _torrentContentLayout;
        public TorrentContentLayout? TorrentContentLayout
        {
            get => _torrentContentLayout;
            set
            {
                if (_torrentContentLayout != value)
                {
                    _torrentContentLayout = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backing field for AdditionalData property
        private IDictionary<string, JToken> _additionalData;
        /// <summary>
        /// <inheritdoc cref="QBittorrent.Client.Preferences.AdditionalData"/>
        /// </summary>
        public IDictionary<string, JToken> AdditionalData
        {
            get => _additionalData;
            set
            {
                if (_additionalData != value)
                {
                    _additionalData = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}
using Avalonia.Controls;
using DynamicData;
using Newtonsoft.Json.Linq;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public enum TorrentStopConditions
    {
        None,
        MetadataReceived,
        FilesChecked
    }
    public class PreferencesWindowViewModel : BytesBaseViewModel
    {
        public new static string[] SizeOptions => BytesBaseViewModel.SizeOptions.Take(3).ToArray();

        Dictionary<string, string> localeDictionary = new()
        {
            ["ar"] = "عربي",
            ["az@latin"] = "Azərbaycan dili",
            ["be"] = "Беларуская",
            ["bg"] = "Български",
            ["ca"] = "Català",
            ["cs"] = "Čeština",
            ["da"] = "Dansk",
            ["de"] = "Deutsch",
            ["el"] = "Ελληνικά",
            ["en"] = "English",
            ["en_AU"] = "English (Australia)",
            ["en_GB"] = "English (United Kingdom)",
            ["eo"] = "Esperanto",
            ["es"] = "Español",
            ["et"] = "Eesti, eesti keel",
            ["eu"] = "Euskara",
            ["fa"] = "فارسی",
            ["fi"] = "Suomi",
            ["fr"] = "Français",
            ["gl"] = "Galego",
            ["he"] = "עברית",
            ["hi_IN"] = "हिन्दी, हिंदी",
            ["hr"] = "Hrvatski",
            ["hu"] = "Magyar",
            ["hy"] = "Հայերեն",
            ["id"] = "Bahasa Indonesia",
            ["is"] = "Íslenska",
            ["it"] = "Italiano",
            ["ja"] = "日本語",
            ["ka"] = "ქართული",
            ["ko"] = "한국어",
            ["lt"] = "Lietuvių",
            ["ltg"] = "Latgalīšu volūda",
            ["lv_LV"] = "Latviešu valoda",
            ["mn_MN"] = "Монгол хэл",
            ["ms_MY"] = "بهاس ملايو",
            ["nb"] = "Norsk",
            ["nl"] = "Nederlands",
            ["oc"] = "lenga d'òc",
            ["pl"] = "Polski",
            ["pt_BR"] = "Português brasileiro",
            ["pt_PT"] = "Português",
            ["ro"] = "Română",
            ["ru"] = "Русский",
            ["sk"] = "Slovenčina",
            ["sl"] = "Slovenščina",
            ["sr"] = "Српски",
            ["sv"] = "Svenska",
            ["th"] = "ไทย",
            ["tr"] = "Türkçe",
            ["uk"] = "Українська",
            ["uz@Latn"] = "أۇزبېك‎",
            ["vi"] = "Tiếng Việt",
            ["zh_CN"] = "简体中文",
            ["zh_HK"] = "香港正體字",
            ["zh_TW"] = "正體中文"
        };

        public string[] Languages => localeDictionary.Values.ToArray<string>();

        public enum DataStorageType { Legacy, SQLite }
        public string[] DataStorageTypes => [
            DataConverter.DataStorageTypes.Legacy,
            DataConverter.DataStorageTypes.SQLite
        ];

        public static string[] DayOptions => [
            "Every day", "Weekdays", "Weekends",
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday",
            "Saturday", "Sunday"
        ];

        public string[] ProxyTypes => [
            DataConverter.ProxyTypeDescriptions.None, DataConverter.ProxyTypeDescriptions.Http,
            DataConverter.ProxyTypeDescriptions.Socks5, DataConverter.ProxyTypeDescriptions.HttpAuth,
            DataConverter.ProxyTypeDescriptions.Socks5Auth, DataConverter.ProxyTypeDescriptions.Socks4,
        ];

        public string[] DiskIOReadModes => ["Enable OS cache", "Disable OS cache"];
        public string[] DiskIOWriteModes => ["Enable OS cache", "Disable OS cache", "Write through (requires libtorrent >= 2.0.6)"];

        public string[] UploadSlotsBehaviors => [
            DataConverter.UploadSlotBehaviors.FixedSlots,
            DataConverter.UploadSlotBehaviors.UploadRateBased
        ];

        public string[] UploadChokingAlgorithms => [
            DataConverter.UploadChokingAlgorithms.RoundRobin,
            DataConverter.UploadChokingAlgorithms.FastestUpload,
            DataConverter.UploadChokingAlgorithms.AntiLeech
        ];

        public static string[] DnsServices => [
            DataConverter.DnsServices.None,
            DataConverter.DnsServices.DynDns,
            DataConverter.DnsServices.NoIp
        ];

        public ReactiveCommand<Unit, Unit> RestoreMaxConnectionsCommand { get; }
        public void RestoreMaxConnections() { MaxConnections = 500; }
        public ReactiveCommand<Unit, Unit> RestoreMaxConnectionsPerTorrentCommand { get; }
        public void RestoreMaxConnectionsPerTorrent() { MaxConnectionsPerTorrent = 100; }
        public ReactiveCommand<Unit, Unit> RestoreMaxUploadsCommand { get; }
        public void RestoreMaxUploads() { MaxUploads = 5; }
        public ReactiveCommand<Unit, Unit> RestoreMaxUploadsPerTorrentCommand { get; }
        public void RestoreMaxUploadsPerTorrent() { MaxUploadsPerTorrent = 5; }
        public ReactiveCommand<string, Unit> OpenUrlCommand { get; }

#pragma warning disable CS8618, CS8603 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PreferencesWindowViewModel()
#pragma warning restore CS8618, CS8603 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            RestoreMaxConnectionsCommand = ReactiveCommand.Create(RestoreMaxConnections);
            RestoreMaxConnectionsPerTorrentCommand = ReactiveCommand.Create(RestoreMaxConnectionsPerTorrent);
            RestoreMaxUploadsCommand = ReactiveCommand.Create(RestoreMaxUploads);
            RestoreMaxUploadsPerTorrentCommand = ReactiveCommand.Create(RestoreMaxUploadsPerTorrent);

            SaveDataCommand = ReactiveCommand.CreateFromTask(SaveData);
            OpenUrlCommand = ReactiveCommand.Create<string>(OpenUrl);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void OpenUrl(string url)
        {
            // Logic to open the URL in a browser
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public class IpDummy : ReactiveObject
        {
            private string _ip = "";
            public string Ip
            {
                get { return _ip; }
                set => this.RaiseAndSetIfChanged(ref _ip, value);
            }

            public IpDummy(string ip)
            {
                Ip = ip;
            }
        }

        private List<string> _networkInterfaces = [""];
        public List<string> NetworkInterfaces
        {
            get => _networkInterfaces;
            set
            {
                if(_networkInterfaces != value)
                {
                    _networkInterfaces = value;
                    OnPropertyChanged(nameof(NetworkInterfaces));
                }
            }
        }

        public class NetworkAddressDummy : ReactiveObject
        {
            private string _networkAddress = "";
            public string NetworkAddress
            {
                get { return _networkAddress; }
                set => this.RaiseAndSetIfChanged(ref _networkAddress, value);
            }

            public NetworkAddressDummy(string networkAddress)
            {
                NetworkAddress = networkAddress;
            }
        }

        private Dictionary<string, string> _networkInterfaceAddresses = new()
        {
            [""] = "All addresses",
            ["0.0.0.0"] = "All IPv4 addresses",
            ["::"] = "All IPv6 addresses",
        };

        public HashSet<string> NetworkInterfaceAddresses => 
            _networkInterfaceAddresses.Values.ToHashSet();

        private int _currentNetworkInterfaceIndex = 0;
        public int CurrentNetworkInterfaceIndex
        {
            get => _currentNetworkInterfaceIndex;
            set
            {
                if(_currentNetworkInterfaceIndex != value)
                {
                    _currentNetworkInterfaceIndex = value;
                    OnPropertyChanged(nameof(CurrentNetworkInterfaceIndex));
                }
            }
        }

        private int _currentNetworkAddressIndex = 0;
        public int CurrentNetworkAddressIndex
        {
            get => _currentNetworkAddressIndex;
            set
            {
                if (_currentNetworkAddressIndex != value)
                {
                    _currentNetworkAddressIndex = value;
                    OnPropertyChanged(nameof(CurrentNetworkAddressIndex));
                }
            }
        }

        public async Task FetchData()
        {
            var networkInterfacesTask = QBittorrentService.QBittorrentClient.GetNetworkInterfacesAsync();
            var networkInterfaceAddressesTask = QBittorrentService.QBittorrentClient.GetNetworkInterfaceAddressesAsync();
            var prefsTask = QBittorrentService.QBittorrentClient.GetPreferencesAsync();

            // Send out all 3 request simultanously
            await Task.WhenAll(networkInterfacesTask, networkInterfaceAddressesTask, prefsTask);

            var networkInterfaces = await networkInterfacesTask;
            NetworkInterfaces = new List<string> { "Any interface" }
                .Concat(networkInterfaces.Select(n => n.Id).ToList())
                .ToList();
            foreach (string address in await networkInterfaceAddressesTask)
                _networkInterfaceAddresses.Add(address, address);
            OnPropertyChanged(nameof(NetworkInterfaceAddresses));

            var prefs = await prefsTask;

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
            DownloadLimit = prefs.DownloadLimit ?? 0;
            UploadLimit = prefs.UploadLimit ?? 0;
            MaxConnections = prefs.MaxConnections;
            MaxConnectionsPerTorrent = prefs.MaxConnectionsPerTorrent;
            MaxUploads = prefs.MaxUploads;
            MaxUploadsPerTorrent = prefs.MaxUploadsPerTorrent;
            EnableUTP = prefs.EnableUTP;
            LimitUTPRate = prefs.LimitUTPRate;
            LimitTcpOverhead = prefs.LimitTcpOverhead;
            AlternativeDownloadLimit = prefs.AlternativeDownloadLimit ?? 0;
            AlternativeUploadLimit = prefs.AlternativeUploadLimit ?? 0;
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
            ProxyType = prefs.ProxyType ?? ProxyType.None;
            ProxyAddress = prefs.ProxyAddress;
            CurrentNetworkInterface = prefs.CurrentNetworkInterface;
            if (CurrentNetworkInterface != "")
                CurrentNetworkInterfaceIndex = NetworkInterfaces.IndexOf(CurrentNetworkInterface);
            CurrentInterfaceAddress = prefs.CurrentInterfaceAddress;
            if (_networkInterfaceAddresses.ContainsKey(CurrentNetworkInterface))
                CurrentNetworkAddressIndex =
                    _networkInterfaceAddresses.Keys.IndexOf(CurrentInterfaceAddress);
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
            foreach(string header in prefs.WebUICustomHttpHeaders)
                WebUICustomHttpHeaders.Add(new CustomHttpHeader(header));

            BypassLocalAuthentication = prefs.BypassLocalAuthentication;
            BypassAuthenticationSubnetWhitelistEnabled = prefs.BypassAuthenticationSubnetWhitelistEnabled;
            BypassAuthenticationSubnetWhitelist = prefs.BypassAuthenticationSubnetWhitelist;
            DynamicDnsEnabled = prefs.DynamicDnsEnabled;
            DynamicDnsService = prefs.DynamicDnsService ?? DynamicDnsService.None;
            DynamicDnsUsername = prefs.DynamicDnsUsername;
            DynamicDnsPassword = prefs.DynamicDnsPassword;
            DynamicDnsDomain = prefs.DynamicDnsDomain;
            RssRefreshInterval = prefs.RssRefreshInterval;
            RssMaxArticlesPerFeed = prefs.RssMaxArticlesPerFeed;
            RssProcessingEnabled = prefs.RssProcessingEnabled;
            RssAutoDownloadingEnabled = prefs.RssAutoDownloadingEnabled;
            RssDownloadRepackProperEpisodes = prefs.RssDownloadRepackProperEpisodes;
            foreach (string smartEpFilter in prefs.RssSmartEpisodeFilters)
                RssSmartEpisodeFilters.Add(new SmartEpFilterDummy(smartEpFilter));
            RssSmartEpisodeFilters.Add(new SmartEpFilterDummy(""));
            AdditionalTrackersEnabled = prefs.AdditionalTrackersEnabled;
            foreach (string tracker in prefs.AdditinalTrackers)
                AdditinalTrackers.Add(new TrackerDummy(tracker));
            AdditinalTrackers.Add(new TrackerDummy(""));
            foreach (string bannedIpAddres in prefs.BannedIpAddresses)
                BannedIpAddresses.Add(new IpDummy(bannedIpAddres));
            BannedIpAddresses.Add(new IpDummy("")); //Empty entry for adding entries

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

            LibtorrentAsynchronousIOThreads = prefs.LibtorrentAsynchronousIOThreads;
            SaveResumeDataInterval = prefs.SaveResumeDataInterval;
            RecheckCompletedTorrents = prefs.RecheckCompletedTorrents;
            LibtorrentEnableEmbeddedTracker = prefs.LibtorrentEnableEmbeddedTracker;
            LibtorrentEmbeddedTrackerPort = prefs.LibtorrentEmbeddedTrackerPort;
            LibtorrentFilePoolSize = prefs.LibtorrentFilePoolSize;
            LibtorrentOutstandingMemoryWhenCheckingTorrent = prefs.LibtorrentOutstandingMemoryWhenCheckingTorrent;
            LibtorrentDiskCache = prefs.LibtorrentDiskCache;
            LibtorrentDiskCacheExpiryInterval = prefs.LibtorrentDiskCacheExpiryInterval;
            LibtorrentCoalesceReadsAndWrites = prefs.LibtorrentCoalesceReadsAndWrites;
            LibtorrentPieceExtentAffinity = prefs.LibtorrentPieceExtentAffinity;
            LibtorrentSendUploadPieceSuggestions = prefs.LibtorrentSendUploadPieceSuggestions;
            LibtorrentSendBufferWatermark = prefs.LibtorrentSendBufferWatermark;
            LibtorrentSendBufferLowWatermark = prefs.LibtorrentSendBufferLowWatermark;
            LibtorrentSendBufferWatermarkFactor = prefs.LibtorrentSendBufferWatermarkFactor;
            LibtorrentSocketBacklogSize = prefs.LibtorrentSocketBacklogSize;
            LibtorrentOutgoingPortsMin = prefs.LibtorrentOutgoingPortsMin;
            LibtorrentOutgoingPortsMax = prefs.LibtorrentOutgoingPortsMax;
            LibtorrentAllowMultipleConnectionsFromSameIp = prefs.LibtorrentAllowMultipleConnectionsFromSameIp;
            LibtorrentMaxConcurrentHttpAnnounces = prefs.LibtorrentMaxConcurrentHttpAnnounces;
            LibtorrentStopTrackerTimeout = prefs.LibtorrentStopTrackerTimeout;
            LibtorrentUploadSlotsBehavior = prefs.LibtorrentUploadSlotsBehavior;
            LibtorrentUploadChokingAlgorithm = prefs.LibtorrentUploadChokingAlgorithm;
            LibtorrentAnnounceToAllTrackers = prefs.LibtorrentAnnounceToAllTrackers;
            LibtorrentAnnounceToAllTiers = prefs.LibtorrentAnnounceToAllTiers;
            LibtorrentAnnounceIp = prefs.LibtorrentAnnounceIp;
            LibtorrentMaxConcurrentHttpAnnounces = prefs.LibtorrentMaxConcurrentHttpAnnounces;
            LibtorrentStopTrackerTimeout = prefs.LibtorrentStopTrackerTimeout;
            ResolvePeerCountries = prefs.ResolvePeerCountries;
            TorrentContentLayoutProperty = prefs.TorrentContentLayout ?? TorrentContentLayout.Original;

            // Additional data properties memory_working_set_limit
            FileLogEnabled = bool.Parse(prefs.AdditionalData["file_log_enabled"].ToString());
            FileLogBackupEnabled = bool.Parse(prefs.AdditionalData["file_log_backup_enabled"].ToString());
            FileLogDeleteOld = bool.Parse(prefs.AdditionalData["file_log_delete_old"].ToString());
            FileLogMaxSize = int.Parse(prefs.AdditionalData["file_log_max_size"].ToString());
            FileLogAge = int.Parse(prefs.AdditionalData["file_log_age"].ToString());
            FileLogAgeType = int.Parse(prefs.AdditionalData["file_log_age_type"].ToString());
            PerformanceWarning = bool.Parse(prefs.AdditionalData["performance_warning"].ToString());

            AddToTopOfQueue = bool.Parse(prefs.AdditionalData["add_to_top_of_queue"].ToString());
            TorrentStopCondition = Enum.Parse<TorrentStopConditions>(prefs.AdditionalData["torrent_stop_condition"].ToString());
            UseSubcategories = bool.Parse(prefs.AdditionalData["use_subcategories"].ToString());
            ExcludedFileNamesEnabled = bool.Parse(prefs.AdditionalData["excluded_file_names_enabled"].ToString());
            ExcludedFileNames = prefs.AdditionalData["excluded_file_names"].ToString();
            AutorunOnTorrentAddedEnabled = bool.Parse(prefs.AdditionalData["autorun_on_torrent_added_enabled"].ToString());
            AutorunOnTorrentAddedProgram = prefs.AdditionalData["autorun_on_torrent_added_program"].ToString();

            I2pEnabled = bool.Parse(prefs.AdditionalData["i2p_enabled"].ToString());
            I2pAddress = prefs.AdditionalData["i2p_address"].ToString();
            I2pPort = int.Parse(prefs.AdditionalData["i2p_port"].ToString());
            I2pMixedMode = bool.Parse(prefs.AdditionalData["i2p_mixed_mode"].ToString());

            ResumeDataStorageType = Enum.Parse<DataStorageType>(prefs.AdditionalData["resume_data_storage_type"].ToString());
            MemoryWorkingSetLimit = int.Parse(prefs.AdditionalData["memory_working_set_limit"].ToString());
            RefreshInterval = int.Parse(prefs.AdditionalData["refresh_interval"].ToString());
            ReannounceWhenAddressChanged = bool.Parse(prefs.AdditionalData["reannounce_when_address_changed"].ToString());
            EmbeddedTrackerPortForwarding = bool.Parse(prefs.AdditionalData["embedded_tracker_port_forwarding"].ToString());

            MaxActiveCheckingTorrents = int.Parse(prefs.AdditionalData["max_active_checking_torrents"].ToString());

            WebUiReverseProxiesList = prefs.AdditionalData["web_ui_reverse_proxies_list"].ToString();
            WebUiReverseProxyEnabled = bool.Parse(prefs.AdditionalData["web_ui_reverse_proxy_enabled"].ToString());
            BDecodeDepthLimit = int.Parse(prefs.AdditionalData["bdecode_depth_limit"].ToString());
            BDecodeTokenLimit = int.Parse(prefs.AdditionalData["bdecode_token_limit"].ToString());
            HashingThreads = int.Parse(prefs.AdditionalData["hashing_threads"].ToString());
            DiskQueueSize = int.Parse(prefs.AdditionalData["disk_queue_size"].ToString());
            DiskIOType = int.Parse(prefs.AdditionalData["disk_io_type"].ToString());
            DiskIOReadMode = int.Parse(prefs.AdditionalData["disk_io_read_mode"].ToString());
            DiskIOWriteMode = int.Parse(prefs.AdditionalData["disk_io_write_mode"].ToString());
            ConnectionSpeed = int.Parse(prefs.AdditionalData["connection_speed"].ToString());
            SocketSendBufferSize = int.Parse(prefs.AdditionalData["socket_send_buffer_size"].ToString());
            SocketReceiveBufferSize = int.Parse(prefs.AdditionalData["socket_receive_buffer_size"].ToString());
            UpnpLeaseDuration = int.Parse(prefs.AdditionalData["upnp_lease_duration"].ToString());
            PeerTos = int.Parse(prefs.AdditionalData["peer_tos"].ToString());
            IdnSupportEnabled = bool.Parse(prefs.AdditionalData["idn_support_enabled"].ToString());
            ValidateHttpsTrackerCertificate = bool.Parse(prefs.AdditionalData["validate_https_tracker_certificate"].ToString());
            SsrfMitigation = bool.Parse(prefs.AdditionalData["ssrf_mitigation"].ToString());
            BlockPeersOnPrivilegedPorts = bool.Parse(prefs.AdditionalData["block_peers_on_privileged_ports"].ToString());
            PeerTurnover = int.Parse(prefs.AdditionalData["peer_turnover"].ToString());
            PeerTurnoverCutoff = int.Parse(prefs.AdditionalData["peer_turnover_cutoff"].ToString());
            PeerTurnoverInterval = int.Parse(prefs.AdditionalData["peer_turnover_interval"].ToString());
            RequestQueueSize = int.Parse(prefs.AdditionalData["request_queue_size"].ToString());
            I2pInboundQuantity = int.Parse(prefs.AdditionalData["i2p_inbound_quantity"].ToString());
            I2pOutboundQuantity = int.Parse(prefs.AdditionalData["i2p_outbound_quantity"].ToString());
            I2pInboundLength = int.Parse(prefs.AdditionalData["i2p_inbound_length"].ToString());
            I2pOutboundLength = int.Parse(prefs.AdditionalData["i2p_outbound_length"].ToString());
        }

        public ReactiveCommand<Unit, Unit> SaveDataCommand { get; }

        public async Task SaveData()
        {
            var extPrefs = new ExtendedPreferences();

            extPrefs.Locale = Locale;
            extPrefs.SavePath = SavePath;
            extPrefs.TempPathEnabled = TempPathEnabled;
            extPrefs.TempPath = TempPath;
            extPrefs.ScanDirectories = ScanDirectories;
            extPrefs.ExportDirectory = ExportDirectory;
            extPrefs.ExportDirectoryForFinished = ExportDirectoryForFinished;
            extPrefs.MailNotificationEnabled = MailNotificationEnabled;
            extPrefs.MailNotificationEmailAddress = MailNotificationEmailAddress;
            extPrefs.MailNotificationSmtpServer = MailNotificationSmtpServer;
            extPrefs.MailNotificationSslEnabled = MailNotificationSslEnabled;
            extPrefs.MailNotificationAuthenticationEnabled = MailNotificationAuthenticationEnabled;
            extPrefs.MailNotificationUsername = MailNotificationUsername;
            extPrefs.MailNotificationPassword = MailNotificationPassword;
            extPrefs.AutorunEnabled = AutorunEnabled;
            extPrefs.AutorunProgram = AutorunProgram;
            extPrefs.PreallocateAll = PreallocateAll;
            extPrefs.QueueingEnabled = QueueingEnabled;
            extPrefs.MaxActiveDownloads = MaxActiveDownloads;
            extPrefs.MaxActiveTorrents = MaxActiveTorrents;
            extPrefs.MaxActiveUploads = MaxActiveUploads;
            extPrefs.DoNotCountSlowTorrents = DoNotCountSlowTorrents;
            extPrefs.MaxRatioEnabled = MaxRatioEnabled;
            extPrefs.MaxRatio = MaxRatio;
            extPrefs.MaxRatioAction = MaxRatioAction;
            extPrefs.MaxSeedingTime = MaxSeedingTime;
            extPrefs.MaxSeedingTimeEnabled = MaxSeedingTimeEnabled;
            extPrefs.MaxInactiveSeedingTime = MaxInactiveSeedingTime;
            extPrefs.MaxInactiveSeedingTimeEnabled = MaxInactiveSeedingTimeEnabled;
            extPrefs.AppendExtensionToIncompleteFiles = AppendExtensionToIncompleteFiles;
            extPrefs.ListenPort = ListenPort;
            extPrefs.UpnpEnabled = UpnpEnabled;
            extPrefs.RandomPort = RandomPort;
            extPrefs.DownloadLimit = DownloadLimit;
            extPrefs.UploadLimit = UploadLimit;
            extPrefs.MaxConnections = MaxConnections;
            extPrefs.MaxConnectionsPerTorrent = MaxConnectionsPerTorrent;
            extPrefs.MaxUploads = MaxUploads;
            extPrefs.MaxUploadsPerTorrent = MaxUploadsPerTorrent;
            extPrefs.EnableUTP = EnableUTP;
            extPrefs.LimitUTPRate = LimitUTPRate;
            extPrefs.LimitTcpOverhead = LimitTcpOverhead;
            extPrefs.AlternativeDownloadLimit = AlternativeDownloadLimit;
            extPrefs.AlternativeUploadLimit = AlternativeUploadLimit;
            extPrefs.SchedulerEnabled = SchedulerEnabled;
            extPrefs.ScheduleFromHour = ScheduleFromHour;
            extPrefs.ScheduleFromMinute = ScheduleFromMinute;
            extPrefs.ScheduleToHour = ScheduleToHour;
            extPrefs.ScheduleToMinute = ScheduleToMinute;
            extPrefs.SchedulerDays = SchedulerDays;
            extPrefs.DHT = DHT;
            extPrefs.DHTSameAsBT = DHTSameAsBT;
            extPrefs.DHTPort = DHTPort;
            extPrefs.PeerExchange = PeerExchange;
            extPrefs.LocalPeerDiscovery = LocalPeerDiscovery;
            extPrefs.Encryption = Encryption;
            extPrefs.AnonymousMode = AnonymousMode;
            extPrefs.ProxyType = ProxyType;
            extPrefs.ProxyAddress = ProxyAddress;
            extPrefs.CurrentNetworkInterface = CurrentNetworkInterface;
            extPrefs.CurrentInterfaceAddress = CurrentInterfaceAddress;
            extPrefs.ProxyPort = ProxyPort;
            extPrefs.ProxyPeerConnections = ProxyPeerConnections;
            extPrefs.ForceProxy = ForceProxy;
            extPrefs.ProxyTorrentsOnly = ProxyTorrentsOnly;
            extPrefs.ProxyAuthenticationEnabled = ProxyAuthenticationEnabled;
            extPrefs.ProxyUsername = ProxyUsername;
            extPrefs.ProxyPassword = ProxyPassword;
            extPrefs.ProxyHostnameLookup = ProxyHostnameLookup;
            extPrefs.ProxyBittorrent = ProxyBittorrent;
            extPrefs.ProxyMisc = ProxyMisc;
            extPrefs.ProxyRss = ProxyRss;
            extPrefs.IpFilterEnabled = IpFilterEnabled;
            extPrefs.IpFilterPath = IpFilterPath;
            extPrefs.IpFilterTrackers = IpFilterTrackers;
            extPrefs.WebUIAddress = WebUIAddress;
            extPrefs.WebUIPort = WebUIPort;
            extPrefs.WebUIDomain = WebUIDomain;
            extPrefs.WebUIUpnp = WebUIUpnp;
            extPrefs.WebUIUsername = WebUIUsername;
            extPrefs.WebUIPassword = WebUIPassword;
            extPrefs.WebUIPasswordHash = WebUIPasswordHash;
            extPrefs.WebUIHttps = WebUIHttps;
            extPrefs.WebUISslKey = WebUISslKey;
            extPrefs.WebUISslCertificate = WebUISslCertificate;
            extPrefs.WebUIClickjackingProtection = WebUIClickjackingProtection;
            extPrefs.WebUICsrfProtection = WebUICsrfProtection;
            extPrefs.WebUISecureCookie = WebUISecureCookie;
            extPrefs.WebUIMaxAuthenticationFailures = WebUIMaxAuthenticationFailures;
            extPrefs.WebUIBanDuration = WebUIBanDuration;
            extPrefs.WebUICustomHttpHeadersEnabled = WebUICustomHttpHeadersEnabled;
            
            foreach (var header in WebUICustomHttpHeaders)
                extPrefs.WebUICustomHttpHeaders.Add(header.Header);

            extPrefs.BypassLocalAuthentication = BypassLocalAuthentication;
            extPrefs.BypassAuthenticationSubnetWhitelistEnabled = BypassAuthenticationSubnetWhitelistEnabled;
            extPrefs.BypassAuthenticationSubnetWhitelist = BypassAuthenticationSubnetWhitelist;
            extPrefs.DynamicDnsEnabled = DynamicDnsEnabled;
            extPrefs.DynamicDnsService = DynamicDnsService;
            extPrefs.DynamicDnsUsername = DynamicDnsUsername;
            extPrefs.DynamicDnsPassword = DynamicDnsPassword;
            extPrefs.DynamicDnsDomain = DynamicDnsDomain;
            extPrefs.RssRefreshInterval = RssRefreshInterval;
            extPrefs.RssMaxArticlesPerFeed = RssMaxArticlesPerFeed;
            extPrefs.RssProcessingEnabled = RssProcessingEnabled;
            extPrefs.RssAutoDownloadingEnabled = RssAutoDownloadingEnabled;
            extPrefs.RssDownloadRepackProperEpisodes = RssDownloadRepackProperEpisodes;
            extPrefs.RssSmartEpisodeFilters = RssSmartEpisodeFilters
                .Where(e => e.SmartEpFilter != string.Empty)
                .Select(x => x.SmartEpFilter)
                .ToList();
            extPrefs.AdditionalTrackersEnabled = AdditionalTrackersEnabled;
            extPrefs.AdditinalTrackers = AdditinalTrackers
                .Where(t => t.Tracker != string.Empty)
                .Select(t => t.Tracker)
                .ToList();
            extPrefs.BannedIpAddresses = BannedIpAddresses
                .Where(ipa => ipa.Ip != string.Empty)
                .Select(ipa => ipa.Ip)
                .ToList();
            extPrefs.BittorrentProtocol = BittorrentProtocol;
            extPrefs.CreateTorrentSubfolder = CreateTorrentSubfolder;
            extPrefs.AddTorrentPaused = AddTorrentPaused;
            extPrefs.TorrentFileAutoDeleteMode = TorrentFileAutoDeleteMode;
            extPrefs.AutoTMMEnabledByDefault = AutoTMMEnabledByDefault;
            extPrefs.AutoTMMRetainedWhenCategoryChanges = AutoTMMRetainedWhenCategoryChanges;
            extPrefs.AutoTMMRetainedWhenDefaultSavePathChanges = AutoTMMRetainedWhenDefaultSavePathChanges;
            extPrefs.AutoTMMRetainedWhenCategorySavePathChanges = AutoTMMRetainedWhenCategorySavePathChanges;
            extPrefs.MailNotificationSender = MailNotificationSender;
            extPrefs.LimitLAN = LimitLAN;
            extPrefs.SlowTorrentDownloadRateThreshold = SlowTorrentDownloadRateThreshold;
            extPrefs.SlowTorrentUploadRateThreshold = SlowTorrentUploadRateThreshold;
            extPrefs.SlowTorrentInactiveTime = SlowTorrentInactiveTime;
            extPrefs.AlternativeWebUIEnabled = AlternativeWebUIEnabled;
            extPrefs.AlternativeWebUIPath = AlternativeWebUIPath;
            extPrefs.WebUIHostHeaderValidation = WebUIHostHeaderValidation;
            extPrefs.WebUISslKeyPath = WebUISslKeyPath;

            extPrefs.WebUISslCertificatePath = WebUISslCertificatePath;

            extPrefs.LibtorrentAsynchronousIOThreads = LibtorrentAsynchronousIOThreads;
            extPrefs.SaveResumeDataInterval = SaveResumeDataInterval;
            extPrefs.RecheckCompletedTorrents = RecheckCompletedTorrents;
            extPrefs.LibtorrentEnableEmbeddedTracker = LibtorrentEnableEmbeddedTracker;
            extPrefs.LibtorrentEmbeddedTrackerPort = LibtorrentEmbeddedTrackerPort;
            extPrefs.LibtorrentFilePoolSize = LibtorrentFilePoolSize;
            extPrefs.LibtorrentOutstandingMemoryWhenCheckingTorrent = LibtorrentOutstandingMemoryWhenCheckingTorrent;
            extPrefs.LibtorrentDiskCache = LibtorrentDiskCache;
            extPrefs.LibtorrentDiskCacheExpiryInterval = LibtorrentDiskCacheExpiryInterval;
            extPrefs.LibtorrentCoalesceReadsAndWrites = LibtorrentCoalesceReadsAndWrites;
            extPrefs.LibtorrentPieceExtentAffinity = LibtorrentPieceExtentAffinity;
            extPrefs.LibtorrentSendUploadPieceSuggestions = LibtorrentSendUploadPieceSuggestions;
            extPrefs.LibtorrentSendBufferWatermark = LibtorrentSendBufferWatermark;
            extPrefs.LibtorrentSendBufferLowWatermark = LibtorrentSendBufferLowWatermark;
            extPrefs.LibtorrentSendBufferWatermarkFactor = LibtorrentSendBufferWatermarkFactor;
            extPrefs.LibtorrentSocketBacklogSize = LibtorrentSocketBacklogSize;
            extPrefs.LibtorrentOutgoingPortsMin = LibtorrentOutgoingPortsMin;
            extPrefs.LibtorrentOutgoingPortsMax = LibtorrentOutgoingPortsMax;
            extPrefs.LibtorrentAllowMultipleConnectionsFromSameIp = LibtorrentAllowMultipleConnectionsFromSameIp;
            extPrefs.LibtorrentMaxConcurrentHttpAnnounces = LibtorrentMaxConcurrentHttpAnnounces;
            extPrefs.LibtorrentStopTrackerTimeout = LibtorrentStopTrackerTimeout;
            extPrefs.LibtorrentUploadSlotsBehavior = LibtorrentUploadSlotsBehavior;
            extPrefs.LibtorrentUploadChokingAlgorithm = LibtorrentUploadChokingAlgorithm;
            extPrefs.LibtorrentAnnounceToAllTrackers = LibtorrentAnnounceToAllTrackers;
            extPrefs.LibtorrentAnnounceToAllTiers = LibtorrentAnnounceToAllTiers;
            extPrefs.LibtorrentAnnounceIp = LibtorrentAnnounceIp;
            extPrefs.LibtorrentMaxConcurrentHttpAnnounces = LibtorrentMaxConcurrentHttpAnnounces;
            extPrefs.LibtorrentStopTrackerTimeout = LibtorrentStopTrackerTimeout;
            extPrefs.ResolvePeerCountries = ResolvePeerCountries;
            extPrefs.TorrentContentLayout = TorrentContentLayoutProperty;

            // Additional data properties 
            extPrefs.FileLogEnabled = FileLogEnabled;
            extPrefs.FileLogBackupEnabled = FileLogBackupEnabled;
            extPrefs.FileLogDeleteOld = FileLogDeleteOld;
            extPrefs.FileLogMaxSize = FileLogMaxSize;
            extPrefs.FileLogAge = FileLogAge;
            extPrefs.FileLogAgeType = FileLogAgeType;
            extPrefs.PerformanceWarning = PerformanceWarning;

            extPrefs.AddToTopOfQueue = AddToTopOfQueue;
            extPrefs.TorrentStopCondition = TorrentStopCondition;
            extPrefs.UseSubcategories = UseSubcategories;
            extPrefs.ExcludedFileNamesEnabled = ExcludedFileNamesEnabled;
            extPrefs.ExcludedFileNames = ExcludedFileNames;
            extPrefs.AutorunOnTorrentAddedEnabled = AutorunOnTorrentAddedEnabled;
            extPrefs.AutorunOnTorrentAddedProgram = AutorunOnTorrentAddedProgram;

            extPrefs.I2pEnabled = I2pEnabled;
            extPrefs.I2pAddress = I2pAddress;
            extPrefs.I2pPort = I2pPort;
            extPrefs.I2pMixedMode = I2pMixedMode;

            extPrefs.ResumeDataStorageType = ResumeDataStorageType;
            extPrefs.MemoryWorkingSetLimit = MemoryWorkingSetLimit;
            extPrefs.RefreshInterval = RefreshInterval;
            extPrefs.ReannounceWhenAddressChanged = ReannounceWhenAddressChanged;
            extPrefs.EmbeddedTrackerPortForwarding = EmbeddedTrackerPortForwarding;

            extPrefs.MaxActiveCheckingTorrents = MaxActiveCheckingTorrents;

            extPrefs.WebUiReverseProxiesList = WebUiReverseProxiesList;
            extPrefs.WebUiReverseProxyEnabled = WebUiReverseProxyEnabled;
            extPrefs.BDecodeDepthLimit = BDecodeDepthLimit;
            extPrefs.BDecodeTokenLimit = BDecodeTokenLimit;
            extPrefs.HashingThreads = HashingThreads;
            extPrefs.DiskQueueSize = DiskQueueSize;
            extPrefs.DiskIOType = DiskIOType;
            extPrefs.DiskIOReadMode = DiskIOReadMode;
            extPrefs.DiskIOWriteMode = DiskIOWriteMode;
            extPrefs.ConnectionSpeed = ConnectionSpeed;
            extPrefs.SocketSendBufferSize = SocketSendBufferSize;
            extPrefs.SocketReceiveBufferSize = SocketReceiveBufferSize;
            extPrefs.UpnpLeaseDuration = UpnpLeaseDuration;
            extPrefs.PeerTos = PeerTos;
            extPrefs.IdnSupportEnabled = IdnSupportEnabled;
            extPrefs.ValidateHttpsTrackerCertificate = ValidateHttpsTrackerCertificate;
            extPrefs.SsrfMitigation = SsrfMitigation;
            extPrefs.BlockPeersOnPrivilegedPorts = BlockPeersOnPrivilegedPorts;
            extPrefs.PeerTurnover = PeerTurnover;
            extPrefs.PeerTurnoverCutoff = PeerTurnoverCutoff;
            extPrefs.PeerTurnoverInterval = PeerTurnoverInterval;
            extPrefs.RequestQueueSize = RequestQueueSize;
            extPrefs.I2pInboundQuantity = I2pInboundQuantity;
            extPrefs.I2pOutboundQuantity = I2pOutboundQuantity;
            extPrefs.I2pInboundLength = I2pInboundLength;
            extPrefs.I2pOutboundLength = I2pOutboundLength;

            await QBittorrentService.QBittorrentClient.SetPreferencesAsync(extPrefs);
        }

        // Properties
        private string? _locale;

        /// <summary>
        /// <inheritdoc cref="Preferences.Locale"/><br/>
        ///  Use <see cref="Locale_Proxy">Locale_Proxy</see> to display to the end user
        /// </summary>
        public string? Locale
        {
            get => _locale;
            set
            {
                if (_locale != value)
                {
                    _locale = value;
                    OnPropertyChanged(nameof(Locale));

                    _locale_Proxy = value == null
                        ? null
                        : localeDictionary[value];
                    OnPropertyChanged(nameof(Locale_Proxy));
                }
            }
        }

        private string? _locale_Proxy;
        /// <summary>
        /// Functions as a proxy for <see cref="Locale">Locale</see>, this can be used 
        /// to display to the end user so they see the actual language whilst
        ///<see cref="Locale">Locale</see> contains the value for the backend.
        /// </summary>
        public string? Locale_Proxy
        {
            get => _locale_Proxy;
            set
            {
                if (_locale_Proxy != value)
                {
                    _locale_Proxy = value;
                    OnPropertyChanged(nameof(Locale_Proxy));

                    _locale = value == null
                        ? null
                        : localeDictionary.First(l => l.Value == value).Key;
                    OnPropertyChanged(nameof(Locale));
                }
            }
        }

        private string? _savePath;
        public string? SavePath
        {
            get => _savePath;
            set
            {
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
                    OnPropertyChanged(nameof(TempPathEnabled));
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
                    OnPropertyChanged(nameof(TempPath));
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
                    OnPropertyChanged(nameof(ScanDirectories));
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
                    OnPropertyChanged(nameof(ExportDirectory));
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
                    OnPropertyChanged(nameof(ExportDirectoryForFinished));
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
                    OnPropertyChanged(nameof(MailNotificationEnabled));
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
                    OnPropertyChanged(nameof(MailNotificationEmailAddress));
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
                    OnPropertyChanged(nameof(MailNotificationSmtpServer));
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
                    OnPropertyChanged(nameof(MailNotificationSslEnabled));
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
                    OnPropertyChanged(nameof(MailNotificationAuthenticationEnabled));
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
                    OnPropertyChanged(nameof(MailNotificationUsername));
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
                    OnPropertyChanged(nameof(MailNotificationPassword));
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
                    OnPropertyChanged(nameof(AutorunEnabled));
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
                    OnPropertyChanged(nameof(AutorunProgram));
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
                    OnPropertyChanged(nameof(PreallocateAll));
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
                    OnPropertyChanged(nameof(QueueingEnabled));
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
                    OnPropertyChanged(nameof(MaxActiveDownloads));
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
                    OnPropertyChanged(nameof(MaxActiveTorrents));
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
                    OnPropertyChanged(nameof(MaxActiveUploads));
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
                    OnPropertyChanged(nameof(DoNotCountSlowTorrents));
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
                    OnPropertyChanged(nameof(MaxRatioEnabled));
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
                    OnPropertyChanged(nameof(MaxRatio));
                }
            }
        }

        /// <summary>
        /// FIXME: MaxRatioAction only supports 2 values, there should be 4?
        /// </summary>
        private MaxRatioAction? _maxRatioAction;
        public MaxRatioAction? MaxRatioAction
        {
            get => _maxRatioAction;
            set
            {
                if (_maxRatioAction != value)
                {
                    _maxRatioAction = value;
                    OnPropertyChanged(nameof(MaxRatioAction));
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
                    OnPropertyChanged(nameof(MaxSeedingTime));
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
                    OnPropertyChanged(nameof(MaxSeedingTimeEnabled));
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
                    OnPropertyChanged(nameof(MaxInactiveSeedingTime));
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
                    OnPropertyChanged(nameof(MaxInactiveSeedingTimeEnabled));
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
                    OnPropertyChanged(nameof(AppendExtensionToIncompleteFiles));
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
                    OnPropertyChanged(nameof(ListenPort));
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
                    OnPropertyChanged(nameof(UpnpEnabled));
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
                    OnPropertyChanged(nameof(RandomPort));
                }
            }
        }

        private int _downloadLimit = 0;
        public int DownloadLimit
        {
            get => _downloadLimit;
            set
            {
                if (_downloadLimit != value)
                {
                    _downloadLimit = value;
                    OnPropertyChanged(nameof(DownloadLimit));
                }
            }
        }

        private int _uploadLimit = 0; 
        public int UploadLimit
        {
            get => _uploadLimit;
            set
            {
                if (_uploadLimit != value)
                {
                    _uploadLimit = value;
                    OnPropertyChanged(nameof(UploadLimit));
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
                    OnPropertyChanged(nameof(MaxConnections));
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
                    OnPropertyChanged(nameof(MaxConnectionsPerTorrent));
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
                    OnPropertyChanged(nameof(MaxUploads));
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
                    OnPropertyChanged(nameof(MaxUploadsPerTorrent));
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
                    OnPropertyChanged(nameof(EnableUTP));
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
                    OnPropertyChanged(nameof(LimitUTPRate));
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
                    OnPropertyChanged(nameof(LimitTcpOverhead));
                }
            }
        }

        private int _alternativeDownloadLimit = 0;
        public int AlternativeDownloadLimit
        {
            get => _alternativeDownloadLimit;
            set
            {
                if (_alternativeDownloadLimit != value)
                {
                    _alternativeDownloadLimit = value;
                    OnPropertyChanged(nameof(AlternativeDownloadLimit));
                }
            }
        }

        private int _alternativeUploadLimit = 0;
        public int AlternativeUploadLimit
        {
            get => _alternativeUploadLimit;
            set
            {
                if (_alternativeUploadLimit != value)
                {
                    _alternativeUploadLimit = value;
                    OnPropertyChanged(nameof(AlternativeUploadLimit));
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
                    OnPropertyChanged(nameof(SchedulerEnabled));
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
                    OnPropertyChanged(nameof(ScheduleFromHour));
                    OnPropertyChanged(nameof(ScheduleFrom));
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
                    OnPropertyChanged(nameof(ScheduleFromMinute));
                    OnPropertyChanged(nameof(ScheduleFrom));
                }
            }
        }

        /// <summary>
        /// Formats given Hours/Minutes to a format suitable for use with a TimePicker 
        /// (its SelectedTime attribute specifically)
        /// If the gives hours or minutes are null they'll default to 00.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <returns>String with hours and minutes formatted as double digits seperated by a colon, e.g. 00:00</returns>
        private string ScheduleFormat(int? hours, int? minutes)
        {
            return string.Format(
                "{0:D2}:{1:D2}",
                (hours ?? 0), (minutes ?? 0)
            );
        }

        public string ScheduleFrom
        {
            get => ScheduleFormat(ScheduleFromHour, ScheduleFromMinute);
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
                    OnPropertyChanged(nameof(ScheduleToHour));
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
                    OnPropertyChanged(nameof(ScheduleToMinute));
                }
            }
        }
        public string ScheduleTo
        {
            get => ScheduleFormat(ScheduleToHour, ScheduleToMinute);
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
                    OnPropertyChanged(nameof(SchedulerDays));
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
                    OnPropertyChanged(nameof(DHT));
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
                    OnPropertyChanged(nameof(DHTSameAsBT));
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
                    OnPropertyChanged(nameof(DHTPort));
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
                    OnPropertyChanged(nameof(PeerExchange));
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
                    OnPropertyChanged(nameof(LocalPeerDiscovery));
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
                    OnPropertyChanged(nameof(Encryption));
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
                    OnPropertyChanged(nameof(AnonymousMode));
                }
            }
        }

        private ProxyType _proxyType;
        public ProxyType ProxyType
        {
            get => _proxyType;
            set
            {
                if (_proxyType != value)
                {
                    _proxyType = value;
                    OnPropertyChanged(nameof(ProxyType));
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
                    OnPropertyChanged(nameof(ProxyAddress));
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
                    OnPropertyChanged(nameof(ProxyPort));
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
                    OnPropertyChanged(nameof(ProxyPeerConnections));
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
                    OnPropertyChanged(nameof(ForceProxy));
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
                    OnPropertyChanged(nameof(ProxyTorrentsOnly));
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
                    OnPropertyChanged(nameof(ProxyAuthenticationEnabled));
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
                    OnPropertyChanged(nameof(ProxyUsername));
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
                    OnPropertyChanged(nameof(ProxyPassword));
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
                    OnPropertyChanged(nameof(ProxyHostnameLookup));
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
                    OnPropertyChanged(nameof(ProxyBittorrent));
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
                    OnPropertyChanged(nameof(ProxyMisc));
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
                    OnPropertyChanged(nameof(ProxyRss));
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
                    OnPropertyChanged(nameof(IpFilterEnabled));
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
                    OnPropertyChanged(nameof(IpFilterPath));
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
                    OnPropertyChanged(nameof(IpFilterTrackers));
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
                    OnPropertyChanged(nameof(WebUIAddress));
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
                    OnPropertyChanged(nameof(WebUIPort));
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
                    OnPropertyChanged(nameof(WebUIDomain));
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
                    OnPropertyChanged(nameof(WebUIUpnp));
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
                    OnPropertyChanged(nameof(WebUIUsername));
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
                    OnPropertyChanged(nameof(WebUIPassword));
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
                    OnPropertyChanged(nameof(WebUIPasswordHash));
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
                    OnPropertyChanged(nameof(WebUIHttps));
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
                    OnPropertyChanged(nameof(WebUISslKey));
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
                    OnPropertyChanged(nameof(WebUISslCertificate));
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
                    OnPropertyChanged(nameof(WebUIClickjackingProtection));
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
                    OnPropertyChanged(nameof(WebUICsrfProtection));
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
                    OnPropertyChanged(nameof(WebUISecureCookie));
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
                    OnPropertyChanged(nameof(WebUIMaxAuthenticationFailures));
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
                    OnPropertyChanged(nameof(WebUIBanDuration));
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
                    OnPropertyChanged(nameof(WebUICustomHttpHeadersEnabled));
                }
            }
        }
        public class CustomHttpHeader : ReactiveObject
        {
            private string _header = "";
            public string Header
            {
                get { return _header; }
                set => this.RaiseAndSetIfChanged(ref _header, value);
            }

            public CustomHttpHeader(string header)
            {
                Header = header;
            }
        }

        private ObservableCollection<CustomHttpHeader> _webUICustomHttpHeaders = [];
        public ObservableCollection<CustomHttpHeader> WebUICustomHttpHeaders
        {
            get => _webUICustomHttpHeaders;
            set
            {
                if (_webUICustomHttpHeaders != value)
                {
                    _webUICustomHttpHeaders = value;
                    OnPropertyChanged(nameof(WebUICustomHttpHeaders));
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
                    OnPropertyChanged(nameof(BypassLocalAuthentication));
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
                    OnPropertyChanged(nameof(BypassAuthenticationSubnetWhitelistEnabled));
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
                    OnPropertyChanged(nameof(BypassAuthenticationSubnetWhitelist));
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
                    OnPropertyChanged(nameof(DynamicDnsEnabled));
                }
            }
        }

        private DynamicDnsService _dynamicDnsService = DynamicDnsService.None;
        public DynamicDnsService DynamicDnsService
        {
            get => _dynamicDnsService;
            set
            {
                if (_dynamicDnsService != value)
                {
                    _dynamicDnsService = value;
                    OnPropertyChanged(nameof(DynamicDnsService));
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
                    OnPropertyChanged(nameof(DynamicDnsUsername));
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
                    OnPropertyChanged(nameof(DynamicDnsPassword));
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
                    OnPropertyChanged(nameof(DynamicDnsDomain));
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
                    OnPropertyChanged(nameof(RssRefreshInterval));
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
                    OnPropertyChanged(nameof(RssMaxArticlesPerFeed));
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
                    OnPropertyChanged(nameof(RssProcessingEnabled));
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
                    OnPropertyChanged(nameof(RssAutoDownloadingEnabled));
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
                    OnPropertyChanged(nameof(RssDownloadRepackProperEpisodes));
                }
            }
        }

        public class SmartEpFilterDummy : ReactiveObject
        {
            private string _smartEpFilter = "";
            public string SmartEpFilter
            {
                get { return _smartEpFilter; }
                set => this.RaiseAndSetIfChanged(ref _smartEpFilter, value);
            }

            public SmartEpFilterDummy(string smartEpFilter)
            {
                SmartEpFilter = smartEpFilter;
            }
        }

        private ObservableCollection<SmartEpFilterDummy> _rssSmartEpisodeFilters = [];
        public ObservableCollection<SmartEpFilterDummy> RssSmartEpisodeFilters
        {
            get => _rssSmartEpisodeFilters;
            set
            {
                if (_rssSmartEpisodeFilters != value)
                {
                    _rssSmartEpisodeFilters = value;
                    OnPropertyChanged(nameof(RssSmartEpisodeFilters));
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
                    OnPropertyChanged(nameof(AdditionalTrackersEnabled));
                }
            }
        }

        public class TrackerDummy : ReactiveObject
        {
            private string _tracker = "";
            public string Tracker
            {
                get { return _tracker; }
                set => this.RaiseAndSetIfChanged(ref _tracker, value);
            }

            public TrackerDummy(string tracker)
            {
                Tracker = tracker;
            }
        }

        private ObservableCollection<TrackerDummy> _additinalTrackers = [];
        public ObservableCollection<TrackerDummy> AdditinalTrackers
        {
            get => _additinalTrackers;
            set
            {
                if (_additinalTrackers != value)
                {
                    _additinalTrackers = value;
                    OnPropertyChanged(nameof(AdditinalTrackers));
                }
            }
        }

        private ObservableCollection<IpDummy> _bannedIpAddresses = [];
        public ObservableCollection<IpDummy> BannedIpAddresses
        {
            get => _bannedIpAddresses;
            set
            {
                if (_bannedIpAddresses != value)
                {
                    _bannedIpAddresses = value;
                    OnPropertyChanged(nameof(BannedIpAddresses));
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
                    OnPropertyChanged(nameof(BittorrentProtocol));
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
                    OnPropertyChanged(nameof(CreateTorrentSubfolder));
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
                    OnPropertyChanged(nameof(AddTorrentPaused));
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
                    OnPropertyChanged(nameof(TorrentFileAutoDeleteMode));
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
                    OnPropertyChanged(nameof(AutoTMMEnabledByDefault));
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
                    OnPropertyChanged(nameof(AutoTMMRetainedWhenCategoryChanges));
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
                    OnPropertyChanged(nameof(AutoTMMRetainedWhenDefaultSavePathChanges));
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
                    OnPropertyChanged(nameof(AutoTMMRetainedWhenCategorySavePathChanges));
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
                    OnPropertyChanged(nameof(MailNotificationSender));
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
                    OnPropertyChanged(nameof(LimitLAN));
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
                    OnPropertyChanged(nameof(SlowTorrentDownloadRateThreshold));
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
                    OnPropertyChanged(nameof(SlowTorrentUploadRateThreshold));
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
                    OnPropertyChanged(nameof(SlowTorrentInactiveTime));
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
                    OnPropertyChanged(nameof(AlternativeWebUIEnabled));
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
                    OnPropertyChanged(nameof(AlternativeWebUIPath));
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
                    OnPropertyChanged(nameof(WebUIHostHeaderValidation));
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
                    OnPropertyChanged(nameof(WebUISslKeyPath));
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
                    OnPropertyChanged(nameof(WebUISslCertificatePath));
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
                    OnPropertyChanged(nameof(WebUISessionTimeout));
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
                    OnPropertyChanged(nameof(CurrentNetworkInterface));
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
                    OnPropertyChanged(nameof(CurrentInterfaceAddress));
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
                    OnPropertyChanged(nameof(ListenOnIPv6Address));
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
                    OnPropertyChanged(nameof(SaveResumeDataInterval));
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
                    OnPropertyChanged(nameof(RecheckCompletedTorrents));
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
                    OnPropertyChanged(nameof(ResolvePeerCountries));
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
                    OnPropertyChanged(nameof(LibtorrentAsynchronousIOThreads));
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
                    OnPropertyChanged(nameof(LibtorrentFilePoolSize));
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
                    OnPropertyChanged(nameof(LibtorrentOutstandingMemoryWhenCheckingTorrent));
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
                    OnPropertyChanged(nameof(LibtorrentDiskCache));
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
                    OnPropertyChanged(nameof(LibtorrentDiskCacheExpiryInterval));
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
                    OnPropertyChanged(nameof(LibtorrentUseOSCache));
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
                    OnPropertyChanged(nameof(LibtorrentCoalesceReadsAndWrites));
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
                    OnPropertyChanged(nameof(LibtorrentPieceExtentAffinity));
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
                    OnPropertyChanged(nameof(_libtorrentSendUploadPieceSuggestions));
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
                    OnPropertyChanged(nameof(LibtorrentSendBufferWatermark));
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
                    OnPropertyChanged(nameof(LibtorrentSendBufferLowWatermark));
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
                    OnPropertyChanged(nameof(LibtorrentSendBufferWatermarkFactor));
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
                    OnPropertyChanged(nameof(LibtorrentSocketBacklogSize));
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
                    OnPropertyChanged(nameof(LibtorrentOutgoingPortsMin));
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
                    OnPropertyChanged(nameof(LibtorrentOutgoingPortsMax));
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
                    OnPropertyChanged(nameof(LibtorrentUtpTcpMixedModeAlgorithm));
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
                    OnPropertyChanged(nameof(LibtorrentAllowMultipleConnectionsFromSameIp));
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
                    OnPropertyChanged(nameof(LibtorrentEnableEmbeddedTracker));
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
                    OnPropertyChanged(nameof(LibtorrentEmbeddedTrackerPort));
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
                    OnPropertyChanged(nameof(LibtorrentUploadSlotsBehavior));
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
                    OnPropertyChanged(nameof(LibtorrentUploadChokingAlgorithm));
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
                    OnPropertyChanged(nameof(LibtorrentStrictSuperSeeding));
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
                    OnPropertyChanged(nameof(LibtorrentAnnounceToAllTrackers));
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
                    OnPropertyChanged(nameof(LibtorrentAnnounceToAllTiers));
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
                    OnPropertyChanged(nameof(LibtorrentAnnounceIp));
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
                    OnPropertyChanged(nameof(LibtorrentStopTrackerTimeout));
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
                    OnPropertyChanged(nameof(LibtorrentMaxConcurrentHttpAnnounces));
                }
            }
        }

        // Backing field for TorrentContentLayout property
        private TorrentContentLayout _torrentContentLayout = TorrentContentLayout.Original;
        /// <summary>
        /// <inheritdoc cref="Preferences.TorrentContentLayout"/>
        /// </summary>
        public TorrentContentLayout TorrentContentLayoutProperty
        {
            get => _torrentContentLayout;
            set
            {
                if (_torrentContentLayout != value)
                {
                    _torrentContentLayout = value;
                    OnPropertyChanged(nameof(TorrentContentLayoutProperty));
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
                    OnPropertyChanged(nameof(AdditionalData));
                }
            }
        }

        ///Passed this point will be the data found under "AdditionalData" 
        ///
        ///

        private int _bdecodeDepthLimit = 0;
        public int BDecodeDepthLimit
        {
            get => _bdecodeDepthLimit;
            set
            {
                if (_bdecodeDepthLimit != value)
                {
                    _bdecodeDepthLimit = value;
                    OnPropertyChanged(nameof(BDecodeDepthLimit));
                }
            }
        }

        private int? _bdecodeTokenLimit;
        public int? BDecodeTokenLimit
        {
            get => _bdecodeTokenLimit;
            set
            {
                if (_bdecodeTokenLimit != value)
                {
                    _bdecodeTokenLimit = value;
                    OnPropertyChanged(nameof(BDecodeTokenLimit));
                }
            }
        }

        private int? _hashingThreads;
        public int? HashingThreads
        {
            get => _hashingThreads;
            set
            {
                if (value != _hashingThreads)
                {
                    _hashingThreads = value;
                    OnPropertyChanged(nameof(HashingThreads));
                }
            }
        }

        private int? _diskQueueSize;
        public int? DiskQueueSize
        {
            get => _diskQueueSize;
            set
            {
                if (value != _diskQueueSize)
                {
                    _diskQueueSize = value;
                    OnPropertyChanged(nameof(DiskQueueSize));
                }
            }
        }

        private int? _diskIOType;
        public int? DiskIOType
        {
            get => _diskIOType;
            set
            {
                if (value != _diskIOType)
                {
                    _diskIOType = value;
                    OnPropertyChanged(nameof(DiskIOType));
                }
            }
        }

        private int? _diskIOReadMode;
        public int? DiskIOReadMode
        {
            get => _diskIOReadMode;
            set
            {
                if (value != _diskIOReadMode)
                {
                    _diskIOReadMode = value;
                    OnPropertyChanged(nameof(DiskIOReadMode));
                }
            }
        }

        private int? _diskIOWrite;
        public int? DiskIOWriteMode
        {
            get => _diskIOWrite;
            set
            {
                if (value != _diskIOWrite)
                {
                    _diskIOWrite = value;
                    OnPropertyChanged(nameof(DiskIOWriteMode));
                }
            }
        }

        private int? _connectionSpeed;
        public int? ConnectionSpeed
        {
            get => _connectionSpeed;
            set
            {
                if (value != _connectionSpeed)
                {
                    _connectionSpeed = value;
                    OnPropertyChanged(nameof(ConnectionSpeed));
                }
            }
        }

        private int? _socketSendBufferSize;
        public int? SocketSendBufferSize
        {
            get => _socketSendBufferSize;
            set
            {
                if (value != _socketSendBufferSize)
                {
                    _socketSendBufferSize = value;
                    OnPropertyChanged(nameof(SocketSendBufferSize));
                }
            }
        }

        private int? _socketReceiveBufferSize;
        public int? SocketReceiveBufferSize
        {
            get => _socketReceiveBufferSize;
            set
            {
                if (value != _socketReceiveBufferSize)
                {
                    _socketReceiveBufferSize = value;
                    OnPropertyChanged(nameof(SocketReceiveBufferSize));
                }
            }
        }

        private int? _upnpLeaseDuration;
        public int? UpnpLeaseDuration
        {
            get => _upnpLeaseDuration;
            set
            {
                if (value != _upnpLeaseDuration)
                {
                    _upnpLeaseDuration = value;
                    OnPropertyChanged(nameof(UpnpLeaseDuration));
                }
            }
        }

        private int? _peerTos;
        public int? PeerTos
        {
            get => _peerTos;
            set
            {
                if (value != _peerTos)
                {
                    _peerTos = value;
                    OnPropertyChanged(nameof(PeerTos));
                }
            }
        }

        private bool? _idnSupportEnabled;
        public bool? IdnSupportEnabled
        {
            get => _idnSupportEnabled;
            set
            {
                if (value != _idnSupportEnabled)
                {
                    _idnSupportEnabled = value;
                    OnPropertyChanged(nameof(IdnSupportEnabled));
                }
            }
        }

        private bool? _validateHttpsTrackerCertificate;
        public bool? ValidateHttpsTrackerCertificate
        {
            get => _validateHttpsTrackerCertificate;
            set
            {
                if (value != _validateHttpsTrackerCertificate)
                {
                    _validateHttpsTrackerCertificate = value;
                    OnPropertyChanged(nameof(ValidateHttpsTrackerCertificate));
                }
            }
        }

        private bool? _ssrfMitigation;
        public bool? SsrfMitigation
        {
            get => _ssrfMitigation;
            set
            {
                if (value != _ssrfMitigation)
                {
                    _ssrfMitigation = value;
                    OnPropertyChanged(nameof(SsrfMitigation));
                }
            }
        }

        private bool? _blockPeersOnPrivilegedPorts;
        public bool? BlockPeersOnPrivilegedPorts
        {
            get => _blockPeersOnPrivilegedPorts;
            set
            {
                if (value != _blockPeersOnPrivilegedPorts)
                {
                    _blockPeersOnPrivilegedPorts = value;
                    OnPropertyChanged(nameof(BlockPeersOnPrivilegedPorts));
                }
            }
        }

        private int? _peerTurnover;
        public int? PeerTurnover
        {
            get => _peerTurnover;
            set
            {
                if (value != _peerTurnover)
                {
                    _peerTurnover = value;
                    OnPropertyChanged(nameof(PeerTurnover));
                }
            }
        }

        private int? _peerTurnoverCutoff;
        public int? PeerTurnoverCutoff
        {
            get => _peerTurnoverCutoff;
            set
            {
                if (value != _peerTurnoverCutoff)
                {
                    _peerTurnoverCutoff = value;
                    OnPropertyChanged(nameof(PeerTurnoverCutoff));
                }
            }
        }

        private int? _peerTurnoverInterval;
        public int? PeerTurnoverInterval
        {
            get => _peerTurnoverInterval;
            set
            {
                if (value != _peerTurnoverInterval)
                {
                    _peerTurnoverInterval = value;
                    OnPropertyChanged(nameof(PeerTurnoverInterval));
                }
            }
        }

        private int? _requestQueueSize;
        public int? RequestQueueSize
        {
            get => _requestQueueSize;
            set
            {
                if (value != _requestQueueSize)
                {
                    _requestQueueSize = value;
                    OnPropertyChanged(nameof(RequestQueueSize));
                }
            }
        }

        private int? _i2pInboundQuantity;
        public int? I2pInboundQuantity
        {
            get => _i2pInboundQuantity;
            set
            {
                if (value != _i2pInboundQuantity)
                {
                    _i2pInboundQuantity = value;
                    OnPropertyChanged(nameof(I2pInboundQuantity));
                }
            }
        }

        private int? _i2pOutboundQuantity;
        public int? I2pOutboundQuantity
        {
            get => _i2pOutboundQuantity;
            set
            {
                if (value != _i2pOutboundQuantity)
                {
                    _i2pOutboundQuantity = value;
                    OnPropertyChanged(nameof(I2pOutboundQuantity));
                }
            }
        }

        private int? _i2pInboundLength;
        public int? I2pInboundLength
        {
            get => _i2pInboundLength;
            set
            {
                if (value != _i2pInboundLength)
                {
                    _i2pInboundLength = value;
                    OnPropertyChanged(nameof(I2pInboundLength));
                }
            }
        }

        private int? _i2pOutboundLength;
        public int? I2pOutboundLength
        {
            get => _i2pOutboundLength;
            set
            {
                if (value != _i2pOutboundLength)
                {
                    _i2pOutboundLength = value;
                    OnPropertyChanged(nameof(I2pOutboundLength));
                }
            }
        }

        private DataStorageType? _resumeDataStorageType;
        public DataStorageType? ResumeDataStorageType
        {
            get => _resumeDataStorageType;
            set
            {
                if (value != _resumeDataStorageType)
                {
                    _resumeDataStorageType = value;
                    OnPropertyChanged(nameof(ResumeDataStorageType));
                }
            }
        }

        private int? _memoryWorkingSetLimit;
        public int? MemoryWorkingSetLimit
        {
            get => _memoryWorkingSetLimit;
            set
            {
                if (value != _memoryWorkingSetLimit)
                {
                    _memoryWorkingSetLimit = value;
                    OnPropertyChanged(nameof(MemoryWorkingSetLimit));
                }
            }
        }

        private int? _refreshInterval;
        public int? RefreshInterval
        {
            get => _refreshInterval;
            set
            {
                if (value != _refreshInterval)
                {
                    _refreshInterval = value;
                    OnPropertyChanged(nameof(RefreshInterval));
                }
            }
        }

        private bool? _reannounceWhenAddressChanged;
        public bool? ReannounceWhenAddressChanged
        {
            get => _reannounceWhenAddressChanged;
            set
            {
                if (value != _reannounceWhenAddressChanged)
                {
                    _reannounceWhenAddressChanged = value;
                    OnPropertyChanged(nameof(ReannounceWhenAddressChanged));
                }
            }
        }

        private bool? _embeddedTrackerPortForwarding;
        public bool? EmbeddedTrackerPortForwarding
        {
            get => _embeddedTrackerPortForwarding;
            set
            {
                if (value != _embeddedTrackerPortForwarding)
                {
                    _embeddedTrackerPortForwarding = value;
                    OnPropertyChanged(nameof(EmbeddedTrackerPortForwarding));
                }
            }
        }

        private bool? _fileLogEnabled;
        public bool? FileLogEnabled
        {
            get => _fileLogEnabled;
            set
            {
                if (value != _fileLogEnabled)
                {
                    _fileLogEnabled = value;
                    OnPropertyChanged(nameof(FileLogEnabled));
                }
            }
        }

        private bool? _fileLogBackupEnabled;
        public bool? FileLogBackupEnabled
        {
            get => _fileLogBackupEnabled;
            set
            {
                if (value != _fileLogBackupEnabled)
                {
                    _fileLogBackupEnabled = value;
                    OnPropertyChanged(nameof(FileLogBackupEnabled));
                }
            }
        }

        private bool? _fileLogDeleteOld;
        public bool? FileLogDeleteOld
        {
            get => _fileLogDeleteOld;
            set
            {
                if (value != _fileLogDeleteOld)
                {
                    _fileLogDeleteOld = value;
                    OnPropertyChanged(nameof(FileLogDeleteOld));
                }
            }
        }

        private int? _fileLogMaxSize;
        public int? FileLogMaxSize
        {
            get => _fileLogMaxSize;
            set
            {
                if (value != _fileLogMaxSize)
                {
                    _fileLogMaxSize = value;
                    OnPropertyChanged(nameof(FileLogMaxSize));
                }
            }
        }

        private int? _fileLogAge;
        public int? FileLogAge
        {
            get => _fileLogAge;
            set
            {
                if (value != _fileLogAge)
                {
                    _fileLogAge = value;
                    OnPropertyChanged(nameof(FileLogAge));
                }
            }
        }

        private int? _fileLogAgeType;
        public int? FileLogAgeType
        {
            get => _fileLogAgeType;
            set
            {
                if (value != _fileLogAgeType)
                {
                    _fileLogAgeType = value;
                    OnPropertyChanged(nameof(FileLogAgeType));
                }
            }
        }

        private bool? _performanceWarning;
        public bool? PerformanceWarning
        {
            get => _performanceWarning;
            set
            {
                if (value != _performanceWarning)
                {
                    _performanceWarning = value;
                    OnPropertyChanged(nameof(PerformanceWarning));
                }
            }
        }

        private bool? _addToTopOfQueue;
        public bool? AddToTopOfQueue
        {
            get => _addToTopOfQueue;
            set
            {
                if (value != _addToTopOfQueue)
                {
                    _addToTopOfQueue = value;
                    OnPropertyChanged(nameof(AddToTopOfQueue));
                }
            }
        }

        private TorrentStopConditions _torrentStopCondition = TorrentStopConditions.None;
        public TorrentStopConditions TorrentStopCondition
        {
            get => _torrentStopCondition;
            set
            {
                if (value != _torrentStopCondition)
                {
                    _torrentStopCondition = value;
                    OnPropertyChanged(nameof(TorrentStopCondition));
                }
            }
        }

        private bool? _useSubcategories;
        public bool? UseSubcategories
        {
            get => _useSubcategories;
            set
            {
                if (value != _useSubcategories)
                {
                    _useSubcategories = value;
                    OnPropertyChanged(nameof(UseSubcategories));
                }
            }
        }

        private bool? _excludedFileNamesEnabled;
        public bool? ExcludedFileNamesEnabled
        {
            get => _excludedFileNamesEnabled;
            set
            {
                if (value != _excludedFileNamesEnabled)
                {
                    _excludedFileNamesEnabled = value;
                    OnPropertyChanged(nameof(ExcludedFileNamesEnabled));
                }
            }
        }

        private string? _excludedFileNames;
        public string? ExcludedFileNames
        {
            get => _excludedFileNames;
            set
            {
                if (value != _excludedFileNames)
                {
                    _excludedFileNames = value;
                    OnPropertyChanged(nameof(ExcludedFileNames));
                }
            }
        }

        private bool? _autorunOnTorrentAddedEnabled;        
        public bool? AutorunOnTorrentAddedEnabled
        {
            get => _autorunOnTorrentAddedEnabled;
            set
            {
                if (value != _autorunOnTorrentAddedEnabled)
                {
                    _autorunOnTorrentAddedEnabled = value;
                    OnPropertyChanged(nameof(AutorunOnTorrentAddedEnabled));
                }
            }
        }

        private string? _autorunOnTorrentAddedProgram;
        public string? AutorunOnTorrentAddedProgram
        {
            get => _autorunOnTorrentAddedProgram;
            set
            {
                if (value != _autorunOnTorrentAddedProgram)
                {
                    _autorunOnTorrentAddedProgram = value;
                    OnPropertyChanged(nameof(AutorunOnTorrentAddedProgram));
                }
            }
        }

        private bool? _i2pEnabled;
        public bool? I2pEnabled
        {
            get => _i2pEnabled;
            set
            {
                if (value != _i2pEnabled)
                {
                    _i2pEnabled = value;
                    OnPropertyChanged(nameof(I2pEnabled));
                }
            }
        }

        private string? _i2pAddress;
        public string? I2pAddress
        {
            get => _i2pAddress;
            set
            {
                if (value != _i2pAddress)
                {
                    _i2pAddress = value;
                    OnPropertyChanged(nameof(I2pAddress));
                }
            }
        }

        private int? _i2pPort;
        public int? I2pPort
        {
            get => _i2pPort;
            set
            {
                if (value != _i2pPort)
                {
                    _i2pPort = value;
                    OnPropertyChanged(nameof(I2pPort));
                }
            }
        }

        private bool? _i2pMixedMode;
        public bool? I2pMixedMode
        {
            get => _i2pMixedMode;
            set
            {
                if (value != _i2pMixedMode)
                {
                    _i2pMixedMode = value;
                    OnPropertyChanged(nameof(I2pMixedMode));
                }
            }
        }

        private int? _maxActiveCheckingTorrents;
        public int? MaxActiveCheckingTorrents
        {
            get => _maxActiveCheckingTorrents;
            set
            {
                if (value != _maxActiveCheckingTorrents)
                {
                    _maxActiveCheckingTorrents = value;
                    OnPropertyChanged(nameof(MaxActiveCheckingTorrents));
                }
            }
        }

        private string? _webUiReverseProxiesList;
        public string? WebUiReverseProxiesList
        {
            get => _webUiReverseProxiesList;
            set
            {
                if (value != _webUiReverseProxiesList)
                {
                    _webUiReverseProxiesList = value;
                    OnPropertyChanged(nameof(WebUiReverseProxiesList));
                }
            }
        }

        private bool? _webUiReverseProxyEnabled;
        public bool? WebUiReverseProxyEnabled
        {
            get => _webUiReverseProxyEnabled;
            set
            {
                if (value != _webUiReverseProxyEnabled)
                {
                    _webUiReverseProxyEnabled = value;
                    OnPropertyChanged(nameof(I2pEnabled));
                }
            }
        }
        
    }
}
using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData;
using Newtonsoft.Json.Linq;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static qBittorrentCompanion.Helpers.DataConverter;

namespace qBittorrentCompanion.ViewModels
{
    public enum TorrentStopConditions
    {
        None,
        MetadataReceived,
        FilesChecked
    }
    public partial class PreferencesWindowViewModel : ViewModelBase
    {
        public static List<ByteUnit> SizeOptions =>
            [.. Enum.GetValues<ByteUnit>().Take(3)];

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
            ProxyTypeDescriptions.None, ProxyTypeDescriptions.Http,
            ProxyTypeDescriptions.Socks5, ProxyTypeDescriptions.HttpAuth,
            ProxyTypeDescriptions.Socks5Auth, ProxyTypeDescriptions.Socks4,
        ];

        public string[] DiskIOReadModes => ["Enable OS cache", "Disable OS cache"];
        public string[] DiskIOWriteModes => ["Enable OS cache", "Disable OS cache", "Write through (requires libtorrent >= 2.0.6)"];

        public string[] UploadSlotsBehaviors => [
            UploadSlotBehaviors.FixedSlots,
            UploadSlotBehaviors.UploadRateBased
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

        private void OpenUrl(string url)
        {
            if (Avalonia.Application.Current is Avalonia.Application app
                && app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && TopLevel.GetTopLevel(desktop.MainWindow) is TopLevel topLevel)
                topLevel.Launcher.LaunchUriAsync(new Uri(url));
        }

        public partial class IpDummy(string ip = "") : ReactiveObject
        {
            [AutoPropertyChanged]
            private string _ip = ip;
        }

        [AutoPropertyChanged]
        private List<string> _networkInterfaces = [""];

        public partial class NetworkAddressDummy(string networkAddress) : ReactiveObject
        {
            [AutoPropertyChanged]
            private string _networkAddress = networkAddress;
        }

        private readonly Dictionary<string, string> _networkInterfaceAddresses = new()
        {
            [""] = "All addresses",
            ["0.0.0.0"] = "All IPv4 addresses",
            ["::"] = "All IPv6 addresses",
        };

        public HashSet<string> NetworkInterfaceAddresses => 
            _networkInterfaceAddresses.Values.ToHashSet();

        [AutoPropertyChanged]
        private int _currentNetworkInterfaceIndex = 0;
        [AutoPropertyChanged]
        private int _currentNetworkAddressIndex = 0;

        public async Task FetchData()
        {
            var networkInterfacesTask = QBittorrentService.GetNetworkInterfacesAsync();
            var networkInterfaceAddressesTask = QBittorrentService.GetNetworkInterfaceAddressesAsync();
            var prefsTask = QBittorrentService.GetPreferencesAsync();

            // Send out all 3 request simultanously
            await Task.WhenAll(networkInterfacesTask, networkInterfaceAddressesTask, prefsTask);


            if (networkInterfaceAddressesTask != null)
            {
                var networkInterfaces = await networkInterfacesTask;
                if (networkInterfaces != null)
                {
                    NetworkInterfaces = ["Any interface", .. networkInterfaces.Select(n => n.Id).ToList()];
                    var networkInterfaceAddresses = await networkInterfaceAddressesTask;
                    if (networkInterfaceAddresses != null)
                        foreach (string address in networkInterfaceAddresses)
                            _networkInterfaceAddresses.Add(address, address);

                    this.RaisePropertyChanged(nameof(NetworkInterfaceAddresses));
                }

                var prefs = await prefsTask;

                if (prefs == null)
                    return;

                // Set properties based on the fetched preferences
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
                //EnableUTP = prefs.EnableUTP;
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
                Dht = prefs.DHT;
                DhtSameAsBT = prefs.DHTSameAsBT;
                DhtPort = prefs.DHTPort;
                PeerExchange = prefs.PeerExchange;
                LocalPeerDiscovery = prefs.LocalPeerDiscovery;
                Encryption = prefs.Encryption;
                AnonymousMode = prefs.AnonymousMode;
                ProxyType = prefs.ProxyType ?? QBittorrent.Client.ProxyType.None;
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
                //ProxyAuthenticationEnabled = prefs.ProxyAuthenticationEnabled;
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
                foreach (string header in prefs.WebUICustomHttpHeaders)
                    WebUICustomHttpHeaders.Add(new CustomHttpHeader(header));

                BypassLocalAuthentication = prefs.BypassLocalAuthentication;
                BypassAuthenticationSubnetWhitelistEnabled = prefs.BypassAuthenticationSubnetWhitelistEnabled;
                BypassAuthenticationSubnetWhitelist = prefs.BypassAuthenticationSubnetWhitelist;
                DynamicDnsEnabled = prefs.DynamicDnsEnabled;
                DynamicDnsService = prefs.DynamicDnsService ?? QBittorrent.Client.DynamicDnsService.None;
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
                TorrentContentLayout = prefs.TorrentContentLayout ?? QBittorrent.Client.TorrentContentLayout.Original;

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
                BdecodeDepthLimit = int.Parse(prefs.AdditionalData["bdecode_depth_limit"].ToString());
                BdecodeTokenLimit = int.Parse(prefs.AdditionalData["bdecode_token_limit"].ToString());
                HashingThreads = int.Parse(prefs.AdditionalData["hashing_threads"].ToString());
                DiskQueueSize = int.Parse(prefs.AdditionalData["disk_queue_size"].ToString());
                DiskIOType = int.Parse(prefs.AdditionalData["disk_io_type"].ToString());
                DiskIOReadMode = int.Parse(prefs.AdditionalData["disk_io_read_mode"].ToString());
                //DiskIOWriteModes = int.Parse(prefs.AdditionalData["disk_io_write_mode"].ToString());
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
        }

        public ReactiveCommand<Unit, Unit> SaveDataCommand { get; }

        public async Task SaveData()
        {
            ExtendedPreferences extPrefs = new()
            {
                Locale = Locale,
                SavePath = SavePath,
                TempPathEnabled = TempPathEnabled,
                TempPath = TempPath,
                ScanDirectories = ScanDirectories,
                ExportDirectory = ExportDirectory,
                ExportDirectoryForFinished = ExportDirectoryForFinished,
                MailNotificationEnabled = MailNotificationEnabled,
                MailNotificationEmailAddress = MailNotificationEmailAddress,
                MailNotificationSmtpServer = MailNotificationSmtpServer,
                MailNotificationSslEnabled = MailNotificationSslEnabled,
                MailNotificationAuthenticationEnabled = MailNotificationAuthenticationEnabled,
                MailNotificationUsername = MailNotificationUsername,
                MailNotificationPassword = MailNotificationPassword,
                AutorunEnabled = AutorunEnabled,
                AutorunProgram = AutorunProgram,
                PreallocateAll = PreallocateAll,
                QueueingEnabled = QueueingEnabled,
                MaxActiveDownloads = MaxActiveDownloads,
                MaxActiveTorrents = MaxActiveTorrents,
                MaxActiveUploads = MaxActiveUploads,
                DoNotCountSlowTorrents = DoNotCountSlowTorrents,
                MaxRatioEnabled = MaxRatioEnabled,
                MaxRatio = MaxRatio,
                MaxRatioAction = MaxRatioAction,
                MaxSeedingTime = MaxSeedingTime,
                MaxSeedingTimeEnabled = MaxSeedingTimeEnabled,
                MaxInactiveSeedingTime = MaxInactiveSeedingTime,
                MaxInactiveSeedingTimeEnabled = MaxInactiveSeedingTimeEnabled,
                AppendExtensionToIncompleteFiles = AppendExtensionToIncompleteFiles,
                ListenPort = ListenPort,
                UpnpEnabled = UpnpEnabled,
                RandomPort = RandomPort,
                DownloadLimit = DownloadLimit,
                UploadLimit = UploadLimit,
                MaxConnections = MaxConnections,
                MaxConnectionsPerTorrent = MaxConnectionsPerTorrent,
                MaxUploads = MaxUploads,
                MaxUploadsPerTorrent = MaxUploadsPerTorrent,
                //EnableUTP = EnableUTP,
                LimitUTPRate = LimitUTPRate,
                LimitTcpOverhead = LimitTcpOverhead,
                AlternativeDownloadLimit = AlternativeDownloadLimit,
                AlternativeUploadLimit = AlternativeUploadLimit,
                SchedulerEnabled = SchedulerEnabled,
                ScheduleFromHour = ScheduleFromHour,
                ScheduleFromMinute = ScheduleFromMinute,
                ScheduleToHour = ScheduleToHour,
                ScheduleToMinute = ScheduleToMinute,
                SchedulerDays = SchedulerDays,
                DHT = Dht,
                DHTSameAsBT = DhtSameAsBT,
                DHTPort = DhtPort,
                PeerExchange = PeerExchange,
                LocalPeerDiscovery = LocalPeerDiscovery,
                Encryption = Encryption,
                AnonymousMode = AnonymousMode,
                ProxyType = ProxyType,
                ProxyAddress = ProxyAddress,
                CurrentNetworkInterface = CurrentNetworkInterface,
                CurrentInterfaceAddress = CurrentInterfaceAddress,
                ProxyPort = ProxyPort,
                ProxyPeerConnections = ProxyPeerConnections,
                ForceProxy = ForceProxy,
                ProxyTorrentsOnly = ProxyTorrentsOnly,
                //ProxyAuthenticationEnabled = ProxyAuthenticationEnabled,
                ProxyUsername = ProxyUsername,
                ProxyPassword = ProxyPassword,
                ProxyHostnameLookup = ProxyHostnameLookup,
                ProxyBittorrent = ProxyBittorrent,
                ProxyMisc = ProxyMisc,
                ProxyRss = ProxyRss,
                IpFilterEnabled = IpFilterEnabled,
                IpFilterPath = IpFilterPath,
                IpFilterTrackers = IpFilterTrackers,
                WebUIAddress = WebUIAddress,
                WebUIPort = WebUIPort,
                WebUIDomain = WebUIDomain,
                WebUIUpnp = WebUIUpnp,
                WebUIUsername = WebUIUsername,
                WebUIPassword = WebUIPassword,
                WebUIPasswordHash = WebUIPasswordHash,
                WebUIHttps = WebUIHttps,
                WebUISslKey = WebUISslKey,
                WebUISslCertificate = WebUISslCertificate,
                WebUIClickjackingProtection = WebUIClickjackingProtection,
                WebUICsrfProtection = WebUICsrfProtection,
                WebUISecureCookie = WebUISecureCookie,
                WebUIMaxAuthenticationFailures = WebUIMaxAuthenticationFailures,
                WebUIBanDuration = WebUIBanDuration,
                WebUICustomHttpHeadersEnabled = WebUICustomHttpHeadersEnabled
            };

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
            extPrefs.TorrentContentLayout = TorrentContentLayout;

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
            extPrefs.BDecodeDepthLimit = BdecodeDepthLimit;
            extPrefs.BDecodeTokenLimit = BdecodeTokenLimit;
            extPrefs.HashingThreads = HashingThreads;
            extPrefs.DiskQueueSize = DiskQueueSize;
            extPrefs.DiskIOType = DiskIOType;
            extPrefs.DiskIOReadMode = DiskIOReadMode;
            extPrefs.DiskIOWriteMode = DiskIOWrite;
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

            await QBittorrentService.SetPreferencesAsync(extPrefs);
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
                    this.RaisePropertyChanged(nameof(Locale));

                    _locale_Proxy = value == null
                        ? null
                        : localeDictionary[value];
                    this.RaisePropertyChanged(nameof(Locale_Proxy));
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
                    this.RaisePropertyChanged(nameof(Locale_Proxy));

                    _locale = value == null
                        ? null
                        : localeDictionary.First(l => l.Value == value).Key;
                    this.RaisePropertyChanged(nameof(Locale));
                }
            }
        }

        [AutoPropertyChanged]
        private string? _savePath;
        [AutoPropertyChanged]
        private bool? _tempPathEnabled;
        [AutoPropertyChanged]
        private string _tempPath;
        [AutoPropertyChanged]
        private IDictionary<string, QBittorrent.Client.SaveLocation> _scanDirectories;
        [AutoPropertyChanged]
        private string _exportDirectory;
        [AutoPropertyChanged]
        private string _exportDirectoryForFinished;
        [AutoPropertyChanged]
        private bool? _mailNotificationEnabled;
        [AutoPropertyChanged]
        private string _mailNotificationEmailAddress;
        [AutoPropertyChanged]
        private string _mailNotificationSmtpServer;
        [AutoPropertyChanged]
        private bool? _mailNotificationSslEnabled;
        [AutoPropertyChanged]
        private bool? _mailNotificationAuthenticationEnabled;
        [AutoPropertyChanged]
        private string _mailNotificationUsername;
        [AutoPropertyChanged]
        private string _mailNotificationPassword;
        [AutoPropertyChanged]
        private bool? _autorunEnabled;
        [AutoPropertyChanged]
        private string _autorunProgram;
        [AutoPropertyChanged]
        private bool? _preallocateAll;
        [AutoPropertyChanged]
        private bool? _queueingEnabled;
        [AutoPropertyChanged]
        private int? _maxActiveDownloads;
        [AutoPropertyChanged]
        private int? _maxActiveTorrents;
        [AutoPropertyChanged]
        private int? _maxActiveUploads;
        [AutoPropertyChanged]
        private bool? _doNotCountSlowTorrents;
        [AutoPropertyChanged]
        private bool? _maxRatioEnabled;
        [AutoPropertyChanged]
        private double? _maxRatio;

        /// <summary>
        /// FIXME: MaxRatioAction only supports 2 values, there should be 4?
        /// </summary>
        [AutoPropertyChanged]
        private QBittorrent.Client.MaxRatioAction? _maxRatioAction;
        [AutoPropertyChanged]
        private int? _maxSeedingTime;
        [AutoPropertyChanged]
        private bool? _maxSeedingTimeEnabled;
        [AutoPropertyChanged]
        private int? _maxInactiveSeedingTime;
        [AutoPropertyChanged]
        private bool? _maxInactiveSeedingTimeEnabled;
        [AutoPropertyChanged]
        private bool? _appendExtensionToIncompleteFiles;
        [AutoPropertyChanged]
        private int? _listenPort;
        [AutoPropertyChanged]
        private bool? _upnpEnabled;
        [AutoPropertyChanged]
        private bool? _randomPort;
        [AutoPropertyChanged]
        private int _downloadLimit = 0;
        [AutoPropertyChanged]
        private int _uploadLimit = 0;
        [AutoPropertyChanged]
        private int? _maxConnections;
        [AutoPropertyChanged]
        private int? _maxConnectionsPerTorrent;
        [AutoPropertyChanged]
        private int? _maxUploads;
        [AutoPropertyChanged]
        private int? _maxUploadsPerTorrent;
        [AutoPropertyChanged]
        private bool? _enableUTP;
        [AutoPropertyChanged]
        private bool? _limitUTPRate;
        [AutoPropertyChanged]
        private bool? _limitTcpOverhead;
        [AutoPropertyChanged]
        private int _alternativeDownloadLimit = 0;
        [AutoPropertyChanged]
        private int _alternativeUploadLimit = 0;
        [AutoPropertyChanged]
        private bool? _schedulerEnabled;

        private int? _scheduleFromHour;
        public int? ScheduleFromHour
        {
            get => _scheduleFromHour;
            set
            {
                if (_scheduleFromHour != value)
                {
                    _scheduleFromHour = value;
                    this.RaisePropertyChanged(nameof(ScheduleFromHour));
                    this.RaisePropertyChanged(nameof(ScheduleFrom));
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
                    this.RaisePropertyChanged(nameof(ScheduleFromMinute));
                    this.RaisePropertyChanged(nameof(ScheduleFrom));
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
        [AutoPropertyChanged]
        private int? _scheduleToHour;
        [AutoPropertyChanged]
        private int? _scheduleToMinute;

        public string ScheduleTo
        {
            get => ScheduleFormat(ScheduleToHour, ScheduleToMinute);
        }

        [AutoPropertyChanged]
        private QBittorrent.Client.SchedulerDay? _schedulerDays;
        [AutoPropertyChanged]
        private bool? _dht;
        [AutoPropertyChanged]
        private bool? _dhtSameAsBT;
        [AutoPropertyChanged]
        private int? _dhtPort;
        [AutoPropertyChanged]
        private bool? _peerExchange;
        [AutoPropertyChanged]
        private bool? _localPeerDiscovery;
        [AutoPropertyChanged]
        private QBittorrent.Client.Encryption? _encryption;
        [AutoPropertyChanged]
        private bool? _anonymousMode;
        [AutoPropertyChanged]
        private QBittorrent.Client.ProxyType _proxyType = QBittorrent.Client.ProxyType.None;
        [AutoPropertyChanged]
        private string _proxyAddress;
        [AutoPropertyChanged]
        private int? _proxyPort;
        [AutoPropertyChanged]
        private bool? _proxyPeerConnections;
        [AutoPropertyChanged]
        private bool? _forceProxy;
        [AutoPropertyChanged]
        private bool? _proxyTorrentsOnly;
        [AutoPropertyChanged]
        private bool? _proxyAuthenticationEnabled;
        [AutoPropertyChanged]
        private string _proxyUsername;
        [AutoPropertyChanged]
        private string _proxyPassword;
        [AutoPropertyChanged]
        private bool? _proxyHostnameLookup;
        [AutoPropertyChanged]
        private bool? _proxyBittorrent;
        [AutoPropertyChanged]
        private bool? _proxyMisc;
        [AutoPropertyChanged]
        private bool? _proxyRss;
        [AutoPropertyChanged]
        private bool? _ipFilterEnabled;
        [AutoPropertyChanged]
        private string _ipFilterPath;
        [AutoPropertyChanged]
        private bool? _ipFilterTrackers;
        [AutoPropertyChanged]
        private string _webUIAddress;
        [AutoPropertyChanged]
        private int? _webUIPort;
        [AutoPropertyChanged]
        private string _webUIDomain;
        [AutoPropertyChanged]
        private bool? _webUIUpnp;
        [AutoPropertyChanged]
        private string _webUIUsername;
        [AutoPropertyChanged]
        private string _webUIPassword;
        [AutoPropertyChanged]
        private string _webUIPasswordHash;
        [AutoPropertyChanged]
        private bool? _webUIHttps;
        [AutoPropertyChanged]
        private string _webUISslKey;
        [AutoPropertyChanged]
        private string _webUISslCertificate;
        [AutoPropertyChanged]
        private bool? _webUIClickjackingProtection;
        [AutoPropertyChanged]
        private bool? _webUICsrfProtection;
        [AutoPropertyChanged]
        private bool? _webUISecureCookie;
        [AutoPropertyChanged]
        private int? _webUIMaxAuthenticationFailures;
        [AutoPropertyChanged]
        private int? _webUIBanDuration;
        [AutoPropertyChanged]
        private bool? _webUICustomHttpHeadersEnabled;

        public partial class CustomHttpHeader(string header) : ReactiveObject
        {
            [AutoPropertyChanged]
            private string _header = header;
        }

        [AutoPropertyChanged]
        private ObservableCollection<CustomHttpHeader> _webUICustomHttpHeaders = [];
        [AutoPropertyChanged]
        private bool? _bypassLocalAuthentication;
        [AutoPropertyChanged]
        private bool? _bypassAuthenticationSubnetWhitelistEnabled;
        [AutoPropertyChanged]
        private IList<string> _bypassAuthenticationSubnetWhitelist;
        [AutoPropertyChanged]
        private bool? _dynamicDnsEnabled;
        [AutoPropertyChanged]
        private QBittorrent.Client.DynamicDnsService _dynamicDnsService = QBittorrent.Client.DynamicDnsService.None;
        [AutoPropertyChanged]
        private string _dynamicDnsUsername;
        [AutoPropertyChanged]
        private string _dynamicDnsPassword;
        [AutoPropertyChanged]
        private string _dynamicDnsDomain;
        [AutoPropertyChanged]
        private uint? _rssRefreshInterval;
        [AutoPropertyChanged]
        private int? _rssMaxArticlesPerFeed;
        [AutoPropertyChanged]
        private bool? _rssProcessingEnabled;
        [AutoPropertyChanged]
        private bool? _rssAutoDownloadingEnabled;
        [AutoPropertyChanged]
        private bool? _rssDownloadRepackProperEpisodes;

        public partial class SmartEpFilterDummy(string smartEpFilter) : ReactiveObject
        {
            [AutoPropertyChanged]
            private string _smartEpFilter = smartEpFilter;
        }

        [AutoPropertyChanged]
        private ObservableCollection<SmartEpFilterDummy> _rssSmartEpisodeFilters = [];
        [AutoPropertyChanged]
        private bool? _additionalTrackersEnabled;

        public partial class TrackerDummy(string tracker) : ReactiveObject
        {
            [AutoPropertyChanged]
            private string _tracker = tracker;
        }

        [AutoPropertyChanged]
        private ObservableCollection<TrackerDummy> _additinalTrackers = [];
        [AutoPropertyChanged]
        private ObservableCollection<IpDummy> _bannedIpAddresses = [];
        [AutoPropertyChanged]
        private QBittorrent.Client.BittorrentProtocol? _bittorrentProtocol;
        [AutoPropertyChanged]
        private bool? _createTorrentSubfolder;
        [AutoPropertyChanged]
        private bool? _addTorrentPaused;
        [AutoPropertyChanged]
        private QBittorrent.Client.TorrentFileAutoDeleteMode? _torrentFileAutoDeleteMode;
        [AutoPropertyChanged]
        private bool? _autoTMMEnabledByDefault;
        [AutoPropertyChanged]
        private bool? _autoTMMRetainedWhenCategoryChanges;
        [AutoPropertyChanged]
        private bool? _autoTMMRetainedWhenDefaultSavePathChanges;
        [AutoPropertyChanged]
        private bool? _autoTMMRetainedWhenCategorySavePathChanges;
        [AutoPropertyChanged]
        private string _mailNotificationSender;
        [AutoPropertyChanged]
        private bool? _limitLAN;
        [AutoPropertyChanged]
        private int? _slowTorrentDownloadRateThreshold;
        [AutoPropertyChanged]
        private int? _slowTorrentUploadRateThreshold;
        [AutoPropertyChanged]
        private int? _slowTorrentInactiveTime;
        [AutoPropertyChanged]
        private bool? _alternativeWebUIEnabled;
        [AutoPropertyChanged]
        private string _alternativeWebUIPath;
        [AutoPropertyChanged]
        private bool? _webUIHostHeaderValidation;
        [AutoPropertyChanged]
        private string _webUISslKeyPath;

        // New properties
        [AutoPropertyChanged]
        private string _webUISslCertificatePath;
        [AutoPropertyChanged]
        private int? _webUISessionTimeout;
        [AutoPropertyChanged]
        private string _currentNetworkInterface;
        [AutoPropertyChanged]
        private string _currentInterfaceAddress;
        [AutoPropertyChanged]
        private bool? _listenOnIPv6Address;
        [AutoPropertyChanged]
        private int? _saveResumeDataInterval;
        [AutoPropertyChanged]
        private bool? _recheckCompletedTorrents;
        [AutoPropertyChanged]
        private bool? _resolvePeerCountries;
        [AutoPropertyChanged]
        private int? _libtorrentAsynchronousIOThreads;
        [AutoPropertyChanged]
        private int? _libtorrentFilePoolSize;
        [AutoPropertyChanged]
        private int? _libtorrentOutstandingMemoryWhenCheckingTorrent;
        [AutoPropertyChanged]
        private int? _libtorrentDiskCache;
        [AutoPropertyChanged]
        private int? _libtorrentDiskCacheExpiryInterval;
        [AutoPropertyChanged]
        private bool? _libtorrentUseOSCache;
        [AutoPropertyChanged]
        private bool? _libtorrentCoalesceReadsAndWrites;
        [AutoPropertyChanged]
        private bool? _libtorrentPieceExtentAffinity;
        [AutoPropertyChanged]
        private bool? _libtorrentSendUploadPieceSuggestions;
        [AutoPropertyChanged]
        private int? _libtorrentSendBufferWatermark;
        [AutoPropertyChanged]
        private int? _libtorrentSendBufferLowWatermark;
        [AutoPropertyChanged]
        private int? _libtorrentSendBufferWatermarkFactor;
        [AutoPropertyChanged]
        private int? _libtorrentSocketBacklogSize;
        [AutoPropertyChanged]
        private int? _libtorrentOutgoingPortsMin;
        [AutoPropertyChanged]
        private int? _libtorrentOutgoingPortsMax;
        [AutoPropertyChanged]
        private QBittorrent.Client.UtpTcpMixedModeAlgorithm? _libtorrentUtpTcpMixedModeAlgorithm;


        /// <summary>
        /// <inheritdoc cref="QBittorrent.Client.Preferences.LibtorrentAllowMultipleConnectionsFromSameIp"/>
        /// </summary>
        [AutoPropertyChanged]
        private bool? _libtorrentAllowMultipleConnectionsFromSameIp;

        [AutoPropertyChanged]
        private bool? _libtorrentEnableEmbeddedTracker;
        [AutoPropertyChanged]
        private int? _libtorrentEmbeddedTrackerPort;
        [AutoPropertyChanged]
        private QBittorrent.Client.ChokingAlgorithm? _libtorrentUploadSlotsBehavior;
        [AutoPropertyChanged]
        private QBittorrent.Client.SeedChokingAlgorithm? _libtorrentUploadChokingAlgorithm;
        [AutoPropertyChanged]
        private bool? _libtorrentStrictSuperSeeding;
        [AutoPropertyChanged]
        private bool? _libtorrentAnnounceToAllTrackers;
        [AutoPropertyChanged]
        private bool? _libtorrentAnnounceToAllTiers;
        [AutoPropertyChanged]
        private string _libtorrentAnnounceIp;
        [AutoPropertyChanged]
        private int? _libtorrentStopTrackerTimeout;
        [AutoPropertyChanged]
        private int? _libtorrentMaxConcurrentHttpAnnounces;
        
        /// <summary>
        /// <inheritdoc cref="Preferences.TorrentContentLayout"/>
        /// </summary>
        [AutoPropertyChanged]
        private QBittorrent.Client.TorrentContentLayout _torrentContentLayout = QBittorrent.Client.TorrentContentLayout.Original;
        /// <summary>
        /// <inheritdoc cref="QBittorrent.Client.Preferences.AdditionalData"/>
        /// </summary>
        [AutoPropertyChanged]
        private IDictionary<string, JToken> _additionalData;


        ///Passed this point will be the data found under "AdditionalData" 

        [AutoPropertyChanged]
        private int _bdecodeDepthLimit = 0;
        [AutoPropertyChanged]
        private int? _bdecodeTokenLimit;
        [AutoPropertyChanged]
        private int? _hashingThreads;
        [AutoPropertyChanged]
        private int? _diskQueueSize;
        [AutoPropertyChanged]
        private int? _diskIOType;
        [AutoPropertyChanged]
        private int? _diskIOReadMode;
        [AutoPropertyChanged]
        private int? _diskIOWrite;
        [AutoPropertyChanged]
        private int? _connectionSpeed;
        [AutoPropertyChanged]
        private int? _socketSendBufferSize;
        [AutoPropertyChanged]
        private int? _socketReceiveBufferSize;
        [AutoPropertyChanged]
        private int? _upnpLeaseDuration;
        [AutoPropertyChanged]
        private int? _peerTos;
        [AutoPropertyChanged]
        private bool? _idnSupportEnabled;
        [AutoPropertyChanged]
        private bool? _validateHttpsTrackerCertificate;
        [AutoPropertyChanged]
        private bool? _ssrfMitigation;
        [AutoPropertyChanged]
        private bool? _blockPeersOnPrivilegedPorts;
        [AutoPropertyChanged]
        private int? _peerTurnover;
        [AutoPropertyChanged]
        private int? _peerTurnoverCutoff;
        [AutoPropertyChanged]
        private int? _peerTurnoverInterval;
        [AutoPropertyChanged]
        private int? _requestQueueSize;
        [AutoPropertyChanged]
        private int? _i2pInboundQuantity;
        [AutoPropertyChanged]
        private int? _i2pOutboundQuantity;
        [AutoPropertyChanged]
        private int? _i2pInboundLength;
        [AutoPropertyChanged]
        private int? _i2pOutboundLength;
        [AutoPropertyChanged]
        private DataStorageType? _resumeDataStorageType;
        [AutoPropertyChanged]
        private int? _memoryWorkingSetLimit;
        [AutoPropertyChanged]
        private int? _refreshInterval;
        [AutoPropertyChanged]
        private bool? _reannounceWhenAddressChanged;
        [AutoPropertyChanged]
        private bool? _embeddedTrackerPortForwarding;
        [AutoPropertyChanged]
        private bool? _fileLogEnabled;
        [AutoPropertyChanged]
        private bool? _fileLogBackupEnabled;
        [AutoPropertyChanged]
        private bool? _fileLogDeleteOld;
        [AutoPropertyChanged]
        private int? _fileLogMaxSize;
        [AutoPropertyChanged]
        private int? _fileLogAge;
        [AutoPropertyChanged]
        private int? _fileLogAgeType;
        [AutoPropertyChanged]
        private bool? _performanceWarning;
        [AutoPropertyChanged]
        private bool? _addToTopOfQueue;
        [AutoPropertyChanged]
        private TorrentStopConditions _torrentStopCondition = TorrentStopConditions.None;
        [AutoPropertyChanged]
        private bool? _useSubcategories;
        [AutoPropertyChanged]
        private bool? _excludedFileNamesEnabled;
        [AutoPropertyChanged]
        private string? _excludedFileNames;
        [AutoPropertyChanged]
        private bool? _autorunOnTorrentAddedEnabled;
        [AutoPropertyChanged]
        private string? _autorunOnTorrentAddedProgram;
        [AutoPropertyChanged]
        private bool? _i2pEnabled;
        [AutoPropertyChanged]
        private string? _i2pAddress;
        [AutoPropertyChanged]
        private int? _i2pPort;
        [AutoPropertyChanged]
        private bool? _i2pMixedMode;
        [AutoPropertyChanged]
        private int? _maxActiveCheckingTorrents;
        [AutoPropertyChanged]
        private string? _webUiReverseProxiesList;
        [AutoPropertyChanged]
        private bool? _webUiReverseProxyEnabled;        
    }
}
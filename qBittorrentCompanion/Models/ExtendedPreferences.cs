using System.Text.Json.Serialization;
using Newtonsoft.Json;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using static qBittorrentCompanion.ViewModels.PreferencesWindowViewModel;

namespace qBittorrentCompanion.Models
{
    public class ExtendedPreferences : Preferences
    {
        // Existing properties
        [JsonProperty("file_log_enabled")]
        public bool? FileLogEnabled { get; set; }

        [JsonProperty("file_log_backup_enabled")]
        public bool? FileLogBackupEnabled { get; set; }

        [JsonProperty("file_log_delete_old")]
        public bool? FileLogDeleteOld { get; set; }

        [JsonProperty("file_log_max_size")]
        public int? FileLogMaxSize { get; set; }

        [JsonProperty("file_log_age")]
        public int? FileLogAge { get; set; }

        [JsonProperty("file_log_age_type")]
        public int? FileLogAgeType { get; set; }

        [JsonProperty("performance_warning")]
        public bool? PerformanceWarning { get; set; }

        // Additional properties from the JSON data
        [JsonProperty("add_to_top_of_queue")]
        public bool AddToTopOfQueue { get; set; }

        [JsonProperty("torrent_stop_condition")]
        public TorrentStopConditions TorrentStopCondition { get; set; }

        [JsonProperty("use_subcategories")]
        public bool UseSubcategories { get; set; }

        [JsonProperty("excluded_file_names_enabled")]
        public bool ExcludedFileNamesEnabled { get; set; }

        [JsonProperty("excluded_file_names")]
        public string ExcludedFileNames { get; set; }

        [JsonProperty("autorun_on_torrent_added_enabled")]
        public bool AutorunOnTorrentAddedEnabled { get; set; }

        [JsonProperty("autorun_on_torrent_added_program")]
        public string AutorunOnTorrentAddedProgram { get; set; }

        [JsonProperty("i2p_enabled")]
        public bool I2pEnabled { get; set; }

        [JsonProperty("i2p_address")]
        public string I2pAddress { get; set; }

        [JsonProperty("i2p_port")]
        public int I2pPort { get; set; }

        [JsonProperty("i2p_mixed_mode")]
        public bool I2pMixedMode { get; set; }

        [JsonProperty("resume_data_storage_type")]
        public DataStorageType ResumeDataStorageType { get; set; }

        [JsonProperty("memory_working_set_limit")]
        public int MemoryWorkingSetLimit { get; set; }

        [JsonProperty("refresh_interval")]
        public int RefreshInterval { get; set; }

        [JsonProperty("reannounce_when_address_changed")]
        public bool ReannounceWhenAddressChanged { get; set; }

        [JsonProperty("embedded_tracker_port_forwarding")]
        public bool EmbeddedTrackerPortForwarding { get; set; }

        [JsonProperty("max_active_checking_torrents")]
        public int MaxActiveCheckingTorrents { get; set; }

        [JsonProperty("web_ui_reverse_proxies_list")]
        public string WebUiReverseProxiesList { get; set; }

        [JsonProperty("web_ui_reverse_proxy_enabled")]
        public bool WebUiReverseProxyEnabled { get; set; }

        [JsonProperty("bdecode_depth_limit")]
        public int BDecodeDepthLimit { get; set; }

        [JsonProperty("bdecode_token_limit")]
        public int BDecodeTokenLimit { get; set; }

        [JsonProperty("hashing_threads")]
        public int HashingThreads { get; set; }

        [JsonProperty("disk_queue_size")]
        public int DiskQueueSize { get; set; }

        [JsonProperty("disk_io_type")]
        public int DiskIOType { get; set; }

        [JsonProperty("disk_io_read_mode")]
        public int DiskIOReadMode { get; set; }

        [JsonProperty("disk_io_write_mode")]
        public int DiskIOWriteMode { get; set; }

        [JsonProperty("connection_speed")]
        public int ConnectionSpeed { get; set; }

        [JsonProperty("socket_send_buffer_size")]
        public int SocketSendBufferSize { get; set; }

        [JsonProperty("socket_receive_buffer_size")]
        public int SocketReceiveBufferSize { get; set; }

        [JsonProperty("upnp_lease_duration")]
        public int UpnpLeaseDuration { get; set; }

        [JsonProperty("peer_tos")]
        public int PeerTos { get; set; }

        [JsonProperty("idn_support_enabled")]
        public bool IdnSupportEnabled { get; set; }

        [JsonProperty("validate_https_tracker_certificate")]
        public bool ValidateHttpsTrackerCertificate { get; set; }

        [JsonProperty("ssrf_mitigation")]
        public bool SsrfMitigation { get; set; }

        [JsonProperty("block_peers_on_privileged_ports")]
        public bool BlockPeersOnPrivilegedPorts { get; set; }

        [JsonProperty("peer_turnover")]
        public int PeerTurnover { get; set; }

        [JsonProperty("peer_turnover_cutoff")]
        public int PeerTurnoverCutoff { get; set; }

        [JsonProperty("peer_turnover_interval")]
        public int PeerTurnoverInterval { get; set; }

        [JsonProperty("request_queue_size")]
        public int RequestQueueSize { get; set; }

        [JsonProperty("i2p_inbound_quantity")]
        public int I2pInboundQuantity { get; set; }

        [JsonProperty("i2p_outbound_quantity")]
        public int I2pOutboundQuantity { get; set; }

        [JsonProperty("i2p_inbound_length")]
        public int I2pInboundLength { get; set; }

        [JsonProperty("i2p_outbound_length")]
        public int I2pOutboundLength { get; set; }
    }
}

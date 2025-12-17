using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPeerViewModel : ViewModelBase
    {
        [RaiseChangeProxy(nameof(PeerPartialInfo.Client))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.ConnectionType), "Connection")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.ConnectionType), "Country")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.CountryCode))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.DownloadSpeed), "DlSpeed")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Downloaded))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Files))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Flags))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.FlagsDescription))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Address), "Ip")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Client), "PeerIdClient")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Port))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Progress))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Relevance))]
        [RaiseChangeProxy(nameof(PeerPartialInfo.UploadSpeed), "UpSpeed")]
        [RaiseChangeProxy(nameof(PeerPartialInfo.Uploaded))]
        private PeerPartialInfo _peer;

        public string Id { get; }

        [RaiseChange]
        private bool _hasIpAndPort = false;

        public ReactiveCommand<Unit, Unit> PermaBanCommand { get; }
        public ReactiveCommand<Unit, Unit> AddPeersCommand { get; }

        public TorrentPeerViewModel(PeerPartialInfo peer, string id)
        {
            _peer = peer;
            Id = id;
            HasIpAndPort = peer.Address.ToString().Length > 0;

            PermaBanCommand = ReactiveCommand.CreateFromTask(PermaBanAsync);
            AddPeersCommand = ReactiveCommand.CreateFromTask(AddPeersAsync);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<Unit> AddPeersAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

        private async Task PermaBanAsync()
        {
            await QBittorrentService.BanPeerAsync(Id);
        }

        public void Update(PeerPartialInfo peer)
        {
            _peer = peer;

            Client = _peer.Client;
            Connection = _peer.ConnectionType;
            Country = _peer.Country;
            CountryCode = _peer.CountryCode;
            DlSpeed = _peer.DownloadSpeed;
            Downloaded = _peer.Downloaded;
            Files = _peer.Files;
            Flags = _peer.Flags;
            FlagsDescription = _peer.FlagsDescription;
            Ip = _peer.Address;
            PeerIdClient = _peer.Client;
            Port = _peer.Port;
            Progress = _peer.Progress;
            Relevance = _peer.Relevance;
            UpSpeed = _peer.UploadSpeed;
            Uploaded = _peer.Uploaded;
        }
    }
}
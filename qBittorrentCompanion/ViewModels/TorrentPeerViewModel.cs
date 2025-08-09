using AutoPropertyChangedGenerator;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentPeerViewModel : ViewModelBase
    {
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Client))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.ConnectionType), "Connection")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.ConnectionType), "Country")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.CountryCode))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.DownloadSpeed), "DlSpeed")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Downloaded))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Files))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Flags))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.FlagsDescription))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Address), "Ip")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Client), "PeerIdClient")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Port))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Progress))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Relevance))]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.UploadSpeed), "UpSpeed")]
        [AutoProxyPropertyChanged(nameof(PeerPartialInfo.Uploaded))]
        private PeerPartialInfo _peer;

        public string Id { get; }

        [AutoPropertyChanged]
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
﻿using QBittorrent.Client;
using qBittorrentCompanion.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class TorrentPeerViewModel : INotifyPropertyChanged
{
    private PeerPartialInfo _peer;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Id { get; }

    public TorrentPeerViewModel(PeerPartialInfo peer, string id)
    {
        _peer = peer;
        Id = id;
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Client"/>
    /// </summary>
    public string Client
    {
        get => _peer.Client;
        set
        {
            if (value != _peer.Client)
            {
                _peer.Client = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.ConnectionType"/>
    /// </summary>
    public string Connection
    {
        get => _peer.ConnectionType;
        set
        {
            if (value != _peer.ConnectionType)
            {
                _peer.ConnectionType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Country"/>
    /// </summary>
    public string Country
    {
        get => _peer.Country;
        set
        {
            if (value != _peer.Country)
            {
                _peer.Country = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.CountryCode"/>
    /// </summary>
    public string CountryCode
    {
        get => _peer.CountryCode;
        set
        {
            if (value != _peer.CountryCode)
            {
                _peer.CountryCode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.DownloadSpeed"/>
    /// </summary>
    public int? DlSpeed
    {
        get => _peer.DownloadSpeed;
        set
        {
            if (value != _peer.DownloadSpeed)
            {
                _peer.DownloadSpeed = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Downloaded"/>
    /// </summary>
    public long? Downloaded
    {
        get => _peer?.Downloaded;
        set
        {
            if (value != _peer.Downloaded)
            {
                _peer.Downloaded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Files"/>
    /// </summary>
    public IReadOnlyList<string> Files
    {
        get => _peer.Files;
        set
        {
            if (value != _peer.Files)
            {
                _peer.Files = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Flags"/>
    /// </summary>
    public string Flags
    {
        get => _peer.Flags;
        set
        {
            if (value != _peer.Flags)
            {
                _peer.Flags = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.FlagsDescription"/>
    /// </summary>
    public string FlagsDesc
    {
        get => _peer.FlagsDescription;
        set
        {
            if (value != _peer.FlagsDescription)
            {
                _peer.FlagsDescription = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Address"/>
    /// </summary>
    public System.Net.IPAddress Ip
    {
        get => _peer.Address;
        set
        {
            if (value != _peer.Address)
            {
                _peer.Address = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Client"/>
    /// </summary>
    public string PeerIdClient
    {
        get => _peer.Client;
        set
        {
            if (value != _peer.Client)
            {
                _peer.Client = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Port"/>
    /// </summary>
    public int? Port
    {
        get => _peer.Port;
        set
        {
            if (value != _peer.Port)
            {
                _peer.Port = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Progress"/>
    /// </summary>
    public double? Progress
    {
        get => _peer?.Progress;
        set
        {
            if (value != _peer.Progress)
            {
                _peer.Progress = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Relevance"/>
    /// </summary>
    public double? Relevance
    {
        get => _peer.Relevance;
        set
        {
            if (value != _peer.Relevance)
            {
                _peer.Relevance = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.UploadSpeed/>
    /// </summary>
    public int? UpSpeed
    {
        get => _peer.UploadSpeed;
        set
        {
            if (value != _peer.UploadSpeed)
            {
                _peer.UploadSpeed = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// <inheritdoc cref="PeerPartialInfo.Uploaded"/>
    /// </summary>
    public long? Uploaded
    {
        get => _peer.Uploaded;
        set
        {
            if (value != _peer.Uploaded)
            {
                _peer.Uploaded = value;
                OnPropertyChanged();
            }
        }
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
        FlagsDesc = _peer.FlagsDescription;
        Ip = _peer.Address;
        PeerIdClient = _peer.Client;
        Port = _peer.Port;
        Progress = _peer.Progress;
        Relevance = _peer.Relevance;
        UpSpeed = _peer.UploadSpeed;
        Uploaded = _peer.Uploaded;
    }

}
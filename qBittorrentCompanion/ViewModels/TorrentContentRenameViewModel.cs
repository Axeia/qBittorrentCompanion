using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using FluentIcons.Common;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a TreeDataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     */
    public class TorrentContentRenameViewModel : INotifyPropertyChanged
    {
        private TorrentContent? _torrentContent;
        private List<string> _pathParts = [];
        private string _fileName = "";
        public string FileName => _fileName;
        private string _fileExtension = "";
        public string FileExtension => _fileExtension;


        /// <summary>
        /// Constructor for files
        /// </summary>
        /// <param name="infoHash"></param>
        /// <param name="torrentContent"></param>
        public TorrentContentRenameViewModel(string infoHash, TorrentContent torrentContent)
        {
            _infoHash = infoHash;
            var pathParts = torrentContent.Name.Split('/').ToList();
            _pathParts = pathParts[..^1];
            string nameAndExtension = pathParts.Last();
            int lastDotIndex = nameAndExtension.LastIndexOf('.');
            _fileName = lastDotIndex == -1
                ? nameAndExtension
                : nameAndExtension.Substring(0, lastDotIndex);
            _fileExtension = lastDotIndex == -1
                ? ""
                : nameAndExtension.Substring(lastDotIndex + 1);
            _torrentContent = torrentContent;
            IsFile = true;

            CalculateSpacing();
        }
        
        private bool _isMatch = false;
        public bool IsMatch
        {
            get => _isMatch;
            set
            {
                if(_isMatch != value)
                {
                    _isMatch = value;
                    OnPropertyChanged(nameof(IsMatch));
                }
            }
        }

        public void CalculateSpacing()
        {
            if (_pathParts.Count > 0)
            {
                int times = _pathParts.Count() - 1;
                if (IsFile)
                    times += 1;
                _spacing = new string(' ', times*4);
            }

        }

        /// <summary>
        /// Constructor for directories
        /// </summary>
        /// <param name="infoHash"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        public TorrentContentRenameViewModel(string infoHash, List<string> pathParts)
        {
            _infoHash = infoHash;
            _pathParts = pathParts;
            CalculateSpacing();
            Debug.WriteLine(string.Join('/', _pathParts));
        }

        private string _spacing = "";
        public string Spacing => _spacing;

        private bool _isUpdating = false;
        /// <summary>
        /// If an ASync method is run this should be used to indicate that this node is currently updating
        /// Set to false again when it's done.
        /// </summary>
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if(_isUpdating != value)
                {
                    _isUpdating = value; 
                    OnPropertyChanged(nameof(IsUpdating));
                }
            }
        }
        /// <summary>If true this ViewModel represent a file rather than a folder</summary>
        public bool IsFile = false;
        /// <summary>The hash of the Torrent these files/directories belong to <see cref="TorrentInfo.Hash"/></summary>
        private string _infoHash;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _renameMe = true;
        /// <summary>
        /// Represents whether this file (or folder) should be renamed (if it matches the search criteria)
        /// </summary>
        public bool RenameMe
        {
            get => _renameMe;
            set
            {
                if (_renameMe != value)
                {
                    _renameMe = value;
                    OnPropertyChanged(nameof(RenameMe));
                }
            }
        }

        public Symbol Icon
        {
            get => _torrentContent is null 
                ? Symbol.Folder
                : Symbol.Document;
        }

        public string OriginalName => _torrentContent == null
            ? string.Join('/', _pathParts) + FileName + (string.IsNullOrEmpty(FileExtension) ? "" : "." + FileExtension)
            : _torrentContent.Name;


        /// <summary><inheritdoc cref="TorrentContent.Index"/></summary>
        public int Index
        {
            get => _torrentContent?.Index ?? -1;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Index)
                {
                    _torrentContent.Index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        /// <summary>
        /// The name displayed in the UI 
        /// <list type="bullet">
        /// <item><term>File</term><description> displays the file name only (not the directory)</description></item>
        /// <item><term>Directory</term><description> displays the relevant part of the directory only (as nesting should display the rest)</description></item>
        /// </list>
        /// </summary>
        public string DisplayName => IsFile 
            ? FileName + (FileExtension == string.Empty ? "" : ".") + FileExtension 
            : _pathParts.Last();

        public string FullRenamedPath => IsFile
            ? string.Join('/', _pathParts)
            : string.Join('/', _pathParts) + '/' + DisplayName;

        private string _renamed = "";
        public string Renamed
        {
            get => _renamed;
            set
            {
                if(value != _renamed)
                {
                    _renamed = value;
                    OnPropertyChanged(nameof(Renamed));
                }
            }
        }

        public void Update(TorrentContent tc)
        {
        }
    }
}

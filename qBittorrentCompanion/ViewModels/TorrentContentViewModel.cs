﻿using QBittorrent.Client;
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
using DynamicData;
using AutoPropertyChangedGenerator;
using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    ///<summary>
    ///Avalonia seems to run into problems displaying a TreeDataGrid with multiple classes even if 
    ///they enherit from the same baseclass/follow the same blueprint. That means that this class <summary>
    /// Avalonia seems to run into problems displaying a TreeDataGrid with multiple classes even if 
    // is trying to fulfill the role of two classes (file and folder viewmodels).
    // </summary>
    public partial class TorrentContentViewModel : ViewModelBase
    {
        [AutoPropertyChanged]
        private bool _isEditing;

        public bool IsTopLevelItem = true;
        protected TorrentContent? _torrentContent;

        private ObservableCollection<TorrentContentViewModel> _children = [];

        public IReadOnlyList<TorrentContentViewModel> Children => _children;
        public void AddChild(TorrentContentViewModel child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            child.PropertyChanged += Child_PropertyChanged;

            _children.Add(child);
            folderPriority = _children.All(c => c.Priority == child.Priority)
                ? child.Priority
                : null;
            this.RaisePropertyChanged(nameof(Children));
        }

        protected void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Priority))
            {
                var firstChildPriority = Children.First().Priority;
                folderPriority = Children.All(c => c.Priority == firstChildPriority)
                    ? firstChildPriority
                    : null;
                this.RaisePropertyChanged(nameof(Priority));
            }
        }

        // Going by: https://github.com/fedarovich/qbittorrent-net-client/issues/20 
        // it seems like VeryLow, Low, Normal & High have been removed in an old version.
        public string[] TorrentContentPriorities => [
            DataConverter.TorrentContentPriorities.Skip, // Do not download
            DataConverter.TorrentContentPriorities.Minimal, //Normal
            //DataConverter.TorrentContentPriorities.VeryLow,
            //DataConverter.TorrentContentPriorities.Low,
            //DataConverter.TorrentContentPriorities.Normal,
            //DataConverter.TorrentContentPriorities.High,
            DataConverter.TorrentContentPriorities.VeryHigh, // High
            DataConverter.TorrentContentPriorities.Maximal, // Maximal
            DataConverter.TorrentContentPriorities.Mixed
        ];

        /// <summary>
        /// Represents the expanded state for the node in the TreeDataGrid
        /// </summary>
        [AutoPropertyChanged]
        private bool _isExpanded = true;

        /// <summary>
        /// If an async method is run this should be used to indicate that this node is currently updating
        /// Set to false again when it's done.
        /// </summary>
        [AutoPropertyChanged]
        private bool _isUpdating = false;

        /// <summary>
        /// The name displayed in the UI 
        /// <br/><br/>
        /// Do <b>NOT</b> set this manually other than in a rename operation to be followed by a call to <see cref="SaveAndCascadeDisplayNameChange(string)"/>
        /// <list type="bullet">
        /// <item><term>File</term><description> displays the file name only (not the directory)</description></item>
        /// <item><term>Directory</term><description> displays the relevant part of the directory only (as nesting should display the rest)</description></item>
        /// </list>
        /// </summary>
        [AutoPropertyChanged]
        private string _displayName = string.Empty;

        /// <summary>If true this ViewModel represent a file rather than a folder</summary>
        public bool IsFile = false;
        /// <summary>The hash of the Torrent these files/directories belong to <see cref="TorrentInfo.Hash"/></summary>
        private readonly string _infoHash;

        /// <summary>
        /// Constructor for files
        /// </summary>
        /// <param name="infoHash"></param>
        /// <param name="torrentContent"></param>
        public TorrentContentViewModel(string infoHash, TorrentContent torrentContent)
        {
            _infoHash = infoHash;
            _torrentContent = torrentContent;
            _displayName = torrentContent.Name.Split('/').Last(); // One time initialise
            IsFile = true;
        }

        /// <summary>
        /// Constructor for directories
        /// </summary>
        /// <param name="infoHash"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        public TorrentContentViewModel(string infoHash, string name)
        {
            _infoHash = infoHash;
            Name = name;
        }

        /// <summary><inheritdoc cref="TorrentContent.Availability"/></summary>
        public double? Availability
        {
            get => _torrentContent?.Availability ?? 0;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Availability)
                {
                    _torrentContent.Availability = value;
                    this.RaisePropertyChanged(nameof(Availability));
                }
            }
        }

        /// <summary>
        /// Fluent icon that can be displayed which is based on the file extension, 
        /// OpenAI's ChatGPT was used to generate it (although it has been modified)
        /// </summary>
        public FluentIcons.Common.Symbol Icon
        {
            get
            {
                return _torrentContent is null
                    ? FluentIcons.Common.Symbol.Folder
                    : DataConverter.FileToFluentIcon(_torrentContent.Name);
            }
        }

        /// <summary><inheritdoc cref="TorrentContent.Index"/></summary>
        public int Index
        {
            get => _torrentContent?.Index ?? -1;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Index)
                {
                    _torrentContent.Index = value;
                    this.RaisePropertyChanged(nameof(Index));
                }
            }
        }

        /// <summary>
        /// Returns all indexes of this node and its children seperated by a pipe symbol |
        /// that have the Priority given as the parameter
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public string RecursiveGetIndexesForPriority(TorrentContentPriority priority)
        {
            List<int> indexes = [];

            foreach (var content in Children)
            {
                if (content.IsFile && !content.Priority.Equals(priority) && content.Index is int ind)
                {
                    indexes.Add(ind);
                }
                else
                {
                    var childIndexes = content.RecursiveGetIndexesForPriority(priority);
                    if (!string.IsNullOrEmpty(childIndexes))
                    {
                        indexes.AddRange(childIndexes.Split('|').Select(int.Parse));
                    }
                }
            }

            return string.Join('|', indexes);
        }

        /// <summary><inheritdoc cref="TorrentContent.IsSeeding"/></summary>
        public bool IsSeed
        {
            get => _torrentContent?.IsSeeding ?? false;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.IsSeeding)
                {
                    _torrentContent.IsSeeding = value;
                    this.RaisePropertyChanged(nameof(IsSeed));
                }
            }
        }

        protected string _directoryName = "";
        /// <summary><inheritdoc cref="TorrentContent.Name"/></summary>
        public string Name
        {
            get => _torrentContent?.Name ?? _directoryName;
            set
            {
                if (_torrentContent is not null)
                {
                    if (value != _torrentContent.Name)
                    {
                        _torrentContent.Name = value;
                        this.RaisePropertyChanged(nameof(Name));

                        _displayName = _torrentContent.Name.Split('/').Last();
                        this.RaisePropertyChanged(nameof(DisplayName));
                    }
                }
                else if (value != _directoryName)
                {
                    _directoryName = value;
                    this.RaisePropertyChanged(nameof(Name));

                    _displayName = _directoryName.Split('/').Last();
                    this.RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary><inheritdoc cref="TorrentContent.PieceRange"/></summary>
        public QBittorrent.Client.Range PieceRange
        {
            get => _torrentContent?.PieceRange ?? new QBittorrent.Client.Range();
            set
            {
                if (_torrentContent is not null && !_torrentContent.PieceRange.Equals(value))
                {
                    _torrentContent.PieceRange = value;
                    this.RaisePropertyChanged(nameof(PieceRange));
                }
            }
        }

        public TorrentContentPriority? folderPriority = null;
        /// <summary><inheritdoc cref="TorrentContent.Priority"/></summary>
        public TorrentContentPriority? Priority
        {
            get => _torrentContent?.Priority ?? folderPriority;
            set
            {
                //File
                if (_torrentContent is not null)
                {
                    if (value != _torrentContent.Priority)
                    {
                        _ = UpdatePriority(value);
                        if (value != null)
                            _torrentContent.Priority = (TorrentContentPriority)value;
                        this.RaisePropertyChanged(nameof(Priority));
                        this.RaisePropertyChanged(nameof(Remaining)); // Might trigger change if switching from or to `.Skip` 
                    }
                }
                //Folder
                else if (_torrentContent is null && value != folderPriority)
                {
                    _ = UpdatePriority(value);
                    if (value != null)
                        folderPriority = (TorrentContentPriority)value;
                    this.RaisePropertyChanged(nameof(Priority));
                    this.RaisePropertyChanged(nameof(Remaining)); // Might trigger change if switching from or to `.Skip` 
                }
            }
        }

        /// <summary>
        /// Sets the priority and triggers OnPropertyChanged but does nothing else
        /// Useful for avoiding triggering all file async calls when setting priority on a directory
        /// </summary>
        public void SetPriority(TorrentContentPriority priority)
        {
            if (_torrentContent is TorrentContent torrentContent)
            {
                torrentContent.Priority = priority;
                this.RaisePropertyChanged(nameof(Priority));
            }
            else // Is directory
            {
                folderPriority = priority;
                this.RaisePropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// Updates the priority of this Node and all its children to the parameter.
        /// This will contact the QBittorrent WebUI and set IsUpdating to true for all affected nodes whilst that takes place. 
        /// (if they already have this Priority they won't get updated)
        /// </summary>
        private async Task UpdatePriority(TorrentContentPriority? priority)
        {
            if (priority == null)
                return;

            RecursiveSetUpdating(true);
            if (IsFile)
            {
                await QBittorrentService.SetFilePriorityAsync(_infoHash, Index, (TorrentContentPriority)priority);
            }
            else
            {
                string indexes = RecursiveGetIndexesForPriority((TorrentContentPriority)priority);
                if (indexes != "")
                    RecursiveSetPriority(priority ?? TorrentContentPriority.Minimal);

                await SetMultipleFilePrioritiesAsync(_infoHash, indexes, (TorrentContentPriority)priority!);
            }
            RecursiveSetUpdating(false);
        }

        /// <summary>
        /// qbittorrent-net-client doesn't support updating multiple files in one go natively, but the QBittorrent WebUI API does.<br/>
        /// This method implements its own logic to do so using <see cref="QBittorrentService.GetHttpClient"/> and <see cref="QBittorrentService.GetUrl"/>
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="fileIds"></param>
        /// <param name="priority"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async Task SetMultipleFilePrioritiesAsync(string hash, string fileIds, TorrentContentPriority priority, CancellationToken? token = null)
        {
            if (!Enum.GetValues(typeof(TorrentContentPriority)).Cast<TorrentContentPriority>().Contains(priority))
            {
                throw new ArgumentOutOfRangeException(nameof(priority));
            }

            var baseUrl = QBittorrentService.GetUrl();
            var requestUri = new Uri(baseUrl, "api/v2/torrents/filePrio");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", hash),
                new KeyValuePair<string, string>("id", fileIds),
                new KeyValuePair<string, string>("priority", ((int)priority).ToString())
            });

            try
            {
                using (content)
                {
                    var client = QBittorrentService.GetHttpClient();

                    // Ensure the client has the necessary headers
                    client.DefaultRequestHeaders.Clear();
                    foreach (var header in QBittorrentService.QBittorrentClient.DefaultRequestHeaders)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    using HttpResponseMessage message =
                        await client.PostAsync(requestUri, content, token ?? CancellationToken.None).ConfigureAwait(false);
                    message.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
                Debug.WriteLine($"Status Code: {ex.StatusCode}");
                Debug.WriteLine($"Request URI: {requestUri} {fileIds}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sets IsUpdating on this node and all its children
        /// </summary>
        /// <param name="isUpdating"></param>
        public void RecursiveSetUpdating(bool isUpdating)
        {
            if (IsFile)
                IsUpdating = isUpdating;
            foreach (var child in Children)
            {
                child.RecursiveSetUpdating(isUpdating);
            }
        }

        /// <summary>
        /// Set the priority on this node and all of its children
        /// </summary>
        /// <param name="pr"></param>
        public void RecursiveSetPriority(TorrentContentPriority pr)
        {
            SetPriority(pr);
            foreach (var child in Children)
            {
                child.RecursiveSetPriority(pr);
            }
        }

        /// <summary>
        /// Calculates the progress as a percentage based on the size and completion in child nodes.
        /// </summary>
        private double CalculateProgress()
        {
            double totalSize = 0;
            double completedSize = 0;

            foreach (TorrentContentViewModel tcvm in Children)
            {
                totalSize += tcvm.Size;
                completedSize += tcvm.Size * tcvm.Progress;
            }

            return totalSize == 0 ? 0 : completedSize / totalSize;
        }

        /// <summary>
        /// Progress based on the calculated or node-specific value.
        /// </summary>
        public double Progress
        {
            get => _torrentContent?.Progress ?? CalculateProgress();
            set
            {
                if (_torrentContent != null && value != _torrentContent.Progress)
                {
                    _torrentContent.Progress = value;
                    this.RaisePropertyChanged(nameof(Progress));
                    this.RaisePropertyChanged(nameof(Remaining));
                }
            }
        }

        /// <summary>
        /// The total size, calculated from child nodes if not set.
        /// </summary>
        public long Size
        {
            get => _torrentContent?.Size ?? Children.Sum(tcvm => tcvm.Size);
            set
            {
                if (_torrentContent != null && value != _torrentContent.Size)
                {
                    _torrentContent.Size = value;
                    this.RaisePropertyChanged(nameof(Size));
                    this.RaisePropertyChanged(nameof(Remaining));
                }
            }
        }

        /// <summary>
        /// Calculates the remaining size based on priority.
        /// </summary>
        private long CalculateRemaining()
        {
            return Children
                .Where(tcvm => tcvm.Priority != TorrentContentPriority.Skip)
                .Sum(tcvm => tcvm.Remaining);
        }

        /// <summary>
        /// Gets the remaining size, calculated if node-specific or based on progress.
        /// </summary>
        public long Remaining
        {
            get
            {
                if (_torrentContent == null)
                    return CalculateRemaining();
                else
                    return Priority == TorrentContentPriority.Skip
                        ? 0
                        : Size - (long)(Size * Progress);
            }
        }

        /// <summary>
        /// Updates this node to the values of the given TorrentContent
        /// </summary>
        /// <param name="tc"></param>
        public void Update(TorrentContent tc)
        {
            Availability = tc.Availability;
            Index = tc.Index ?? -1;
            IsSeed = tc.IsSeeding;
            Name = tc.Name;
            PieceRange = tc.PieceRange;
            Priority = tc.Priority;
            Progress = tc.Progress;
            Size = tc.Size;
        }

        /// <summary>
        /// Sends the change to the server and if successfull
        /// updates all children (if any) to reflect the new name
        /// 
        /// On failure it restores the oldName
        /// </summary>
        public async Task SaveAndCascadeDisplayNameChange(string oldName)
        {
            string oldPath = Name;
            string newPath = Name.ReplaceLastOccurrence(oldName, DisplayName);
            try
            {
                //Apply remotely - only if done successfully apply the changes locally
                if (IsFile)
                {
                    await QBittorrentService.RenameFileAsync(_infoHash, oldPath, newPath);
                    Name = newPath;
                }
                else
                {
                    await QBittorrentService.RenameFolderAsync(_infoHash, oldPath, newPath);

                    // Apply rename to this node
                    Name = newPath;
                    // Apply rename to all children of this node
                    RecursiveApplyNameChange(oldPath, newPath);
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        public void RecursiveApplyNameChange(string oldPath, string newPath)
        {
            foreach(var child in Children)
            {
                child.Name = child.Name.ReplaceFirstOccurrence(oldPath, Name);
                if (child.Children.Count > 0)
                    child.RecursiveApplyNameChange(oldPath, newPath);
            }
        }
    }
}
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public enum SearchInOption { NamePlusExtension, Name, Extension }
    public class RenameTorrentFilesWindowViewModel : AutoUpdateViewModelBase
    {
        private ObservableCollection<TorrentContentRenameViewModel> _torrentContents = [];
        public ObservableCollection<TorrentContentRenameViewModel> TorrentContents
        {
            get => _torrentContents;
            set
            {
                if(value != _torrentContents)
                {
                    _torrentContents = value;
                    OnPropertyChanged(nameof(TorrentContents));
                }
            }
        }

        private bool _isUpdating = false;
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

        public RenameTorrentFilesWindowViewModel(string hash, int interval = 1500 * 7)
        {
            _infoHash = hash;
            _ = FetchDataAsync();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);

            RenameFolderCommand = ReactiveCommand.CreateFromTask(RenameFolderAsync);
        }

        public ReactiveCommand<Unit, Unit> RenameFolderCommand { get; }
        public async Task RenameFolderAsync()
        {
            try
            {
                IsUpdating = true;

                var filesToRename = TorrentContents
                    .Where(x => x.IsFile && x.Renamed != string.Empty)
                    .Reverse(); //The UI might start updating the naming in an unwanted wa

                foreach (var file in filesToRename)
                {
                    file.IsUpdating = true;
                    try
                    {
                        Debug.WriteLine($"Rename: {file.OriginalName} to: {file.FullRenamedPath}");
                        await QBittorrentService.QBittorrentClient.RenameFileAsync(_infoHash, file.OriginalName, file.FullRenamedPath);

                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        file.IsUpdating = false;
                    }
                }

                var directoriesToRename = 
                    TorrentContents.Where(x => !x.IsFile && x.Renamed != string.Empty)
                    .Reverse();

                foreach(var directory in directoriesToRename)
                {
                    directory.IsUpdating = true;
                    try
                    {
                        await QBittorrentService.QBittorrentClient.RenameFolderAsync(_infoHash, directory.OriginalName, directory.FullRenamedPath);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        directory.IsUpdating = false;
                    }
                }
                //await
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        protected override Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected async override Task FetchDataAsync()
        {
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
        }

        private void Initialise(IReadOnlyList<TorrentContent> torrentContents)
        {
            var addedDirectories = new HashSet<string>();

            foreach (var torrentContent in torrentContents)
            {
                var directories = torrentContent.Name.Split('/')[..^1];
                List<string> dirPath = [];
                foreach (var directory in directories)
                {
                    if (!addedDirectories.Contains(directory))
                    {
                        dirPath.Add(directory);
                        TorrentContents.Add(new TorrentContentRenameViewModel(_infoHash, dirPath));
                        addedDirectories.Add(directory);
                        TorrentContents.Last().PropertyChanged += RenameTorrentFilesWindowViewModel_PropertyChanged;
                    }
                }
                TorrentContents.Add(new TorrentContentRenameViewModel(_infoHash, torrentContent));
                TorrentContents.Last().PropertyChanged += RenameTorrentFilesWindowViewModel_PropertyChanged;
            }
        }

        private void RenameTorrentFilesWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TorrentContentRenameViewModel.RenameMe) && sender is TorrentContentRenameViewModel tctvm)
            {
                UpdateRenamedPreviewOn(tctvm);
            }
        }

        private List<String> Directories(string path)
        {
            List<string> directories = [];
            if (path.Contains('/'))
            {
                directories.Add(path.Substring(0, path.IndexOf("/")));
                directories.AddRange(Directories(path.Substring(path.IndexOf("/"))));
            }
            return directories;
        }

        private bool _replaceAll = false;
        public bool ReplaceAll
        {
            get => _replaceAll;
            set
            {
                if (value != _replaceAll)
                {
                    _replaceAll = value;
                    OnPropertyChanged(nameof(ReplaceAll));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _useRegex = false;
        public bool UseRegex
        {
            get => _useRegex;
            set
            {
                if (value != _useRegex)
                {
                    _useRegex = value;
                    OnPropertyChanged(nameof(UseRegex));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _matchMultiple = true;
        public bool MatchMultiple
        {
            get => _matchMultiple;
            set
            {
                if (value != _matchMultiple)
                {
                    _matchMultiple = value;
                    OnPropertyChanged(nameof(MatchMultiple));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _caseSensitive = false;
        public bool CaseSensitive
        {
            get => _caseSensitive;
            set
            {
                if (value != _caseSensitive)
                {
                    _caseSensitive = value;
                    OnPropertyChanged(nameof(CaseSensitive));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _includeFiles = true;
        public bool IncludeFiles
        {
            get => _includeFiles;
            set
            {
                if (value != _includeFiles)
                {
                    _includeFiles = value;
                    OnPropertyChanged(nameof(IncludeFiles));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _includeFolders = true;
        public bool IncludeFolders
        {
            get => _includeFolders;
            set
            {
                if (value != _includeFolders)
                {
                    _includeFolders = value;
                    OnPropertyChanged(nameof(IncludeFolders));
                    UpdateRenamedPreview();
                }
            }
        }

        private string _searchFor = "";
        public string SearchFor
        {
            get => _searchFor;
            set
            {
                if (value != _searchFor)
                {
                    _searchFor = value;
                    OnPropertyChanged(nameof(CaseSensitive));
                    UpdateRenamedPreview();
                }
            }
        }

        private string _replaceWith = "";
        public string ReplaceWith
        { 
            get => _replaceWith;
            set
            {
                if (value != _replaceWith)
                {
                    _replaceWith = value;
                    OnPropertyChanged(nameof(ReplaceWith));
                    UpdateRenamedPreview();
                }
            }
        }

        private int _enumerationStart = 1;
        public int EnumerationStart
        {
            get => _enumerationStart;
            set
            {
                if (value != _enumerationStart)
                {
                    _enumerationStart = value;
                    OnPropertyChanged(nameof(EnumerationStart));
                    UpdateRenamedPreview();
                }
            }
        }

        private int _enumerationLength = 1;
        public int EnumerationLength
        {
            get => _enumerationLength;
            set
            {
                if (value != _enumerationLength)
                {
                    _enumerationLength = value;
                    OnPropertyChanged(nameof(EnumerationLength));
                    UpdateRenamedPreview();
                }
            }
        }

        public string[] AvailableSearchInOptions =>
        [
            DataConverter.SearchInOptionDescriptions.NamePlusExtension,
            DataConverter.SearchInOptionDescriptions.Name,
            DataConverter.SearchInOptionDescriptions.Extension
        ];

        private SearchInOption _searchInOption = SearchInOption.NamePlusExtension;
        public SearchInOption SearchInOption
        {
            get => _searchInOption;
            set
            {
                if (value != _searchInOption)
                {
                    _searchInOption = value;
                    OnPropertyChanged(nameof(SearchInOption));
                    UpdateRenamedPreview();
                }
            }
        }

        private void UpdateRenamedPreview()
        {
            foreach (var torrentContent in TorrentContents)
            {
                UpdateRenamedPreviewOn(torrentContent);
            }
            PreventDuplicateNaming();
        }

        private void PreventDuplicateNaming()
        {
            var grouped = TorrentContents.Where(x => x.Renamed != "")
                .GroupBy(x => x.FullRenamedPath);

            foreach (var group in grouped)
            {
                if (group.Count() > 1)
                {
                    int counter = EnumerationStart;
                    foreach (var item in group)
                    {
                        string formattedCounter = counter.ToString("D" + EnumerationLength);

                        if (item.IsFile && item.Renamed.Contains('.'))
                        {
                            int lastDotIndex = item.Renamed.LastIndexOf('.');
                            item.Renamed = item.Renamed.Substring(0, lastDotIndex) + $"_{formattedCounter}" + item.Renamed.Substring(lastDotIndex);
                        }
                        else
                            item.Renamed = $"{item.Renamed}_{formattedCounter}";
                        counter++;
                    }
                }
            }
        }

        private void UpdateRenamedPreviewOn(TorrentContentRenameViewModel torrentContent)
        {
            if (!string.IsNullOrEmpty(SearchFor) && IsMatch(torrentContent))
            {
                torrentContent.IsMatch = true;
                Debug.WriteLine($"Matched: `{SearchFor}` on `{torrentContent.DisplayName}` - Renaming to {ReplaceWith}");
                RenameTorrent(torrentContent);
                Debug.WriteLine($"Renamed {torrentContent.DisplayName} to: {torrentContent.Renamed}");
            }
            else
            {
                torrentContent.Renamed = "";
                torrentContent.IsMatch = false;
            }
        }

        private void RenameTorrent(TorrentContentRenameViewModel torrentContent)
        {
            string name = torrentContent.IsFile ? torrentContent.FileName : torrentContent.DisplayName;
            string extension = torrentContent.FileExtension;

            if (SearchInOption == SearchInOption.NamePlusExtension)
            {
                torrentContent.Renamed = Replace(torrentContent.DisplayName);
            }
            else if (SearchInOption == SearchInOption.Name)
            {
                name = Replace(name);
                if (torrentContent.FileExtension != string.Empty)
                    name += $".{torrentContent.FileExtension}";
                torrentContent.Renamed = name;
            }
            else if (torrentContent.IsFile 
                && torrentContent.FileExtension != string.Empty
                && SearchInOption == SearchInOption.Extension)
            {
                extension = Replace(extension);
                torrentContent.Renamed = $"{name}.{extension}";
            }
        }

        /// <summary>
        /// Replaces all occurences of <c>SearchFor</c> in the DisplayName with <c>ReplaceWith</c>
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string Replace(string text)
        {
            if (UseRegex)
            {
                // Ensure regex matches the whole text, not every possible segment
                string pattern = $"^{SearchFor}$";
                return MatchMultiple
                    ? Regex.Replace(text, pattern, ReplaceWith, GetRegexOptions())
                    : ReplaceFirstRegex(text, pattern, ReplaceWith, GetRegexOptions());
            }
            else
            {
                return MatchMultiple
                    ? text.Replace(SearchFor, ReplaceWith, GetStringComparison())
                    : ReplaceFirst(text, SearchFor, ReplaceWith, GetStringComparison());
            }
        }



        /// <summary>
        /// Replaces only the first occurence
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        private string ReplaceFirst(string text, string search, string replace, StringComparison comparison)
        {
            int pos = text.IndexOf(search, comparison);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Replaces only the first occurence 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <param name="replace"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private string ReplaceFirstRegex(string text, string pattern, string replace, RegexOptions options)
        {
            Regex regex = new Regex(pattern, options);
            return regex.Replace(text, replace, 1);
        }

        /// <summary>
        /// Returns true if the given torrentContent.DisplayName matches the filters
        /// </summary>
        /// <param name="torrentContent"></param>
        /// <returns></returns>
        private bool IsMatch(TorrentContentRenameViewModel torrentContent)
        {
            if (!torrentContent.RenameMe)
                return false;

            if (torrentContent.IsFile && !IncludeFiles)
                return false;

            if (!torrentContent.IsFile && !IncludeFolders)
                return false;

            string name = torrentContent.FileName;
            string extension = torrentContent.FileExtension;

            if (SearchInOption == SearchInOption.NamePlusExtension)
                return IsMatch(torrentContent.DisplayName);
            else if (SearchInOption == SearchInOption.Name && !IsMatch(name))
                return false;
            else if (SearchInOption == SearchInOption.Extension && !IsMatch(extension))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given text matches the filters
        /// (Takes all filters into account)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsMatch(string text)
        {
            if (UseRegex)
            {
                try
                {
                    return Regex.IsMatch(text, SearchFor, GetRegexOptions());
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return text.Contains(SearchFor, GetStringComparison());
            }
        }

        private RegexOptions GetRegexOptions() => CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        private StringComparison GetStringComparison() => CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }
}
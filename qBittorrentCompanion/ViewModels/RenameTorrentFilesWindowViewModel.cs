﻿using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentIcons.Avalonia;
using Avalonia;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using System.ComponentModel;
using AutoPropertyChangedGenerator;

namespace qBittorrentCompanion.ViewModels
{
    public enum SearchInOption { NamePlusExtension, Name, Extension }
    public enum RenameOption { Replace, ReplaceOneByOne, ReplaceAll }

    public partial class RenameTorrentFilesWindowViewModel : AutoUpdateViewModelBase
    {
        [AutoPropertyChanged]
        protected ObservableCollection<TorrentRenameContentViewModel> _torrentContents = [];

        protected override async Task FetchDataAsync()
        {
            var torrentContent = await QBittorrentService.GetTorrentContentsAsync(_infoHash);
            if (torrentContent != null)
            {
                Initialise(torrentContent);
            }
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            await FetchDataAsync();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Used to prevent a loop condition, 
        /// the _masterCheckBox affects IsChecked affects all TorrentContent IsChecked values,
        /// however the _masterCheckBox is also affected by TorrentContent IsChecked values themselves.
        /// To ensure things are temporarily changed in only one direction this boolean is used.
        /// </summary>
        private bool _suspendIsCheckedPropagation = false;
        /// <summary>
        /// Controls the IsChecked property of all TorrentContents items
        /// </summary>
        private readonly CheckBox _masterCheckBox;
        public CheckBox MasterCheckBox { get => _masterCheckBox; }

        /// <summary>
        /// Used in the UI to demonstrate what enumeration looks like
        /// (which is affected by <see cref="EnumerationStart"/> and <see cref="EnumerationLength"/>)
        /// </summary>
        public ObservableCollection<string> Previews { get; set; } = ["file_01", "file_02"];

        public HierarchicalTreeDataGridSource<TorrentRenameContentViewModel> TorrentContentsSource { get; set; } = default!;

        /// <summary>
        /// Useful for enabling/disabling UI elements whilst an update is taking place
        /// </summary>
        [AutoPropertyChanged]
        private bool _isUpdating = false;

        public RenameTorrentFilesWindowViewModel(string hash, int interval = 1500 * 7)
        {
            _infoHash = hash;
            _ = FetchDataAsync();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);

            var iconTemplate = new FuncDataTemplate<TorrentRenameContentViewModel>((x, _) =>
            {
                var spinner = new SymbolIcon
                {
                    Symbol = FluentIcons.Common.Symbol.SpinnerIos,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                spinner.Classes.Add("Spinner");
                spinner.Bind(Control.IsVisibleProperty, new Binding(nameof(x.IsUpdating)));

                var textBlock = new TextBlock
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = Thickness.Parse("5, 0"),
                    LineHeight = 16 // Keeps things vertically centered - same as the font size
                };
                textBlock.Bind(TextBox.TextProperty, new Binding(nameof(x.DisplayName)));

                var dockPanel = new DockPanel
                {                    
                    Children =
                    {
                        spinner,
                        new SymbolIcon
                        {
                            Symbol = x.Icon,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        },
                        textBlock
                    }
                };

                var toolTipTextBlock = new TextBlock();
                toolTipTextBlock.Bind(TextBlock.TextProperty, new Binding(nameof(x.Name)));
                ToolTip.SetTip(dockPanel, toolTipTextBlock);

                return dockPanel;
            }, true);

            var checkBoxColumnTemplate = new FuncDataTemplate<TorrentRenameContentViewModel>((x, _) =>
            {
                var checkBox = new CheckBox()
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(x.IsChecked)));

                var stackPanel = new StackPanel()
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 8,

                    Children = {
                        new SymbolIcon
                        {
                            Symbol = FluentIcons.Common.Symbol.ExtendedDock,
                            IconVariant = FluentIcons.Common.IconVariant.Filled,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            RenderTransform = new RotateTransform(90),
                            Margin = Thickness.Parse("12 0 -6 0"),
                            Opacity = 0.5
                        },
                        checkBox
                    }
                };

                return stackPanel;
            }, true);

            _masterCheckBox = new CheckBox()
            {
                IsThreeState = false,
                IsChecked = true,
                Margin = Thickness.Parse("0 0 0 -8")
            };
            _masterCheckBox.AddHandler(InputElement.PointerPressedEvent, _checkAllCheckBox_PointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

            TorrentContentsSource = new HierarchicalTreeDataGridSource<TorrentRenameContentViewModel>(TorrentContents)
            {
                Columns =
                {
                    //CheckBoxColumn seems to have isues with displaying updates to IsChecked thus a TemplateColumn with a CheckBox is used.
                    new TemplateColumn<TorrentRenameContentViewModel>(_masterCheckBox, checkBoxColumnTemplate),
                    new HierarchicalExpanderColumn<TorrentRenameContentViewModel>( 
                        new TemplateColumn<TorrentRenameContentViewModel>("Name", iconTemplate, null, GridLength.Auto),
                        x => x.Children,
                        null,
                        x => x.IsExpanded
                    ),
                    new TextColumn<TorrentRenameContentViewModel, string>("Renamed", x => x.RenameTo, GridLength.Auto)
                }
            };

            RenameCommand = ReactiveCommand.CreateFromTask(RenameAsync);
            SetSelectedRenameOptionCommand = ReactiveCommand.Create<RenameOption>(SetSelectedRenameOption);
        }

        private void _checkAllCheckBox_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            List<TorrentRenameContentViewModel> allTorrentRenameContentViewModels = [];
            foreach (var tc in TorrentContents)
                tc.GetAll(allTorrentRenameContentViewModels);

            bool newCheckState = _masterCheckBox.IsChecked == false;

            _suspendIsCheckedPropagation = true;
            foreach (var tcvm in allTorrentRenameContentViewModels)
                tcvm.IsChecked = newCheckState;
            _suspendIsCheckedPropagation = false; ;

        }

        public ReactiveCommand<RenameOption, Unit> SetSelectedRenameOptionCommand { get; }

        private void SetSelectedRenameOption(RenameOption renameOption)
        {
            SelectedRenameOption = renameOption;
        }

        public ReactiveCommand<Unit, Unit> RenameCommand { get; }
        public async Task RenameAsync()
        {
            IsUpdating = true;
            try
            {
                //Get all the items that have to be renamed (Their .RenameTo value isn't empty)
                List<TorrentRenameContentViewModel> itemsToRename = [];
                foreach (var torrentContent in TorrentContents)
                    torrentContent.GetAllToBeRenamed(itemsToRename);

                switch(SelectedRenameOption)
                {
                    case RenameOption.Replace:
                        await RenameSingle(itemsToRename.First());
                        break;
                    case RenameOption.ReplaceOneByOne:
                        await RenameOneByOne(itemsToRename);
                        break;
                    case RenameOption.ReplaceAll:
                        //To avoid ending up with empty directories, rename directories with children first
                        var directoriesWithChildren = itemsToRename.Where(trcvm => trcvm.Children.Count() > 0);
                        foreach (var directory in directoriesWithChildren)
                            await RenameSingle(directory);

                        //Folders with children have been processed, discard them for batch operation
                        itemsToRename.RemoveAll(trcvm => directoriesWithChildren.Contains(trcvm));

                        //All that should be left is files or directories without children,
                        //Should be safe to batch process without one change affecting another
                        await RenameAll(itemsToRename);
                        break;
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            finally { IsUpdating = false; }
        }

        private async Task RenameSingle(TorrentRenameContentViewModel torrentContent)
        {
            torrentContent.IsUpdating = true;
            var newName = torrentContent.NameToRenameTo();

            try
            {
                Debug.WriteLine($"{torrentContent.Name} » {newName}");
                if(torrentContent.IsFile)
                    await QBittorrentService.RenameFileAsync(_infoHash, torrentContent.Name, newName);
                else
                    await QBittorrentService.RenameFolderAsync(_infoHash, torrentContent.Name, newName);

                ApplyPostRenameChanges(torrentContent, newName);
                UpdateRenamedPreviewOn(torrentContent);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            finally { torrentContent.IsUpdating = false; }
        }

        /// <summary>
        /// Waits for a response before sending the next change request. This should prevent hitting any safeguards.
        /// </summary>
        /// <param name="torrentContents"></param>
        /// <returns></returns>
        private async Task RenameOneByOne(IEnumerable<TorrentRenameContentViewModel> torrentContents)
        {
            foreach(var torrentContent in torrentContents)
                await RenameSingle(torrentContent);
        }

        /// <summary>
        /// Sends out all rename requests at once and waits for all of them to complete.
        /// </summary>
        /// <param name="torrentContents"></param>
        /// <returns></returns>
        private async Task RenameAll(IEnumerable<TorrentRenameContentViewModel> torrentContents)
        {
            var renameTasks = new List<Task>();
            Dictionary<TorrentRenameContentViewModel, string> newNameStorage = [];

            Debug.WriteLine("Batch:");
            foreach (var torrentContent in torrentContents)
            {
                torrentContent.IsUpdating = true;
                var newName = torrentContent.NameToRenameTo();
                newNameStorage.Add(torrentContent, newName);

                try
                {
                    Debug.WriteLine($"{torrentContent.Name} » {newName}");
                    var renameTask = torrentContent.IsFile
                        ? QBittorrentService.RenameFileAsync(_infoHash, torrentContent.Name, newName)
                        : QBittorrentService.RenameFolderAsync(_infoHash, torrentContent.Name, newName);
                    renameTasks.Add(renameTask);
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
            }

            try { await Task.WhenAll(renameTasks); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            finally { torrentContents.ToList().ForEach(tcvm => tcvm.IsUpdating = false); }

            foreach (var (tcvm, newName) in newNameStorage)
                ApplyPostRenameChanges(tcvm, newName);

            UpdateRenamedPreview();
        }

        private void ApplyPostRenameChanges(TorrentRenameContentViewModel torrentContent, string newName)
        {
            torrentContent.Name = newName;
            torrentContent.IsChecked = false;
        }

        private void RenameTorrentFilesWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TorrentRenameContentViewModel.IsChecked) && sender is TorrentRenameContentViewModel tctvm)
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
                    this.RaisePropertyChanged(nameof(ReplaceAll));
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
                    this.RaisePropertyChanged(nameof(UseRegex));
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
                    this.RaisePropertyChanged(nameof(MatchMultiple));
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
                    this.RaisePropertyChanged(nameof(CaseSensitive));
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
                    this.RaisePropertyChanged(nameof(IncludeFiles));
                    UpdateRenamedPreview();
                }
            }
        }

        private bool _includeDirectories = true;
        public bool IncludeDirectories
        {
            get => _includeDirectories;
            set
            {
                if (value != _includeDirectories)
                {
                    _includeDirectories = value;
                    this.RaisePropertyChanged(nameof(IncludeDirectories));
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
                    this.RaisePropertyChanged(nameof(CaseSensitive));
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
                    this.RaisePropertyChanged(nameof(ReplaceWith));
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
                    this.RaisePropertyChanged(nameof(EnumerationStart));
                    this.RaisePropertyChanged(nameof(EnumerationPreview1));
                    this.RaisePropertyChanged(nameof(EnumerationPreview2));
                    UpdateRenamedPreview();
                }
            }
        }

        private int _enumerationLength = 2;
        public int EnumerationLength
        {
            get => _enumerationLength;
            set
            {
                if (value != _enumerationLength)
                {
                    _enumerationLength = value;
                    this.RaisePropertyChanged(nameof(EnumerationLength));
                    this.RaisePropertyChanged(nameof(EnumerationPreview1));
                    this.RaisePropertyChanged(nameof(EnumerationPreview2));
                    UpdateRenamedPreview();
                }
            }
        }

        public string EnumerationPreview1 => "file" + GetEnumeratedString(0);
        public string EnumerationPreview2 => "file" + GetEnumeratedString(1);

        private string GetEnumeratedString(int value)
        {
            return "_" + (value + EnumerationStart).ToString("D" + EnumerationLength);
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
                    this.RaisePropertyChanged(nameof(SearchInOption));
                    UpdateRenamedPreview();
                }
            }
        }

        public static RenameOption[] RenameOptions =>
            [ RenameOption.Replace, RenameOption.ReplaceOneByOne, RenameOption.ReplaceAll ];

        [AutoPropertyChanged]
        private RenameOption _selectedRenameOption = RenameOption.ReplaceAll;

        private void UpdateRenamedPreview()
        {
            foreach (var torrentContent in TorrentContents)
                UpdateRenamedPreviewOn(torrentContent);

            PreventDuplicateNaming(TorrentContents);
        }

        private void PreventDuplicateNaming(IEnumerable<TorrentRenameContentViewModel> ocTorrentRenameContentViewModels)
        {
            var groups = ocTorrentRenameContentViewModels
                .Where(tcvm => !string.IsNullOrEmpty(tcvm.RenameTo))
                .GroupBy(tcvm => tcvm.RenameTo);

            foreach (var group in groups.Where(g => g.Count() > 1))
            {
                int counter = EnumerationStart - 1;
                foreach(var tcvm in group)
                {
                    string formattedCounter = GetEnumeratedString(counter);
                    int lastDotIndex = tcvm.RenameTo.LastIndexOf('.');

                    tcvm.RenameTo = tcvm.IsFile && lastDotIndex > -1
                        ? tcvm.RenameTo.Substring(0, lastDotIndex) + formattedCounter + tcvm.RenameTo.Substring(lastDotIndex)
                        : tcvm.RenameTo + formattedCounter;

                    counter++;
                }
            }

            foreach (var ocTorrentRenameContentViewModel in ocTorrentRenameContentViewModels)
                PreventDuplicateNaming(ocTorrentRenameContentViewModel.Children);
        }

        private void UpdateRenamedPreviewOn(TorrentRenameContentViewModel torrentContent)
        {
            var shouldBeRenamed = torrentContent.IsChecked && !string.IsNullOrEmpty(SearchFor) && IsMatch(torrentContent);

            if (shouldBeRenamed)
                RenameTorrent(torrentContent);
            else
                torrentContent.RenameTo = "";

            foreach(var child in torrentContent.Children)
                UpdateRenamedPreviewOn(child);
        }

        private void RenameTorrent(TorrentRenameContentViewModel torrentContent)
        {
            string name = torrentContent.DisplayName;
            string extension = torrentContent.FileExtension;

            if (SearchInOption == SearchInOption.NamePlusExtension)
            {
                torrentContent.RenameTo = Replace(torrentContent.DisplayName);
            }
            else if (SearchInOption == SearchInOption.Name)
            {
                name = Replace(name);
                if (torrentContent.FileExtension != string.Empty)
                    name += $".{torrentContent.FileExtension}";
                torrentContent.RenameTo = name;
            }
            else if (torrentContent.IsFile 
                && torrentContent.FileExtension != string.Empty
                && SearchInOption == SearchInOption.Extension)
            {
                extension = Replace(extension);
                torrentContent.RenameTo = $"{name[..^torrentContent.FileExtension.Length]}{extension}";
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
        private bool IsMatch(TorrentRenameContentViewModel torrentContent)
        {
            if (!torrentContent.IsChecked)
                return false;

            if (torrentContent.IsFile && !IncludeFiles)
                return false;

            if (!torrentContent.IsFile && !IncludeDirectories)
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
                try
                {
                    return Regex.IsMatch(text, SearchFor, GetRegexOptions());
                }
                catch { return false; }
            else
                return text.Contains(SearchFor, GetStringComparison());
        }

        private RegexOptions GetRegexOptions() => CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        private StringComparison GetStringComparison() => CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        public void Initialise(IReadOnlyList<TorrentContent> torrentContents)
        {
            CreateRootItems(torrentContents);
            CreateChildItems(torrentContents);
        }

        private void CreateRootItems(IReadOnlyList<TorrentContent> torrentContents)
        {
            foreach (var torrentContent in torrentContents)
            {
                string[] pathParts = torrentContent.Name.Split("/");
                string rootPart = pathParts[0];

                if (!_torrentContents.Any(x => x.DisplayName == rootPart))
                {
                    TorrentRenameContentViewModel rootItem = 
                        (pathParts.Length == 1)
                        ? new TorrentRenameContentViewModel(_infoHash, torrentContent, null)
                        : new TorrentRenameContentViewModel(_infoHash, rootPart, null);

                    rootItem.PropertyChanged += TorrentRenameContentViewModel_PropertyChanged;
                    _torrentContents.Add(rootItem);
                }
            }
        }

        private void CreateChildItems(IReadOnlyList<TorrentContent> torrentContents)
        {
            foreach (var torrentContent in torrentContents)
            {
                string[] pathParts = torrentContent.Name.Split("/");
                if (pathParts.Length == 1) continue; // Skip root items

                var rootPart = pathParts[0];
                var rootItem = _torrentContents.First(x => x.DisplayName == rootPart);

                AddChildToHierarchy(rootItem, torrentContent, pathParts, 1);
            }
        }

        private void AddChildToHierarchy(TorrentRenameContentViewModel parent, TorrentContent torrentContent, string[] pathParts, int currentIndex)
        {
            if (currentIndex >= pathParts.Length) return;

            var currentPart = pathParts[currentIndex];
            var existingChild = parent.Children.FirstOrDefault(x => x.DisplayName == currentPart);

            if (existingChild == null)
            {
                TorrentRenameContentViewModel newChild = currentIndex == pathParts.Length - 1
                    ? new TorrentRenameContentViewModel(_infoHash, torrentContent, parent)
                    : new TorrentRenameContentViewModel(_infoHash, string.Join("/", pathParts.Take(currentIndex + 1)), parent);

                newChild.PropertyChanged += TorrentRenameContentViewModel_PropertyChanged;
                parent.AddChild(newChild);
                existingChild = newChild;
            }

            AddChildToHierarchy(existingChild, torrentContent, pathParts, currentIndex + 1);
        }

        private void TorrentRenameContentViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Change was caused by the mastercheckbox, let's not try to change IT in return.
            if (_suspendIsCheckedPropagation)
                return;

            if (e.PropertyName == nameof(TorrentRenameContentViewModel.IsChecked) 
            && sender is TorrentRenameContentViewModel tcvm)
            {
                if (tcvm.IsChecked != _masterCheckBox.IsChecked)
                    UpdateCheckAllCheckBox();
            }
        }

        private IEnumerable<TorrentRenameContentViewModel> GetAllTorrentRenameContentViewModels(TorrentRenameContentViewModel parent)
        {
            yield return parent;

            foreach (var child in parent.Children)
            {
                foreach (var descendant in GetAllTorrentRenameContentViewModels(child))
                {
                    yield return descendant;
                }
            }
        }

        private void UpdateCheckAllCheckBox()
        {
            List<TorrentRenameContentViewModel> allTorrentContents = [];
            foreach (var torrentContent in TorrentContents)
                torrentContent.GetAll(allTorrentContents);

            if (allTorrentContents.All(t => t.IsChecked == true))
                _masterCheckBox.IsChecked = true;
            else if (allTorrentContents.All(t => t.IsChecked == false))
                _masterCheckBox.IsChecked = false;
            else
                _masterCheckBox.IsChecked = null;

            _masterCheckBox.InvalidateVisual();
        }
    }
}
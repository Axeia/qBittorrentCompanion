﻿using AutoPropertyChangedGenerator;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class TorrentContentsViewModel : AutoUpdateViewModelBase
    {
        
        private string _oldName = string.Empty;

        [AutoPropertyChanged]
        protected ObservableCollection<TorrentContentViewModel> _torrentContents = [];

        protected override async Task FetchDataAsync()
        {
            IReadOnlyList<TorrentContent>? torrentContent = await QBittorrentService.GetTorrentContentsAsync(_infoHash);
            if (torrentContent != null)
            {
                Initialise(torrentContent);
            }
        }

        protected override async Task UpdateDataAsync(object? sender, EventArgs e)
        {
            await FetchDataAsync();
        }

        /// Part of a workaround for the view not updating properly.
        public Action? UpdatedData;
        public HierarchicalTreeDataGridSource<TorrentContentViewModel> TorrentContentsSource { get; set; } = default!;

        public TorrentContentsViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500 * 7)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is string hash)
            {
                _infoHash = hash;
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }

            var editableTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                DockPanel dockPanel = new()
                {
                    // If a background isn't assigned the PressPointer further on does not
                    // work when not clicking directly on the text iself.
                    Background = new SolidColorBrush()
                };
                dockPanel.Children.Add(
                    new SymbolIcon() { 
                        Symbol = x.IsFile 
                            ? DataConverter.FileToFluentIcon(x.Name)
                            : FluentIcons.Common.Symbol.Folder,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                );

                var textBlock = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(x.DisplayName)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0),
                    LineHeight = 16
                };
                textBlock.Bind(TextBlock.IsVisibleProperty, x.WhenAnyValue(vm => vm.IsEditing).Select(isEditing => !isEditing));
                dockPanel.Children.Add(textBlock);

                var textBox = new TextBox
                {
                    [!TextBox.TextProperty] = new Binding(nameof(x.DisplayName)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0),
                    LineHeight = 16,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                textBox.Bind(TextBox.IsVisibleProperty, x.WhenAnyValue(vm => vm.IsEditing).Select(isEditing => isEditing));
                textBox.AddHandler(InputElement.KeyDownEvent, (sender, args) =>
                {
                    if (args.Key == Key.Enter || args.Key == Key.Return)
                    {
                        // shift away the focus triggering LostFocusEvent
                        if (textBox.FindAncestorOfType<TreeDataGridRow>() is TreeDataGridRow tdgr)
                            tdgr.Focus();
                        // Swap back to a textblock rather than textbox
                        x.IsEditing = false;
                    }
                    // Restores old value if escape is pressed
                    else if (args.Key == Key.Escape)
                    {
                        x.IsEditing = false;
                        x.DisplayName = _oldName;
                    }
                });
                textBox.AddHandler(InputElement.LostFocusEvent, (sender, args) =>
                {
                    x.IsEditing = false;
                    Task task = x.SaveAndCascadeDisplayNameChange(_oldName);
                });
                dockPanel.Children.Add(textBox);

                dockPanel.AddHandler(InputElement.PointerPressedEvent, (sender, args) =>
                {
                    if (args.ClickCount == 1
                    && args.GetCurrentPoint(dockPanel).Properties.IsLeftButtonPressed
                    && dockPanel.FindAncestorOfType<TreeDataGridRow>() is TreeDataGridRow tdgr
                    && tdgr.IsSelected
                    && tdgr.FindAncestorOfType<TreeDataGrid>() is TreeDataGrid tdg
                    && tdg.RowSelection != null && tdg.RowSelection.SelectedIndexes.Count == 1)
                    {
                        x.IsEditing = true;
                        textBox.Focus();
                        textBox.SelectAll();

                        _oldName = textBox.Text!;
                    }
                });

                return dockPanel;
            }, true);

            var progressBarTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                return new Grid
                {
                    Children = {
                        new ProgressBar
                        {
                            [!ProgressBar.ValueProperty] = new Binding(nameof(x.Progress)),
                            Minimum = 0,
                            Maximum = 1,
                            Height = 20,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = Thickness.Parse("2"),
                            MinWidth = 100
                        },
                        new TextBlock
                        {
                            [!TextBox.TextProperty] = new Binding(nameof(x.Progress)){ StringFormat = "{0:P1}" },
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                };
            }, true);

            var comboBoxTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                var cb = new ContentControl
                {
                    [!ContentControl.ContentProperty] = new Binding(),
                    ContentTemplate = (DataTemplate)Application.Current!.FindResource("TorrentContentComboBoxTemplate")!
                };
                if (x.IsFile)
                    cb.Classes.Add("File");

                return cb;
            }, true);

            FuncDataTemplate<TorrentContentViewModel> CreateMonoSpacedTemplate(Func<TorrentContentViewModel, string> selector)
            {
                return new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
                {
                    return new Grid // Grid is needed for VerticalAlignment to get applied
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = selector(x),
                                MinWidth = 100,
                                FontFamily = FontFamily.Parse("Inconsolata, Consolas, Monospace, Courier"),
                                TextAlignment = TextAlignment.Right,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                Margin = Thickness.Parse("5, 0, 5, -5"),
                            }
                        },
                    };
                }, true);
            }

            TorrentContentsSource = new HierarchicalTreeDataGridSource<TorrentContentViewModel>(TorrentContents)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<TorrentContentViewModel>( //, GridLength.Star
                        new TemplateColumn<TorrentContentViewModel>("Name", editableTemplate, null, GridLength.Star),
                        x => x.Children,
                        null,
                        x => x.IsExpanded
                    ),
                    /*new TextColumn<TorrentContentViewModel, string>("Full path", x => x.Name),*/
                    new TemplateColumn<TorrentContentViewModel>("Total size", CreateMonoSpacedTemplate(x => DataConverter.BytesToHumanReadable(x.Size)), null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Remaining", CreateMonoSpacedTemplate(x => DataConverter.BytesToHumanReadable(x.Remaining)), null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Progress", progressBarTemplate, null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Download Priority", comboBoxTemplate, null, GridLength.Parse("158")),
                    new TemplateColumn<TorrentContentViewModel>("Availability", CreateMonoSpacedTemplate(x => DataConverter.AvailabilityToPercentageString(x.Availability)), null, GridLength.Parse("110")),
                }
            }; 
        }
        
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
                    TorrentContentViewModel rootItem = 
                        (pathParts.Length == 1) // It's a file
                        ? rootItem = new TorrentContentViewModel(_infoHash, torrentContent)
                        : rootItem = new TorrentContentViewModel(_infoHash, rootPart);

                    rootItem.PropertyChanged += ExistingChild_PropertyChanged;
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

        private void AddChildToHierarchy(TorrentContentViewModel parent, TorrentContent torrentContent, string[] pathParts, int currentIndex)
        {
            if (currentIndex >= pathParts.Length) return;

            var currentPart = pathParts[currentIndex];
            var existingChild = parent.Children.FirstOrDefault(x => x.DisplayName == currentPart);

            if (existingChild == null)
            {
                existingChild = (currentIndex == pathParts.Length - 1)
                    ? new TorrentContentViewModel(_infoHash, torrentContent)
                    : new TorrentContentViewModel(_infoHash, string.Join("/", pathParts.Take(currentIndex + 1)));

                existingChild.PropertyChanged += ExistingChild_PropertyChanged;
                parent.AddChild(existingChild);
            }

            AddChildToHierarchy(existingChild, torrentContent, pathParts, currentIndex + 1);
        }

        public Action<TorrentContentViewModel>? TorrentPriorityUpdating { get; set; }
        public Action<TorrentContentViewModel>? TorrentPriorityUpdated { get; set; }
        /// <summary>
        /// Invokes <see cref="TorrentPriorityUpdating"/> with the relevant TorrentContentViewModel
        /// Part of a workaround for the ComboBox not updating properly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExistingChild_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TorrentContentViewModel tcvm)
            {
                if (e.PropertyName == nameof(TorrentContentViewModel.Priority))
                    TorrentPriorityUpdating!.Invoke(tcvm);
                else if (e.PropertyName == nameof(TorrentContentViewModel.IsUpdating))
                    TorrentPriorityUpdated!.Invoke(tcvm);
            }
        }
    }
}
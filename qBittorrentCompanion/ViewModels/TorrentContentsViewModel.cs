using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace qBittorrentCompanion.ViewModels
{
    public class TorrentContentsViewModel : AutoUpdateViewModelBase
    {
        /// Part of a workaround for the ComboBox not updating properly
        public Action UpdatedData;

        private ObservableCollection<TorrentContentViewModel> _torrentContents = [];

        public ObservableCollection<TorrentContentViewModel> TorrentContents
        {
            get => _torrentContents;
            set
            {
                if (value != _torrentContents)
                {
                    _torrentContents = value;
                    OnPropertyChanged(nameof(TorrentContents));
                }
            }
        }

        public HierarchicalTreeDataGridSource<TorrentContentViewModel> TorrentContentsSource { get; }

        public TorrentContentsViewModel(TorrentInfoViewModel? torrentInfoViewModel, int interval = 1500 * 7)
        {
            if (torrentInfoViewModel is not null && torrentInfoViewModel.Hash is string hash)
            {
                _infoHash = hash;
                _ = FetchDataAsync();
                _refreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }

            var iconTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                return new DockPanel
                {
                    Children =
                    {
                        new Panel{ }, // Placeholder to be populated by TorrentsView.axaml.cs
                        new SymbolIcon
                        {
                            Symbol = x.Icon,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = x.DisplayName,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = Thickness.Parse("5, 0"),
                            LineHeight = 16 // Keeps things vertically centered - same as the font size
                        }
                    }
                };
            }, true);

            var progressBarTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                return new ProgressBar
                {
                    Value = x.Progress,
                    Minimum = 0,
                    Maximum = 1,
                    Height = 20,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    Margin = Thickness.Parse("2"),
                    MinWidth = 100,
                    ShowProgressText = true,
                };
            }, true);

            var comboBoxTemplate = new FuncDataTemplate<TorrentContentViewModel>((x, _) =>
            {
                var cb = new ContentControl
                {
                    Content = x,
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
                        new TemplateColumn<TorrentContentViewModel>("Name", iconTemplate, null, GridLength.Star),
                        x => x.Children,
                        null,
                        x => x.IsExpanded
                    ),
                    new TemplateColumn<TorrentContentViewModel>("Total size", CreateMonoSpacedTemplate(x => DataConverter.BytesToHumanReadable(x.Size)), null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Remaining", CreateMonoSpacedTemplate(x => DataConverter.BytesToHumanReadable(x.Remaining)), null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Progress", progressBarTemplate, null, GridLength.Parse("110")),
                    new TemplateColumn<TorrentContentViewModel>("Download Priority", comboBoxTemplate, null, GridLength.Parse("140")),
                    new TemplateColumn<TorrentContentViewModel>("Availability", CreateMonoSpacedTemplate(x => DataConverter.AvailabilityToPercentageString(x.Availability)), null, GridLength.Parse("110")),
                }
            }; 
        }

        protected override async Task FetchDataAsync()
        {
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
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
                    TorrentContentViewModel rootItem;

                    if (pathParts.Length == 1) // It's a file
                    {
                        rootItem = new TorrentContentViewModel(_infoHash, torrentContent);
                    }
                    else // It's a directory
                    {
                        rootItem = new TorrentContentViewModel(
                            _infoHash,
                            rootPart,
                            rootPart
                        );
                    }

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
                if (currentIndex == pathParts.Length - 1)
                {
                    existingChild = new TorrentContentViewModel(_infoHash, torrentContent);
                }
                else
                {
                    existingChild = new TorrentContentViewModel(
                        _infoHash,
                        string.Join("/", pathParts.Take(currentIndex + 1)),
                        currentPart
                    );
                }

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
                if(e.PropertyName == nameof(TorrentContentViewModel.Priority))
                    TorrentPriorityUpdating!.Invoke(tcvm);
                else if(e.PropertyName == nameof(TorrentContentViewModel.IsUpdating))
                    TorrentPriorityUpdated!.Invoke(tcvm);
            }
        }


        protected override async Task UpdateDataAsync(object? sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"Updating contents for {_infoHash}");
            IReadOnlyList<TorrentContent> torrentContent = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(_infoHash);
            Initialise(torrentContent);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
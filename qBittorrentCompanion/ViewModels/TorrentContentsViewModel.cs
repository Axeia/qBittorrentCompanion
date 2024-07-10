using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
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
        public Action UpdatedData;
        private ObservableCollection<TorrentContentViewModel> _torrentContents = new ObservableCollection<TorrentContentViewModel>();
        public static string[] TorrentContentPriorities = ["Skip", "Minimal", "Very low", "Low", "Normal", "High", "Very high", "Maximal", "Mixed"];

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
                        new SymbolIcon
                        {
                            Symbol = x.icon,
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
                return new ComboBox
                {
                    ItemsSource = TorrentContentPriorities,
                    MinWidth = 135,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    SelectedIndex = x.ComboBoxIndex
                };
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
                                Margin = Thickness.Parse("5, 0"),
                                LineHeight = 12 // Keeps things vertically centered - same as the font size
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
                        x => x.Contents,
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
            var topLevelItems = new Dictionary<string, TorrentContentViewModel>();

            foreach (var torrentContent in torrentContents)
            {
                Debug.WriteLine(torrentContent.Name);
                AddToHierarchy(TorrentContents, torrentContent);
            }

            //UpdatedData.Invoke();
        }

        private void AddToHierarchy(ObservableCollection<TorrentContentViewModel> parentCollection, TorrentContent torrentContent, int currentIndex = 0)
        {
            string[] pathParts = torrentContent.Name.Split("/");
            if (currentIndex >= pathParts.Length) return;

            var currentPart = pathParts[currentIndex];
            var existingChild = parentCollection.FirstOrDefault(x => x.DisplayName == currentPart);

            if (existingChild == null)
            {
                if (currentIndex == pathParts.Length - 1)
                {
                    // If it's the last part of the path, create a file node
                    existingChild = new TorrentContentViewModel(torrentContent);
                }
                else
                {
                    // Otherwise, create a folder node
                    existingChild = new TorrentContentViewModel(
                        string.Join("/", pathParts.Take(currentIndex + 1)),
                        currentPart
                    );
                }
                parentCollection.Add(existingChild);
            }

            AddToHierarchy(existingChild.Contents, torrentContent, currentIndex + 1);
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
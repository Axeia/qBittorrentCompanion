using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    public partial class TorrentsView : UserControl
    {
        public TorrentsView()
        {
            InitializeComponent();
            DataContext = new TorrentsViewModel();
            Loaded += TorrentsView_Loaded;
        }

        private void TorrentsView_Loaded(object? sender, RoutedEventArgs e)
        {
            TagFilterListBox.SelectionChanged += TagFilterListBox_SelectionChanged;
            StatusFilterListBox.SelectionChanged += StatusFilterListBox_SelectionChanged;
            CategoryFilterListBox.SelectionChanged += CategoryFilterListBox_SelectionChanged;
            TrackerFilterListBox.SelectionChanged += TrackerFilterListBox_SelectionChanged;

            if (DataContext is TorrentsViewModel viewModel)
            {
                viewModel.TagCounts.CollectionChanged += (s, e) 
                    => FilterListBox_CollectionChanged(TagFilterListBox);
                viewModel.TrackerCounts.CollectionChanged += (s, e) 
                    => FilterListBox_CollectionChanged(TrackerFilterListBox);
                viewModel.CategoryCounts.CollectionChanged += (s, e)
                    => FilterListBox_CollectionChanged(CategoryFilterListBox);
            }
        }

        public void OnToggleStatusClicked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                Debug.WriteLine("That'll do");
            }
        }

        private double OldScrollPosition = 0;
        private string[] vars = { nameof(TorrentsViewModel.TagCounts), nameof(TorrentsViewModel.CategoryCounts), nameof(TorrentsViewModel.TrackerCounts) };

        private void FilterListBox_CollectionChanged(ListBox listBox)
        {
            //Save scrollbar position
            OldScrollPosition = LeftPaneScrollViewer.Offset.Y;

            //Select first index if none has been set
            if (listBox.SelectedIndex == -1)
                listBox.SelectedIndex = 0;

            //Restore scrollbar position
            LeftPaneScrollViewer.Offset = new Avalonia.Vector(LeftPaneScrollViewer.Offset.X, 0);
        }

        private void TagFilterListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && this.DataContext is not null)
            {
                if (listbox.SelectedIndex == 0)// All
                {
                    ((TorrentsViewModel)this.DataContext).FilterTag = "";
                }
                else
                {
                    //Debug.WriteLine(listbox.SelectedItem.ToString());
                    var tagCountViewModel = listbox.SelectedItem as TagCountViewModel;
                    if (tagCountViewModel is not null) 
                        ((TorrentsViewModel)this.DataContext).FilterTag = tagCountViewModel.Tag;
                }

            }
        }

        private void StatusFilterListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is null)
                return;

            List<TorrentState> filterStatuses = [];
            if (sender is ListBox listbox)
            {
                if (listbox.SelectedIndex == 3) // Special case
                    ((TorrentsViewModel)this.DataContext).FilterCompleted = true;
                else
                {
                    ((TorrentsViewModel)this.DataContext).FilterCompleted = false;

                    switch (listbox.SelectedIndex)
                    {
                        case 1: //downloading
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Download;
                            break;
                        case 2: //seeding
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Seeding;
                            break;
                        case 3: //completed

                            break;
                        case 4: //resumed
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Resumed;
                            // Handle "Resumed" case here
                            break;
                        case 5: //paused
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Paused;
                            break;
                        case 6: //active
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Active;
                            break;
                        case 7: //inactive
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.InActive;
                            break;
                        case 8: //stalled
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Stalled;
                            break;
                        case 9: //stalled uploading
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.StalledUpload;
                            break;
                        case 10: //stalled downloading
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.StalledDownload;
                            break;
                        case 11: //checking
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Checking;
                            break;
                        case 12: //Error
                            filterStatuses = TorrentsViewModel.TorrentStateGroupings.Error;
                            break;
                        case 0: //all
                        default:
                            // Handle default case here
                            // Handle "All" case here
                            break;
                    }
                    ((TorrentsViewModel)this.DataContext).FilterStatuses = filterStatuses;
                }
            }
        }

        private void CategoryFilterListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && this.DataContext is not null)
            {
                if (listbox.SelectedIndex == 0)// All
                    ((TorrentsViewModel)this.DataContext).FilterCategory = "";
                else if (listbox.SelectedIndex == 1)
                    ((TorrentsViewModel)this.DataContext).FilterCategory = "Uncategorised";
                else
                {
                    //Debug.WriteLine(listbox.SelectedItem.ToString());
                    var categoryCountViewModel = listbox.SelectedItem as CategoryCountViewModel;
                    if (categoryCountViewModel is not null)
                        ((TorrentsViewModel)this.DataContext).FilterCategory = categoryCountViewModel.Name;
                }

            }
        }

        private void TrackerFilterListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && this.DataContext is not null)
            {
                TrackerCountViewModel? trackerCountViewModel = listbox.SelectedItem as TrackerCountViewModel;
                if (trackerCountViewModel is not null)
                    ((TorrentsViewModel)this.DataContext).FilterTracker = trackerCountViewModel.Url;
            }
        }

        private void TorrentsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateDetails();

            if (DataContext is null)
                return;

            //NOTE: SelectedTorrents has to be set before SelectedTorrent because SelectedTorrent is monitored to enable the right buttons.
            var torrentsViewModel = (TorrentsViewModel)DataContext;
            torrentsViewModel.SelectedTorrents = TorrentsDataGrid.SelectedItems.Cast<TorrentInfoViewModel>().ToList();
            torrentsViewModel.SelectedTorrent = (TorrentInfoViewModel)TorrentsDataGrid.SelectedItem;
        }

        private void TorrentDetailsTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateDetails();
        }

        public void PausePaneUpdates()
        {
            if (DataContext is TorrentsViewModel torrentsViewModel)
            {
                torrentsViewModel.PropertiesForSelectedTorrent?.Pause();
                torrentsViewModel.TorrentPieceStatesViewModel?.Pause();
                torrentsViewModel.PauseTrackers();
                torrentsViewModel.PausePeers();
                torrentsViewModel.PauseHttpSources();
                torrentsViewModel.PauseTorrentContents();
            }
        }

        private void UpdateDetails()
        {
            if (DataContext is TorrentsViewModel torrentsViewModel)
            {
                PausePaneUpdates();

                var selectedItem = (TorrentInfoViewModel)TorrentsDataGrid.SelectedItem;
                switch (TorrentDetailsTabControl.SelectedIndex)
                {
                    case 0: // General
                    {
                        torrentsViewModel.PropertiesForSelectedTorrent = new TorrentPropertiesViewModel(selectedItem);
                        torrentsViewModel.TorrentPieceStatesViewModel = new TorrentPieceStatesViewModel(selectedItem);
                        break;
                    }
                    case 1: // Trackers
                    {
                        torrentsViewModel.TorrentTrackersViewModel = new TorrentTrackersViewModel(selectedItem);
                        break;
                    }
                    case 2: // Peers
                    {
                        torrentsViewModel.TorrentPeersViewModel = new TorrentPeersViewModel(selectedItem);
                        break;
                    }
                    case 3: // HTTP Sources
                    {
                        torrentsViewModel.TorrentHttpSourcesViewModel = new TorrentHttpSourcesViewModel(selectedItem);
                        break;
                    }
                    case 4: // Content
                    {
                        torrentsViewModel.TorrentContentsViewModel = new TorrentContentsViewModel(selectedItem);
                        break;
                    }
                }
            }
        }

        private void DeleteTagActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (TagFilterListBox.SelectedItem is TagCountViewModel tagCountViewModel)
            {
                QBittorrentService.QBittorrentClient.DeleteTagAsync(tagCountViewModel.Tag);
                if(DeleteTagButton.Flyout is Flyout flyout)
                    flyout.Hide();
            }
        }

        private void AddTagActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AddTagTextBox.Text))
                QBittorrentService.QBittorrentClient.CreateTagAsync(AddTagTextBox.Text);
            if (AddTagButton.Flyout is Flyout flyout)
                flyout.Hide();
        }

        private void DeleteCategoryActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (CategoryFilterListBox.SelectedItem is CategoryCountViewModel categoryCountViewModel)
            {
                QBittorrentService.QBittorrentClient.DeleteCategoryAsync(categoryCountViewModel.Name);
                if (DeleteCategoryButton.Flyout is Flyout flyout)
                    flyout.Hide();
            }
        }

        private void AddCategoryActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CategoryNameTextBox.Text) && CategorySavePathTextBox.Text is not null)
                QBittorrentService.QBittorrentClient.AddCategoryAsync(CategoryNameTextBox.Text, CategorySavePathTextBox.Text);
            if (AddCategoryButton.Flyout is Flyout flyout)
                flyout.Hide();
        }
    }
}

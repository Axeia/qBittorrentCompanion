using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using FluentIcons.Avalonia;
using System.Runtime.CompilerServices;

namespace qBittorrentCompanion.Views
{
    public partial class TorrentsView : UserControl
    {
        private TypeToSelectDataGridHelper<TorrentInfoViewModel>? _torrentsTypeSelect;
        private TypeToSelectDataGridHelper<TorrentTrackerViewModel>? _trackersTypeSelect;
        private TypeToSelectDataGridHelper<TorrentPeerViewModel>? _peersTypeSelect;
        private TypeToSelectDataGridHelper<string>? _httpTypeselect;
        private TypeToSelectDataGridHelper<TorrentContentViewModel>? _contentTypeSelect;

        public delegate void ShowMessageHandler(string message);
        public event ShowMessageHandler ShowMessage;


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

            _torrentsTypeSelect = new TypeToSelectDataGridHelper<TorrentInfoViewModel>(
                TorrentsDataGrid, nameof(TorrentInfoViewModel.Name)
            );
            _trackersTypeSelect = new TypeToSelectDataGridHelper<TorrentTrackerViewModel>(
                TorrentTrackersDataGrid, nameof(TorrentTrackerViewModel.Url)
            );
            _peersTypeSelect = new TypeToSelectDataGridHelper<TorrentPeerViewModel>(
                TorrentPeersDataGrid, nameof(TorrentPeerViewModel.Ip)
            );
            _httpTypeselect = new TypeToSelectDataGridHelper<string>(HttpSourcesDataGrid, ".");
            /*_contentTypeSelect = new TypeToSelectDataGridHelper<TorrentContentViewModel>(
                TorrentContentDataGrid, nameof(TorrentContentViewModel.DisplayName)
            );*/
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
                        if (torrentsViewModel.TorrentContentsViewModel != null)
                        {
                            torrentsViewModel.TorrentContentsViewModel.TorrentPriorityUpdating -= TorrentContentsViewModel_Updating;
                            torrentsViewModel.TorrentContentsViewModel.TorrentPriorityUpdated -= TorrentContentsViewModel_Updated;
                        }
                        torrentsViewModel.TorrentContentsViewModel = new TorrentContentsViewModel(selectedItem);
                        torrentsViewModel.TorrentContentsViewModel.TorrentPriorityUpdating += TorrentContentsViewModel_Updating;
                        torrentsViewModel.TorrentContentsViewModel.TorrentPriorityUpdated += TorrentContentsViewModel_Updated;

                        break;                            
                    }
                }
            }
        }

        private void TorrentContentsViewModel_Updated(TorrentContentViewModel tcvm)
        {
            var spinnerPanel = TorrentContentsTreeDataGrid.GetVisualDescendants().OfType<Panel>().FirstOrDefault(vd => vd.DataContext == tcvm && vd.GetType() == typeof(Panel));
            if (spinnerPanel != null)
            {
                var spinner = new SymbolIcon
                {
                    Symbol = FluentIcons.Common.Symbol.SpinnerIos,
                    Foreground = new SolidColorBrush((Color)Application.Current!.FindResource("SystemAccentColor")!),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                spinner.Classes.Add("Spinner");
                spinnerPanel.Children.Clear();
                if(tcvm.IsUpdating)
                    spinnerPanel.Children.Add(spinner);
            }
        }

        /// <summary>
        /// By recreating the ComboBox using the template model as in <see cref="TorrentContentsViewModel"/>
        /// it will retain the same look. By using the same DataViewModel it will select the right selected item.
        /// The act of recreating it updates the UI to actually show this value.
        /// 
        /// Part of a workaround for the ComboBox not updating properly
        /// </summary>
        /// <param name="tcvm"></param>
        private void TorrentContentsViewModel_Updating(TorrentContentViewModel tcvm)
        {
            var comboBox = TorrentContentsTreeDataGrid.GetVisualDescendants().OfType<ComboBox>().FirstOrDefault(vd => vd.DataContext == tcvm);
            if (comboBox != null && comboBox.Parent is ContentControl cc)
            {
                // Save the original content
                var originalContent = cc.Content;
                // Clear the current content to remove the existing ComboBox
                cc.Content = null;
                // Set the DataTemplate to the defined DataTemplate
                cc.ContentTemplate = (DataTemplate)cc.FindResource("TorrentContentComboBoxTemplate")!;
                // Reassign the original content to refresh the ComboBox with the DataTemplate
                cc.Content = originalContent;
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

        /// <summary>
        /// When a file or folder in the Contents tab is clicked it gets opened directly 
        /// (explorer for folders, whatever program is associated for files)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TorrentContentsTreeDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            var source = e.Source as ContentPresenter;
            if (source is null) { return; }

            if (DataContext is TorrentsViewModel torrentsViewModel 
            && torrentsViewModel.SelectedTorrent is TorrentInfoViewModel tivm
            && source.DataContext is TorrentContentViewModel tcvm)
            {
                string fileOrFolderPath = tivm.Progress == 1.00 
                    ? Path.GetFullPath(Path.Combine(ConfigService.DownloadDirectory, tcvm.Name))
                    : Path.GetFullPath(Path.Combine(ConfigService.TemporaryDirectory, tcvm.Name));


                if (File.Exists(fileOrFolderPath) || Directory.Exists(fileOrFolderPath))
                    Process.Start("explorer.exe", fileOrFolderPath);
                else
                    ShowMessage?.Invoke($"{tcvm.Name} does not exist");
            }
        }

        /// <summary>
        /// When a listed torrent is double clicked:<br/>
        /// - If it's a file it opens the explorer with the file selected<br/>
        /// - If it's a folder it opens the folder<br/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TorrentsDataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var source = e.Source as Border;
            if (source is null) return;

            if (source.Name == "CellBorder" && source.DataContext is TorrentInfoViewModel tivm) 
            {
                string fileOrFolderPath = Path.Combine(
                    tivm.Progress == 1.00
                        ? ConfigService.DownloadDirectory
                        : ConfigService.TemporaryDirectory,
                    tivm.Name!
                );

                if (File.Exists(fileOrFolderPath))
                {
                    LaunchFileOrExplorer(fileOrFolderPath);
                }
                // Missmatch between torrent name and file (or directory) name
                // Will have to get the contents to figure out the path
                else
                {
                    _ = GetContentsAndLaunchPath(sender, e);
                }
            }
        }

        /// <summary>
        /// <list type="bullet">
        /// <item>
        /// <term>The given path leads to a file</term>
        /// <description>open the containing directory in the explorer with the file pre-selected.</description></item>
        /// <item>
        /// <term>The given path is a directory</term>
        /// <description>open it in the explorer</description>
        /// </item>
        /// </list>
        /// TODO: Cross platform code (current implemention is for Windows)
        /// </summary>
        /// <param name="absolutePath">Path to a location accessible on this system</param>
        private void LaunchFileOrExplorer(string absolutePath)
        {
            Debug.WriteLine($"Opening explorer with: {absolutePath}");
            if (File.Exists(absolutePath))
            {
                Process.Start("explorer.exe", "/select, \"" + absolutePath + "\"");
            }
            else if (Directory.Exists(absolutePath))
            {
                Process.Start("explorer.exe", absolutePath);
            }
            else
            {
                Debug.WriteLine($"{absolutePath} doesn't exist! Unable to launch");
            }
        }

        /// <summary>
        /// If <see cref="TorrentsDataGrid_DoubleTapped"/> detected a missmatch between the torrent name
        /// and the file (or directory) of the torrent the contents need to be fetched.<br/>
        /// This method fetches the content and then launches explorer based on the retrieved content.<br/>
        /// </summary>
        /// <remarks>
        /// Whilst it's fetching the content it shows the Details of the row (and hides them when done)
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task GetContentsAndLaunchPath(object? sender, TappedEventArgs e)
        {
            if (e.Source is Control control)
            {
                var parent = control.GetVisualParent();
                while (parent != null && !(parent is DataGridRow))
                {
                    parent = parent.GetVisualParent();
                }

                if (parent is DataGridRow row && row.DataContext is TorrentInfoViewModel tivm)
                {
                    row.AreDetailsVisible = true;
                    IReadOnlyList<TorrentContent> result = await QBittorrentService.QBittorrentClient.GetTorrentContentsAsync(tivm.Hash);
                    row.AreDetailsVisible = false;
                    if (result.Count > 0)
                    {
                        string path = result[0].Name;
                        if (path.Contains("/"))
                        {
                            int lastIndex = path.IndexOf('/');
                            path = path.Substring(0, lastIndex);
                        }

                        string fileOrFolderPath = Path.Combine(
                            tivm.Progress == 1.00
                                ? ConfigService.DownloadDirectory
                                : ConfigService.TemporaryDirectory,
                             path // Root element
                        );
                        LaunchFileOrExplorer(fileOrFolderPath);
                    }
                }
            }

        }
    }
}
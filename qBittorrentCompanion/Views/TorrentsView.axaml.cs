using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using FluentIcons.Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using ReactiveUI;
using System.Reactive.Linq;
using System.ComponentModel;
using Avalonia.Threading;
using RssPlugins;
using System.Windows.Input;

namespace qBittorrentCompanion.Views
{
    public partial class TorrentsView : RssRulePluginUserControl
    {
        private TypeToSelectDataGridHelper<TorrentInfoViewModel>? _torrentsTypeSelect;
        private TypeToSelectDataGridHelper<TorrentTrackerViewModel>? _trackersTypeSelect;
        private TypeToSelectDataGridHelper<TorrentPeerViewModel>? _peersTypeSelect;
        private TypeToSelectDataGridHelper<string>? _httpTypeselect;

        public delegate void ShowMessageHandler(string message);
        public event ShowMessageHandler? ShowMessage;
        public ICommand PluginClickCommand { get; }

        public TorrentsView()
        {
            PluginClickCommand = ReactiveCommand.Create<RssRulePluginBase>(OnPluginClicked);
            InitializeComponent();
            var torrentsViewModel = new TorrentsViewModel();

            Loaded += TorrentsView_Loaded;
        }

        private void OnPluginClicked(RssRulePluginBase plugin)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.MainTabStrip.SelectedIndex = 3;

            Dispatcher.UIThread.Post(() =>
            {
                mainWindow.RssRulesView.AddNewRule(plugin.RuleTitle, plugin.Result, []);
            }, DispatcherPriority.Background);
        }

        private void OnShowSideBarChanged(bool showSideBar)
        {
            if (showSideBar)
            {
                Grid.SetColumn(MainColumnGrid, 2);
                Grid.SetColumnSpan(MainColumnGrid, 1);
            }
            else
            {
                Grid.SetColumn(MainColumnGrid, 0);
                Grid.SetColumnSpan(MainColumnGrid, 3);
            }
        }

        private void TorrentsView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel viewModel)
            {
                viewModel.TagCounts.CollectionChanged += (s, e)
                    => FilterListBox_CollectionChanged(TagFilterListBox);
                viewModel.TrackerCounts.CollectionChanged += (s, e)
                    => FilterListBox_CollectionChanged(TrackerFilterListBox);
                viewModel.CategoryCounts.CollectionChanged += (s, e)
                    => FilterListBox_CollectionChanged(CategoryFilterListBox);
                viewModel.WhenAnyValue(vm => vm.ShowSideBar)
                    .Subscribe(OnShowSideBarChanged);
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

            if(!Design.IsDesignMode)
            {
                var sideBarColumn = TorrentsLayoutGrid.ColumnDefinitions[0];
                var configSideBarWidth = ConfigService.SideBarWidth;
                if (configSideBarWidth >= sideBarColumn.MinWidth || configSideBarWidth <= sideBarColumn.MaxWidth)
                {
                    sideBarColumn.Width = new GridLength(configSideBarWidth);
                }
            }

            RssPluginButtonView.GenerateRssRuleSplitButton.Click += GenerateRssRuleSplitButton_Click;

            TorrentsDataGrid.SelectionChanged += TorrentsDataGrid_SelectionChanged;
        }

        private void TorrentsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine("Selection changed");
            UpdatePauseResumeButtonStates();

            // Newly selected items
            foreach (var item in e.AddedItems)
            {
                TorrentInfoViewModel? selectedItem = item as TorrentInfoViewModel;
                if (selectedItem is not null)
                {
                    selectedItem.PropertyChanged += SelectedTorrent_PropertyChanged;
                }
            }

            // Items that lost selection
            foreach (var item in e.RemovedItems)
            {
                TorrentInfoViewModel? unselectedItem = item as TorrentInfoViewModel;
                if (unselectedItem is not null)
                    unselectedItem.PropertyChanged -= SelectedTorrent_PropertyChanged;
            }


            UpdateDetails();

            if (DataContext is null)
                return;

            //NOTE: SelectedTorrents has to be set before SelectedTorrent because SelectedTorrent is monitored to enable the right buttons.
            if (DataContext is TorrentsViewModel torrentsViewModel)
            {
                torrentsViewModel.SelectedTorrents = TorrentsDataGrid.SelectedItems.Cast<TorrentInfoViewModel>().ToList();
                torrentsViewModel.SelectedTorrent = (TorrentInfoViewModel)TorrentsDataGrid.SelectedItem;
            }
        }

        ///If the state changes the buttons may have to be enabled/disabled.
        private void SelectedTorrent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TorrentInfoViewModel.State) && sender is not null)
                UpdatePauseResumeButtonStates();
        }

        private void UpdatePauseResumeButtonStates()
        {
            if (DataContext is null)
                return;

            var selectedTorrents = TorrentsDataGrid.SelectedItems.OfType<TorrentInfoViewModel>();

            RemoveButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            MaxPriorityButton.IsEnabled = false;
            IncreasePriorityButton.IsEnabled = false;
            DecreasePriorityButton.IsEnabled = false;
            MinPriorityButton.IsEnabled = false;

            if (selectedTorrents is not null)
            {
                var pausedTorrents = selectedTorrents
                    .Where(torrent => TorrentsViewModel.TorrentStateGroupings.Paused.Contains((TorrentState)torrent.State!))
                    .ToList();
                if (selectedTorrents.Any())
                    RemoveButton.IsEnabled = true;
                PauseButton.IsEnabled = pausedTorrents.Count < selectedTorrents.Count();
                ResumeButton.IsEnabled = pausedTorrents.Count > 0;


                MaxPriorityButton.IsEnabled = true;
                IncreasePriorityButton.IsEnabled = true;
                DecreasePriorityButton.IsEnabled = true;
                MinPriorityButton.IsEnabled = true;
            }
        }

        private void FilterListBox_CollectionChanged(ListBox listBox)
        {
            //Select first index if none has been set
            if (listBox.SelectedIndex == -1)
                listBox.SelectedIndex = 0;

            //Restore scrollbar position
            SideBarScrollViewer.Offset = new Vector(SideBarScrollViewer.Offset.X, 0);
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
                            torrentsViewModel.HttpSourcesViewModel = new TorrentHttpSourcesViewModel(selectedItem);
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
                if (tcvm.IsUpdating)
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
                QBittorrentService.DeleteTagAsync(tagCountViewModel.Tag);
                if (DeleteTagButton.Flyout is Flyout flyout)
                    flyout.Hide();
            }
        }

        private void AddTagActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AddTagTextBox.Text))
                QBittorrentService.CreateTagAsync(AddTagTextBox.Text);
            if (AddTagButton.Flyout is Flyout flyout)
                flyout.Hide();
        }

        private void DeleteCategoryActionButton_Click(object? sender, RoutedEventArgs e)
        {
            if (CategoryFilterListBox.SelectedItem is CategoryCountViewModel categoryCountViewModel)
            {
                QBittorrentService.DeleteCategoryAsync(categoryCountViewModel.Name);
                if (DeleteCategoryButton.Flyout is Flyout flyout)
                    flyout.Hide();
            }
        }

        private void AddCategoryActionButton_Click(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Clicked");
            _ = AddCategoryFromFlyout();
            if (AddCategoryButton.Flyout is Flyout flyout)
                flyout.Hide();
        }

        private async Task AddCategoryFromFlyout()
        {
            if (!string.IsNullOrEmpty(CategoryNameTextBox.Text))
            {
                Debug.WriteLine($"Adding category `{CategoryNameTextBox.Text}` with path: `{CategorySavePathTextBox.Text}`");
                await QBittorrentService.AddCategoryAsync(CategoryNameTextBox.Text, CategorySavePathTextBox.Text ?? "");
                CategoryNameTextBox.Clear();
                CategorySavePathTextBox.Clear();
            }
        }

        /// <summary>
        /// When a file or folder in the Contents tab is clicked it gets opened directly 
        /// (explorer for folders, whatever program is associated for files)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TorrentContentsTreeDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (e.Source is not ContentPresenter source) { return; }

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
        private void TorrentsDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (e.Source is not Border source) return;

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
                    PlatformAgnosticLauncher.OpenDirectoryAndSelectFile(fileOrFolderPath);
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
        private static async Task GetContentsAndLaunchPath(object? sender, TappedEventArgs e)
        {
            if (e.Source is Control control)
            {
                var parent = control.GetVisualParent();
                while (parent != null && parent is not DataGridRow)
                {
                    parent = parent.GetVisualParent();
                }

                if (parent is DataGridRow row && row.DataContext is TorrentInfoViewModel tivm)
                {
                    row.AreDetailsVisible = true;
                    IReadOnlyList<TorrentContent>? result = await QBittorrentService.GetTorrentContentsAsync(tivm.Hash);

                    if (result != null)
                    { 
                        row.AreDetailsVisible = false;
                        if (result.Count > 0)
                        {
                            string path = result[0].Name;
                            if (path.Contains('/'))
                            {
                                int lastIndex = path.IndexOf('/');
                                path = path[..lastIndex];
                            }

                            string fileOrFolderPath = Path.Combine(
                                tivm.Progress == 1.00
                                    ? ConfigService.DownloadDirectory
                                    : ConfigService.TemporaryDirectory,
                                 path // Root element
                            );
                            PlatformAgnosticLauncher.OpenDirectoryAndSelectFile(fileOrFolderPath);
                        }
                    }
                }
            }
        }

        private void SetLocationMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                var setTorrentLocationWindow = new SetTorrentLocationWindow(torrentsVm.SelectedTorrent);
                if (this.VisualRoot is MainWindow mainWindow)
                    setTorrentLocationWindow.ShowDialog(mainWindow);
            }
        }

        private void SetNameMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                var setTorrentNameWindow = new SetTorrentNameWindow(torrentsVm.SelectedTorrent);
                if (this.VisualRoot is MainWindow mainWindow)
                    setTorrentNameWindow.ShowDialog(mainWindow);
            }
        }

        private void RenameFilesMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                var renameTorrentFilesWindow = new RenameTorrentFilesWindow(torrentsVm.SelectedTorrent);
                if (this.VisualRoot is MainWindow mainWindow)
                    renameTorrentFilesWindow.ShowDialog(mainWindow);
            }
        }
        private void EditTrackersMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                var editTrackersWindow = new EditTrackersWindow(torrentsVm.SelectedTorrent);
                if (this.VisualRoot is MainWindow mainWindow)
                    editTrackersWindow.ShowDialog(mainWindow);
            }
        }

        private void CopyNameMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(torrentsVm.SelectedTorrent.Name);
            }
        }

        private void CopyHashMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(torrentsVm.SelectedTorrent.Hash);
            }
        }

        private void CopyMagnetLinkMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(torrentsVm.SelectedTorrent.MagnetUri!.ToString());
            }
        }

        private void DownloadDotTorrentMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm 
                && torrentsVm.SelectedTorrent != null)
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                    _ = ShowSaveDialog(torrentsVm.SelectedTorrent, mainWindow);
            }
        }

        private static async Task ShowSaveDialog(TorrentInfoViewModel tivm, Window mainWindow)
        {
            var options = new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedFileName = $"{tivm.Name}.torrent",
                FileTypeChoices =
                [
                    new FilePickerFileType("Torrent Files") { Patterns = ["*.torrent"] },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            };

            var result = await mainWindow.StorageProvider.SaveFilePickerAsync(options);
            if (result != null)
            {
                var fileBytes = await tivm.SaveDotTorrentAsync();
                await File.WriteAllBytesAsync(result.Path.LocalPath, fileBytes);
            }
        }

        private static Window? GetMainWindow()
        {
            return Application.Current is Application app
            && app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow
                : null;
        }

        private void AddCategoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["AddCategoryFlyout"] is Flyout flyout)
            {
                var lbi = CategoryFilterListBox.ContainerFromItem(CategoryFilterListBox.SelectedItem!);
                flyout.ShowAt(lbi!);
            }
        }

        private CategoryCountViewModel? _ccvm;
        private void EditCategoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["EditCategoryFlyout"] is Flyout flyout && CategoryFilterListBox.SelectedItem is CategoryCountViewModel ccvm)
            {
                var lbi = CategoryFilterListBox.ContainerFromItem(ccvm);
                CategorySavePathEditTextBox.Text = ccvm.SavePath;

                flyout.ShowAt(lbi!);

                CategorySavePathEditTextBox.Focus();
                CategorySavePathEditTextBox.SelectAll();

                _ccvm = ccvm;
            }
        }

        private void RemoveCategoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["RemoveCategoryFlyout"] is Flyout flyout && CategoryFilterListBox.SelectedItem is CategoryCountViewModel ccvm)
            {
                var lbi = CategoryFilterListBox.ContainerFromItem(ccvm);
                flyout.ShowAt(lbi!);
                _ccvm = ccvm;
            }
        }

        private void SaveCategoryButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = SaveNewCategoryPath();
        }

        private async Task SaveNewCategoryPath()
        {
            if (_ccvm == null)
                return;

            SaveCategoryButton.IsEnabled = false;

            var newPath = CategorySavePathEditTextBox.Text ?? string.Empty;
            await QBittorrentService.EditCategoryAsync(_ccvm.Name, newPath);
            _ccvm.SavePath = newPath;

            SaveCategoryButton.IsEnabled = true;
            if (Resources["EditCategoryFlyout"] is Flyout flyout)
                flyout.Hide();
        }

        private void DeleteTorrentsForCategoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            DeleteTorrents(sender, e, DeleteBy.Category);
        }

        private void DeleteTorrentsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            DeleteTorrents(sender, e, DeleteBy.Selected);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void DeleteTorrents(object? sender, RoutedEventArgs e, DeleteBy deleteBy)
        {
            var og = this.FindAncestorOfType<MainWindow>();
            if (og != null)
            {
                var removeTorrentWindow = new RemoveTorrentWindow(deleteBy);
                removeTorrentWindow.ShowDialog(og);
            }
        }

        private void AddTagMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["AddTagFlyout"] is Flyout flyout)
            {
                var lbi = TagFilterListBox.ContainerFromItem(TagFilterListBox.SelectedItem!);
                flyout.ShowAt(lbi!);
            }
        }

        private void RemoveTagMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["RemoveTagFlyout"] is Flyout flyout && TagFilterListBox.SelectedItem is TagCountViewModel tcvm)
            {
                var lbi = TagFilterListBox.ContainerFromItem(tcvm);
                flyout.ShowAt(lbi!);
            }
        }

        private void DeleteTorrentsForTagMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            DeleteTorrents(sender, e, DeleteBy.Tag);
        }

        private void DeleteTorrentsForTrackerMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            DeleteTorrents(sender, e, DeleteBy.Tracker);
        }

        private void AddTrackersMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm && torrentsVm.SelectedTorrent != null)
            {
                var addTrackersWindow = new AddTrackersWindow(torrentsVm.SelectedTorrent);
                if (this.VisualRoot is MainWindow mainWindow)
                    addTrackersWindow.ShowDialog(mainWindow);
            }
        }

        private void RemoveTrackerMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel tvm && tvm.TorrentTrackersViewModel is TorrentTrackersViewModel ttvm)
            {
                _ = ttvm.DeleteTrackerAsync();
            }
        }

        private void TorrentTrackersDataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            if (dataGrid?.SelectedItem is TorrentTrackerViewModel ttvm && _preEditTorrentTrackerUri != null)
            {
                var context = new ValidationContext(ttvm, null, null) { MemberName = "Url" };
                List<ValidationResult> results = [];

                // Validate URL
                if (Validator.TryValidateProperty(ttvm.Url, context, results))
                {
                    // Looking good, try to save remotely.
                    _ = RenameUrlAsync(_preEditTorrentTrackerUri, ttvm.Url);
                }
            }
        }

        private async Task RenameUrlAsync(Uri oldUrl, Uri newUrl)
        {
            if (TorrentsDataGrid.SelectedItem is TorrentInfoViewModel tivm)
            {
                await QBittorrentService.EditTrackerAsync(tivm.Hash, oldUrl, newUrl);
            }
        }

        private Uri? _preEditTorrentTrackerUri;
        private void TorrentTrackersDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if(TorrentTrackersDataGrid.SelectedItem is TorrentTrackerViewModel ttvm)
            {
                _preEditTorrentTrackerUri = ttvm.Url;
                if(_preEditTorrentTrackerUri != null)
                    Debug.WriteLine($"_preEditTorrentTrackerUri: {_preEditTorrentTrackerUri}");
            }
        }

        private void EditTrackerMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem
                && menuItem.CommandParameter is TorrentTrackerViewModel selectedItem)
            {
                // Get the DataGridRow for the selected item
                var rowIndex = ((IList)TorrentTrackersDataGrid.ItemsSource).IndexOf(selectedItem);
                var row = (DataGridRow)TorrentTrackersDataGrid
                    .GetVisualDescendants()
                    .OfType<DataGridRow>()
                    .First(r => r.Index == rowIndex);

                if (row != null)
                {
                    // Get the Url column
                    var urlColumn = TorrentTrackersDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == "URL");

                    if (urlColumn != null)
                    {
                        // Get the cell content
                        var cellContent = urlColumn.GetCellContent(row);
                        var cell = (DataGridCell?)cellContent?.Parent;

                        if (cell != null)
                        {
                            // Begin editing the cell
                            TorrentTrackersDataGrid.BeginEdit(); // Begin editing the cell
                            var textBox = cell.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
                            textBox!.Focus();
                            textBox.SelectAll();
                        }
                    }
                }
            }
        }

        private void CopyTrackerUrlMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (TorrentTrackersDataGrid.SelectedItem is TorrentTrackerViewModel ttvm)
            {
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(ttvm.Url.ToString());
            }
        }

        private void CopyPeerIpAndPortMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm
                && torrentsVm.TorrentPeersViewModel is TorrentPeersViewModel tpvm
                && tpvm.SelectedPeer is TorrentPeerViewModel selectedPeer)
            {
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(
                    selectedPeer.Ip.ToString()
                    + ":"
                    + selectedPeer.Port.ToString()
                );
            }
        }

        private void AddPeersMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel tvm)
            {
                var og = this.FindAncestorOfType<MainWindow>();
                if (og != null)
                {
                    var addPeersWindow = new AddPeersWindow(tvm.SelectedTorrent!);
                    addPeersWindow.ShowDialog(og);
                }
            }
        }

        private void CopyHttpSourceUrlMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel torrentsVm
                && torrentsVm.HttpSourcesViewModel is TorrentHttpSourcesViewModel httpSourceVm
                && httpSourceVm.SelectedHttpSource is string url)
            {
                Debug.WriteLine(url);
                _ = TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(url);
            }
        }

        private void TorrentContentRenameMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem 
                && menuItem.DataContext is TorrentContentViewModel tcvm)
            {
                tcvm.IsEditing = true;
                var textBox = TorrentContentsTreeDataGrid
                    .GetVisualDescendants()
                    .OfType<TextBox>()
                    .Where(tb => tb.IsVisible).FirstOrDefault();
                if (textBox is not null)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }

        private void SideBarScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            double minWidth = TorrentsLayoutGrid.ColumnDefinitions[0].MinWidth;

            if (e.NewSize.Width < e.PreviousSize.Width
                && e.NewSize.Width == minWidth
                && this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.ShowFlashMessage("Reached minimum size, there is an option to hide the sidebar in the settings menu");
            }
        }

        private void GridSplitter_DragCompleted(object? sender, VectorEventArgs e)
        {
            if (!Design.IsDesignMode)
            {
                ConfigService.SideBarWidth = TorrentsLayoutGrid.ColumnDefinitions[0].ActualWidth;
            }
        }

        public void AddTorrentFileClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentsWindow();
            addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 1;

            var mw = this.FindAncestorOfType<MainWindow>();
            if (mw is not null)
                addTorrentWindow.ShowDialog(mw);
        }

        public void OnAddTorrentUrlClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentsWindow();
            addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 0;

            var mw = this.FindAncestorOfType<MainWindow>();
            if (mw is not null)
                addTorrentWindow.ShowDialog(mw);
        }

        public void OnRemoveTorrentClicked(object? sender, RoutedEventArgs e)
        {
            var removeTorrentWindow = new RemoveTorrentWindow(DeleteBy.Selected);

            var mw = this.FindAncestorOfType<MainWindow>();
            if (mw is not null)
                removeTorrentWindow.ShowDialog(mw);
        }

        public void OnPauseClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel tvm)
                _ = tvm.PauseSelectedTorrentsAsync();
        }

        public void OnResumeClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TorrentsViewModel tvm)
                _ = tvm.ResumeSelectedTorrentsAsync();
        }
    }
}
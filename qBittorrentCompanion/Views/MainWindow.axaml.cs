
using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Threading.Tasks;
using System;
using qBittorrentCompanion.Services;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Primitives;
using QBittorrent.Client;
using Avalonia.Data;
using Avalonia;
using qBittorrentCompanion.Helpers;
using System.Collections.Generic;
using System.IO;
using Avalonia.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ConstrainedExecution;
using Avalonia.Input;
using ReactiveUI;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Views.Preferences;

namespace qBittorrentCompanion.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _flashMessageTimer = new();

        public MainWindow()
        {
            InitializeComponent();
            /*if (Design.IsDesignMode)
            {
                this.DataContext = new MainWindowViewModel()
                {
                    ServerStateViewModel = new ServerStateViewModel(
                        JsonConvert.DeserializeObject<ServerState>(@"
                {
                    ""alltime_dl"": 8630139882641,
                    ""alltime_ul"": 7859885871206,
                    ""average_time_queue"": 3187,
                    ""connection_status"": ""connected"",
                    ""dht_nodes"": 369,
                    ""dl_info_data"": 213944266,
                    ""dl_info_speed"": 0,
                    ""dl_rate_limit"": 5120000,
                    ""free_space_on_disk"": 143772954624,
                    ""global_ratio"": ""0.91"",
                    ""queued_io_jobs"": 0,
                    ""queueing"": true,
                    ""read_cache_hits"": ""0"",
                    ""read_cache_overload"": ""0"",
                    ""refresh_interval"": 1500,
                    ""total_buffers_size"": 0,
                    ""total_peer_connections"": 10,
                    ""total_queued_size"": 0,
                    ""total_wasted_session"": 149450,
                    ""up_info_data"": 739751315,
                    ""up_info_speed"": 14057,
                    ""up_rate_limit"": 25600,
                    ""use_alt_speed_limits"": false,
                    ""use_subcategories"": false,
                    ""write_cache_overload"": ""0""
                }"))
                };
            }*/

            _flashMessageTimer.Tick += HideFlashMessage;
            _flashMessageTimer.Interval = TimeSpan.FromSeconds(5);
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mwvm)
            {
                mwvm.PropertyChanged += Mwvm_PropertyChanged;
            }

            SearchView.SearchResultDataGrid.DoubleTapped += SearchResultDataGrid_DoubleTapped;
            TransfersTorrentsView.ShowMessage += ShowFlashMessage;

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private void ShowFlashMessage (string message)
        {
            SelectedTorrentTextBlock.Opacity = 0;
            FlashMessageTextBlock.Opacity = 1;

            FlashMessageTextBlock.Text = message;

            _flashMessageTimer.Stop();
            _flashMessageTimer.Start(); // Hides the message after a defined period of time
        }

        private void HideFlashMessage(object? sender, EventArgs e)
        {
            SelectedTorrentTextBlock.Opacity = 1;
            FlashMessageTextBlock.Opacity = 0;
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            // Only allow Copy or Link as Drop Operations.
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Link;

            // Only allow if the dragged data contains files.
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null && files.All(file => Path.GetExtension(file.Name).Equals(".torrent", StringComparison.OrdinalIgnoreCase)))
                {
                    e.DragEffects = DragDropEffects.Copy | DragDropEffects.Link;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }

            e.Handled = true;
        }
        private async void Drop(object? sender, DragEventArgs e)
        {
            try
            {
                // Retrieve the files
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    var torrentFiles = files.Where(file => Path.GetExtension(file.Name).Equals(".torrent", StringComparison.OrdinalIgnoreCase)).ToList();
                    Debug.WriteLine($"Contains {torrentFiles.Count} .torrent files");

                    foreach (var file in torrentFiles)
                    {
                        var localPath = file.TryGetLocalPath();
                        if (localPath != null)
                        {
                            // Handle the dropped .torrent files here
                            Debug.WriteLine(localPath);
                            AddToFileQueue(localPath);
                        }
                    }
                    await ProcessFileQueue(false);
                }
                else
                {
                    Debug.WriteLine("Contains nothing");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        private async void SearchResultDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            var source = e.Source as Border;
            if (source is null) return;

            if (source.DataContext is SearchResult searchResult)
            {
                string strFileUrl = searchResult.FileUrl.ToString();
                var flyout = FlyoutBase.GetAttachedFlyout(TransfersTab);

                if (strFileUrl.ToString().EndsWith(".torrent") || strFileUrl.StartsWith("magnet:"))
                {
                    FlyoutTextBlock.Text = $"Starting download: {searchResult.FileName}";
                    flyout!.ShowAt(TransfersHeaderTextBlock);
                    AddToUrlQueue(strFileUrl);
                    _ = ProcessUrlQueue(true);
                }
                else // Do some checks
                {
                    FlyoutTextBlock.Text = $"Verifying it's an actual torrent link";
                    flyout!.ShowAt(TransfersHeaderTextBlock);
                    if (await LinkChecker.IsTorrentLink(strFileUrl))
                    {
                        FlyoutTextBlock.Text = $"Starting download: {searchResult.FileName}";
                    }
                    else
                    {
                        FlyoutTextBlock.Text = "Does not appear to be a torrent, opening in browser";
                        Process.Start(new ProcessStartInfo{ FileName = strFileUrl, UseShellExecute = true });
                    }

                    flyout.ShowAt(TransfersHeaderTextBlock);
                }
            }
        }

        public void AddTorrentFileClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentsWindow();
            addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 1;
            addTorrentWindow.ShowDialog(this);
        }

        public void OnAddTorrentUrlClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentsWindow();
            addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 0;
            addTorrentWindow.ShowDialog(this);
        }

        public void OnHelpAboutClicked(object sender, RoutedEventArgs e)
        {
            //var aboutWindow = new OwnAboutWindow();
            //aboutWindow.ShowDialog(this); // 'this' refers to the MainWindow instance
        }

        public void OnRemoveTorrentClicked(object sender, RoutedEventArgs e)
        {
            var removeTorrentWindow = new RemoveTorrentWindow();
            removeTorrentWindow.ShowDialog(this);
        }

        public void OnPauseClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.PauseSelectedTorrentsAsync();
        }

        public void OnResumeClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.ResumeSelectedTorrentsAsync();
        }

        public void OnMinPriorityButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.SelectedTorrentsSetPriorityAsync(TorrentPriorityChange.Minimal);
        }
        public void OnIncreasePriorityButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.SelectedTorrentsSetPriorityAsync(TorrentPriorityChange.Increase);
        }
        public void OnDecreasePriorityButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.SelectedTorrentsSetPriorityAsync(TorrentPriorityChange.Decrease);
        }
        public void OnMaxPriorityButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = TorrentsViewDataContent?.SelectedTorrentsSetPriorityAsync(TorrentPriorityChange.Maximal);

        }

        public void HttpRemoveTorrentsClicked(bool deleteFiles)
        {
            _ = TorrentsViewDataContent?.DeleteSelectedTorrentsAsync(deleteFiles);
        }

        
        public void AltSpeedLimitsToggled(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var viewModel = this.DataContext as MainWindowViewModel;

            // Only send the request if the checkbox state is different from the server state
            if (checkBox!.IsChecked != viewModel!.ServerStateViewModel!.UseAltSpeedLimits)
            {
                QBittorrentService.QBittorrentClient.ToggleAlternativeSpeedLimitsAsync();
            }
        }

        private void SettingsContextMenu_Closed(object? sender, RoutedEventArgs e)
        {
            SettingsMenuButton.IsChecked = false;
        }

        private void RssTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RssRulesControlsDockPanel.IsVisible = 
                MainTabcontrol.SelectedIndex == 2 && RssView.RssTabControl.SelectedIndex == 1;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            _ = AuthenticateAndProcessQueues();

            SettingsContextMenu.Closed += SettingsContextMenu_Closed;
            RssView.RssTabControl.SelectionChanged += RssTabControl_SelectionChanged;
        }

        private async Task<bool> AuthenticateAndProcessQueues()
        {
            SecureStorage ss = new();
            bool authenticated = false;

            // Cannot authenticate if there's no login data
            if (!ss.HasSavedData())
            {
                Debug.WriteLine("No login data was found, showing login window");
                ShowLogInWindow();
            }
            else
            {
                Debug.WriteLine("Login data was found, attempting to authenticate...");
                authenticated = await Authenticate();

                // Saved login data couldn't be used to log in, either
                // A) The server isn't running
                // B) Authentication didn't go right (invalid login data was saved?)
                if(!authenticated)
                {
                    Debug.WriteLine("Unable to log in using saved data, incorrect data or the server is down?");
                    ShowLogInWindow();
                }
                else
                {
                    Debug.WriteLine("Successfully authenticated using saved login data");
                    if (DataContext is MainWindowViewModel mwvm)
                        mwvm.IsLoggedIn = true;
                }
            }

            await ProcessFileQueue(true);
            await ProcessUrlQueue(true);

            return authenticated;
        }

        /// <summary>
        /// User info is stored? » login automatically,<br/>
        /// User info unknown » present the Login window
        /// </summary>
        private async Task<bool> Authenticate()
        {
            if (DataContext is MainWindowViewModel mwvm)
            {   //Attempt automatic login with saved log in details
                var loggedIn = await mwvm.LogIn();

                if (!loggedIn)
                    Debug.WriteLine("Couldn't automatically authenticate, presenting Login window");

                return loggedIn;
            }

            return false;
        }

        private void Mwvm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.IsLoggedIn))
            {
                if (DataContext is MainWindowViewModel mwvm)
                {
                    if(mwvm.IsLoggedIn)
                    {
                        LoadUpTorrents();
                    }
                    else
                    {
                        Debug.WriteLine("MainWindowViewModel IsLoggedIn set to false");
                        ShowLogInWindow();
                    }
                }
                else
                {
                    // This should never trigger
                    Debug.WriteLine("DataContext is not MainWindowViewModel");
                }
            }
        }

        private List<string> FileQueue = [];

        public void AddToFileQueue(string filePath)
        {
            if (File.Exists(filePath))
                FileQueue.Add(filePath);
        }

        public async Task ProcessFileQueue(bool bypassWindow)
        {
            if (FileQueue.Count > 0)
            {
                await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
                {
                    var addTorrentWindow = new AddTorrentsWindow();
                    addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 1;
                    foreach (var file in FileQueue)
                    {
                        addTorrentWindow.AddFile(file);
                    }

                    if (bypassWindow)
                    {
                        // Cheat a little bit and act as if the button was clicked.
                        addTorrentWindow.AddSplitButton_Clicked(null, new RoutedEventArgs());
                    }
                    else
                    {
                        Debug.WriteLine("Should launch Add Torrent dialog.");
                        await addTorrentWindow.ShowDialog(this);
                    }
                }));
            }
        }


        private List<string> UrlQueue = [];
        public void AddToUrlQueue(string urlPath)
        {
            Uri? uri = null;
            if (Uri.TryCreate(urlPath, UriKind.Absolute, out uri))
                UrlQueue.Add(uri!.ToString());
            else
                Console.WriteLine($"Did not add the URL, something is malformed: {urlPath}");
        }

        private async Task ProcessUrlQueue(bool bypassWindow)
        {
            if (UrlQueue.Count > 0)
            {
                await Dispatcher.UIThread.InvokeAsync((Func<Task>)(async () =>
                {
                    Debug.WriteLine("Faking dialog");
                    var addTorrentWindow = new AddTorrentsWindow();
                    addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 0;
                    foreach (var url in UrlQueue)
                    {
                        addTorrentWindow.AddUrl(url);
                    }

                    if (bypassWindow)
                    {
                        // Cheat a little bit and act as if the button was clicked.
                        addTorrentWindow.AddSplitButton_Clicked(null, new RoutedEventArgs());
                    }
                    else
                    {
                        await addTorrentWindow.ShowDialog(this);
                    }
                }));
            }
        }

        private void LoadUpTorrents()
        {
            if (DataContext is MainWindowViewModel mainWindowViewModel
            && TransfersTorrentsView.DataContext is TorrentsViewModel torrentsViewModel)
            {
                mainWindowViewModel.PopulateAndUpdate(torrentsViewModel);
                TransfersTorrentsView.TorrentsDataGrid.SelectionChanged += TorrentsDataGrid_SelectionChanged;
            }
        }

        private async void ShowLogInWindow()
        {
            var logInWindow = new LogInWindow(this);
            await logInWindow.ShowDialog(this);
            if (DataContext is MainWindowViewModel mainWindowVm)
                mainWindowVm.IsLoggedIn = true;
        }

        private TorrentsViewModel? TorrentsViewDataContent
        {
            get
            {
                if (TransfersTorrentsView.DataContext is not null)
                    return (TorrentsViewModel)TransfersTorrentsView.DataContext;

                return null;
            }
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
        }

        private void RssRuleLayoutToggleButton_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (RssView is null)
                return;
            if (sender is ToggleButton doubleToggleButton && doubleToggleButton.IsChecked == true)
            {
                RssRuleAddRemoveButtonsStackPanel.IsVisible = false;
                RssRulesComboBox.IsVisible = false;
                RssView.RssRulesView.SetLayOut(RssRulesView.RssRulesLayout.TripleColumn);
            }
            else
            {
                RssRuleAddRemoveButtonsStackPanel.IsVisible = true;
                RssRulesComboBox.IsVisible = true;
                RssView.RssRulesView.SetLayOut(RssRulesView.RssRulesLayout.DoubleColumn);
            }
        }

        ///If the state changes the buttons may have to be enabled/disabled.
        private void SelectedTorrent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"{e.PropertyName} changed");
            if (e.PropertyName == nameof(TorrentInfoViewModel.State) && sender is not null)
            {
                var torrent = sender as TorrentInfoViewModel;
                //Debug.WriteLine($"{torrent!.Name}'s state changed to: {torrent.State}");
                UpdatePauseResumeButtonStates();
            }
        }

        private void UpdatePauseResumeButtonStates()
        {
            if (TransfersTorrentsView.DataContext is null)
                return;

            var selectedTorrents = TransfersTorrentsView.TorrentsDataGrid.SelectedItems.OfType<TorrentInfoViewModel>();

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
                if (selectedTorrents.Count() > 0)
                    RemoveButton.IsEnabled = true;
                PauseButton.IsEnabled = pausedTorrents.Count < selectedTorrents.Count();
                ResumeButton.IsEnabled = pausedTorrents.Count > 0;


                MaxPriorityButton.IsEnabled = true;
                IncreasePriorityButton.IsEnabled = true;
                DecreasePriorityButton.IsEnabled = true;
                MinPriorityButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Avalonia's Tab SelectionChanged event seems a little bit dodgy and triggers for nested tabs to the outer tabs.
        /// Keeping track of the selected index we can filter these out.
        /// </summary>
        private int lastMainTabControlIndex = 0;
        private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainTabcontrol is not null && MainTabcontrol.SelectedIndex != lastMainTabControlIndex)
            {
                // The tab has actually changed!
                lastMainTabControlIndex = MainTabcontrol.SelectedIndex;

                switch (MainTabcontrol.SelectedIndex)
                {
                    case 0: // Transfers
                        break;
                    case 1: // Search
                        SetVmForSearch();
                        break;
                    case 2: // RSS
                        SetVmForRssRulesComboBox();
                        break;
                }
            }
        }

        private void RemoveVmForRssRulesComboBox()
        {
            if (RssRulesComboBox.DataContext is RssAutoDownloadingRulesViewModel)
            {
                // Clear previous bindings for the garbage collector 
                RssRulesComboBox.ClearValue(ComboBox.ItemsSourceProperty);
                RssRulesComboBox.ClearValue(ComboBox.SelectedItemProperty);
                RssRulesComboBox.ClearValue(Control.DataContextProperty);
            }
        }

        private void SetVmForRssRulesComboBox()
        {
            if (RssView?.RssRulesView?.DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                ClearComboBoxValues(RssRulesComboBox);
                RssRulesComboBox.DataContext = rssRulesVm;
                RssRulesComboBox.ItemsSource = rssRulesVm.RssRules;

                var selectedRssRuleBinding = new Binding
                {
                    Path = "SelectedRssRule",
                    Mode = BindingMode.TwoWay,
                    Source = rssRulesVm
                };

                RssRulesComboBox.Bind(ComboBox.SelectedItemProperty, selectedRssRuleBinding);
            }
        }

        private void DeleteRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (RssView?.RssRulesView?.DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.DeleteRules([rssRulesVm.SelectedRssRule]);
            }
        }

        private void AddRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (RssView?.RssRulesView?.DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.AddRule(AddRuleTextBox.Text!);
            }
        }



        private void SetVmForSearch()
        {
            if (SearchView?.DataContext is SearchViewModel searchVm)
            {
                SearchQueryTextBox.ClearValue(Control.DataContextProperty);
                SearchQueryTextBox.DataContext = searchVm;
                SearchQueryTextBox.Bind(TextBox.TextProperty, new Binding 
                { 
                    Path = "SearchQuery",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });

                ClearComboBoxValues(SearchPluginsComboBox);
                SearchPluginsComboBox.DataContext = searchVm;
                SearchPluginsComboBox.ItemsSource = searchVm.SearchPlugins;
                SearchPluginsComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
                {
                    Path = "SelectedSearchPlugin",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });
                SearchPluginsComboBox.DisplayMemberBinding = new Binding(nameof(SearchPlugin.FullName));


                ClearComboBoxValues(SearchPluginCategoriesComboBox);
                SearchPluginCategoriesComboBox.DataContext = searchVm;
                SearchPluginCategoriesComboBox.ItemsSource = searchVm.PluginCategories;
                SearchPluginCategoriesComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
                {
                    Path = "SelectedSearchPluginCategory",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });
                SearchPluginCategoriesComboBox.DisplayMemberBinding = new Binding(nameof(SearchPluginCategory.Name));
            }
        }

        /// <summary>
        /// Clear previous bindings if any to avoid conflicts
        /// </summary>
        /// <param name="comboBox"></param>
        private void ClearComboBoxValues(ComboBox comboBox)
        {
            comboBox.ClearValue(ComboBox.ItemsSourceProperty);
            comboBox.ClearValue(ComboBox.SelectedItemProperty);
            comboBox.ClearValue(Control.DataContextProperty);
        }

        private void LogInMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            ShowLogInWindow();
        }

        private void LogOutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            LogOut();
        }
        private void LogOutDeleteMenuItem_Click(object? sender, RoutedEventArgs e)
        {

            LogOut();
            SecureStorage.DestroyData();
        }
        private async void LogOut()
        {
            //Do the actual log out
            await QBittorrentService.QBittorrentClient.LogoutAsync();

            // Pause the timer so it can get garbage collected and no calls are made whilst logged out.
            if (DataContext is MainWindowViewModel mainWindowVm)
            {
                mainWindowVm.Pause();
                // Ensure TorrentsView isn't updating anything.
                if (TransfersTorrentsView.DataContext is TorrentsViewModel torrentsViewVm)
                {
                    TransfersTorrentsView.PausePaneUpdates();
                    torrentsViewVm.SelectedTorrent = null;

                }

                // Stop the automatic updating, it's no longer displayed and serves no purpose.
                // Stopping the timer allows the garbage collector to clean it up.
                mainWindowVm.Pause();
                mainWindowVm.PropertyChanged -= Mwvm_PropertyChanged;
            }

            // Set a new ViewModel to reset the UI.
            MainWindowViewModel mwvm = new();
            mwvm.PropertyChanged += Mwvm_PropertyChanged;
            DataContext = mwvm;            

            // If the user is logged out - they'll want to log in.
            ShowLogInWindow();
        }

        private void SettingsMenuButton_Click(object? sender, RoutedEventArgs e)
        {
            SettingsContextMenu.Open();
        }

        private void DownloadDirectoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var downloadDirectoryWindow = new DownloadDirectoryWindow();
            downloadDirectoryWindow.Show(this);
        }        

        private void RemoteSettingsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var ownAboutWindow = new PreferencesWindow();
            ownAboutWindow.ShowDialog(this);
        }

        private void OwnAboutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var ownAboutWindow = new OwnAboutWindow();
            ownAboutWindow.ShowDialog(this);
        }

        private void SearchPluginButton_Click(object? sender, RoutedEventArgs e)
        {
            var searchPluginsWindow = new SearchPluginsWindow();
            searchPluginsWindow.ShowDialog(this);
        }

        private void SearchToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            if (SearchView.DataContext is SearchViewModel searchViewModel)
            {
                searchViewModel.EndSearch();
            }
        }

        private void SearchToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (SearchView.DataContext is SearchViewModel searchViewModel)
            {
                searchViewModel.StartSearch();
            }
        }

        private void SearchQueryTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchToggleButton.IsChecked = !SearchToggleButton.IsChecked;
        }
    }
}
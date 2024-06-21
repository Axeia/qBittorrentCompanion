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

namespace qBittorrentCompanion.Views
{
    public partial class MainWindow : Window
    {


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

        }

        private List<string> FileQueue = [];

        public void AddToFileQueue(string filePath)
        {
            if (File.Exists(filePath))
                FileQueue.Add(filePath);
        }

        public void AddTorrentFileClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentFileWindow();
            addTorrentWindow.FilesUrlsTabControl.SelectedIndex = 1;
            addTorrentWindow.ShowDialog(this);
        }

        public void OnAddTorrentUrlClicked(object sender, RoutedEventArgs e)
        {
            var addTorrentWindow = new AddTorrentFileWindow();
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

        public void HttpRemoveTorrents(bool deleteFiles)
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

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            Debug.WriteLine("Main Window opened");
            _ = AuthenticateAndProcessFileQueue();

            SettingsContextMenu.Closed += SettingsContextMenu_Closed;
            RssView.RssTabControl.SelectionChanged += RssTabControl_SelectionChanged;
        }

        private void RssTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RssRulesControlsDockPanel.IsVisible = 
                MainTabcontrol.SelectedIndex == 2 && RssView.RssTabControl.SelectedIndex == 1;
        }

        private async Task AuthenticateAndProcessFileQueue()
        {
            await Authenticate();
            await ProcessFileQueue(true);
        }

        public async Task ProcessFileQueue(bool bypassWindow)
        {
            if (FileQueue.Count > 0)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var addTorrentWindow = new AddTorrentFileWindow();
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
                });
            }
        }

        /// <summary>
        /// User info is stored? » login automatically,<br/>
        /// User info unknown » present the Login window
        /// </summary>
        private async Task Authenticate()
        {
            var authenticated = await QBittorrentService.AutoAthenticate();
            if (!authenticated) // Can't login automatically, present login window
            {
                Debug.WriteLine("Couldn't automatically authenticate, presenting Login Window");
                ShowLogInWindow();
            }
            else // Bypass the login window
            {
                Debug.WriteLine("Automatically authenticated");

                if (this.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    mainWindowViewModel.IsLoggedIn = true;
                    LoadUpTorrents();
                }
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
            LoadUpTorrents();
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
                Debug.WriteLine($"{torrent!.Name}'s state changed to: {torrent.State}");
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
            MoveToTopButton.IsEnabled = false;
            MoveUpButton.IsEnabled = false;
            MoveDownButton.IsEnabled = false;
            MoveToBottomButton.IsEnabled = false;

            if (selectedTorrents is not null)
            {
                var pausedTorrents = selectedTorrents
                    .Where(torrent => TorrentsViewModel.TorrentStateGroupings.Paused.Contains((TorrentState)torrent.State!))
                    .ToList();
                if (selectedTorrents.Count() > 0)
                    RemoveButton.IsEnabled = true;
                PauseButton.IsEnabled = pausedTorrents.Count < selectedTorrents.Count();
                ResumeButton.IsEnabled = pausedTorrents.Count > 0;
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
                // Clear previous bindings if any to avoid conflicts
                RssRulesComboBox.ClearValue(ComboBox.ItemsSourceProperty);
                RssRulesComboBox.ClearValue(ComboBox.SelectedItemProperty);
                RssRulesComboBox.ClearValue(Control.DataContextProperty);

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

        private void LogInMenuItem_Click(object? sender, RoutedEventArgs e)
        {

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
            }

            // Set a new ViewModel to reset the UI.
            DataContext = new MainWindowViewModel();

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
    }
}
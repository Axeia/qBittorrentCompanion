
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
using qBittorrentCompanion.Helpers;
using System.Collections.Generic;
using System.IO;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using qBittorrentCompanion.Views.Preferences;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;

namespace qBittorrentCompanion.Views
{
    public partial class MainWindow : IcoWindow
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

            TransfersTorrentsView.ContextMenuDeleteMenuItem.Click += TransfersTorrentsView.OnRemoveTorrentClicked;
            SetWindowIcon();
            SetSelectedTab();
            SyncRssTabStripWithCarousel();
        }

        private void SetWindowIcon()
        {
            try
            {
                var xamlUri = new Uri("avares://qBittorrentCompanion/Assets/Logo.axaml");
                var logoCanvasContent = (Canvas)AvaloniaXamlLoader.Load(xamlUri);

                WindowIconViewBox.Child = logoCanvasContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Canvas content: {ex.Message}");
            }
        }

        public void ShowFlashMessage (string message)
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
            /*
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
            }*/
        }


        public void OnHelpAboutClicked(object sender, RoutedEventArgs e)
        {
            //var aboutWindow = new OwnAboutWindow();
            //aboutWindow.ShowDialog(this); // 'this' refers to the MainWindow instance
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

            if (!Design.IsDesignMode)
                _ = AuthenticateAndProcessQueues();

            SettingsContextMenu.Closed += SettingsContextMenu_Closed;
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
        private void Grid_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            // Ensure the left mouse button is pressed
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var originalSource = e.Source as Control;

                // Check if the source of the event is not one of the non-draggable controls
                if (originalSource != null &&
                    !(originalSource is TabStrip) &&
                    !(originalSource is TabItem) &&
                    !(originalSource is ToggleButton))
                {
                    // Initiate window dragging
                    var window = this as Window;
                    window?.BeginMoveDrag(e);
                }
            }
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
            }
        }

        private async void ShowLogInWindow()
        {
            var logInWindow = new LogInWindow(this);
            await logInWindow.ShowDialog(this);
            if (DataContext is MainWindowViewModel mainWindowVm)
                mainWindowVm.IsLoggedIn = true;
        }

        private TorrentsViewModel? TorrentsViewDataContext
        {
            get
            {
                if (TransfersTorrentsView.DataContext is not null)
                    return (TorrentsViewModel)TransfersTorrentsView.DataContext;

                return null;
            }
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

        private void SettingsLocalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var downloadDirectoryWindow = new LocalSettingsWindow();
            downloadDirectoryWindow.Show(this);
        }        

        private void RemoteSettingsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var preferencesWindow = new PreferencesWindow();
            preferencesWindow.ShowDialog(this);
        }

        private void OwnAboutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var ownAboutWindow = new OwnAboutWindow();
            ownAboutWindow.ShowDialog(this);
        }

        private void DownloadStatsButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Resources["TransfersFlyout"] is Flyout flyout)
            {
                // Enables styling from the .axaml
                if (!flyout.FlyoutPresenterClasses.Contains("StatusBar"))
                    flyout.FlyoutPresenterClasses.Add("StatusBar");

                flyout.HorizontalOffset = -24;
                flyout.ShowAt(BottomBorder);

                // Subscribe to the Closed event to uncheck the ToggleButton when the flyout is closed
                flyout.Closed += (s, args) =>
                {
                    if (sender is ToggleButton toggleButton)
                    {
                        toggleButton.IsChecked = false;
                    }
                };
            }
        }

        private void DownloadStatsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Resources["TransfersFlyout"] is Flyout flyout){ flyout.Hide(); }
        }

        private void TabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainTabStrip != null)
            {
                MainCarousel.SelectedIndex = MainTabStrip.SelectedIndex;
                SetSelectedTab();
            }
        }

        /// <summary>
        /// For some reason TabStrip doesn't actually set IsSelected to true on its items
        /// This ensures it does for MainTabStrip allowing some XAML bindings to IsSelected
        /// </summary>
        private void SetSelectedTab()
        {
            foreach (TabItem tabItem in MainTabStrip.Items.Cast<TabItem>())
            {
                tabItem.IsSelected = tabItem == MainTabStrip.Items[MainTabStrip.SelectedIndex];
            }

        }

        private void RssTabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SyncRssTabStripWithCarousel();
        }

        private void SyncRssTabStripWithCarousel()
        {
            if (RssView != null)
                RssView.RssCarousel.SelectedIndex = RssTabStrip.SelectedIndex;
        }

        private void TitleBarGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }
}
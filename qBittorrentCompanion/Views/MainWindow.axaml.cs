
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
using Avalonia;
using System.Reactive;
using Avalonia.Media;
using static qBittorrentCompanion.Services.QBittorrentService;
using qBittorrentCompanion.Logging;

namespace qBittorrentCompanion.Views
{
    public partial class MainWindow : TabControlsIcoWindow
    {
        private readonly DispatcherTimer _flashMessageTimer = new();

        public MainWindow()
        {
            InitializeComponent();

            _flashMessageTimer.Tick += HideFlashMessage;
            _flashMessageTimer.Interval = TimeSpan.FromSeconds(5);
            Loaded += MainWindow_Loaded;
            this.GetObservable(WindowStateProperty).Subscribe(OnWindowStateChanged);
            //EnableTestingMode();
        }

        private void OnWindowStateChanged(WindowState state)
        {
            var firstRow = FakeTitleBarGrid.RowDefinitions.First();
            firstRow.Height = state == WindowState.Maximized
                ? new GridLength(23)
                : new GridLength(28);
            FakeWindowBorder.Margin = state == WindowState.Maximized
                ? new Thickness(6)
                : new Thickness(0);
            FakeTitleGrid.Margin = state == WindowState.Maximized
                ? new Thickness(0, 0, 0, 0)
                : new Thickness(0, 2, 0, 0);

            if(OperatingSystem.IsWindows())
                FakeTitleBarGrid.Margin = new Thickness(0, 0, 138, 0);
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mwvm)
            {
                mwvm.PropertyChanged += Mwvm_PropertyChanged;
            }

            TransfersTorrentsView.ShowMessage += ShowFlashMessage;

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);

            TransfersTorrentsView.ContextMenuDeleteMenuItem.Click += TransfersTorrentsView.OnRemoveTorrentClicked;
            SetWindowIcon();
            SetSelectedTab();

            TabStrip = MainTabStrip;
            SetKeyBindings();
        }


        protected new void SetKeyBindings()
        {
            base.SetKeyBindings(); // Sorts out ctrl+tab, ctrl+shift+tab and ctrl+1 and ctrl+2

            var focusThirdTabBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.D3, KeyModifiers.Control),
                Command = FocusTabCommand,
                CommandParameter = 2
            };
            KeyBindings.Add(focusThirdTabBinding);

            var focusFourthTabBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.D4, KeyModifiers.Control),
                Command = FocusTabCommand,
                CommandParameter = 3
            };
            KeyBindings.Add(focusFourthTabBinding);
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

        public void ShowFlashMessage(string message)
        {
            SelectedTorrentTextBlock.Opacity = 0;
            PermanentMessageTextBlock.Opacity = 0;
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

        /// <summary>
        /// If it's not entirely clear whether the provided link is a torrent or link to a page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void AddTorrent(Uri uri, string name)
        {
            MainTabStrip.SelectedIndex = 0; // Focus transfers tab
            string strFileUrl = uri.ToString();
            var flyout = FlyoutBase.GetAttachedFlyout(TransfersTabItem);

            // Reset the TextBlock to initial state with small scale
            if (FlyoutTextBlock.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = 0.2;
                scaleTransform.ScaleY = 0.2;
            }
            FlyoutTextBlock.Opacity = 0.4;

            if (strFileUrl.ToString().EndsWith(".torrent") || strFileUrl.StartsWith("magnet:"))
            {
                FlyoutTextBlock.Text = $"Starting download: {name}";
                flyout!.ShowAt(TransfersTabItem);
                await AnimateTextBlock();

                AddToUrlQueue(strFileUrl);
                _ = ProcessUrlQueue(true);
            }
            else // Do some checks
            {
                FlyoutTextBlock.Text = $"Verifying it's an actual torrent link";
                flyout!.ShowAt(TransfersTabItem);
                await AnimateTextBlock();

                if (await LinkChecker.IsTorrentLink(strFileUrl))
                {
                    FlyoutTextBlock.Text = $"Starting download: {name}";
                }
                else
                {
                    FlyoutTextBlock.Text = "Does not appear to be a torrent, opening in browser";
                    Process.Start(new ProcessStartInfo { FileName = strFileUrl, UseShellExecute = true });
                }

                await AnimateTextBlock();
            }
        }

        private async Task AnimateTextBlock()
        {
            // Animate the TextBlock using manual steps
            const int steps = 10;
            const int stepDuration = 30; // milliseconds

            for (int i = 1; i <= steps; i++)
            {
                double progress = (double)i / steps;
                double scale = 0.2 + (0.8 * progress); // 0.2 to 1.0
                double opacity = 0.4 + (0.6 * progress); // 0.4 to 1.0

                if (FlyoutTextBlock.RenderTransform is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX = scale;
                    scaleTransform.ScaleY = scale;
                }
                FlyoutTextBlock.Opacity = opacity;

                await Task.Delay(stepDuration);
            }

            // Ensure final state
            if (FlyoutTextBlock.RenderTransform is ScaleTransform finalTransform)
            {
                finalTransform.ScaleX = 1.0;
                finalTransform.ScaleY = 1.0;
            }
            FlyoutTextBlock.Opacity = 1.0;
        }

        public void AltSpeedLimitsToggled(object sender, RoutedEventArgs e)
        {
            //TODO run as command on ViewModel and only if the checkbox value is different
            QBittorrentService.ToggleAlternativeSpeedLimitsAsync();
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
                if (!authenticated)
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
        /// User info is stored? � login automatically,<br/>
        /// User info unknown � present the Login window
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
                // Check if the source of the event is not one of the non-draggable controls
                if (e.Source is Control originalSource 
                    && originalSource is not Avalonia.Controls.Primitives.TabStrip 
                    && originalSource is not TabItem 
                    && originalSource is not ToggleButton)
                {
                    // Initiate window dragging
                    var window = this as Window;
                    window?.BeginMoveDrag(e);
                }
            }
        }


        private void Mwvm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mwvm)
            {
                if (e.PropertyName == nameof(MainWindowViewModel.IsLoggedIn))
                {
                    if (mwvm.IsLoggedIn)
                    {
                        LoadUpTorrents();
                    }
                    else
                    {
                        ShowLogInWindow();
                    }
                }
            }
            else
            {
                // This should never trigger
                Debug.WriteLine("DataContext is not MainWindowViewModel");
            }
        }

        private readonly List<string> FileQueue = [];

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


        private readonly List<string> UrlQueue = [];
        public void AddToUrlQueue(string urlPath)
        {
            if (Uri.TryCreate(urlPath, UriKind.Absolute, out Uri? uri))
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
            await QBittorrentService.LogoutAsync();

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
            if (Resources["TransfersFlyout"] is Flyout flyout) { flyout.Hide(); }
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

        private void TitleBarGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CopyNetworkRequestButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mwvm && mwvm.SelectedHttpData is HttpData hd)
            {
                Clipboard!.SetTextAsync(hd.Request);
            }
        }

        private void CopyNetworkRequestLinkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel mwvm && mwvm.SelectedHttpData is HttpData hd)
            {
                Clipboard!.SetTextAsync(hd.Url.ToString());
            }
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Labs.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using qBittorrentCompanion.Converters;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.Views;
using qBittorrentCompanion.Views.Preferences;
using ReactiveUI;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace qBittorrentCompanion
{
    public partial class App : Application
    {
        public static bool IsPython3Available { get; private set; } = false;
        public static string? PythonVersion { get; private set; } = null;
        public static string? PythonExecutable { get; private set; } = null;
        /// <summary>
        /// Absolute path to the default directory used to store Logo preset exports
        /// </summary>
        public static string LogoColorsExportDirectory => Path.Combine(AppContext.BaseDirectory, "IconColors");
        /// <summary>
        /// The icon to be used whilst in light mode
        /// Should be created in the root of the app and thus not require a path
        /// </summary>
        public static string LightModeIconFileName => "qbc-icon-light.ico";
        /// <summary>
        /// The icon to be used whilst in dark mode
        /// Should be created in the root of the app and thus not require a path
        /// </summary>
        public static string DarkModeIconFileName => "qbc-icon-dark.ico";

        public static readonly StyledProperty<WindowIcon?> DarkModeWindowIconProperty =
            AvaloniaProperty.Register<App, WindowIcon?>(nameof(DarkModeWindowIcon));

        public WindowIcon? DarkModeWindowIcon
        {
            get => GetValue(DarkModeWindowIconProperty);
            set => SetValue(DarkModeWindowIconProperty, value);
        }

        public static readonly StyledProperty<WindowIcon?> LightModeWindowIconProperty =
            AvaloniaProperty.Register<App, WindowIcon?>(nameof(LightModeWindowIcon));

        public WindowIcon? LightModeWindowIcon
        {
            get => GetValue(LightModeWindowIconProperty);
            set => SetValue(LightModeWindowIconProperty, value);
        }
        public Bitmap? LightModeWindowIconBitmap { get; private set; }
        public Bitmap? DarkModeWindowIconBitmap { get; private set; }
        public static readonly StyledProperty<WindowIcon?> CurrentModeWindowIconProperty =
            AvaloniaProperty.Register<App, WindowIcon?>(nameof(CurrentModeWindowIcon));

        public WindowIcon? CurrentModeWindowIcon
        {
            get => GetValue(CurrentModeWindowIconProperty);
            set
            {
                SetValue(CurrentModeWindowIconProperty, value);
                NotifyIconBitmapChange();
            }
        }

        private Bitmap? _previousBitmap;
        private void NotifyIconBitmapChange()
        {
            var newBitmap = CurrentModeWindowIconBitmap;
            RaisePropertyChanged(CurrentModeWindowIconBitmapProperty, _previousBitmap, newBitmap);
            _previousBitmap = newBitmap;
        }

        public static readonly DirectProperty<App, Bitmap?> CurrentModeWindowIconBitmapProperty =
            AvaloniaProperty.RegisterDirect<App, Bitmap?>(
                nameof(CurrentModeWindowIconBitmap),
                o => o.CurrentModeWindowIconBitmap);

        public Bitmap? CurrentModeWindowIconBitmap =>
            ActualThemeVariant == ThemeVariant.Dark
                ? DarkModeWindowIconBitmap
                : LightModeWindowIconBitmap;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Run in background whilst Avalonia gets busy
            // Won't need python until app is fully loaded and the search tab is clicked, should provide ample time
            _ = Task.Run(() =>
            {
                if (TryPython("python3", out var version))
                {
                    IsPython3Available = true;
                    PythonVersion = version;
                    PythonExecutable = "python3";
                    Debug.WriteLine($"Python available via 'python3': {version}");
                }
                // Windows sometimes makes it available without the 3. Check and check the version as well as plugins **need** python 3
                else if (TryPython("python", out version)
                  && Version.TryParse(version!.Split(' ').LastOrDefault(), out var parsedVersion)
                  && parsedVersion.Major >= 3)
                {
                    IsPython3Available = true;
                    PythonVersion = version;
                    PythonExecutable = "python";
                    Debug.WriteLine($"Python available via 'python': {version}");
                }
                else
                {
                    Debug.WriteLine("Python 3 not found. Local search plugins will be disabled.");
                }
            });

            if (!Design.IsDesignMode)
            {
                CreateLogoColorsExportDirectory();
                CreateLogoIconFiles();
            }
        }

        private static DirectoryInfo? CreateLogoColorsExportDirectory()
        {
            if (!Directory.Exists(LogoColorsExportDirectory))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(LogoColorsExportDirectory);
                Debug.WriteLine($"Created {dirInfo.FullName}");
                return dirInfo;
            }

            return null;
        }

        /// <summary>
        /// Convenience method, calls <see cref="Application.Current"/> and casts it to this class (<see cref="App"/>
        /// </value>
        public static new App? Current
        {
            get => (App?)Application.Current;
        }

        /// <summary>
        /// Creates the .ico files if they don't exist yet or if overwrite is set to true.<br/>
        /// Also sets <see cref="DarkModeWindowIcon"/> and <see cref="LightModeWindowIcon"/> and <see cref="App.CurrentModeWindowIcon"/>,
        /// the last one should propogate the change throughout the app updating all windows to the new icon.
        /// </summary>
        /// <param name="forceOverwriteDarkMode"></param>
        /// <param name="forceOverwriteLightMode"></param>
        /// <returns></returns>
        public static bool CreateLogoIconFiles(bool forceOverwriteDarkMode = false, bool forceOverwriteLightMode = false)
        {
            bool darkLogoExists = CreateLogoIconFile(DarkModeIconFileName, ConfigService.LogoColorsDark, forceOverwriteDarkMode);
            bool lightLogoExists = CreateLogoIconFile(LightModeIconFileName, ConfigService.LogoColorsLight, forceOverwriteLightMode);

            if (App.Current is App app)
            {
                if (darkLogoExists)
                {
                    app.DarkModeWindowIcon = new WindowIcon(DarkModeIconFileName);
                    app.DarkModeWindowIconBitmap = new Bitmap(DarkModeIconFileName);
                }

                if (lightLogoExists)
                {
                    app.LightModeWindowIcon = new WindowIcon(LightModeIconFileName);
                    app.LightModeWindowIconBitmap = new Bitmap(LightModeIconFileName);
                }

                app.CurrentModeWindowIcon = app.ActualThemeVariant == ThemeVariant.Dark
                    ? App.Current?.DarkModeWindowIcon
                    : App.Current?.LightModeWindowIcon;
            }

            return lightLogoExists && darkLogoExists;
        }

        /// <summary>
        /// Returns true if the file already exists, or if it was created successfully
        /// </summary>
        /// <param name="dotIcofileName"></param>
        /// <param name="colorScheme"></param>
        /// <returns></returns>
        private static bool CreateLogoIconFile(string dotIcofileName, LogoDataRecord colorScheme, bool forceOverwrite = false)
        {
            string outputDirectory = AppContext.BaseDirectory;
            string dotIcoPath = Path.Combine(outputDirectory, dotIcofileName);
            bool fileExists = File.Exists(dotIcoPath);

            if (forceOverwrite || !fileExists)
            {
                if (!fileExists)
                    Debug.WriteLine($"Could not find {dotIcofileName}, creating it");
                else
                    Debug.WriteLine($"Overwriting {dotIcofileName}");
                SKSvg svg = SKSvg.CreateFromSvg(LogoHelper.GetLogoAsXDocument(colorScheme).ToString());
                return svg.SaveAsIco(dotIcoPath);
            }

            return true;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Dispatcher.UIThread.UnhandledException += (s, e) =>
            {
                WriteCrashLog("DispatcherException", e.Exception);
                e.Handled = true; // Prevents the app from disappearing
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = new();
                desktop.MainWindow = mainWindow;
                mainWindow.AltSpeedLimitsCheckBox.IsCheckedChanged += AltSpeedLimitsCheckBox_IsCheckedChanged;

                var args = desktop.Args ?? [];
                foreach (var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);

                if (mainWindow.DataContext is MainWindowViewModel mwvm)
                {
                    mwvm
                        .WhenAnyValue(m => m.ServerStateVm)
                        .Subscribe(s =>
                        {
                            s
                                .WhenAnyValue(
                                    v1 => v1.DlInfoSpeed,
                                    v2 => v2.UpInfoSpeed
                                )
                                .Subscribe(UpdateTrayIconToolTip());
                        });
                    mwvm.TorrentsViewModel.WhenAnyValue(
                        cbmp => cbmp.CanBeMassPaused,
                        cbmu => cbmu.CanBeMassUnpaused
                    ).Subscribe(_ => UpdatePauseAndUnpauseAllMenuItems());
                }

            }

            base.OnFrameworkInitializationCompleted();
        }

        private CancellationToken UpdatePauseAndUnpauseAllMenuItems()
        {
            if (GetNativeMenuItemEndingOn("Pause all") is NativeMenuItem pauseAllnmi
                && GetNativeMenuItemEndingOn("Unpause all") is NativeMenuItem unpauseAllNmi
                && GetMainWindow() is MainWindow mw
                && mw.DataContext is MainWindowViewModel mwvm
                && mwvm.TorrentsViewModel is TorrentsViewModel tvm)
            {
                pauseAllnmi.IsEnabled = tvm.CanBeMassPaused;
                unpauseAllNmi.IsEnabled = tvm.CanBeMassUnpaused;
            }

            return CancellationToken.None;
        }

        private CancellationToken UpdateTrayIconToolTip()
        {
            if (GetMainWindow() is MainWindow mw
                && mw.DataContext is MainWindowViewModel mwvm
                && mwvm.ServerStateVm is ServerStateViewModel ssvm)
            {
                BytesSpeedToHumanReadableConverter bsthrc = new();
                string dlSpeed = bsthrc.Convert(ssvm.DlInfoSpeed, typeof(string), "", CultureInfo.CurrentCulture)?.ToString() ?? "";
                string upSpeed = bsthrc.Convert(ssvm.UpInfoSpeed, typeof(string), "", CultureInfo.CurrentCulture)?.ToString() ?? "";

                var trayIcon = TrayIcon
                    .GetIcons(this)
                    ?.FirstOrDefault();

                if(trayIcon is not null)
                    trayIcon.ToolTipText = "qBittorrent Companion\n\n"
                            + $"Download: {dlSpeed}\n"
                            + $"Upload: {upSpeed}";
            }

            return CancellationToken.None;
        }

        private void AltSpeedLimitsCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if(GetMainWindow() is MainWindow mainWindow
                && GetAltSpeedNativeMenuItem() is NativeMenuItem altSpeedNativeMenuItem)
            {
                altSpeedNativeMenuItem.IsEnabled = true;
                altSpeedNativeMenuItem.IsChecked = mainWindow.AltSpeedLimitsCheckBox.IsChecked == true;
            }
        }

        /// <summary>
        /// Gets one of tray icon native menu items, or null if it can't be found.
        /// For native menu items there's no easy way to obtain a reference so it's found
        /// by matching the end of the text on it - this is rather error prone.
        /// </summary>
        /// <returns></returns>
        public NativeMenuItem? GetNativeMenuItemEndingOn(string textEnd)
        {
            var trayIcon = TrayIcon
                .GetIcons(this)
                ?.FirstOrDefault();

            if (trayIcon?.Menu is NativeMenu menu)
            {
                var toggleItem = menu.Items
                    .OfType<NativeMenuItem>()
                    .FirstOrDefault(i => i.Header != null && i.Header.EndsWith(textEnd));

                return toggleItem;
            }

            return null;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                WriteCrashLog("UnhandledException", ex);
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            WriteCrashLog("UnobservedTaskException", e.Exception);
            e.SetObserved(); // Prevents app from crashing
        }

        public static void WriteCrashLog(string type, Exception ex)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string logDir = Path.Combine(baseDir, "Logs");
                Directory.CreateDirectory(logDir);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string filePath = Path.Combine(logDir, $"Crash_{type}_{timestamp}.txt");

                File.WriteAllText(filePath, $"[{type}] {timestamp}\n{ex}");
                Debug.WriteLine($"Crash log written to: {filePath}");
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Failed to write crash log: {logEx}");
            }
        }

        private static bool TryPython(string executable, out string? version)
        {
            version = null;

            try
            {
                var info = new ProcessStartInfo(executable)
                {
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(info);
                if (process == null)
                    return false;

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                version = string.IsNullOrWhiteSpace(output) ? error.Trim() : output.Trim();
                return true;
            }
            catch { return false; }
        }

        private void ShowHideNativeMenuItem_Click(object? sender, EventArgs e)
        {
            if (GetMainWindow() is MainWindow mainWindow)
            {
                if (mainWindow.ShowInTaskbar)
                    mainWindow.HideToTray();
                else
                    mainWindow.RestoreFromTray();
            }
        }

        private void AddTorrentFileNativeMenuItem_Click(object? sender, EventArgs e)
        {

            if (GetMainWindow() is MainWindow mainWindow)
            {
                mainWindow.TransfersTorrentsView.AddTorrentFileClicked(this, new RoutedEventArgs());
            }
        }

        private void AddTorrentLinkNativeMenuItem_Click(object? sender, EventArgs e)
        {
            if (GetMainWindow() is MainWindow mainWindow)
            {
                mainWindow.TransfersTorrentsView.OnAddTorrentUrlClicked(this, new RoutedEventArgs());
            }
        }

        private void ExitNativeMenuItem_Click(object? sender, EventArgs e)
        {
            if (GetMainWindow() is MainWindow mainWindow)
            {
                mainWindow.IsActuallyExiting = true;
                mainWindow.Close();
            }
        }

        private void PauseAll_Click(object? sender, EventArgs e)
        {
            if (GetTorrentsViewModel() is TorrentsViewModel tvm)
            {
                _ = tvm.PauseAll();
            }
        }

        private void UnpauseAll_Click(object? sender, EventArgs e)
        {
            if (GetTorrentsViewModel() is TorrentsViewModel tvm)
            {
                _ = tvm.UnpauseAll();
            }
        }

        private void OpenPreferencesSpeedLimits_Click(object? sender, EventArgs e)
        {
            if (GetMainWindow() is MainWindow mainWindow)
            {
                var preferencesWindow = new PreferencesWindow();
                preferencesWindow.ShowDialog(mainWindow);
                Dispatcher.UIThread.Post(() => {
                    preferencesWindow.PreferencesTabControl.SelectedIndex = 3;
                });
            }
        }

        private void TestNotification_Click(object? sender, EventArgs e)
        {
            if (NativeNotificationManager.Current is { } manager)
            {
                INativeNotification? inn = manager.CreateNotification("Downloads");
                if (inn is INativeNotification innn)
                {
                    innn.Message = "Leap-16.0-offline-installer-x86_64-Build171.1.install.iso";
                    innn.Title = "Download completed";
                    innn.Tag = "qBittorrent Companion";
                    innn.Icon = CurrentModeWindowIconBitmap;
                    innn.Show();
                }
                else
                {
                    Debug.WriteLine("No native notification");
                }
            }
            else
            {
                Debug.WriteLine("No native notification manager");
            }
        }

        private MainWindow? GetMainWindow()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is MainWindow mainWindow)
                return mainWindow;

            return null;
        }

        private TorrentsViewModel? GetTorrentsViewModel()
        {
            if (GetMainWindow() is MainWindow mainWindow
                 && mainWindow.DataContext is MainWindowViewModel mwvm
                 && mwvm.TorrentsViewModel is TorrentsViewModel tvm)
                return tvm;

            return null;
        }

        private void ToggleAlternativeSpeedTorrentLinkNativeMenuItem(object? sender, EventArgs e)
        {
            if (GetMainWindow() is MainWindow mainWindow)
            {
                mainWindow.AltSpeedLimitsCheckBox.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                if (mainWindow.DataContext is MainWindowViewModel mwvm
                    && mwvm.ServerStateVm is ServerStateViewModel ssvm)
                {
                    ssvm.GlobalAltSpeedLimitsEnabled = !ssvm.GlobalAltSpeedLimitsEnabled;
                }
            }
        }

        public NativeMenuItem? GetAltSpeedNativeMenuItem()
        {
            return GetNativeMenuItemEndingOn("Alternative speed limits");
        }
    }
}

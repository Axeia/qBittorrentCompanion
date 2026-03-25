using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
        public static string LogoColorsExportDirectory 
            => Path.Combine(AppContext.BaseDirectory, "IconColors");
        /// <summary>
        /// The icon to be used whilst in light mode
        /// Should be created in the root of the app and thus not require a path
        /// </summary>
        public static string LightModeIconFileName 
            => "qbc-icon-light.ico";
        /// <summary>
        /// The icon to be used whilst in dark mode
        /// Should be created in the root of the app and thus not require a path
        /// </summary>
        public static string DarkModeIconFileName 
            => "qbc-icon-dark.ico";
        public Bitmap? LightModeWindowIconBitmap { get; private set; }
        public static readonly StyledProperty<WindowIcon?> DarkModeWindowIconProperty =
            AvaloniaProperty.Register<App, WindowIcon?>(nameof(DarkModeWindowIcon));
        public WindowIcon? DarkModeWindowIcon
        {
            get => GetValue(DarkModeWindowIconProperty);
            set => SetValue(DarkModeWindowIconProperty, value);
        }

        public static readonly StyledProperty<WindowIcon?> LightModeWindowIconProperty =
            AvaloniaProperty.Register<App, WindowIcon?>(nameof(LightModeWindowIcon));
        public Bitmap? DarkModeWindowIconBitmap { get; private set; }
        public WindowIcon? LightModeWindowIcon
        {
            get => GetValue(LightModeWindowIconProperty);
            set => SetValue(LightModeWindowIconProperty, value);
        }

        public static readonly StyledProperty<WindowIcon?> CurrentModeWindowIconProperty 
            = AvaloniaProperty.Register<App, WindowIcon?>(nameof(CurrentModeWindowIcon));
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
                o => o.CurrentModeWindowIconBitmap
            );

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

            RequestedThemeVariant = Design.IsDesignMode 
                ? ThemeVariant.Default
                : ConfigService.AppTheme.ToActualThemeVariant();
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
            => (App?)Application.Current;

        /// <summary>
        /// Creates the .ico files if they don't exist yet or if overwrite is set to true.<br/>
        /// Also sets <see cref="DarkModeWindowIcon"/> and <see cref="LightModeWindowIcon"/> and <see cref="App.CurrentModeWindowIcon"/>,
        /// the last one should propogate the change throughout the app updating all windows to the new icon.
        /// </summary>
        /// <param name="forceOverwriteDarkMode"></param>
        /// <param name="forceOverwriteLightMode"></param>
        /// <returns></returns>
        public bool CreateLogoIconFiles(bool forceOverwriteDarkMode = false, bool forceOverwriteLightMode = false)
        {
            bool darkLogoExists = CreateLogoIconFile(DarkModeIconFileName, ConfigService.LogoColorsDark, forceOverwriteDarkMode);
            bool lightLogoExists = CreateLogoIconFile(LightModeIconFileName, ConfigService.LogoColorsLight, forceOverwriteLightMode);

            string outputDirectory = AppContext.BaseDirectory;
            string darkIconPath = Path.Combine(outputDirectory, DarkModeIconFileName);
            string lightIconPath = Path.Combine(outputDirectory, LightModeIconFileName);


            if (darkLogoExists)
            {
                DarkModeWindowIcon = new WindowIcon(darkIconPath);
                DarkModeWindowIconBitmap = new Bitmap(darkIconPath);
            }
            if (lightLogoExists)
            {
                LightModeWindowIcon = new WindowIcon(lightIconPath);
                LightModeWindowIconBitmap = new Bitmap(lightIconPath);
            }

            CurrentModeWindowIcon = ActualThemeVariant == ThemeVariant.Dark
                ? DarkModeWindowIcon
                : LightModeWindowIcon;

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
                // ---  === Important must be done before MainWindow is created === ---
                string lang = Design.IsDesignMode ? MainWindowViewModel.enUS : ConfigService.Language;
                qBittorrentCompanion.Resources.Resources.Culture = new CultureInfo(lang);

                // Now that the language is set, create MainWindow
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
                                    v2 => v2.UpInfoSpeed,
                                    (dl, up) => (dl, up)
                                 )
                                .Subscribe(speeds => {
                                    UpdateTrayIconToolTip(speeds);
                                    UpdateStatusOnWindowIcon(speeds);
                                });
                        });
                    mwvm.TorrentsViewModel.WhenAnyValue(
                        cbmp => cbmp.CanBeMassPaused,
                        cbmu => cbmu.CanBeMassUnpaused
                    ).Subscribe(_ => UpdatePauseAndUnpauseAllMenuItems());
                }

            }

            // The Resource file isn't accessible to App.axaml as it hasn't loaded yet
            // As a workaround it's done here inside OnFrameworkInitializationCompleted
            Application.Current!.Styles.Add(new Style(x => x.OfType<ToggleSwitch>())
            {
                Setters =
                {
                    new Setter(ToggleSwitch.OnContentProperty, qBittorrentCompanion.Resources.Resources.Toggle_On),
                    new Setter(ToggleSwitch.OffContentProperty, qBittorrentCompanion.Resources.Resources.Toggle_Off),
                }
            });

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

        /// <summary>
        /// A download arrow from the fluent icon set, used by <see cref="UpdateStatusOnWindowIcon(ValueTuple{long?, long?})"/>
        /// </summary>
        private readonly StreamGeometry _downloadArrowStreamGeometry = StreamGeometry.Parse(
            "M12.25,39.5 L35.75,39.5 C36.4403559,39.5 37,40.0596441 37,40.75 C37,41.3972087 36.5081253,41.9295339 35.8778052,41.9935464 L35.75,42 L12.25,42 C11.5596441,42 11,41.4403559 11,40.75 C11,40.1027913 11.4918747,39.5704661 12.1221948,39.5064536 L12.25,39.5 L35.75,39.5 L12.25,39.5 Z M23.6221948,6.00645361 L23.75,6 C24.3972087,6 24.9295339,6.49187466 24.9935464,7.12219476 L25,7.25 L25,31.54 L30.6466793,25.8942911 C31.1348346,25.4061358 31.9262909,25.4061358 32.4144462,25.8942911 C32.9026016,26.3824465 32.9026016,27.1739027 32.4144462,27.6620581 L24.6362716,35.4402327 C24.1481163,35.928388 23.35666,35.928388 22.8685047,35.4402327 L15.0903301,27.6620581 C14.6021747,27.1739027 14.6021747,26.3824465 15.0903301,25.8942911 C15.5784855,25.4061358 16.3699417,25.4061358 16.858097,25.8942911 L22.5,31.536 L22.5,7.25 C22.5,6.60279131 22.9918747,6.0704661 23.6221948,6.00645361 L23.75,6 L23.6221948,6.00645361 Z"
        );
        /// <summary>
        /// A upload arrow from the fluent icon set, used by <see cref="UpdateStatusOnWindowIcon(ValueTuple{long?, long?})"/>
        /// </summary>
        private readonly StreamGeometry _uploadArrowStreamGeometry = StreamGeometry.Parse(
            "M18.2498 3.50871C18.664 3.50883 19 3.17314 19 2.75892C19 2.34471 18.6644 2.00883 18.2502 2.00871L5.25022 2.00494C4.836 2.00482 4.5 2.34051 4.5 2.75473C4.5 3.16894 4.83557 3.50482 5.24978 3.50494L18.2498 3.50871ZM11.6482 21.9969L11.75 22.0038C12.1297 22.0038 12.4435 21.7216 12.4932 21.3555L12.5 21.2538L12.499 7.56876L16.2208 11.2891C16.4871 11.5553 16.9038 11.5795 17.1974 11.3616L17.2815 11.289C17.5477 11.0227 17.5719 10.606 17.354 10.3124L17.2814 10.2283L12.2837 5.23171C12.0176 4.96562 11.6012 4.94131 11.3076 5.15888L11.2235 5.2314L6.22003 10.228C5.92694 10.5207 5.92661 10.9956 6.21931 11.2887C6.48539 11.5551 6.90204 11.5796 7.1958 11.362L7.27997 11.2894L10.999 7.57576L11 21.2538C11 21.6335 11.2822 21.9473 11.6482 21.9969Z"
        );

        /// <summary>
        /// Used to cache the value for <see cref="UpdateStatusOnWindowIcon(ValueTuple{long?, long?})"/>
        /// </summary>
        private bool? _previousShowDownload = null;
        /// <summary><inheritdoc cref="_previousShowDownload"/></summary>
        private bool? _previousShowUpload = null;
        public bool ShowUploadDownloadStatusOnIcon = Design.IsDesignMode || ConfigService.ShowUploadDownloadStatusOnIcon;
        private bool _previousShowUploadDownloadStatusOnIcon = Design.IsDesignMode || ConfigService.ShowUploadDownloadStatusOnIcon;

        /// <summary>
        /// Updates the icon used through qBittorrent Companion.
        /// The provided 'speeds' results are cached to prevent unnecessary updates.
        /// 
        /// If the dl value is bigger than 0 a download arrow is added.
        /// If the up value is bigger than 0 a upload arrow is added.
        /// </summary>
        /// <param name="speeds"></param>
        /// <returns></returns>
        private CancellationToken UpdateStatusOnWindowIcon((long? dl, long? up) speeds)
        {
            if (_previousShowUploadDownloadStatusOnIcon != ShowUploadDownloadStatusOnIcon)
            {
                _previousShowUploadDownloadStatusOnIcon = ShowUploadDownloadStatusOnIcon;
                // Don't update _previousShowDownload/_previousShowUpload here
                // Just force a mismatch by nulling them
                _previousShowDownload = null;
                _previousShowUpload = null;
                // Don't return early - fall through to redraw
            }

            bool shouldShowDownload = ShowUploadDownloadStatusOnIcon && speeds.dl > 0;
            bool shouldShowUpload = ShowUploadDownloadStatusOnIcon && speeds.up > 0;

            if (shouldShowDownload == _previousShowDownload && shouldShowUpload == _previousShowUpload)
                return CancellationToken.None;

            _previousShowDownload = shouldShowDownload;
            _previousShowUpload = shouldShowUpload;

            var iconPath = ActualThemeVariant == ThemeVariant.Dark
                ? Path.Combine(AppContext.BaseDirectory, DarkModeIconFileName)
                : Path.Combine(AppContext.BaseDirectory, LightModeIconFileName);

            if (!File.Exists(iconPath))
                return CancellationToken.None;

            var renderSize = new PixelSize(32, 32);
            var renderBitmap = new RenderTargetBitmap(renderSize, new Vector(96, 96));

            using var sourceStream = File.OpenRead(iconPath);
            var smallBitmap = Bitmap.DecodeToWidth(sourceStream, 32, BitmapInterpolationMode.HighQuality);

            using (var context = renderBitmap.CreateDrawingContext())
            {
                context.DrawImage(smallBitmap, new Rect(0, 0, 32, 32));

                if (shouldShowDownload || shouldShowUpload)
                {
                    var overlaySize = 32 * 0.45;

                    if (shouldShowDownload)
                    {
                        var scale = overlaySize / 48.0;
                        var offset = 32 - overlaySize;
                        using (context.PushTransform(
                            Matrix.CreateScale(scale, scale) *
                            Matrix.CreateTranslation(offset, offset)))
                        {
                            context.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 6 / scale), _downloadArrowStreamGeometry);
                            context.DrawGeometry(Brushes.White, new Pen(Brushes.White, 2 / scale), _downloadArrowStreamGeometry);
                        }
                    }

                    if (shouldShowUpload)
                    {
                        var scale = overlaySize / 24.0;
                        using (context.PushTransform(
                            Matrix.CreateScale(scale, scale) *
                            Matrix.CreateTranslation(0, 0)))
                        {
                            context.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 6 / scale), _uploadArrowStreamGeometry);
                            context.DrawGeometry(Brushes.White, new Pen(Brushes.White, 2 / scale), _uploadArrowStreamGeometry);
                        }
                    }
                }
            }
            
            Dispatcher.UIThread.Post(() =>
            {
                using var stream = new MemoryStream();
                renderBitmap.Save(stream);
                stream.Position = 0;
                CurrentModeWindowIcon = new WindowIcon(stream);
            });

            return CancellationToken.None;
        }

        private CancellationToken UpdateTrayIconToolTip((long? dl, long? up) speeds)
        {
            BytesSpeedToHumanReadableConverter bsthrc = new();
            string dlSpeed = bsthrc.Convert(speeds.dl, typeof(string), "", CultureInfo.CurrentCulture)?.ToString() ?? "";
            string upSpeed = bsthrc.Convert(speeds.up, typeof(string), "", CultureInfo.CurrentCulture)?.ToString() ?? "";

            var trayIcon = TrayIcon
                .GetIcons(this)
                ?.FirstOrDefault();

            trayIcon?.ToolTipText = string.Format(
                qBittorrentCompanion.Resources.Resources.App_TrayIconToolTip,
                dlSpeed,
                upSpeed
            );

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

        // Saves language to config and then applies it to the app
        public void SetLanguage(string language)
        {
            ConfigService.Language = language;
            qBittorrentCompanion.Resources.Resources.Culture = new CultureInfo(language);
        }
    }
}

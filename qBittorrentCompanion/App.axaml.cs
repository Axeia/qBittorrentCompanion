using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Views;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                // Update windows so it uses the icon in task manager
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                    && desktop.MainWindow is Window mainWindow
                    && OperatingSystem.IsWindows())
                {
                    string iconFileName = ActualThemeVariant == ThemeVariant.Dark
                        ? DarkModeIconFileName
                        : LightModeIconFileName;
                    string iconFilePath = Path.Combine(AppContext.BaseDirectory, iconFileName);
                    Debug.WriteLine("Setting icon for windows to : "+iconFilePath);
                    WindowsIconHelper.SetTaskbarIcon(mainWindow, iconFilePath);
                }
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

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                var args = desktop.Args ?? [];
                foreach (var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);
            }

            base.OnFrameworkInitializationCompleted();
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

        private static void WriteCrashLog(string type, Exception ex)
        {
            try
            {
                string logDir = Path.Combine("Logs");
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
    }
}

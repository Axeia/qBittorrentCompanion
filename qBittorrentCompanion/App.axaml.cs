using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using qBittorrentCompanion.Extensions;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.Views;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace qBittorrentCompanion
{
    public partial class App : Application
    {
        public static bool IsPython3Available { get; private set; } = false;
        public static string? PythonVersion { get; private set; } = null;
        public static string? PythonExecutable { get; private set; } = null;

        public static string LogoColorsExportDirectory = Path.Combine(AppContext.BaseDirectory, "IconColors");

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

            if (!Directory.Exists(LogoColorsExportDirectory))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(LogoColorsExportDirectory);
                Debug.WriteLine($"Created {dirInfo.FullName}");
            }

            CreateLogoIcos();
        }

        private void CreateLogoIcos()
        {
            // Create icon
            CreateLogoIfNotExists("qbc-logo-light.ico", ConfigService.LogoColorsLight);
            CreateLogoIfNotExists("qbc-logo-dark.ico", ConfigService.LogoColorsDark);
        }

        private void CreateLogoIfNotExists(string dotIcofileName, LogoColorsRecord colorScheme)
        {
            string outputDirectory = AppContext.BaseDirectory;
            string dotIcoPath = Path.Combine(outputDirectory, dotIcofileName);

            if (!File.Exists(dotIcoPath))
            {
                Debug.WriteLine($"Could not find {dotIcofileName}, creating it");

                Uri logoUri = new("avares://qBittorrentCompanion/Assets/qbc-logo.svg");
                SKSvg svg = SKSvg.CreateFromStream(AssetLoader.Open(logoUri)); // SKSvg was built for rendering, not access to the DOM
                
                svg.SaveAsIco(dotIcoPath);
            }
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
            catch
            {
                return false;
            }
        }
    }
}

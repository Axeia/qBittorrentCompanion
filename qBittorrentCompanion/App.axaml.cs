using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using qBittorrentCompanion.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion
{
    public partial class App : Application
    {
        public static bool IsPython3Available { get; private set; } = false;
        public static string? PythonVersion { get; private set; } = null;
        public static string? PythonExecutable { get; private set; } = null;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Run in background whilst Avalonia gets busy
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
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                var args = desktop.Args ?? [];
                foreach (var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);
            }

            base.OnFrameworkInitializationCompleted();
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

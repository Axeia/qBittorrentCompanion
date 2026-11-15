using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace qbittorrentCompanion.Helpers
{
    public static class LinuxIntegration
    {
        const string AppId = "io.github.axeia.qBittorrentCompanion";

        static bool ShouldSelfRegister()
        {
            if (!OperatingSystem.IsLinux()) return false;

            // Flatpak / Snap / AppImage handle their own integration
            if (File.Exists("/.flatpak-info")) return false;
            if (Environment.GetEnvironmentVariable("SNAP") is { Length: > 0 }) return false;
            if (Environment.GetEnvironmentVariable("APPIMAGE") is { Length: > 0 }) return false;

            // System package (deb/rpm): the packaged .desktop already works
            var exe = Environment.ProcessPath;
            if (exe is null || exe.StartsWith("/usr/") || exe.StartsWith("/opt/")) return false;

            return true;
        }

        static string DataHome =>
            Environment.GetEnvironmentVariable("XDG_DATA_HOME") is { Length: > 0 } x
                ? x
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

        static string DesktopFilePath => Path.Combine(DataHome, "applications", $"{AppId}.desktop");

        public static bool IsRegistered() => File.Exists(DesktopFilePath);

        /// <summary>Call on every startup. Only repairs an existing registration (e.g. folder was moved).</summary>
        public static void RepairIfStale()
        {
            if (ShouldSelfRegister() && IsRegistered())
                Register(setAsDefault: false);
        }

        /// <summary>Call when the user opts in (first-run prompt or a settings button).</summary>
        public static void Register(bool setAsDefault)
        {
            if (!ShouldSelfRegister()) return;

            var exe = Environment.ProcessPath!;
            var appDir = Path.GetDirectoryName(exe)!;

            var templatePath = Path.Combine(appDir, $"{AppId}.desktop");
            if (!File.Exists(templatePath)) return;

            // Install the icon under the app-id name so Icon= can stay a plain name
            var svg = Path.Combine(appDir, "qbc-logo.svg");
            if (File.Exists(svg))
            {
                var iconDir = Path.Combine(DataHome, "icons", "hicolor", "scalable", "apps");
                Directory.CreateDirectory(iconDir);
                File.Copy(svg, Path.Combine(iconDir, $"{AppId}.svg"), overwrite: true);
            }

            var lines = File.ReadAllLines(templatePath).Select(l =>
                l.StartsWith("Exec=") ? $"Exec={QuoteExec(exe)} %U" : l);
            var content = string.Join("\n", lines) + "\n";

            var target = DesktopFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);

            if (!File.Exists(target) || File.ReadAllText(target) != content)
            {
                File.WriteAllText(target, content);
                Run("update-desktop-database", Path.GetDirectoryName(target)!);
            }

            if (setAsDefault)
            {
                Run("xdg-mime", $"default {AppId}.desktop x-scheme-handler/magnet");
                Run("xdg-mime", $"default {AppId}.desktop application/x-bittorrent");
            }
        }

        public static void Unregister()
        {
            if (File.Exists(DesktopFilePath)) File.Delete(DesktopFilePath);
            Run("update-desktop-database", Path.GetDirectoryName(DesktopFilePath)!);
        }

        // Desktop Entry spec: quote the path, escape \ " ` $ inside quotes
        // (the file-level string escaping then doubles the backslashes)
        static string QuoteExec(string path)
        {
            var s = path.Replace("\\", "\\\\\\\\").Replace("\"", "\\\\\"")
                        .Replace("`", "\\\\`").Replace("$", "\\\\$").Replace("%", "%%");
            return $"\"{s}\"";
        }

        static void Run(string file, string args)
        {
            try
            {
                using var p = Process.Start(new ProcessStartInfo(file, args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                p?.WaitForExit(5000);
            }
            catch { /* tool missing: not fatal */ }
        }
    }
}
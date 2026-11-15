using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace qbittorrentCompanion.Helpers
{
    [SupportedOSPlatform("windows")]
    public static class WindowsIntegration
    {
        const string AppName = "qBittorrentCompanion";
        const string DisplayName = "qBittorrent Companion";
        const string MagnetProgId = "qBittorrentCompanion.Magnet";
        const string TorrentProgId = "qBittorrentCompanion.Torrent";
        const string CapabilitiesPath = @"Software\qBittorrentCompanion\Capabilities";
        const string RegisteredAppsPath = @"Software\RegisteredApplications";
        const string Classes = @"Software\Classes";

        public static bool CanSelfRegister =>
            OperatingSystem.IsWindows() && Environment.ProcessPath is not null;

        static string ExePath => Environment.ProcessPath!;
        static string Command => $"\"{ExePath}\" \"%1\"";

        /// <summary>True if our ProgId exists (i.e. the user has registered before).</summary>
        public static bool IsRegistered()
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"{Classes}\{MagnetProgId}\shell\open\command");
            return key is not null;
        }

        /// <summary>Call on every startup. Only fixes an existing registration (moved/updated exe).</summary>
        public static void RepairIfStale()
        {
            if (!CanSelfRegister || !IsRegistered()) return;

            using var key = Registry.CurrentUser.OpenSubKey($@"{Classes}\{MagnetProgId}\shell\open\command");
            if (key?.GetValue("") as string != Command)
                Register();
        }

        /// <summary>
        /// Writes the registration. Safe to call repeatedly (installer hook, settings button).
        /// </summary>
        public static void Register()
        {
            if (!CanSelfRegister) return;

            // 1. ProgIds: what Windows runs for each kind of thing we handle
            WriteProgId(MagnetProgId, "URL:Magnet Protocol", isUrlProtocol: true);
            WriteProgId(TorrentProgId, "BitTorrent file", isUrlProtocol: false);

            // 2. Capabilities: makes the app show up under Settings > Default apps
            using (var cap = Registry.CurrentUser.CreateSubKey(CapabilitiesPath))
            {
                cap.SetValue("ApplicationName", DisplayName);
                cap.SetValue("ApplicationDescription", "Native desktop remote for qBittorrent");

                using var urls = cap.CreateSubKey("URLAssociations");
                urls.SetValue("magnet", MagnetProgId);

                using var files = cap.CreateSubKey("FileAssociations");
                files.SetValue(".torrent", TorrentProgId);
            }
            using (var reg = Registry.CurrentUser.CreateSubKey(RegisteredAppsPath))
                reg.SetValue(AppName, CapabilitiesPath);

            // 3. Classic protocol key: this is what makes magnet: links work
            //    when no other app has been chosen by the user yet
            using (var magnet = Registry.CurrentUser.CreateSubKey($@"{Classes}\magnet"))
            {
                magnet.SetValue("", "URL:Magnet Protocol");
                magnet.SetValue("URL Protocol", "");
                using var icon = magnet.CreateSubKey("DefaultIcon");
                icon.SetValue("", $"\"{ExePath}\",0");
                using var cmd = magnet.CreateSubKey(@"shell\open\command");
                cmd.SetValue("", Command);
            }

            NotifyShell();
        }

        public static void Unregister()
        {
            var hkcu = Registry.CurrentUser;

            hkcu.DeleteSubKeyTree($@"{Classes}\{MagnetProgId}", throwOnMissingSubKey: false);
            hkcu.DeleteSubKeyTree($@"{Classes}\{TorrentProgId}", throwOnMissingSubKey: false);
            hkcu.DeleteSubKeyTree(@"Software\qBittorrentCompanion", throwOnMissingSubKey: false);

            using (var reg = hkcu.OpenSubKey(RegisteredAppsPath, writable: true))
                reg?.DeleteValue(AppName, throwOnMissingValue: false);

            // Only remove the classic magnet key if it's ours, not another client's
            using (var cmd = hkcu.OpenSubKey($@"{Classes}\magnet\shell\open\command"))
            {
                if (cmd?.GetValue("") is string c && c.Contains(AppName, StringComparison.OrdinalIgnoreCase))
                    hkcu.DeleteSubKeyTree($@"{Classes}\magnet", throwOnMissingSubKey: false);
            }

            NotifyShell();
        }

        /// <summary>
        /// Windows 10/11 won't let apps silently become the default (UserChoice is hash protected),
        /// so after Register() the user has to confirm in Settings. This opens the right page.
        /// </summary>
        public static void OpenDefaultAppsSettings()
        {
            try
            {
                // Windows 11 jumps straight to our app's page; Windows 10 ignores the query part
                Process.Start(new ProcessStartInfo(
                    $"ms-settings:defaultapps?registeredAppUser={Uri.EscapeDataString(AppName)}")
                { UseShellExecute = true });
            }
            catch { /* not fatal */ }
        }

        static void WriteProgId(string progId, string description, bool isUrlProtocol)
        {
            using var key = Registry.CurrentUser.CreateSubKey($@"{Classes}\{progId}");
            key.SetValue("", description);
            if (isUrlProtocol) key.SetValue("URL Protocol", "");

            using var icon = key.CreateSubKey("DefaultIcon");
            icon.SetValue("", $"\"{ExePath}\",0");

            using var cmd = key.CreateSubKey(@"shell\open\command");
            cmd.SetValue("", Command);
        }

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        static void NotifyShell() =>
            SHChangeNotify(0x08000000 /* SHCNE_ASSOCCHANGED */, 0 /* SHCNF_IDLIST */, IntPtr.Zero, IntPtr.Zero);
    }
}

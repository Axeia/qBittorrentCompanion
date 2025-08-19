using Avalonia;
using Avalonia.Data;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using qBittorrentCompanion.ViewModels;
using RssPlugins;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class PluginSuccessConverter : IValueConverter
    {
        public static readonly PluginSuccessConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow?.DataContext is MainWindowViewModel mwvm
                && value is RssRulePluginBase plugin)
            {
                System.Diagnostics.Debug.WriteLine($"  Found plugin: {plugin.Name}");

                // TorrentsViewModel IS the RssPluginSupportBaseViewModel
                var rssPluginViewModel = mwvm.TorrentsViewModel;

                System.Diagnostics.Debug.WriteLine($"  RSS Plugin ViewModel: {rssPluginViewModel != null}");

                if (rssPluginViewModel != null)
                {
                    var result = rssPluginViewModel.GetPluginSuccess(plugin);
                    System.Diagnostics.Debug.WriteLine($"  Plugin '{plugin.Name}' success: {result}");
                    System.Diagnostics.Debug.WriteLine($"  Current input: '{rssPluginViewModel.PluginInput}'");
                    return result;
                }
            }

            System.Diagnostics.Debug.WriteLine("  Returning false - no valid conversion");
            return false;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
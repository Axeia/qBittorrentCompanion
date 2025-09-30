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
    /// <summary>
    /// Converts an <see cref="RssRulePluginBase"/> into a boolean indicating success,
    /// by querying the <see cref="TorrentsViewModel"/> from the main window's data context.
    /// </summary>
    /// <remarks>
    /// This converter reaches into the application lifetime to access the main view model.
    /// It assumes that <see cref="MainWindowViewModel.TorrentsViewModel"/> implements <see cref="RssPluginSupportBaseViewModel"/>
    /// and exposes a <c>GetPluginSuccess</c> method.
    /// 
    /// This converter is a bit unusual but it works where it's used at the moment.
    /// </remarks>
    public class PluginSuccessConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance for use in XAML bindings.
        /// </summary>
        public static readonly PluginSuccessConverter Instance = new();

        /// <summary>
        /// Converts an <see cref="RssRulePluginBase"/> into a success state by querying the main view model.
        /// </summary>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow?.DataContext is MainWindowViewModel mwvm
                && value is RssRulePluginBase plugin)
            {
                var rssPluginViewModel = mwvm.TorrentsViewModel;

                if (rssPluginViewModel != null)
                {
                    return rssPluginViewModel.GetPluginSuccess(plugin);
                }
            }

            return false;
        }

        /// <summary>
        /// Not supported — returns <see cref="BindingOperations.DoNothing"/>.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}

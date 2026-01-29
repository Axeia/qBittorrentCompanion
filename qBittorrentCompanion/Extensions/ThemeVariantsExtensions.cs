using Avalonia.Styling;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Extensions
{
    public static class ThemeVariantsExtensions
    {
        public static ThemeVariant ToActualThemeVariant(this ThemeVariants themeVariants)
        {
            if (themeVariants == ThemeVariants.Dark)
                return ThemeVariant.Dark;
            else if (themeVariants == ThemeVariants.Light)
                return ThemeVariant.Light;
            else
                return ThemeVariant.Default;
        }
    }
}
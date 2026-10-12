using System;
using Avalonia;
using Avalonia.Controls;
using qBittorrentCompanion.Helpers;

namespace qBittorrentCompanion.Views
{
    public class IcoWindow : Window
    {
        public WindowOverlayType Overlay { get; set; } = WindowOverlayType.None;

        public IcoWindow()
        {
            if (!Design.IsDesignMode)
            {
                this.GetObservable(ActualThemeVariantProperty).Subscribe(_ => RefreshIcon());
                this.Opened += (s, e) => RefreshIcon();
            }
        }

        // Changed to public so App can call it when the user recolors the logo
        public void RefreshIcon()
        {
            if (App.Current is App app && app.CurrentModeWindowIconBitmap != null)
            {
                this.Icon = IconManager.GetIcon(this.Overlay, ActualThemeVariant, app.CurrentModeWindowIconBitmap);
            }
        }
    }
}

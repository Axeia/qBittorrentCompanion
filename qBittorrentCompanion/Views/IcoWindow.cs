using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;

namespace qBittorrentCompanion.Views
{
    public class IcoWindow : Window
    {
        public IcoWindow()
        {
            if(!Design.IsDesignMode)
            { 
                // Update window icon if the dark/light mode icons are updated
                this.Bind(IconProperty, new Binding
                {
                    Source = App.Current,
                    Path = nameof(App.DarkModeWindowIcon),
                    Mode = BindingMode.OneWay
                });
                this.Bind(IconProperty, new Binding
                {
                    Source = App.Current,
                    Path = nameof(App.LightModeWindowIcon),
                    Mode = BindingMode.OneWay
                });

                this.Icon = App.Current?.CurrentModeWindowIcon;

                // Update window icon if the theme is changed
                ActualThemeVariantChanged += (_, args) =>
                {
                    if (App.Current is App app)
                    {
                        app.CurrentModeWindowIcon = ActualThemeVariant == ThemeVariant.Dark
                            ? app.DarkModeWindowIcon
                            : app.LightModeWindowIcon;
                    }
                };
            }
        }
    }
}

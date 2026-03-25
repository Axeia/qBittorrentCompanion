using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;

namespace qBittorrentCompanion.Views
{
    public class IcoWindow : Window
    {
        public IcoWindow()
        {
            if (!Design.IsDesignMode)
            {
                this.Bind(IconProperty, new Binding
                {
                    Source = App.Current,
                    Path = nameof(App.CurrentModeWindowIcon),
                    Mode = BindingMode.OneWay
                });
            }
        }
    }
}

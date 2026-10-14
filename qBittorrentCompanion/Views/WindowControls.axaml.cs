using Avalonia.Controls;
using Avalonia.Interactivity;

namespace qBittorrentCompanion.Views
{
    public partial class WindowControls : StackPanel
    {
        public WindowControls()
        {
            InitializeComponent();
        }

        private Window? Host => TopLevel.GetTopLevel(this) as Window;

        private void Minimize_Click(object? sender, RoutedEventArgs e)
        {
            if (Host is { } w)
                w.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object? sender, RoutedEventArgs e)
        {
            if (Host is { } w)
                w.WindowState = w.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }

        private void Close_Click(object? sender, RoutedEventArgs e) 
            => Host?.Close();
    }
}

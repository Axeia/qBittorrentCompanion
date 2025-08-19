using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.Views;

namespace qBittorrentCompanion
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                var args = desktop.Args ?? [];
                foreach(var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
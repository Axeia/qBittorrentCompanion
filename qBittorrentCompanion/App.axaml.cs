using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using qBittorrentCompanion.Converters;
using qBittorrentCompanion.ViewModels;
using qBittorrentCompanion.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;

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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                var args = desktop.Args ?? [];
                foreach(var arg in args)
                    ((MainWindow)desktop.MainWindow).AddToFileQueue(arg);

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
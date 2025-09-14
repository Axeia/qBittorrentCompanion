using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class LogoControl : Viewbox
    {
        public LogoControl()
        {
            InitializeComponent();

            try
            {
                var xamlUri = new Uri("avares://qBittorrentCompanion/Assets/Logo.axaml");
                var logoCanvasContent = (Canvas)AvaloniaXamlLoader.Load(xamlUri);

                Child = logoCanvasContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Canvas content: {ex.Message}");
            }

            Observable.FromEventPattern<EventHandler, EventArgs>(
                h => ActualThemeVariantChanged += h,
                h => ActualThemeVariantChanged -= h)
                .Select(_ => Application.Current?.ActualThemeVariant)
                .DistinctUntilChanged()
                // Was triggered 3 times at once on my system, debounce.
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(theme =>
                {
                    if( this.DataContext is LocalSettingsWindowViewModel lswvm)
                    {
                        lswvm.IsInDarkMode = theme == ThemeVariant.Dark;
                    }
                });
        }
    }
}
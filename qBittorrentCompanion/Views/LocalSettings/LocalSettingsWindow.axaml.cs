using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Allow setting of the (likely networked) directory the downloads are saved to
    /// </summary>
    public partial class LocalSettingsWindow : IcoWindow
    {
        private LocalSettingsWindowViewModel _localSettingsWindowViewModel;

        public LocalSettingsWindow()
        {
            InitializeComponent();
            //Closing += DownloadDirectoryWindow_Closing;
            //Loaded += DownloadDirectoryWindow_Loaded;

            //LoadColorsFromConfig(); // Might undo previous step
            //MatchColorPickersToCanvas();
            _localSettingsWindowViewModel = new LocalSettingsWindowViewModel();
            this.DataContext = _localSettingsWindowViewModel;
            Loaded += LocalSettingsWindow_Loaded;
            //PreviewBgColorPicker.Color = Avalonia.Media.Colors.Transparent;
        }

        private void LocalSettingsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            var canvas = TabStripBackgroundCanvas;
            var svgs = canvas.Children.OfType<Avalonia.Svg.Skia.Svg>();
            var random = new Random();

            foreach (var svg in svgs)
            {
                if (svg.Tag is string tagValue && int.TryParse(tagValue, out int delaySeconds))
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(delaySeconds * 1000);

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            svg.Classes.Add("SpinLogo");

                            // Ensure the Svg has a TranslateTransform
                            if (svg.RenderTransform is not TranslateTransform translate)
                            {
                                translate = new TranslateTransform();
                                svg.RenderTransform = translate;
                            }

                            // Random sway amplitude and duration
                            var swayAmplitude = random.Next(4, 7); // e.g. 4–10 px
                            var swayDuration = TimeSpan.FromMilliseconds(random.Next(800, 2000)); // e.g. 0.8–1.6 sec

                            var baseLeft = svg.GetValue(Canvas.LeftProperty);
                            // Create horizontal sway animation
                            var swayAnimation = new Animation
                            {
                                Duration = swayDuration,
                                IterationCount = IterationCount.Infinite,
                                Easing = new LinearEasing(), // Let keyframes shape the curve
                                Children =
                                {
                                    new KeyFrame { Cue = Cue.Parse("0%", null), Setters = { new Setter(Canvas.LeftProperty, baseLeft) } },
                                    new KeyFrame { Cue = Cue.Parse("25%", null), Setters = { new Setter(Canvas.LeftProperty, baseLeft + swayAmplitude) } },
                                    new KeyFrame { Cue = Cue.Parse("50%", null), Setters = { new Setter(Canvas.LeftProperty, baseLeft) } },
                                    new KeyFrame { Cue = Cue.Parse("75%", null), Setters = { new Setter(Canvas.LeftProperty, baseLeft - swayAmplitude) } },
                                    new KeyFrame { Cue = Cue.Parse("100%", null), Setters = { new Setter(Canvas.LeftProperty, baseLeft) } },
                                }
                            };
                            swayAnimation.RunAsync(svg);
                        });
                    });
                }
            }
        }


    }
}

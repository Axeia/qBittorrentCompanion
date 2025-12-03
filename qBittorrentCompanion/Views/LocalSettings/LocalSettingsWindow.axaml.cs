using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Diagnostics;
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
            Closing += LocalSettingsWindow_Closing;
            
            _localSettingsWindowViewModel = new LocalSettingsWindowViewModel();
            this.DataContext = _localSettingsWindowViewModel;
            Loaded += LocalSettingsWindow_Loaded;
        }

        private async void LocalSettingsWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (mdv.DataContext is MonitorDirectoriesViewModel mdvm && mdvm.HasUnsavedChanges)
            {
                // Cancel the close until the user decides
                e.Cancel = true;

                var result = await ShowUnsavedChangesDialog();

                switch (result)
                {
                    case ButtonResult.Yes:
                        mdvm.SaveAndApply();
                        Closing -= LocalSettingsWindow_Closing;
                        Close(); 
                        break;

                    case ButtonResult.No:
                        Closing -= LocalSettingsWindow_Closing;
                        Close();
                        break;

                    case ButtonResult.Cancel:
                        // do nothing, keep window open
                        break;
                }
            }
        }

        private async Task<ButtonResult> ShowUnsavedChangesDialog()
        {
            // Example using MessageBox.Avalonia (community package)
            var messageBoxStandardWindow = MessageBoxManager
                .GetMessageBoxStandard("Unsaved Changes",
                    "There's some changes that haven't been saved, Would you like to do so? \n\nYes - Saves and applies changes. \nNo - ignores changes without saving them. \nCancel - Closes this dialog so you can review or save the changes manually",
                    ButtonEnum.YesNoCancel);

            var result = await messageBoxStandardWindow.ShowWindowDialogAsync(this);

            return result;
        }

        private void LocalSettingsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            /// Add animation for background logos so they spin and move upwards (float up)
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

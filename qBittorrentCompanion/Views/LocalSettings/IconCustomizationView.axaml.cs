
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views.LocalSettings
{

    public partial class IconCustomizationView : ThemeVariantScope
    {
        private bool _hotkeysBound = false;

        public IconCustomizationView()
        {
            InitializeComponent();
            bool isCurrentlyInDarkMode = IsCurrentlyInDarkMode();
            PreviewSwitcher.PageTransition = new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal);

            DataContext = new IconCustomizationViewModel(
                isCurrentlyInDarkMode,
                isCurrentlyInDarkMode ? LogoColorsRecord.DarkModeDefault : LogoColorsRecord.LightModeDefault
            );

            AddPreviewSwitcherClickActions();
            AttachedToVisualTree += IconCustomizationView_AttachedToVisualTree;
        }

        private void IconCustomizationView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (DataContext is IconCustomizationViewModel icvm 
                && this.GetVisualAncestors().OfType<Window>().FirstOrDefault() is Window window)
            {
                if (_hotkeysBound == false)
                {
                    SetKeyBindings(icvm, window);
                    _hotkeysBound = true;
                }
            }
        }

        private void AddPreviewSwitcherClickActions()
        {
            if (Resources["DarkIconCustomizationPreview"] is IconCustomizationPreview dicp
            && Resources["LightIconCustomizationPreview"] is IconCustomizationPreview licp)
            {
                PreviewSwitcher.Content = dicp;

                dicp.SwitchToDarkModeButton.Click += SwitchPreviewModeButton_Click;
                dicp.SwitchToLightModeButton.Click += SwitchPreviewModeButton_Click;
                licp.SwitchToDarkModeButton.Click += SwitchPreviewModeButton_Click;
                licp.SwitchToLightModeButton.Click += SwitchPreviewModeButton_Click;
            }
        }

        private static void SetKeyBindings(IconCustomizationViewModel icvm, Window window)
        {
            Debug.WriteLine("Adding keybindings");
            var undoKeyBinding = new KeyBinding
            {
                Gesture = KeyGesture.Parse("Ctrl+Z"),
                Command = icvm.UndoCommand
            };
            window.KeyBindings.Add(undoKeyBinding);

            var redoKeyBinding = new KeyBinding
            {
                Gesture = KeyGesture.Parse("Ctrl+Y"),
                Command = icvm.RedoCommand
            };
            window.KeyBindings.Add(redoKeyBinding);
        }

        private void SwitchPreviewModeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["DarkIconCustomizationPreview"] is IconCustomizationPreview dicp
            && Resources["LightIconCustomizationPreview"] is IconCustomizationPreview licp)
            {
                if (sender == dicp.SwitchToDarkModeButton || sender == licp.SwitchToDarkModeButton)
                    PreviewSwitcher.Content = dicp;
                else
                    PreviewSwitcher.Content = licp;
            }
        }
        public bool IsCurrentlyInDarkMode()
        {
            var resolvedTheme = this
                .GetSelfAndVisualAncestors()
                .OfType<Control>()
                .Select(e => e.ActualThemeVariant)
                .FirstOrDefault(tv => tv != ThemeVariant.Default);

            // Fallback to application theme if none found
            resolvedTheme ??= Application.Current?.ActualThemeVariant;

            return resolvedTheme == ThemeVariant.Dark;
        }
    }
}
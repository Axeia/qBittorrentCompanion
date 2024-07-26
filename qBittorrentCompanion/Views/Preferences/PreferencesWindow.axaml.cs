using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            var prefVm = new PreferencesWindowViewModel();
            prefVm.SelectedTabItem = (TabItem)PreferencesTabControl.SelectedItem!;
            DataContext = prefVm;
        }

        private void SavePreferences_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Save logic here
        }

        private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            var searchTerm = SearchTextBox.Text!;
            Debug.WriteLine($"{searchTerm} «");
            HighlightControls(PreferencesTabControl, searchTerm);
        }

        private void HighlightControls(Control control, string searchTerm)
        {
            // Reset highlight for all controls first
            ResetHighlight(control);

            // Highlight matching controls and count matches for each tab
            foreach (var tabItem in PreferencesTabControl.Items.OfType<TabItem>())
            {
                if (tabItem.Content is Control tabContent)
                {
                    int matchCount = 0;
                    HighlightControlsRecursive(tabContent, searchTerm, ref matchCount);

                    // Update badge text for the current tab
                    if (tabItem.Header is DockPanel headerPanel)
                    {
                        var border = headerPanel.Children.OfType<Border>().FirstOrDefault();
                        if (border?.Child is TextBlock badgeTextBlock)
                        {
                            badgeTextBlock.Text = matchCount.ToString();
                        }
                        border!.IsVisible = matchCount > 0;
                    }
                }
            }
        }

        private void HighlightControlsRecursive(Control control, string searchTerm, ref int matchCount)
        {
            if (control == null) return;

            foreach (var child in control.GetLogicalChildren())
            {
                if (child is Control childControl)
                {
                    if (childControl is TextBlock textBlock && textBlock.Text != null &&
                        textBlock.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        textBlock.Background = Brushes.Yellow;
                        matchCount++;
                    }
                    else if (childControl is Panel || childControl is Decorator || childControl is ContentControl)
                    {
                        HighlightControlsRecursive(childControl, searchTerm, ref matchCount); // Recurse into child containers
                    }
                }
            }
        }

        private void ResetHighlight(Control control)
        {
            foreach (var child in control.GetLogicalChildren())
            {
                if (child is Control childControl)
                {
                    if (childControl is TextBlock textBlock)
                    {
                        textBlock.Background = Brushes.Transparent;
                    }
                    else if (childControl is Panel || childControl is Decorator || childControl is ContentControl)
                    {
                        ResetHighlight(childControl); // Recurse into child containers
                    }
                }
            }
        }
    }
}
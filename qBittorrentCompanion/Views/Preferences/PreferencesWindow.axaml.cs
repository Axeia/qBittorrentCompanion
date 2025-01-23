using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class PreferencesWindow : IcoWindow
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            var prefVm = new PreferencesWindowViewModel();
            DataContext = prefVm;
            _ = prefVm.FetchData();
            Loaded += PreferencesWindow_Loaded;

            PreferencesTabControl.SelectionChanged += PreferencesTabControl_SelectionChanged;
        }

        private void PreferencesTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateTitleTextBlock();
        }

        private void UpdateTitleTextBlock()
        {
            if (PreferencesTabControl.SelectedItem is TabItem selectedTabItem)
            {
                var headerTextBlock = ((DockPanel)selectedTabItem.Header!).Children.OfType<TextBlock>().FirstOrDefault();
                if (headerTextBlock != null)
                {
                    TitleTextBlock.Text = headerTextBlock.Text;
                }
            }
        }

        private void PreferencesWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Switches tabs to the fields initialise properly.
            if (PreferencesTabControl.Items.Count > 1)
            {
                PreferencesTabControl.SelectedIndex = 1;
                PreferencesTabControl.SelectedIndex = 0;
            }

            UpdateTitleTextBlock();
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
            // Reset highlight and visibility for all controls first
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

        private bool HighlightControlsRecursive(Control control, string searchTerm, ref int matchCount)
        {
            if (control == null) return false;

            bool sectionHasMatch = false;
            var accentColor = (Color)this.FindResource("SystemAccentColorDark2");
            var accentBrush = new SolidColorBrush(accentColor);

            foreach (var child in control.GetLogicalChildren())
            {
                if (child is Control childControl)
                {
                    bool childMatch = false;

                    if (childControl is TextBlock textBlock && textBlock.Text != null &&
                        textBlock.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        textBlock.Background = accentBrush;
                        matchCount++;
                        childMatch = true;
                    }
                    else if (childControl is Panel || childControl is Decorator || childControl is ContentControl)
                    {
                        childMatch = HighlightControlsRecursive(childControl, searchTerm, ref matchCount);
                    }

                    if (childMatch)
                    {
                        sectionHasMatch = true;
                    }

                    // If the child is a section (Border or HeaderedContentControl), set its visibility based on match
                    if ((childControl is Border border) ||
                        (childControl is HeaderedContentControl hcc))
                    {
                        childControl.IsVisible = childMatch;
                    }
                }
            }

            return sectionHasMatch;
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

                    // Reset visibility for sections
                    if ((childControl is Border border) ||
                        (childControl is HeaderedContentControl hcc))
                    {
                        childControl.IsVisible = true;
                    }
                }
            }
        }

        private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
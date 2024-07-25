using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;

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
        }
        

 /*       private void SearchTextBoxBox_TextChanged(TextChangedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text!;
            HighlightControls(PreferencesTabControl, searchTerm);
        }*/

        private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            var searchTerm = SearchTextBox.Text!;
            HighlightControls(PreferencesTabControl, searchTerm);
        }

        private void HighlightControls(Control control, string searchTerm)
        {
            // Reset highlight for all controls first
            ResetHighlight(PreferencesTabControl);

            // Highlight matching controls
            foreach (var child in control.GetLogicalChildren())
            {
                if (child is Control childControl)
                {
                    if (childControl is TextBlock textBlock && textBlock.Text != null &&
                        textBlock.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        textBlock.Background = Brushes.Yellow;
                    }
                    else if (childControl is Panel panel)
                    {
                        HighlightControls(panel, searchTerm); // Recurse into child containers
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
                    else if (childControl is Panel panel)
                    {
                        ResetHighlight(panel); // Recurse into child containers
                    }
                }
            }
        }
    }
}

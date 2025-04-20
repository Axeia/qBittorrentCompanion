using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;
using System.Linq;
using Avalonia.Input;
using System;
using Avalonia.Media;
using qBittorrentCompanion.Helpers;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleView : UserControl
    {
        public RssRuleView()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
            {
                DataContext = new RssAutoDownloadingRuleViewModel(
                    new RssAutoDownloadingRule(),
                    ""
                );
            }
            Loaded += RssRuleView_Loaded;
        }

        private void RssRuleView_Loaded(object? sender, RoutedEventArgs e)
        {
        }

        private void RemoveFeedButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radRuleVm 
                && sender is Button button 
                && button.DataContext is RssFeedViewModel rfvm)
            {
                radRuleVm.SelectedFeeds.Remove(rfvm);
            }
        }

        private void RemoveTagButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRuleViewModel radRuleVm
                && sender is Button button
                && button.DataContext is string tag)
            {
                radRuleVm.SelectedTags.Remove(tag);
            }
        }

        private void SwitchToFeedsButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.MainTabStrip.SelectedIndex = 2;
        }

        private void PluginSourceTextBox_PastingFromClipboard(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    textBox.Text = textBox.Text.Trim();
                });
            }
        }

        private void AutoCompleteBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is AutoCompleteBox acb)
            {
                acb.IsDropDownOpen = true;
            }
        }

        private void MustContainTextEditor_GotFocus(object? sender, GotFocusEventArgs e)
        {
            MustContainBorder.BorderBrush = new SolidColorBrush(ThemeColors.SystemAccent);
        }

        /// <summary>
        /// Adds to Selected list when the dropdown is closed (happens when an items is selected or enter is pressed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoCompleteBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is AutoCompleteBox acb
                && DataContext is RssAutoDownloadingRuleViewModel radrvm)
            {
                var listType = radrvm.RssFeeds.GetType().GetGenericArguments().FirstOrDefault();
                if (listType == null)
                {
                    Debug.WriteLine("Unable to determine list type.");
                    return;
                }

                // Use reflection to find the matching item and cast it
                var matchingItem = radrvm.RssFeeds
                    .FirstOrDefault(item => item.ToString().Equals(acb.Text, StringComparison.OrdinalIgnoreCase));

                if (matchingItem != null && listType.IsInstanceOfType(matchingItem))
                {
                    if (!radrvm.SelectedFeeds.Contains(matchingItem))
                    {
                        radrvm.SelectedFeeds.Add(matchingItem);
                        acb.Text = ""; 
                        acb.IsDropDownOpen = true;
                    }
                }
            }
        }

        private void CategoryComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is Category q)
            {
                SavePathTextBox.Watermark = q is not null && !string.IsNullOrEmpty(q.SavePath)
                    ? q.SavePath
                    : "path/to/save/to";
            }
        }
    }
}
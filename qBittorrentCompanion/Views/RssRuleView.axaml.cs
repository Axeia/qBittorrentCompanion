using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using qBittorrentCompanion.AvaloniaEditor;
using System.ComponentModel;
using System;

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
                    "",
                    new List<string>().AsReadOnly()
                );
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
    }
}
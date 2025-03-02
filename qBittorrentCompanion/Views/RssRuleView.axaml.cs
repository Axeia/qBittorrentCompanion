using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            RuleDefinitionScrollViewer.SizeChanged += RuleDefinitionScrollViewer_SizeChanged;
        }

        private void RuleDefinitionScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("Change afoot");
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
                // Delay the execution to allow the pasted text to be set
                Dispatcher.UIThread.Post(() =>
                {
                    textBox.Text = textBox.Text.Trim();
                });
            }
        }
    }
}

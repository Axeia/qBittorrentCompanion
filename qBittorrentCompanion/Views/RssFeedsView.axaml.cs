using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using RssPlugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;


namespace qBittorrentCompanion.Views
{
    public partial class RssFeedsView : UserControl
    {
        private RssPluginBase? rssPlugin;

        public RssFeedsView()
        {
            InitializeComponent();
            DataContext = new RssFeedsViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssFeedsView_Loaded;
        }

        private void RssFeedsView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssFeedsViewModel rssViewModel && !Design.IsDesignMode)
                rssViewModel.Initialise();

            CreateRuleButton.GenerateRssRuleSplitButton.Click += GenerateRssRuleSplitButton_Click;
        }

        private RssFeedViewModel? _preEditRssFeedViewModel = null;

        /// <summary>
        /// Stores the ViewModel before it gets altered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RssFeedsDataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            var row = e.Row as DataGridRow;

            if (row.DataContext is RssFeedViewModel rssViewModel)
            {
                _preEditRssFeedViewModel = rssViewModel.GetCopy();
            }
        }

        /// <summary>
        /// Renames the ViewModel, its IsLoading will be true whilst this takes place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RssFeedsDataGrid_RowEditEnding(object sender, DataGridRowEditEndedEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var row = e.Row as DataGridRow;

                if (row.DataContext is RssFeedViewModel rssViewModel
                && _preEditRssFeedViewModel!.Name != rssViewModel.Name)
                {
                    _ = _preEditRssFeedViewModel!.Rename(rssViewModel.Name);
                }
            }
        }

        private void RenameFeedMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is RssFeedViewModel rssFeedVm)
            {
                // Find the DataGridRow
                var dataGridRow = RssFeedsDataGrid.ItemsSource.OfType<object>()
                    .Select((item, index) => new { item, index })
                    .FirstOrDefault(x => x.item == rssFeedVm);

                if (dataGridRow != null)
                {
                    RssFeedsDataGrid.SelectedItem = rssFeedVm;

                    var column = RssFeedsDataGrid.Columns[1]; // 1 for the second column
                    if (column != null)
                        RssFeedsDataGrid.BeginEdit();

                    var textBox = RssFeedsDataGrid
                        .GetVisualDescendants()
                        .OfType<TextBox>();
                    if (textBox is TextBox tb)
                    {
                        tb.Focus();
                        tb.SelectAll();
                    }
                }
            }
        }

        private void CopyRssFeedUrlMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssFeedsViewModel rssFeedsVm)
                TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(rssFeedsVm.SelectedFeed!.Url.ToString());
        }

        private void FeedsDockPanel_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            // There's probably some way to calculate why this is value should be what it is.
            // But just eyeballing and trial and error was faster so this static value it is.
            var marginRightOffset = e.NewSize.Width - 124;
            if (e.WidthChanged)
            {
                RssFeedsLeftHandControlsStackPanel.Margin = new Avalonia.Thickness(
                    0, 0, marginRightOffset, 0
                );
            }
        }

        private void ClearRssFeedsBindings()
        {
            if (DataContext is RssFeedsViewModel rssFeedsViewModel)
            {
                _deleteSelectedFeedDisposable?.Dispose();
                _addNewRssFeedDisposable?.Dispose();
            }
        }

        private IDisposable? _deleteSelectedFeedDisposable = null;
        private IDisposable? _addNewRssFeedDisposable = null;
        private void SetRssFeedsBindings()
        {
            Debug.WriteLine("SetRssFeedsBindings");
            if (DataContext is RssFeedsViewModel rssFeedsViewModel)
            {
                Debug.WriteLine("Do's should work");
                RssFeedsControlsDockPanel.DataContext = rssFeedsViewModel;
                _deleteSelectedFeedDisposable = rssFeedsViewModel.DeleteSelectedFeedCommand.Do(_ => {
                    Debug.WriteLine("Closing delete");
                    DeleteSelectedFeedButton.Flyout!.Hide();
                }).Subscribe();
                _addNewRssFeedDisposable = rssFeedsViewModel.AddNewFeedCommand.Do(_ =>
                {
                    Debug.WriteLine("Resetting & closing add feed");
                    RssFeedUrlTextBox.Text = string.Empty;
                    RssFeedLabelTextBox.Text = string.Empty;
                    AddRssFeedButton.Flyout!.Hide();
                })
                .Subscribe();
            }
        }


        private void ClearNewRssFeedFlyoutInputs()
        {
            RssFeedUrlTextBox.Text = string.Empty;
            RssFeedLabelTextBox.Text = string.Empty;
        }

        private void GenerateRssRuleSplitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssFeedsViewModel rssFeedsViewModel)
            {
                var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
                mainWindow.MainTabStrip.SelectedIndex = 3;

                Dispatcher.UIThread.Post(() =>
                {
                    mainWindow.RssRulesView.AddNewRule(
                        rssFeedsViewModel.PluginRuleTitle,
                        rssFeedsViewModel.PluginResult,
                        [rssFeedsViewModel.SelectedFeed!.Url]
                    );
                }, DispatcherPriority.Background);
            }
        }

        private void LaunchArticleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button 
                && ToolTip.GetTip(button) is string url
                && TopLevel.GetTopLevel(this)!.Launcher is ILauncher launcher)
            {
                _ = launcher.LaunchUriAsync(new Uri(url));
            }
        }
    }
}

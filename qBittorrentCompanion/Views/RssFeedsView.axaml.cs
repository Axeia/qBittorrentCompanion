using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
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
    public partial class RssFeedsView : RssRulePluginUserControl
    {
        public RssFeedsView()
        {
            InitializeComponent();
            DataContext = new RssFeedsViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssFeedsView_Loaded;
        }

        private void RssFeedsView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssFeedsViewModel rssFeedsViewModel && !Design.IsDesignMode)
            {
                Debug.WriteLine("Adding change listener");
                /// <summary>
                /// Keeps <see cref="RssRulePluginUserControl.SuggestedFeeds"/> updated so the right feed will be selected on
                /// the Rss Rule tab once the SplitButton in the <see cref="RssPluginButtonView"/> is clicked 
                /// </summary>
                rssFeedsViewModel.WhenAnyValue(r => r.SelectedFeed)
                    .Subscribe(newValue => {
                        SuggestedFeeds.Clear();
                        if (newValue != null)
                            SuggestedFeeds.Add(newValue.Url);
                    });
                rssFeedsViewModel.Initialise();
            }

            CreateRuleButton.GenerateRssRuleSplitButton.Click += GenerateRssRuleSplitButton_Click;
            ShowHideExpanderSplitter();
        }


        private void ShowHideExpanderSplitter()
        {
            var noneExpanded = !RssArticleExpander.IsExpanded && !RssPluginExpander.IsExpanded;
            MainVSplitter.IsVisible = !noneExpanded;

            if (noneExpanded)
            {
                RssArticlesGrid.RowDefinitions[2].Height = new GridLength(76);
                RssArticlesGrid.RowDefinitions[2].MinHeight = 76;
            }
            else
            {
                RssArticlesGrid.RowDefinitions[2].Height = new GridLength(300);
                RssArticlesGrid.RowDefinitions[2].MinHeight = 
                    RssArticleExpander.IsExpanded && RssPluginExpander.IsExpanded
                        ? 240 : 120;
            }

            var articleRow = SelectedArticleInfoGrid.RowDefinitions[0];
            if (RssArticleExpander.IsExpanded)
            {
                RssArticleExpander.MaxHeight = double.PositiveInfinity;
                articleRow.Height = GridLength.Star;
            }
            else
            {
                RssArticleExpander.MaxHeight = 36;
                articleRow.Height = GridLength.Auto;
            }

            var pluginRow = SelectedArticleInfoGrid.RowDefinitions[2];
            if (RssPluginExpander.IsExpanded)
            {
                RssPluginExpander.MaxHeight = double.PositiveInfinity;
                pluginRow.Height = GridLength.Star;
            }
            else
            {
                RssPluginExpander.MaxHeight = 36;
                pluginRow.Height = GridLength.Auto;
            }
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

        private void ClearNewRssFeedFlyoutInputs()
        {
            RssFeedUrlTextBox.Text = string.Empty;
            RssFeedLabelTextBox.Text = string.Empty;
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

        private void Expander_Expanded(object? sender, RoutedEventArgs e)
        {
            ShowHideExpanderSplitter();
        }

        private void Expander_Collapsed(object? sender, RoutedEventArgs e)
        {
            ShowHideExpanderSplitter();
        }

        private void RssArticlesDataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (e.Source is Control control
                && control.DataContext is RssArticle rssArticle
                && this.FindAncestorOfType<MainWindow>() is MainWindow mainWindow)
            {
                mainWindow.AddTorrent(rssArticle.TorrentUri, rssArticle.Title);
            }
        }
    }
}

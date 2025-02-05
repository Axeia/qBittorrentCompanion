using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.RssPlugins;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;


namespace qBittorrentCompanion.Views
{
    public partial class RssFeedsView : UserControl
    {
        private IRssPlugin? rssPlugin;

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

            //TODO Should be based on the splitbutton dropdown value
            rssPlugin = new AnimeEpisodeRssPlugin("");
            SetPluginFields();
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
                    if(textBox is TextBox tb)
                    {
                        tb.Focus();
                        tb.SelectAll();
                    }
                }
            }
        }

        private void CopyRssFeedUrlMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if(DataContext is RssFeedsViewModel rssFeedsVm)
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

        private void RssTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //FIXME 
            //if (sender is Carousel carousel)
            //    RssRulesControlsDockPanel.IsVisible = carousel.SelectedIndex == 1;
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
                AnimeEpisodeRssPlugin aerp = new(rssFeedsViewModel.SelectedArticle!.Title);
                Debug.WriteLine(aerp.Result);
            }
        }

        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (RssArticlesDataGrid.SelectedItem is RssArticle rssArticle)
            {
                rssPlugin = new AnimeEpisodeRssPlugin(rssArticle.Title);
                var mainButton = GenerateRssRuleSplitButton
                    .GetVisualDescendants()
                    .OfType<Button>()
                    .First(btn => btn.Name == "PART_PrimaryButton");

                mainButton.IsEnabled = rssPlugin.IsSuccess;
                PluginPreviewGrid.IsVisible = rssPlugin.IsSuccess;
                RuleNotValidTextBlock.IsVisible = !rssPlugin.IsSuccess;
                RuleNotValidTextBlock.Text = rssPlugin.IsSuccess
                    ? "Placeholder"
                    : "No [anime] rule could be created from the selected entry";


                SetPluginFields();
            }
            else
            {
                RuleNotValidTextBlock.IsVisible = false;
            }
        }

        private void SetPluginFields()
        {
            if (rssPlugin == null)
                return;

            RuleTitleTextBlock.Text = rssPlugin.RuleTitle.ToString();
            RegexTextBlock.Text = rssPlugin.Result;
            LongDescriptionSimpleHtmlTextBlock.Text = rssPlugin.Description;
        }
    }
}

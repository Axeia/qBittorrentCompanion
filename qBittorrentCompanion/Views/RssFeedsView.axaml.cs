using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace qBittorrentCompanion.Views
{
    public partial class RssFeedsView : UserControl
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
            if (DataContext is RssFeedsViewModel rssViewModel && !Design.IsDesignMode)
                rssViewModel.Initialise();
        }

        private void AddFeedActionButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = AddRssFeedAndRefresh();
        }

        private async Task AddRssFeedAndRefresh()
        {
            if (RssFeedUrlTextBox.Text is not null)
            {
                try
                {
                    await QBittorrentService.QBittorrentClient.AddRssFeedAsync(
                        new Uri(RssFeedUrlTextBox.Text!),
                        RssFeedLabelTextBox.Text ?? RssFeedUrlTextBox.Text
                    );
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
                finally
                {
                    if (DataContext is RssFeedsViewModel rssViewModel)
                        rssViewModel.Initialise();

                    RssFeedUrlTextBox.Text = "";
                    RssFeedLabelTextBox.Text = "";
                    AddFeedButton.Flyout!.Hide();
                }
            }
        }

        private void RemoveFeedActionButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = RemoveRssFeedAndRefresh();
        }

        private async Task RemoveRssFeedAndRefresh()
        {
            if (DataContext is RssFeedsViewModel rssViewModel)
            {
                await QBittorrentService.QBittorrentClient.DeleteRssItemAsync(
                    rssViewModel.SelectedFeed!.Name
                );
                RemoveFeedButton.Flyout!.Hide();

                rssViewModel.Initialise();
            }
        }

        private void RefreshAllButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssFeedsViewModel rssViewModel)
            {
                rssViewModel.ForceUpdate();
            }
        }

        private void MarkFeedAsReadButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = MarkFeedAsRead();
        }

        private async Task MarkFeedAsRead()
        {

            if (RssFeedsDataGrid.SelectedItem is RssFeedViewModel selectedFeed)
            {
                await QBittorrentService.QBittorrentClient.MarkRssItemAsReadAsync(
                    selectedFeed.Name
                );
            }

            if (DataContext is RssFeedsViewModel rssViewModel)
            {
                rssViewModel.ForceUpdate();
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
    }
}

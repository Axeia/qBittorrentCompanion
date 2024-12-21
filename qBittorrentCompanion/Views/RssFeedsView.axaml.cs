using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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
    }
}

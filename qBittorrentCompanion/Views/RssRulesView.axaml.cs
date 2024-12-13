using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData.Kernel;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class RssRulesView : UserControl
    {
        public RssRulesView()
        {
            InitializeComponent();
            DataContext = new RssAutoDownloadingRulesViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssRulesView_Loaded;
        }

        private void RssRulesView_Loaded(object? sender, RoutedEventArgs e)
        {
            if(DataContext is RssAutoDownloadingRulesViewModel rssRules)
                rssRules.Initialise(); // Fetches data from the QBittorrent WebUI.
        }

        /// <summary>
        /// If the selected RssRule changes mark the RssFeeds associated with it as selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RssRulesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach(var removedItem in e.RemovedItems)
            {
                if (removedItem is RssAutoDownloadingRuleViewModel rssRule)
                {
                    rssRule.PropertyChanged -= RssRule_PropertyChanged;
                }
            }

            foreach (var addedItem in e.AddedItems)
            {
                if (addedItem is RssAutoDownloadingRuleViewModel rssRule)
                {
                    rssRule.PropertyChanged += RssRule_PropertyChanged;
                }
            }
        }

        private void RssRule_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.MustContain))
            {

            }
            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.MustNotContain))
            {

            }
            if (e.PropertyName == nameof(RssAutoDownloadingRuleViewModel.EpisodeFilter))
            {

            }
        }

        private void DataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm
                && rssRulesVm.SelectedRssRule is RssAutoDownloadingRuleViewModel rssRuleVm)
            {
                var lines = rssRulesVm.Rows.Select(r => r.MatchTest);
                RssRuleTestDataService.SetValue(rssRuleVm.Title, lines.ToList());
            }
        }
    }
}

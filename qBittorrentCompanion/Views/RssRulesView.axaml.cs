using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class RssRulesView : UserControl
    {
        public enum RssRulesLayout
        {
            DoubleColumn,
            TripleColumn
        }

        public RssRulesView()
        {
            InitializeComponent();
            DataContext = new RssAutoDownloadingRulesViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssRulesView_Loaded;
        }

        private void RssRules_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RssRulesDataGrid.InvalidateMeasure();
            RssRulesDataGrid.InvalidateArrange();
        }

        private void RssRulesView_Loaded(object? sender, RoutedEventArgs e)
        {
            if(DataContext is RssAutoDownloadingRulesViewModel rssRules)
                rssRules.Initialise(); // Fetches data from the QBittorrent WebUI.


            if (DataContext is RssAutoDownloadingRulesViewModel rulesViewModel)
            {
                rulesViewModel.RssRules.CollectionChanged += RssRules_CollectionChanged;
                rulesViewModel.PropertyChanged += RulesViewModel_PropertyChanged;
            }

            if (!Design.IsDesignMode)
            {
                SetLayOut(RssRulesLayout.DoubleColumn);
            }
        }

        private void RulesViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e is PropertyChangedEventArgs pcea && DataContext is RssAutoDownloadingRulesViewModel rulesViewModel)
                if (pcea.PropertyName == nameof(RssAutoDownloadingRulesViewModel.SelectedRssRule))
                    RssRulesDataGrid.ScrollIntoView(rulesViewModel.SelectedRssRule, RssRulesDataGrid.Columns[0]);
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

        private void AddRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRules)
                if (sender == AddRuleButton)
                    rssRules.AddRule(AddRuleTextBox.Text!);
                else
                    Debug.WriteLine($"Unexpected call to RssRulesView.axaml.cs»AddRuleButton_Click from {sender}");
        }

        private void DeleteRulesButton_Click(object? sender, RoutedEventArgs e)
        {
            List<RssAutoDownloadingRuleViewModel> rulesToDelete = [];
            if (DataContext is RssAutoDownloadingRulesViewModel rssRules)
            {
                foreach (RssAutoDownloadingRuleViewModel rule in RssRulesDataGrid.SelectedItems)
                {
                    rulesToDelete.Add(rule);
                }
                rssRules.DeleteRules(rulesToDelete);
            }
        }

        GridLength oldWidth = GridLength.Parse("15*");
        double oldMinWidth = 280;
        GridLength oldGridSplitterWidth = GridLength.Parse("8");
        public void SetLayOut(RssRulesLayout rssRulesLayout)
        {
            if (rssRulesLayout == RssRulesLayout.DoubleColumn)
            {
                //oldWidth = RssViewMainGrid.ColumnDefinitions[0].Width;
                //oldMinWidth = RssViewMainGrid.ColumnDefinitions[0].MinWidth;
                //oldGridSplitterWidth = RssViewMainGrid.ColumnDefinitions[1].Width;

                LeftColumnBorder.IsVisible = false;
                LeftGridSplitter.IsVisible = false;
                RssViewMainGrid.ColumnDefinitions[0].Width = GridLength.Parse("0");
                RssViewMainGrid.ColumnDefinitions[0].MinWidth = 0.0;
                RssViewMainGrid.ColumnDefinitions[1].Width = GridLength.Parse("0");
            }
            else
            {
                LeftColumnBorder.IsVisible = true;
                LeftGridSplitter.IsVisible = true;

                RssViewMainGrid.ColumnDefinitions[0].Width = oldWidth;
                RssViewMainGrid.ColumnDefinitions[0].MinWidth = oldMinWidth;
                RssViewMainGrid.ColumnDefinitions[1].Width = oldGridSplitterWidth;
            }
        }
    }
}

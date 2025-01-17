using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using DynamicData.Kernel;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
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

        private void DataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm
                && rssRulesVm.SelectedRssRule is RssAutoDownloadingRuleViewModel rssRuleVm)
            {
                var lines = rssRulesVm.SelectedRssRule.Rows.Select(r => r.MatchTest);
                RssRuleTestDataService.SetValue(rssRuleVm.Title, lines.ToList());
            }
        }

        private void AddRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.AddRule(AddRuleTextBox.Text!);
            }
        }

        /*
        private void DeleteRssRulesButton_Click(object? sender, RoutedEventArgs e)
        { 
            var rssDlRules = RssRulesDataGrid.SelectedItems.OfType<RssAutoDownloadingRuleViewModel>();
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.DeleteRules(rssDlRules);
            }
            //ExpandRssRulesButton.IsChecked = false;
        }*/

        private void ToggleButtonExpandedControls_Checked(object? sender, RoutedEventArgs e)
        {
            var firstRow = SideBarGrid.RowDefinitions.First();
            firstRow.Height = GridLength.Parse("*");
            firstRow.MinHeight = 120;
        }
        private void ToggleButtonExpandedControls_Unchecked(object? sender, RoutedEventArgs e)
        {
            var firstRow = SideBarGrid.RowDefinitions.First();
            firstRow.Height = GridLength.Parse("38");
            firstRow.MinHeight = 0;
        }

        private void TestDataToggleSwitch_Checked(object? sender, RoutedEventArgs e)
        {
            var lastRow = TestGrid.RowDefinitions.Last();
            lastRow.Height = GridLength.Parse("200");
            lastRow.MinHeight = 100;
        }

        private void TestDataToggleSwitch_Unchecked(object? sender, RoutedEventArgs e)
        {
            var lastRow = TestGrid.RowDefinitions.Last();
            lastRow.Height = GridLength.Parse("32");
            lastRow.MinHeight = 0;
        }
    }
}
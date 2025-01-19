using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DynamicData;
using DynamicData.Kernel;
using QBittorrent.Client;
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
            firstRow.Height = GridLength.Parse("40");
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

        private void RssRulesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                radrvm.SelectedRssRules = RssRulesDataGrid.SelectedItems
                    .Cast<RssAutoDownloadingRuleViewModel>()
                    .ToList();
            }
        }

        private string oldTitle = string.Empty;

        /// <summary>
        /// Stores the ViewModel before it gets altered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RssRulesDataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                Debug.WriteLine($"BeginningEdit: oldTitle set to '{radrvm.SelectedRssRule.Title}'");
                oldTitle = radrvm.SelectedRssRule.Title;
            }
        }

        /// <summary>
        /// Renames the ViewModel, its IsSaving will be true whilst this takes place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RssRulesDataGrid_RowEditEnded(object sender, DataGridRowEditEndedEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                RssRulesDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                if (e.Row is DataGridRow dgr 
                    && dgr.DataContext is RssAutoDownloadingRuleViewModel radrvm)
                {

                    if (radrvm.Title != oldTitle)
                    {
                        _ = radrvm.RenameAsync(oldTitle);
                        oldTitle = string.Empty;
                    }
                    //else
                    //    Debug.WriteLine($"No rename required. oldTitle: '{oldTitle}', newTitle: '{radrvm.Title}'");
                }
            }
            //else
            //    Debug.WriteLine("Edit cancelled or incomplete");
        }

        private void RenameButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                var selectedRow = RssRulesDataGrid
                    .GetVisualDescendants()
                    .Where(v => v is DataGridRow && v.DataContext == rssRulesVm.SelectedRssRule)
                    .First();

                if (selectedRow != null)
                {
                    // Set focus on the title column before triggering edit
                    RssRulesDataGrid.CurrentColumn = RssRulesDataGrid.Columns[0];
                    RssRulesDataGrid.BeginEdit();
                    Debug.WriteLine("Edit mode triggered");

                    var textBox = RssRulesDataGrid
                        .GetVisualChildren()
                        .OfType<TextBox>();
                    if (textBox is TextBox tb)
                    {
                        tb.Focus();
                        tb.SelectAll();
                    }
                }
            }
        }
    }
}
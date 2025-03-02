using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using RssPlugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    public partial class RssRulesView : UserControl
    {
        DeleteRuleWindow? deleteRuleWindow;
        public RssRulesView()
        {
            InitializeComponent();
            DataContext = new RssAutoDownloadingRulesViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssRulesView_Loaded;
        }

        private void RssRulesView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRules)
            {
                rssRules.Initialize(); // Fetches data from the QBittorrent WebUI.
                RssRuleView.DeleteButton.Command = rssRules.DeleteSelectedRulesCommand;
            }
            else
            {
                Debug.WriteLine("No DataContext in RssRulesView");
            }
        }

        private void DeleteButton_Click(object? sender, RoutedEventArgs e)
        {
            var og = this.FindAncestorOfType<MainWindow>();
            if (og != null)
            {
                deleteRuleWindow = new DeleteRuleWindow();
                RssRuleView.DeleteButton.Click += DeleteButton_Click;
                deleteRuleWindow.ShowDialog(og);
            }
        }

        private void DeleteRuleDialogConfirmed(object? arg1, RoutedEventArgs args)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRules)
            {
                _ = HandleDeleteRules(rssRules);
            }
        }

        private async Task HandleDeleteRules(RssAutoDownloadingRulesViewModel rssRules)
        {
            deleteRuleWindow!.DeleteButton.IsEnabled = false;
            await rssRules.DeleteSelectedRulesAsync();
            deleteRuleWindow.DeleteButton.IsEnabled = true;
            deleteRuleWindow!.DeleteButton.Click -= DeleteRuleDialogConfirmed;
            deleteRuleWindow.Close();
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

        private void SaveRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                //rssRulesVm.AddRule(AddRuleTextBox.Text!);
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

        private void ExpandedControlsToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            var rowDefs = SideBarGrid.RowDefinitions;
            rowDefs.First().MinHeight = 170;
            rowDefs[1].Height = new GridLength(4d);
        }

        private void ExpandedControlsToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            var rowDefs = SideBarGrid.RowDefinitions;
            rowDefs.First().MinHeight = 20;
            rowDefs[1].Height = new GridLength(0);
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
            /*if (DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                radrvm.SelectedRssRules = RssRulesDataGrid.SelectedItems
                    .Cast<RssAutoDownloadingRuleViewModel>()
                    .ToList();
            }*/
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
                Debug.WriteLine($"Editting row {rssRulesVm.SelectedRssRule.Title}");
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

        private void AddRuleMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["AddRuleFlyout"] is Flyout flyout 
                && DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                var dgi = RssRulesDataGrid
                    .GetVisualDescendants()
                    .OfType<DataGridRow>()
                    .Where(d => d.DataContext == radrvm.SelectedRssRule)
                    .First();
                flyout.ShowAt(dgi);
            }
        }

        private void ClearDownloadedEpisodesMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["ClearDownloadedEpisodesFlyout"] is Flyout flyout
                && DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                var dgi = RssRulesDataGrid
                    .GetVisualDescendants()
                    .OfType<DataGridRow>()
                    .Where(d => d.DataContext == radrvm.SelectedRssRule)
                    .First();
                flyout.ShowAt(dgi);
            }
        }

        private void CloseClearDownloadedEpisodesButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["ClearDownloadedEpisodesFlyout"] is Flyout flyout)
            {
                flyout.Hide();
            }
        }
        private void YesClearDownloadedEpisodesButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Resources["ClearDownloadedEpisodesFlyout"] is Flyout flyout 
                && DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.SelectedRssRule!.ClearDownloadedEpisodesCommand.Execute();
                flyout.Hide();
            }
        }

        private void AddRuleUsingPluginButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {                
                //_ = _addRuleUsingPlugin(radrvm.RssPluginsViewModel.SelectedPlugin);
            }
        }


        public void AddNewRule(string name, string mustContain, List<Uri> selectedFeeds)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel radrvm)
            {
                radrvm.SelectedRssRule = null;
                radrvm.ActiveRssRule.Title = name;
                radrvm.ActiveRssRule.MustContain = mustContain;
                radrvm.ActiveRssRule.Warning = "Filled in the fields, but the rule isn't saved yet";
                radrvm.ActiveRssRule.AffectedFeeds = selectedFeeds.AsReadOnly();
                radrvm.ActiveRssRule.UpdateSelectedFeeds();
                RssRuleView.WarningTexTblock.Classes.Remove("Warning");
                RssRuleView.WarningTexTblock.Classes.Add("Warning");
            }
        }

        private void RssPluginTextBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (!RssPluginPreviewTabItem.IsSelected)
            {
                RssPluginPreviewTabItem.IsSelected = true;
            }
        }

        private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach(var item in RssRuleView.RssFeedsForRuleListBox.SelectedItems)
            {
                Debug.WriteLine(item);
            }
        }
    }
}

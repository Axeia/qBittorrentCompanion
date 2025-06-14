using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
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
            Loaded += RssRulesView_Loaded;
        }

        private void RssRulesView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRules)
            {
                rssRules.Initialize(); // Fetches data from the QBittorrent WebUI.
                RssRuleView.DeleteButton.Command = rssRules.DeleteSelectedRulesCommand;

                CreateRuleButton.GenerateRssRuleSplitButton.Command = rssRules.UsePluginToPopulateFieldsCommand;
            }
            else
            {
                Debug.WriteLine("No DataContext in RssRulesView");
            }

            if (!Design.IsDesignMode)
            {
                if (ConfigService.ExpandRssRuleRssPlugin == false)
                    CollapseRssArticleDetails();
                else
                    ExpandRssArticleDetails();
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

        private void ExpandedControlsToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            var rowDefs = SideBarGrid.RowDefinitions;
            rowDefs.First().MinHeight = 170;
            rowDefs[1].Height = new GridLength(14d);
        }

        private void ExpandedControlsToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            var rowDefs = SideBarGrid.RowDefinitions;
            rowDefs.First().MinHeight = 32;
            rowDefs.First().Height = new GridLength(20);
            rowDefs[1].Height = new GridLength(14);
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
                Debug.WriteLine($"BeginningEdit: oldTitle set to '{radrvm.SelectedRssRule!.Title}'");
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

        RssAutoDownloadingRuleViewModel? newRule = null;

        // Modify your AddNewRule method to defer selection and add additional notifications
        public void AddNewRule(string name, string mustContain, List<Uri> selectedFeeds)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rulesView 
                && RssRuleView.DataContext is RssAutoDownloadingRuleViewModel ruleVm)
            {
                // Not quite sure why selected feeds get deselected, but this fixes it:
                _ = SetSelectedFeeds(selectedFeeds.ToList());

                RssAutoDownloadingRuleViewModel newRule = rulesView.GetNewRssRule(selectedFeeds.AsReadOnly());
                newRule.Title = name;
                newRule.MustContain = mustContain;
                newRule.Warning = "Rule isn't saved yet";
                newRule.UseRegex = true;
                RssRuleView.WarningStackPanel.Classes.Remove("Warning");
                RssRuleView.WarningStackPanel.Classes.Add("Warning");
                rulesView.ActiveRssRule = newRule;
            }
        }

        private async Task SetSelectedFeeds(List<Uri> selectedFeeds)
        {
            await Task.Delay(100);
            Dispatcher.UIThread.Post(() =>
            {
                if(DataContext is RssAutoDownloadingRulesViewModel rulesVm)
                {
                    rulesVm.ActiveRssRule.SelectedFeeds.Clear();
                    rulesVm.ActiveRssRule.SelectedFeeds.Add(
                        rulesVm.ActiveRssRule.RssFeeds.Where(f=>selectedFeeds.Contains(f.Url))
                    );
                }
            }, DispatcherPriority.Background);
        }

        private void RssPluginTextBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (!RssPluginPreviewTabItem.IsSelected)
            {
                RssPluginPreviewTabItem.IsSelected = true;
            }
        }

        private void Expander_Expanded(object? sender, RoutedEventArgs e)
        {
            ExpandRssArticleDetails();
        }

        private void ExpandRssArticleDetails()
        {
            var height = 242;
            RightGrid.RowDefinitions[2].Height = new GridLength(height);
            RightGrid.RowDefinitions[2].MinHeight = height;
            RightGrid.RowDefinitions[2].MaxHeight = 750; // Vertical tabs

            VGridSplitter.IsVisible = true;
            Debug.WriteLine("Expanding");
        }

        private void Expander_Collapsed(object? sender, RoutedEventArgs e)
        {
            CollapseRssArticleDetails();
        }

        // The header of the collapsible should be fully visible even when collapsed,
        // achieved by setting a min height.
        private void CollapseRssArticleDetails()
        {
            RightGrid.RowDefinitions[2].Height = new GridLength(42);
            RightGrid.RowDefinitions[2].MinHeight = 42;
            RightGrid.RowDefinitions[2].MaxHeight = 42;

            VGridSplitter.IsVisible = false;
        }

        private void TabControl_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            RssPluginPreviewTabItem.Height = RssExtrasTabControl.Bounds.Height;
            TestDataTabItem.Height = RssExtrasTabControl.Bounds.Height;
        }
    }
}

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


        private IDisposable? ShowRssRulesTestDataToggleButtonDisposable = null;
        private void SetBindingsForRssRulesShowTestDataButton()
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                ShowRssRulesTestDataToggleButton.DataContext = rssRulesVm;
                var showTestDataBinding = new Binding
                {
                    Path = nameof(RssAutoDownloadingRulesViewModel.ShowTestData),
                    Mode = BindingMode.TwoWay,
                    Source = rssRulesVm,
                };
                ShowRssRulesTestDataToggleButtonDisposable = ShowRssRulesTestDataToggleButton.Bind(ToggleButton.IsCheckedProperty, showTestDataBinding);
            }
        }

        private void RssRulesShowTestDataButtonClearBindings()
        {
            ShowRssRulesTestDataToggleButton.ClearValue(ToggleButton.IsCheckedProperty);
            ShowRssRulesTestDataToggleButtonDisposable?.Dispose();
        }


        private void RssRulesComboBoxClearBindings()
        {
            if (RssRulesComboBox.DataContext is RssAutoDownloadingRulesViewModel)
            {
                // Clear previous bindings for the garbage collector 
                RssRulesComboBox.ClearValue(ComboBox.ItemsSourceProperty);
                RssRulesComboBox.ClearValue(ComboBox.SelectedItemProperty);
                RssRulesComboBox.ClearValue(Control.DataContextProperty);
            }
            RssRulesComboBoxDisposable?.Dispose();
        }

        private IDisposable? RssRulesComboBoxDisposable = null;
        private void SetBindingsForRssRulesComboBox()
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                //FIXME ClearComboBoxValues(RssRulesComboBox);
                RssRulesComboBox.DataContext = rssRulesVm;
                RssRulesComboBox.ItemsSource = rssRulesVm.RssRules;

                var selectedRssRuleBinding = new Binding
                {
                    Path = "SelectedRssRule",
                    Mode = BindingMode.TwoWay,
                    Source = rssRulesVm
                };

                RssRulesComboBoxDisposable = RssRulesComboBox.Bind(ComboBox.SelectedItemProperty, selectedRssRuleBinding);
            }
        }

        private void AddRuleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.AddRule(AddRuleTextBox.Text!);
            }
        }

        private void DeleteRssRulesButton_Click(object? sender, RoutedEventArgs e)
        {
            /* FixMe 
            var rssDlRules = RssRulesView.RssRulesDataGrid.SelectedItems.OfType<RssAutoDownloadingRuleViewModel>();
            if (RssRulesView?.DataContext is RssAutoDownloadingRulesViewModel rssRulesVm)
            {
                rssRulesVm.DeleteRules(rssDlRules);
            }
            ExpandRssRulesButton.IsChecked = false;*/
        }

        private void RssRulesMultiViewToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            if (Resources["RssRulesMultiViewFlyout"] is Flyout flyout)
            {
                flyout.ShowAt(ExpandRssRulesButton);
                flyout.Closed += RssRulesFlyout_Closed;
            }
        }

        private void RssRulesFlyout_Closed(object? sender, EventArgs e)
        {
            if (sender is Flyout flyout)
            {
                flyout.Closed -= RssRulesFlyout_Closed;
                ExpandRssRulesButton.IsChecked = false;
            }
        }

        private void RssRulesMultiViewToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (Resources["RssRulesMultiViewFlyout"] is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }
}

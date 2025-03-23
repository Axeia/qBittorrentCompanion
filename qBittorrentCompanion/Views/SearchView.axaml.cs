using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private void CreateNewTabButton_Click(object? sender, RoutedEventArgs e)
        {
            SearchTabControl.Items.Add(new SearchTabItem());
            SearchTabControl.SelectedIndex = SearchTabControl.Items.Count -1;
            SearchTabControl.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetTabsLeftRightControlsStackpanelVisibility();
        }

        internal void CloseTab(SearchTabItem searchTabItem)
        {
            if (searchTabItem.DataContext is SearchViewModel svm)
                svm.EndSearch();

            var index = SearchTabControl.IndexFromContainer(searchTabItem);

            SearchTabControl.Items.Remove(searchTabItem);

            // Select the tab after it (if there's one), else select the one before it
            SearchTabControl.SelectedIndex = index >= SearchTabControl.Items.Count
                ? SearchTabControl.Items.Count - 1 
                : SearchTabControl.SelectedIndex = index;
        }

        private void SetTabsLeftRightControlsStackpanelVisibility()
        {
            if (SearchTabControl.GetTemplateChildren().Where(c=>c.Name == "TabsLeftRightControlsStackpanel").First() is StackPanel tabsLeftRightControlsStackPanel &&
                SearchTabControl.GetTemplateChildren().Where(c => c.Name == "TabControlScrollViewer").First() is ScrollViewer tabControlScrollViewer)
            {
                tabsLeftRightControlsStackPanel.IsVisible = tabControlScrollViewer.Extent.Width > tabControlScrollViewer.Viewport.Width;
            } 
        }


        private void TabControlTabsScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            SetTabsLeftRightControlsStackpanelVisibility();
        }

        private void ScrollTabsLeftButton_Click(object? sender, RoutedEventArgs e)
        {
            if (SearchTabControl.GetTemplateChildren().Where(c => c.Name == "TabControlScrollViewer").FirstOrDefault() is ScrollViewer tabControlScrollViewer &&
                 SearchTabControl.GetTemplateChildren().Where(c => c.Name == "PART_ItemsPresenter").FirstOrDefault() is ItemsPresenter itemsPresenter)
            {
                var firstTabItem = itemsPresenter.FindDescendantOfType<TabItem>();
                if (firstTabItem != null)
                {
                    double itemWidth = firstTabItem.Bounds.Width;
                    double scrollAmount = itemWidth * 3; // Scroll 3 items

                    tabControlScrollViewer.Offset = new Vector(Math.Max(0, tabControlScrollViewer.Offset.X - scrollAmount), tabControlScrollViewer.Offset.Y);
                }
            }
        }

        private void ScrollTabsRightButton_Click(object? sender, RoutedEventArgs e)
        {
            if (SearchTabControl.GetTemplateChildren().Where(c => c.Name == "TabControlScrollViewer").FirstOrDefault() is ScrollViewer tabControlScrollViewer &&
                 SearchTabControl.GetTemplateChildren().Where(c => c.Name == "PART_ItemsPresenter").FirstOrDefault() is ItemsPresenter itemsPresenter)
            {
                var firstTabItem = itemsPresenter.FindDescendantOfType<TabItem>();
                if (firstTabItem != null)
                {
                    double itemWidth = firstTabItem.Bounds.Width;
                    double scrollAmount = itemWidth * 3; // Scroll 3 items

                    tabControlScrollViewer.Offset = new Vector(Math.Max(0, tabControlScrollViewer.Offset.X + scrollAmount), tabControlScrollViewer.Offset.Y);
                }
            }
        }

        /// <summary>
        /// Scrolls the tabcontrol scrollviewer sideways when scrolling up and down over it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (sender is ScrollViewer tabControlScrollViewer)
            {
                if (tabControlScrollViewer.Extent.Width > tabControlScrollViewer.Viewport.Width) // Only scroll if there's horizontal overflow
                {
                    double scrollAmount = e.Delta.Y * 50;

                    double newOffset = Math.Max(0, Math.Min(
                        tabControlScrollViewer.Extent.Width - tabControlScrollViewer.Viewport.Width,
                        tabControlScrollViewer.Offset.X + scrollAmount));

                    tabControlScrollViewer.Offset = new Vector(newOffset, tabControlScrollViewer.Offset.Y);
                    e.Handled = true;
                }
            }
        }
    }
}

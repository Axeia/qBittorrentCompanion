using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;

namespace qBittorrentCompanion.Views
{
    public partial class SearchView : UserControl
    {
        private Key[] _keys = [Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9];
        protected ReactiveCommand<int, Unit> FocusTabCommand { get; }

        public SearchView()
        {
            InitializeComponent();
            FocusTabCommand = ReactiveCommand.Create<int>(FocusTab);
            SetKeyBindings();
        }

        /// <summary>
        /// Focuses the tab at the given index on the <see cref="SearchTabControl"/>
        /// </summary>
        /// <param name="tabIndex"></param>
        private void FocusTab(int tabIndex)
        {
            SearchTabControl.SelectedIndex = tabIndex;
        }

        /// <summary>
        /// Adds hotkeys ctrl+alt and 1-9 to select one of the first 9 tabs
        /// </summary>
        private void SetKeyBindings()
        {
            for(int i = 0; i < _keys.Length; i++)
            {
                var keyBinding = new KeyBinding
                {
                    Gesture = new KeyGesture(_keys[i], KeyModifiers.Control | KeyModifiers.Alt),
                    Command = FocusTabCommand,
                    CommandParameter = i,
                };
                KeyBindings.Add(keyBinding);
            }
        }

        /// <summary>
        /// Creates a new Tab by adding a <see cref="SearchTabItem"/> instance to <see cref="SearchTabControl"/>'s Items collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// Sets the visibility of the 'move' the tabstrip buttons.
        /// If it's overflowing it shows the controls, if not it hides them
        /// </summary>
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

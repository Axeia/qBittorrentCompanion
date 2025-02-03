using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;
using Avalonia;
using Avalonia.VisualTree;
using System;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Data;
using QBittorrent.Client;

namespace qBittorrentCompanion.Views
{
    public partial class SearchView : UserControl
    {
        //^\[([^\]]+)\] (.*)(?= \- [0-9]{1,3}) \- ([0-9]{1,3}).*\[([A-Z-0-9]{8})\]\.(?:mkv|MKV)
        public SearchView()
        {
            InitializeComponent();
            this.Loaded += SearchView_Loaded;
            this.DataContext = new SearchViewModel();

            // Subscribe to resize actions
            this.AttachedToVisualTree += (sender, e) =>
            {
                if (this.GetVisualRoot() is Window window)
                {
                    window.GetObservable(Window.BoundsProperty).Subscribe(Resize);
                }
            };
        }

        private void SearchView_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel searchVm)
            {
                searchVm.Initialise();
                SetBindings();
            }
        }

        // Callback function for resize actions
        private void Resize(Rect size)
        {
            FiltersGrid.IsVisible = size.Width < 1220;
        }

        private void SearchPluginButton_Click(object? sender, RoutedEventArgs e)
        {
            var searchPluginsWindow = new SearchPluginsWindow();

            var mw = this.FindAncestorOfType<MainWindow>();
            if (mw != null) 
                searchPluginsWindow.ShowDialog(mw);
        }

        private void SearchToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel searchViewModel)
            {
                searchViewModel.EndSearch();
            }
        }

        private void SearchToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel searchViewModel)
            {
                searchViewModel.StartSearch();
            }
        }

        private void SearchQueryTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchToggleButton.IsChecked = !SearchToggleButton.IsChecked;
        }



        private void SetBindings()
        {
            if (DataContext is SearchViewModel searchVm)
            {
                SearchQueryTextBox.ClearValue(Control.DataContextProperty);
                SearchQueryTextBox.DataContext = searchVm;
                SearchQueryTextBox.Bind(TextBox.TextProperty, new Binding
                {
                    Path = "SearchQuery",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });

                ClearComboBoxValues(SearchPluginsComboBox);
                SearchPluginsComboBox.DataContext = searchVm;
                SearchPluginsComboBox.ItemsSource = searchVm.SearchPlugins;
                SearchPluginsComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
                {
                    Path = "SelectedSearchPlugin",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });
                SearchPluginsComboBox.DisplayMemberBinding = new Binding(nameof(SearchPlugin.FullName));


                ClearComboBoxValues(SearchPluginCategoriesComboBox);
                SearchPluginCategoriesComboBox.DataContext = searchVm;
                SearchPluginCategoriesComboBox.ItemsSource = searchVm.PluginCategories;
                SearchPluginCategoriesComboBox.Bind(ComboBox.SelectedItemProperty, new Binding
                {
                    Path = "SelectedSearchPluginCategory",
                    Mode = BindingMode.TwoWay,
                    Source = searchVm
                });
                SearchPluginCategoriesComboBox.DisplayMemberBinding = new Binding(nameof(SearchPluginCategory.Name));
            }
        }

        /// <summary>
        /// Clear previous bindings if any to avoid conflicts
        /// </summary>
        /// <param name="comboBox"></param>
        private void ClearComboBoxValues(ComboBox comboBox)
        {
            comboBox.ClearValue(ComboBox.ItemsSourceProperty);
            comboBox.ClearValue(ComboBox.SelectedItemProperty);
            comboBox.ClearValue(Control.DataContextProperty);
        }
    }
}

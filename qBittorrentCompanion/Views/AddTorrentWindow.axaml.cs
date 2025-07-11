using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    public partial class AddTorrentsWindow : TabControlsIcoWindow
    {
        public AddTorrentsWindow()
        {
            InitializeComponent();
            DataContext = new TorrentButtonViewModel();

            AddSplitButton.Click += AddSplitButton_Clicked;
            Loaded += AddTorrentFileWindow_Loaded;
        }

        private void AddTorrentFileWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            ShowAdvancedToggleButton.IsChecked = Design.IsDesignMode || ConfigService.DownloadWindowShowAdvanced;

            SetWindowIcon();

            if (FilesUrlsTabControl.SelectedIndex == 0)
                UrlsTextBox.Focus();
            else
            {
                SelectFilesButton.Focus(NavigationMethod.Tab);
            }

            TabStrip = TorrentModeTabStrip;
            SetKeyBindings();
        }

        /// <summary>
        /// Public access level to enable downloading without actually showing the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddSplitButton_Clicked(object? sender, RoutedEventArgs e)
        {
            // Get the request with the majority of the fields filled in.
            AddTorrentsRequest addTorrentsRequest = TorrentFields.GetAddTorrentsRequest();

            // Get .Paused from the Split button
            bool IsPaused = !(AddSplitButton!.Content!.ToString() == TorrentButtonViewModel.Actions[0]);
            addTorrentsRequest.Paused = IsPaused;

            // Add any files that might have been added 
            foreach (var item in FilesListBox.Items)
            {
                // the ListBoxItem.ToolTip contains the filepath
                if (item is ListBoxItem fileListBoxItem && fileListBoxItem[ToolTip.TipProperty] is string filePath)
                {
                    if (File.Exists(filePath)) // Double check it hasn't been deleted meanwhile
                        addTorrentsRequest.TorrentFiles.Add(filePath);
                }
            }

            // Add any URLs that might have been added.
            if (UrlsTextBox.Text is not null)
            {
                string[] torrentUrls = UrlsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string url in torrentUrls)
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
                    {
                        addTorrentsRequest.TorrentUrls.Add(uriResult);
                    }
                }
            }

            StartDownloads(addTorrentsRequest);
        }

        private void SetWindowIcon()
        {
            try
            {
                var xamlUri = new Uri("avares://qBittorrentCompanion/Assets/Logo.axaml");
                var logoCanvasContent = (Canvas)AvaloniaXamlLoader.Load(xamlUri);

                WindowIconViewBox.Child = logoCanvasContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Canvas content: {ex.Message}");
            }
        }

        private async void StartDownloads(AddTorrentsRequest addTorrentsRequest)
        {
            await QBittorrentService.AddTorrentsAsync(addTorrentsRequest);

            this.Close();
        }

        public static FilePickerFileType TorrentFiles { get; } = new("Torrent Files")
        {
            Patterns = ["*.torrent"],
            AppleUniformTypeIdentifiers = ["org.bittorrent.torrent"],
            MimeTypes = ["application/x-bittorrent"]
        };

        public new Task ShowDialog(Window owner)
        {
            if (owner is MainWindow mainWindow 
                && mainWindow.TransfersTorrentsView.DataContext is TorrentsViewModel tvm)
            {
                var categories = new List<CategoryCountViewModel> {  }; // Start with a null item
                categories.AddRange(tvm.CategoryCounts.Where(c => c.IsEditable)); // Add all the actual categories
                TorrentFields.CategoryComboBox.ItemsSource = categories;
            }

            return ShowDialog<object>(owner);
        }

        public async void OnAddFilesClicked(object sender, RoutedEventArgs e)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "qBittorrent Companion - Add Torrents",
                AllowMultiple = true,
                FileTypeFilter = new[] { TorrentFiles }
            });


            if (files.Count >= 1)
            {
                foreach (var file in files)
                {
                    AddFile(file.Path.ToString());
                }
            }

        }

        /// <summary>
        /// Public so the window can be 'prepped' externally with a file.
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFile(string filePath)
        {
            if (FilesListBox.Items.Count > 0 
            && FilesListBox.Items[0] is ListBoxItem listboxItem
            && listboxItem.Content is string str
            && str == "No file selected")
                FilesListBox.Items.RemoveAt(0);
                

            // Remove the "file:///" prefix if it exists
            // it confuses FileInfo.
            if (filePath.StartsWith("file:///"))
            {
                filePath = filePath.Substring(8);
            }

            var file = new FileInfo(Uri.UnescapeDataString(filePath));

            var listBoxItem = new ListBoxItem { Content = file.Name };
            listBoxItem[ToolTip.TipProperty] = file.FullName;
            //listBoxItem.IsHitTestVisible = false;

            FilesListBox.Items.Add(listBoxItem);
        }

        public void AddUrl(string url)
        {
            TorrentUrlsTextBox.Text += url + "\n";
        }

        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
            => AddSplitButton.Flyout!.Hide();
    }
}

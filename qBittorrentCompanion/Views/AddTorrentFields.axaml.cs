using Avalonia;
using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System;

namespace qBittorrentCompanion.Views
{
    public partial class AddTorrentFieldsView : UserControl
    {
        public AddTorrentFieldsView()
        {
            InitializeComponent();
        }

        public static readonly AvaloniaProperty<string> OrientationProperty =
            AvaloniaProperty.Register<AddTorrentFieldsView, string>(nameof(Orientation));

        public string Orientation
        {
            get => GetValue(OrientationProperty) as string ?? "Vertical";
            set => SetValue(OrientationProperty, value);
        }

        public AddTorrentsRequest GetAddTorrentsRequest()
        {
            string category = "";
            if (CategoryComboBox.SelectedItem is CategoryCountViewModel ccvm)
                category = ccvm.Name;

            AddTorrentsRequest addTorrentsRequest = new()
            {
                DownloadFolder = SavePathTextBox.Text,
                Cookie = CookieTextBox.Text,
                Rename = RenameTextBox.Text,
                Category = category,
                Tags = [],
                SkipHashChecking = SkipHashCheckCheckBox.IsChecked == true,
                CreateRootFolder = false,
                AutomaticTorrentManagement = AutoTMMComboBox.SelectedIndex == 1,
                RatioLimit = 0.0,
                SeedingTimeLimit = TimeSpan.FromSeconds(3600),
                //stop condition?
                ContentLayout = (TorrentContentLayout)ContentLayoutComboBox.SelectedIndex,
                SequentialDownload = SequentialDownloadCheckBox.IsChecked == true,
                FirstLastPiecePrioritized = FirstLastPrioCheckBox.IsChecked == true,
                DownloadLimit = (int?)dlLimitNumericUpDown.Value,
                UploadLimit = (int?)upLimitNumericUpDown.Value
            };


            return addTorrentsRequest;
        }

        private void CategoryComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox is ComboBox cb 
                && cb.SelectedItem is CategoryCountViewModel ccvm 
                && ccvm.HasPath)
            {
                SavePathTextBox.IsEnabled = false;
                SavePathTextBox.Text = ccvm.SavePath;
            }
            else if(SavePathTextBox != null)
            {
                SavePathTextBox.IsEnabled = true;
                SavePathTextBox.Text = "";
            }
        }
    }
}

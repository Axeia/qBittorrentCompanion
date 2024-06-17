using Avalonia;
using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

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
            get { return (string)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public AddTorrentsRequest GetAddTorrentsRequest()
        {
            string category = "";
            if (CategoryComboBox.SelectedItem is CategoryCountViewModel ccvm)
                category = ccvm.Name;

            string stopCondition = "None";
            if (StopConditionComboBox.SelectedItem is string sc)
                stopCondition = sc;

            string contentLayout = "Original";
            if (ContentLayoutComboBox.SelectedItem is string cl)
                contentLayout = cl;

            AddTorrentsRequest addTorrentsRequest = new AddTorrentsRequest();
            addTorrentsRequest.DownloadFolder = SavePathTextBox.Text;
            addTorrentsRequest.Cookie = CookieTextBox.Text;
            addTorrentsRequest.Rename = RenameTextBox.Text;
            addTorrentsRequest.Category = category;
            addTorrentsRequest.Tags = new List<string>();
            addTorrentsRequest.SkipHashChecking = SkipHashCheckCheckBox.IsChecked == true;
            addTorrentsRequest.CreateRootFolder = false;
            addTorrentsRequest.AutomaticTorrentManagement = AutoTMMComboBox.SelectedIndex == 1;
            addTorrentsRequest.RatioLimit = 0.0;
            addTorrentsRequest.SeedingTimeLimit = TimeSpan.FromSeconds(3600);
            //stop condition?
            addTorrentsRequest.ContentLayout = (TorrentContentLayout)ContentLayoutComboBox.SelectedIndex;
            addTorrentsRequest.SequentialDownload = SequentialDownloadCheckBox.IsChecked == true;
            addTorrentsRequest.FirstLastPiecePrioritized = FirstLastPrioCheckBox.IsChecked == true;
            addTorrentsRequest.DownloadLimit = (int?)dlLimitNumericUpDown.Value;
            addTorrentsRequest.UploadLimit = (int?)upLimitNumericUpDown.Value;


            return addTorrentsRequest;
        }

        private void CategoryComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is CategoryCountViewModel ccvm && ccvm.HasPath)
            {
                SavePathTextBox.IsEnabled = false;
                SavePathTextBox.Text = ccvm.SavePath;
            }
            else
            {
                SavePathTextBox.IsEnabled = true;
                SavePathTextBox.Text = "";
            }
        }
    }
}

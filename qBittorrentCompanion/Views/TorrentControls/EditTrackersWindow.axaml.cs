using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.Views
{
    public partial class EditTrackersWindow : Window
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public EditTrackersWindow()
        {
            var temp = new EditTrackersWindowViewModel("", "some torrent");
            DataContext = temp;

            InitializeComponent();
        }

        public EditTrackersWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            var etwvm = new EditTrackersWindowViewModel(torrentInfoViewModel.Hash, torrentInfoViewModel.Name);

            DataContext = etwvm;

            InitializeComponent();
            this.Title += torrentInfoViewModel.Name;

            ExtraInfoToggleButton.IsChecked = Design.IsDesignMode ? true : ConfigService.EditTrackersWindowShowExtraInfo;
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var dgr = textBox.FindAncestorOfType<DataGridRow>();
                if(dgr != null)
                {
                    dgr.IsSelected = true;
                }
            }
        }

        private void ExtraInfoToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            if (!Design.IsDesignMode)
                ConfigService.DownloadWindowShowAdvanced = true;
        }

        private void ExtraInfoToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (!Design.IsDesignMode)
                ConfigService.DownloadWindowShowAdvanced = false;
        }

        private void RowItem_GotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is Control control 
                && control.FindAncestorOfType<ListBoxItem>() is ListBoxItem listBoxItem 
                && control.FindAncestorOfType<ListBox>() is ListBox listBox)
            {
                listBox.SelectedItems!.Clear();
                listBoxItem.IsSelected = true;
            }
        }
    }
}

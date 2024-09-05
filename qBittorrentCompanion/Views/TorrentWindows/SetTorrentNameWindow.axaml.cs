using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    public partial class SetTorrentNameWindow : ShakerWindow
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public SetTorrentNameWindow()
        {
            InitializeComponent();
        }

        public SetTorrentNameWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            InitializeComponent();
            NameTextBox.Text = torrentInfoViewModel.Name;
        }

        protected override void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = SetNameAsync();
        }

        private async Task SetNameAsync()
        {
            if (_torrentInfoViewModel != null)
            {
                SpinnerSymbolIcon.IsVisible = true;
                SaveButton.IsEnabled = false;
                var nameSaved = await _torrentInfoViewModel.SetNameAsync(NameTextBox.Text!);
                if (nameSaved)
                    this.Close();
                else
                {
                    SetLocationWindow.Title = "Error: torrent was not renamed";
                    SaveButton.IsEnabled = true;
                    _ = ShakeWindowAsync();
                }

                SpinnerSymbolIcon.IsVisible = false;
            }
        }

        private void LocationTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if(_torrentInfoViewModel != null)
                SaveButton.IsEnabled = _torrentInfoViewModel!.Name != NameTextBox.Text;
        }
    }
}

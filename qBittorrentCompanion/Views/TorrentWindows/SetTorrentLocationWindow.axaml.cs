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
    public partial class SetTorrentLocationWindow : ShakerWindow
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public SetTorrentLocationWindow()
        {
            InitializeComponent();
        }

        public SetTorrentLocationWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            InitializeComponent();
            LocationTextBox.Text = torrentInfoViewModel.SavePath;
        }

        protected override void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = SetLocationAsync();
        }

        private async Task SetLocationAsync()
        {
            if (_torrentInfoViewModel != null)
            {
                SpinnerSymbolIcon.IsVisible = true;
                SaveButton.IsEnabled = false;
                var pathSaved = await _torrentInfoViewModel.SetSavePathAsync(LocationTextBox.Text!);
                if (pathSaved)
                    this.Close();
                else
                {
                    SetLocationWindow.Title = "Error: Location not saved";
                    SaveButton.IsEnabled = true;
                    _ = ShakeWindowAsync();
                }

                SpinnerSymbolIcon.IsVisible = false;
            }
        }

        private void LocationTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if(_torrentInfoViewModel != null)
                SaveButton.IsEnabled = _torrentInfoViewModel!.SavePath != LocationTextBox.Text;
        }
    }
}

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
    public partial class EditTrackersWindow : IcoWindow
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
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditModeToggleButton_Checked(object? sender, RoutedEventArgs e)
        {
            this.Title = "✎ " + this.Title;
        }

        private void EditModeToggleButton_Unchecked(object? sender, RoutedEventArgs e)
        {
            this.Title = Title![2..];
        }
    }
}

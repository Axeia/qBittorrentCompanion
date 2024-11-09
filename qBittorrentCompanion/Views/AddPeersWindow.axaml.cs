using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class AddPeersWindow : IcoWindow
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public AddPeersWindow()
        {
            var temp = new AddPeersWindowViewModel("", "some torrent");
            DataContext = temp;

            InitializeComponent();
        }

        public AddPeersWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            
            var etwvm = new AddPeersWindowViewModel(torrentInfoViewModel.Hash, torrentInfoViewModel.Name);
            etwvm.RequestClose += Etwvm_RequestClose;

            DataContext = etwvm;

            InitializeComponent();
            this.Title += torrentInfoViewModel.Name;
        }

        // View model requests closing.
        private void Etwvm_RequestClose()
        {
            if (Owner is MainWindow mainWindow
                && mainWindow.TransfersTorrentsView.TorrentTrackersDataGrid.DataContext is TorrentTrackersViewModel ttvm)
            {
                _ = ttvm.ForceUpdateDataAsync();
            }
            this.Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class AddTrackersWindow : IcoWindow
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public AddTrackersWindow()
        {
            var temp = new AddTrackersWindowViewModel("", "some torrent");
            DataContext = temp;

            InitializeComponent();
        }

        public AddTrackersWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            
            var etwvm = new AddTrackersWindowViewModel(torrentInfoViewModel.Hash, torrentInfoViewModel.Name);
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

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
    public partial class RenameTorrentFilesWindow : Window
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        public RenameTorrentFilesWindow()
        {
            InitializeComponent();
        }

        public RenameTorrentFilesWindow(TorrentInfoViewModel torrentInfoViewModel)
        {
            this._torrentInfoViewModel = torrentInfoViewModel;
            InitializeComponent();
        }
    }
}

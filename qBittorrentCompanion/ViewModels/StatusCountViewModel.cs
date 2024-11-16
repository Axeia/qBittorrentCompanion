using FluentIcons.Common;
using QBittorrent.Client;
using ReactiveUI;
using System.Collections.Generic;

namespace qBittorrentCompanion.ViewModels
{
    public class StatusCountViewModel : ViewModelBase
    {
        public StatusCountViewModel(string name, Symbol symbol, List<TorrentState> torrentStates)
        {
            Name = name;
            _symbol = symbol;
            _torrentStates = torrentStates;
        }

        private List<TorrentState> _torrentStates;
        public List<TorrentState> TorrentStates => _torrentStates;

        private Symbol _symbol;
        public Symbol Symbol => _symbol;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private int _count = 0;
        public int Count
        {
            get => _count;
            set => this.RaiseAndSetIfChanged(ref _count, value);
        }
    }
}

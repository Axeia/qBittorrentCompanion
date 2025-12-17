using FluentIcons.Common;
using QBittorrent.Client;
using RaiseChangeGenerator;
using ReactiveUI;
using System.Collections.Generic;

namespace qBittorrentCompanion.ViewModels
{
    public partial class StatusCountViewModel : ViewModelBase
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

        [RaiseChange]
        private string _name = string.Empty;
        [RaiseChange]
        private int _count = 0;
    }
}

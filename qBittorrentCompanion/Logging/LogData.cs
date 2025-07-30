
using ReactiveUI;
using System;

namespace qBittorrentCompanion.Logging
{
    public class LogData : ReactiveObject
    {
        private DateTime _time = DateTime.Now;

        public DateTime Time
        {
            get => _time;
            set => this.RaiseAndSetIfChanged(ref _time, value);
        }

    }
}

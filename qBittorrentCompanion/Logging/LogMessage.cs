using AutoPropertyChangedGenerator;
using ReactiveUI;
using Splat;
using System;

namespace qBittorrentCompanion.Logging
{
    public partial class LogMessage(LogLevel level, string source, string title = "", string message = "") : ReactiveObject
    {
        [AutoPropertyChanged]
        private DateTime _timestamp = DateTime.Now;
        [AutoPropertyChanged]
        private LogLevel _level = level;
        [AutoPropertyChanged]
        private string _source = source;
        [AutoPropertyChanged]
        private string _title = title;
        [AutoPropertyChanged]
        private string _message = message;
    }
}

using AutoPropertyChangedGenerator;
using ReactiveUI;
using Splat;
using System;

namespace qBittorrentCompanion.Logging
{
    public partial class LogMessage(LogLevel level, string source, string title = "", string message = "", bool isData = false, string secondaryTitle = "") 
        : ReactiveObject
    {
        private readonly DateTime _timestamp = DateTime.Now;
        public DateTime Timestamp => _timestamp;
        public string Time => _timestamp.ToString("HH:mm:ss");
        
        private readonly LogLevel _level = level;
        public LogLevel Level => _level;

        private readonly string _source = source;
        public string Source => _source;

        private readonly string _title = title;
        public string Title => _title;

        private readonly string _secondaryTitle = secondaryTitle;
        public string SecondaryTitle => _secondaryTitle;

        private readonly string _message = message;
        public string Message => _message;

        private readonly bool _isData = isData;
        public bool IsData => _isData;

        [AutoPropertyChanged]
        private bool _isVisible = true;
    }
}

using AutoPropertyChangedGenerator;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;
using System;

namespace qBittorrentCompanion.Logging
{
    public partial class LogMessage : ReactiveObject
    {
        private readonly DateTime _timestamp = DateTime.Now;
        public DateTime Timestamp => _timestamp;
        public string Time => _timestamp.ToString("HH:mm:ss");

        private readonly LogLevel _level;
        public LogLevel Level => _level;

        private readonly string _source;
        public string Source => _source;

        private readonly string _title;
        public string Title => _title;

        private readonly string _secondaryTitle;
        public string SecondaryTitle => _secondaryTitle;

        private readonly string _message;
        public string Message => _message;

        private readonly bool _isData;
        public bool IsData => _isData;

        private readonly bool _isJson = false;
        public bool IsJson => _isJson;

        [AutoPropertyChanged]
        private bool _isVisible = true;
        // Using a primary constructor would assign the value to _isJson a second time leading to UI issues,
        // so a regular constructor is used instead
#pragma warning disable IDE0290 // Use primary constructor
        public LogMessage(LogLevel level, string source, string title = "", string message = "", bool isData = false, string secondaryTitle = "")
#pragma warning restore IDE0290 // Use primary constructor
        {
            _timestamp = DateTime.Now;
            _level = level;
            _source = source;
            _title = title;
            _message = message;
            _isData = isData;
            _secondaryTitle = secondaryTitle;
        }
        
        public LogMessage(LogLevel level, string source, string title = "", object? message = null, bool isData = false, string secondaryTitle = "")
            : this(
                  level, 
                  source, 
                  title,
                  ToJsonString(message), 
                  isData, 
                  secondaryTitle)
        {
            _isJson = true;
        }

        private static string ToJsonString(object? message) =>
            message == null
                ? ""
                : JsonConvert.SerializeObject(message, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });
    }
}

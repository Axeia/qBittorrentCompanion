using Newtonsoft.Json;
using qBittorrentCompanion.Logging;
using Splat;
using System;

namespace qBittorrentCompanion.Services
{
    public partial class AppLoggerService
    {
        public static event Action<LogMessage>? LogMessageAdded;

        public static void AddLogMessage(LogLevel level, string source, string title, string message) => LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: message
            ));

        public static void AddLogMessage(LogLevel level, string source, string title, object message) =>
            LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: JsonConvert.SerializeObject(message, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    })
            ));

    }
}

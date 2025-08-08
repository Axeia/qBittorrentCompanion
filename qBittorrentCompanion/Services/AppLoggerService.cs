using Newtonsoft.Json;
using qBittorrentCompanion.Logging;
using Splat;
using System;

namespace qBittorrentCompanion.Services
{
    public partial class AppLoggerService
    {
        public static event Action<LogMessage>? LogMessageAdded;

        public static void AddLogMessage(LogLevel level, string source, string title, string message, string secondaryTitle = "") 
            => LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: message,
                secondaryTitle: secondaryTitle
            ));

        public static void AddLogMessage(LogLevel level, string source, string title, object message, string secondaryTitle = "") 
            => LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: JsonConvert.SerializeObject(message, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                secondaryTitle: secondaryTitle
            ));

        /// <summary>
        /// Returns class including namespace prefix as a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string NSC<T>()
        {
            return typeof(T).FullName ?? typeof(T).Name;
        }
    }
}

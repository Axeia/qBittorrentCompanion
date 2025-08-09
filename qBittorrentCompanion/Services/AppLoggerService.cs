using Newtonsoft.Json;
using qBittorrentCompanion.Logging;
using Splat;
using System;

namespace qBittorrentCompanion.Services
{
    public partial class AppLoggerService
    {
        public static event Action<LogMessage>? LogMessageAdded;

        /// <summary>
        /// Adds a log message with the specified details and raises the <see cref="LogMessageAdded"/> event.
        /// </summary>
        /// <param name="level">The severity level of the log message.</param>
        /// <param name="source">The source or origin of the log message, typically identifying the component or module. Consider using using <see cref="GetFullTypeName{T}"/></param>
        /// <param name="title">The primary title or summary of the log message.</param>
        /// <param name="message">The detailed content of the log message.</param>
        /// <param name="secondaryTitle">An optional secondary title, recommend to be a file name if a file is involved, 
        /// or if a data class is processed use its name through <see cref="GetFullTypeName{T}"/> </param>
        public static void AddLogMessage(LogLevel level, string source, string title, string message = "", string secondaryTitle = "") 
            => LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: message,
                secondaryTitle: secondaryTitle
            ));

        ///<inheritdoc cref="AddLogMessage(LogLevel, string, string, string, string)"/>
        public static void AddLogMessage(LogLevel level, string source, string title, object message, string secondaryTitle = "") 
            => LogMessageAdded?.Invoke(new LogMessage(
                level: level,
                source: source,
                title: title,
                message: message,
                secondaryTitle: secondaryTitle
            ));
    }
}

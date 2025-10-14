using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Helpers
{
    // Utility class to handle FileSystemWatcher with debouncing
    public sealed class DebouncedFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly DispatcherTimer _debounceTimer;

        /// <summary>
        /// Event fired after the debounce interval, signaling that changes are ready to be processed.
        /// Expects an async method, if that's not the case see <see cref="ChangesReady"/>
        /// </summary>
        public event Func<Task>? ChangesReadyAsync;
        /// <summary>
        /// Event fired after the debounce interval, signaling that changes are ready to be processed.
        /// Expects a regular method, need an async one? <see cref="ChangesReadyAsync"/>
        /// </summary>
        public event Action? ChangesReady;

        /// <summary>
        /// Initializes a new debounced file watcher.
        /// </summary>
        /// <param name="path">The directory to watch.</param>
        /// <param name="filter">The file filter (e.g., "*.py").</param>
        /// <param name="debounceInterval">The time to wait before processing changes in milliseconds.</param>
        public DebouncedFileWatcher(string path, string filter, int debounceInterval = 200)
        {
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(debounceInterval) };
            _debounceTimer.Tick += async (sender, e) =>
            { // Debounced, invoke for notifying the changes
                _debounceTimer.Stop();

                if (ChangesReady != null)
                    Dispatcher.UIThread.Post(() => ChangesReady?.Invoke());

                if (ChangesReadyAsync != null)
                    await Dispatcher.UIThread.InvokeAsync(() => ChangesReadyAsync.Invoke());
            };

            // 3. Setup the File System Watcher
            _watcher = new FileSystemWatcher(path, filter)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false // Matching your original logic
            };

            // 4. Register the change handler to debounce
            _watcher.Created += Watcher_Changed;
            _watcher.Deleted += Watcher_Changed;
            _watcher.Changed += Watcher_Changed;
            _watcher.Renamed += Watcher_Changed;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            // On any file event, reset the timer to debounce
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void Dispose()
        {
            _debounceTimer.Stop();
            _watcher.Dispose();
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using System.ComponentModel;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// Abstract class to be implemented by ViewModels that need to run a timer.
    /// The idea is that <see cref="FetchDataAsync"/> does API calls and sets the initial values
    /// and then it's up to the implementer to call <see cref="_refreshTimer"/>.start() at the end of it.
    /// The timer will automatically call <see cref="UpdateDataAsync(object?, ElapsedEventArgs)"/> which is 
    /// to be used to make more API calls to update the initially obtained data.
    /// </summary>
    public abstract class AutoUpdateViewModelBase : INotifyPropertyChanged
    {
        protected string _infoHash = "";
        protected DispatcherTimer _refreshTimer = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AutoUpdateViewModelBase()
        {
            _refreshTimer.Tick += TimerTick;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(1500); // Set your desired interval
        }

        /// <summary>
        /// Called by <see cref="_refreshTimer"/> and calls 
        /// <see cref="UpdateDataAsync(object?, ElapsedEventArgs)"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object? sender, EventArgs e)
        {
            UpdateDataAsync(sender, e);
        }

        protected abstract Task UpdateDataAsync(object? sender, EventArgs e);

        /// <summary>
        /// Run API requests and populate fields
        /// 
        /// Generally it's a good idea to call _refreshTimer.Start() at the end of this method
        /// so that the timer starts working on updates.
        /// </summary>
        /// <returns></returns>
        protected abstract Task FetchDataAsync();

        /// <summary>
        /// Pauses <see cref="_refreshTimer"/> which needs to be done in order for the garbage collector
        /// to dispose of the instance of this class.
        /// </summary>
        public void Pause()
        {
            _refreshTimer.Stop();
        }
    }
}

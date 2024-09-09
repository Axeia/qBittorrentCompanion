using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.Services;
using qBittorrentCompanion.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Views
{
    public abstract class ShakerWindow : Window
    {
        private TorrentInfoViewModel? _torrentInfoViewModel;

        protected abstract void SaveButton_Click(object? sender, RoutedEventArgs e);

        /// <summary>
        /// Briefly shakes the window to indicate an error has occured and grab the users attention
        /// </summary>
        /// <returns></returns>
        protected async Task ShakeWindowAsync()
        {
            var originalLeft = this.Position.X;
            var rnd = new Random();

            const int shakeAmplitude = 10;
            const int shakeDuration = 500; // in milliseconds
            const int shakeInterval = 20; // in milliseconds

            var endTime = DateTime.Now.AddMilliseconds(shakeDuration);

            while (DateTime.Now < endTime)
            {
                var offsetX = rnd.Next(-shakeAmplitude, shakeAmplitude);

                this.Position = new PixelPoint(originalLeft + offsetX, this.Position.Y);

                await Task.Delay(shakeInterval);
            }

            // Restore original position
            this.Position = new PixelPoint(originalLeft, this.Position.Y);
        }


    }
}

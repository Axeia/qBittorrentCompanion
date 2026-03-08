using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qBittorrentCompanion.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class DownloadService(HttpClient? httpClient = null)
    {
        private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

        /// <summary>
        /// Downloads a file and reports progress as a percentage (0-100).
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <param name="destinationPath">The local path to save the file.</param>
        /// <param name="progress">An IProgress provider to report percentage updates.</param>
        /// <param name="ct">Cancellation token to stop the download.</param>
        public async Task DownloadFileWithProgressAsync(
            string url,
            string destinationPath,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            // Get the response headers first to find the file size
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalReadBytes = 0;
            int readBytes;

            while ((readBytes = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, readBytes), ct);
                totalReadBytes += readBytes;

                if (totalBytes.HasValue && progress != null)
                {
                    // Calculate percentage
                    double percentage = Math.Round((double)totalReadBytes / totalBytes.Value * 100, 2);
                    progress.Report(percentage);
                }
            }

            // Final report to ensure we hit 100%
            progress?.Report(100);
        }
    }
}

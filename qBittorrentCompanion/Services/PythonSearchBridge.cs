using QBittorrent.Client;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace qBittorrentCompanion.Services
{
    public class PythonSearchBridge
    {
        private static readonly string _searchPluginDirectory = Path.Combine(AppContext.BaseDirectory, "nova3");
        private Process? _pythonProcess;

        public Action<SearchResult>? SearchResultProcessed;
        public Action? SearchPluginProcessed;

        public static async Task<List<SearchPlugin>> GetSearchPluginsThroughNova2()
        {
            List<SearchPlugin> plugins = [];

            ProcessStartInfo processStartInfo = new("python")
            {
                Arguments = "nova2.py --capabilities",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _searchPluginDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(processStartInfo);
            string output = await process!.StandardOutput.ReadToEndAsync();
            string outputErrornous = await process!.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var doc = XDocument.Parse(output);

            foreach (var engine in doc.Root!.Elements())
            {
                string id = engine.Name.LocalName;
                string name = engine.Element("name")?.Value ?? id;
                string url = engine.Element("url")?.Value ?? string.Empty;
                List<SearchPluginCategory> categories = [new SearchPluginCategory(SearchPlugin.All, "All categories")];
                List<SearchPluginCategory> categoriesFromXml = [
                    .. (engine.Element("category")?.Value.Split(' ') ?? []).Select(t => new SearchPluginCategory(t))
                ];
                categories.AddRange(categoriesFromXml);

                plugins.Add(new SearchPlugin()
                {
                    Name = name,
                    Url = new Uri(url),
                    Categories = categories.AsReadOnly(),
                });
            }

            return plugins;
        }

        public async Task StartSearchAsync(IEnumerable<string> plugins, string searchFor, string category = SearchPlugin.All)
        {
            ProcessStartInfo processStartInfo = new("python")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            processStartInfo.ArgumentList.Add("nova2.py");
            foreach(string plugin in plugins)
                processStartInfo.ArgumentList.Add(plugin);
            processStartInfo.ArgumentList.Add(category);
            processStartInfo.ArgumentList.Add(searchFor);
            processStartInfo.ArgumentList.Insert(0, "-u"); // unbuffered output
            processStartInfo.WorkingDirectory = _searchPluginDirectory;

            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                GetFullTypeName<PythonSearchBridge>(),
                $"Starting process: {processStartInfo.FileName} {string.Join(" ", processStartInfo.ArgumentList)}"
            );

            _pythonProcess = new() { StartInfo = processStartInfo, EnableRaisingEvents = true };

            _pythonProcess.OutputDataReceived += _pythonProcess_OutputDataReceived;
            _pythonProcess.ErrorDataReceived += _pythonProcess_ErrorDataReceived;

            _pythonProcess.Start();
            _pythonProcess.BeginOutputReadLine();
            _pythonProcess.BeginErrorReadLine();

            await _pythonProcess.WaitForExitAsync();
            _pythonProcess.OutputDataReceived -= _pythonProcess_OutputDataReceived;
        }

        private void _pythonProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                AppLoggerService.AddLogMessage(
                    LogLevel.Error, 
                    GetFullTypeName<PythonSearchBridge>(),
                    $"[Python Error]",
                    e.Data,
                    "Python error output:"
                );
            }
        } 

        private void _pythonProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                SearchResult searchResult = ParseSearchResultLine(e.Data);
                SearchResultProcessed?.Invoke(searchResult);
            }
        }

        private static SearchResult ParseSearchResultLine(string resultLine)
        {
            Debug.WriteLine(resultLine);
            // Based on https://github.com/qbittorrent/search-plugins/wiki/How-to-write-a-search-plugin/#prettyprinter-helper-function
            var (Url, Name, Size, Seeds, Leech, EngineUrl, DescLink, PubDate) = resultLine.Split('|') switch
            {
                [var u, var n, var b, var s, var l, var e, var d, var p] => (u, n, b, s, l, e, d, p),
                _ => default
            };

            SearchResult searchResult = new()
            {
                FileUrl = new Uri(Url),
                FileName = Name,
                FileSize = long.TryParse(Size, out var size) ? size : 0,
                Seeds = long.TryParse(Seeds, out var seeds) ? seeds : -1,
                Leechers = long.TryParse(Leech, out var leech) ? leech : -1,
                SiteUrl = new Uri(EngineUrl),
                DescriptionUrl = new Uri(DescLink)
            };

            return searchResult;
        }
    }
}

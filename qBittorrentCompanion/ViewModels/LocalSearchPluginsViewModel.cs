using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using HtmlAgilityPack;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class LocalSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        public static string SearchPluginWikiLink => "https://github.com/qbittorrent/search-plugins/wiki/Unofficial-search-plugins";

        public ReactiveCommand<Unit, Unit> HideGithubPluginWarningCommand { get; }
        public ReactiveCommand<Unit, Unit> HideLocalPluginCopyrightWarningCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshLocalSearchPluginsCommand { get; }
        public ReactiveCommand<Unit, Unit> InstallGitSearchPluginCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearOutNonPluginPyFilesCommand { get; }
        

        private bool _showGithubPluginWarning = Design.IsDesignMode || ConfigService.ShowGithubPluginWarning;
        public bool ShowGithubPluginWarning
        {
            get => _showGithubPluginWarning;
            set
            {
                if (_showGithubPluginWarning != value)
                {
                    _showGithubPluginWarning = value;
                    ConfigService.ShowGithubPluginWarning = value;
                    this.RaisePropertyChanged(nameof(ShowGithubPluginWarning));
                }
            }
        }

        private bool _showLocalPluginCopyrightWarning = Design.IsDesignMode || ConfigService.ShowGithubPluginWarning;
        public bool ShowLocalPluginCopyrightWarning
        {
            get => _showLocalPluginCopyrightWarning;
            set
            {
                if (_showLocalPluginCopyrightWarning != value)
                {
                    _showLocalPluginCopyrightWarning = value;
                    ConfigService.ShowLocalPluginCopyrightWarning = value;
                    this.RaisePropertyChanged(nameof(ShowLocalPluginCopyrightWarning));
                }
            }
        }

        [AutoPropertyChanged]
        private ObservableCollection<GitSearchPluginViewModel> _gitPublicSearchPlugins = [];
        [AutoPropertyChanged]
        private ObservableCollection<GitSearchPluginViewModel> _gitPrivateSearchPlugins = [];
        [AutoPropertyChanged]
        private GitSearchPluginViewModel? _selectedGitSearchPlugin = null;
        [AutoPropertyChanged]
        private string _statusMessage = string.Empty;
        [AutoPropertyChanged]
        private bool _fetchingOrParsingWiki = false;

        /// <summary>
        /// <see cref="SearchPluginsViewModelBase.SearchPlugins"/> should update automatically if a file is deleted as
        /// <see cref="LocalSearchPluginService"/> monitors the plugin directory for file changes.
        /// </summary>
        protected void UninstallSearchPlugin()
        {
            if (SelectedSearchPlugin is LocalSearchPluginViewModel lspvm)
            {
                string filePath = Path.Combine(LocalSearchPluginService.SearchEngineDirectory, lspvm.FileName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        AppLoggerService.AddLogMessage(
                            Splat.LogLevel.Info,
                            GetFullTypeName<LocalSearchPluginsViewModel>(),
                            $"Deleted {lspvm.FileName}"
                        );
                    }
                    catch(Exception e)
                    {
                        AppLoggerService.AddLogMessage(
                            Splat.LogLevel.Error,
                            GetFullTypeName<LocalSearchPluginsViewModel>(),
                            $"Error deleting {lspvm.FileName}",
                            e.Message,
                            e.GetType().Name
                        );
                    }
                }
                else
                {
                    AppLoggerService.AddLogMessage(
                        Splat.LogLevel.Error,
                        GetFullTypeName<LocalSearchPluginsViewModel>(),
                        $"Could not find {lspvm.FileName}",
                        $"File could not be found: \n {filePath}"
                    );
                }
            }
        }

        protected void ToggleEnabledSearchPlugin(bool enable)
        {
            if (SelectedSearchPlugin != null)
            {
                // As list for LINQ
                var disabledLocalSearchPlugins = ConfigService.DisabledLocalSearchPlugins.ToList();
                if (enable)
                {
                    // Remove from disabled list
                    if (disabledLocalSearchPlugins.Remove(SelectedSearchPlugin.Name))
                        // Only re-assign if something was actually removed
                        ConfigService.DisabledLocalSearchPlugins = [.. disabledLocalSearchPlugins];
                }
                else
                {
                    if (!disabledLocalSearchPlugins.Contains(SelectedSearchPlugin.Name))
                    {
                        disabledLocalSearchPlugins.Add(SelectedSearchPlugin.Name);
                        ConfigService.DisabledLocalSearchPlugins = [.. disabledLocalSearchPlugins];
                    }
                }
            }
        }

        public LocalSearchPluginsViewModel() : base() 
        {
            LocalSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPlugins_CollectionChanged;
            SearchPlugins_CollectionChanged(null, EventArgs.Empty); // Initial populate
            LocalSearchPluginService.Instance.NonPluginPythonFilesChanged += NonPluginPythonFiles_Change;
            NonPluginPythonFiles_Change(LocalSearchPluginService.Instance.NonSearchPluginPythonFileNames);

            if (!Design.IsDesignMode)
                _ = FetchDataAsync();

            HideGithubPluginWarningCommand = 
                ReactiveCommand.Create(() => { ShowGithubPluginWarning = false;  });
            HideLocalPluginCopyrightWarningCommand = 
                ReactiveCommand.Create(() => { ShowLocalPluginCopyrightWarning = false; });
            UninstallSearchPluginCommand = ReactiveCommand.Create(
                () => UninstallSearchPlugin(),
                this.WhenAnyValue(vm => vm.SelectedSearchPlugin)
                    .Select(plugin => plugin != null)
            );
            RefreshLocalSearchPluginsCommand = ReactiveCommand.CreateFromTask(LocalSearchPluginService.Instance.UpdateSearchPluginsAsync);
            InstallGitSearchPluginCommand = ReactiveCommand.CreateFromTask(DownloadGitSearchPluginAsync);
            ClearOutNonPluginPyFilesCommand = ReactiveCommand.Create(
                LocalSearchPluginService.Instance.ClearOutNonPluginPyFiles,
                this.WhenAnyValue(vm => vm.HasNonSearchPluginPythonFile)
                    .Select(b => b)
            );
        }

        private string[] _nonSearchPluginPythonFiles = [];
        public string[] NonSearchPluginPythonFiles
        {
            get => _nonSearchPluginPythonFiles;
            set
            {
                if (_nonSearchPluginPythonFiles != value)
                {
                    this.RaiseAndSetIfChanged(ref _nonSearchPluginPythonFiles, value);
                    this.RaisePropertyChanged(nameof(HasNonSearchPluginPythonFile));
                }
            }
        }

        public bool HasNonSearchPluginPythonFile => NonSearchPluginPythonFiles.Length > 0;

        private void NonPluginPythonFiles_Change(string[] nonSearchPluginPythonFileNames)
        {
            NonSearchPluginPythonFiles = nonSearchPluginPythonFileNames;
        }

        private async Task DownloadGitSearchPluginAsync()
        {
            // Ensure a plugin is selected
            if (SelectedGitSearchPlugin is null || SelectedGitSearchPlugin.DownloadUri is null)
            {
                throw new InvalidOperationException("No valid plugin or download URI selected.");
            }

            using var client = new HttpClient();
            string pythonContent;
            HttpResponseMessage? response = null;

            try
            {
                response = await client.GetAsync(SelectedGitSearchPlugin.DownloadUri);
                response.EnsureSuccessStatusCode(); // This will throw HttpRequestException for 4xx/5xx status codes.

                pythonContent = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                AppLoggerService.AddLogMessage(Splat.LogLevel.Error, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Encountered {e.StatusCode} trying to download plugin", e.Message, $"Error {e.StatusCode}");
                return;
            }

            // This code will only run if the request was successful
            string fileName = string.Empty;

            // Try to get the filename from the Content-Disposition header
            if (response.Content.Headers.ContentDisposition?.FileName is string headerFileName)
            {
                fileName = headerFileName;
            }
            else
            {
                // Content-Disposition header not set? Try to get it from the URL
                fileName = Path.GetFileName(SelectedGitSearchPlugin.DownloadUri.LocalPath);

                // No luck getting a file name - give up
                if (string.IsNullOrEmpty(fileName))
                {
                    AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Couldn't find a name for {SelectedGitSearchPlugin.Name}");
                    return;
                }
            }
                        
            fileName = Path.GetFileName(fileName); // Sanatize the file name
            string filePath = Path.Combine(LocalSearchPluginService.SearchEngineDirectory, fileName);
            await File.WriteAllTextAsync(filePath, pythonContent);

            AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Successfully downloaded plugin to {filePath}");
        }

        private void SearchPlugins_CollectionChanged(object? sender, EventArgs e)
        {
            SearchPlugins.Clear();
            var updatedSearchPlugins = LocalSearchPluginService.Instance.SearchPlugins
                .Where(sp => sp.Name != SearchPlugin.All && sp.Name != SearchPlugin.Enabled);

            foreach (var searchPlugin in updatedSearchPlugins)
                SearchPlugins.Add(searchPlugin);
        }

        protected override async Task FetchDataAsync()
        {
            GitPublicSearchPlugins.Clear();

            using var client = new HttpClient();
            string html = string.Empty;

            FetchingOrParsingWiki = true;
            AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), "Contacting github wiki...");
            try
            {
                html = await client.GetStringAsync(SearchPluginWikiLink);
            }
            catch (HttpRequestException e)
            {
                AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), "Problem contacting github wiki",
                    e.Message, "HTTP Status " + e.StatusCode);
                return;
            }

            AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), "Contacted github wiki");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var xPathedTables = doc.DocumentNode.SelectNodes("//div[@id='wiki-wrapper']//table");

            if (xPathedTables is null)
            {
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Warn,
                    GetFullTypeName<LocalSearchPluginsViewModel>(),
                    "Failed to located search plugin tables",
                    html,
                    "github.com"
                );
                return;
            }

            if (xPathedTables.Count >= 2)
            {
                TableToSearchPlugins(xPathedTables[0], GitPublicSearchPlugins, "public");
                TableToSearchPlugins(xPathedTables[1], GitPrivateSearchPlugins, "private");
            }
            else
                AppLoggerService.AddLogMessage(Splat.LogLevel.Warn, GetFullTypeName<LocalSearchPluginsViewModel>(), "Problem parsing github wiki");

            FetchingOrParsingWiki = false;
        }

        private static void TableToSearchPlugins(HtmlNode pluginsTable, ObservableCollection<GitSearchPluginViewModel> gitSearchPlugins, string logMessageId)
        {
            IEnumerable<HtmlNode> rows = pluginsTable.SelectNodes(".//tr").Skip(1);
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//td");

                if (cells is null || cells.Count < 5)
                    continue;

                GitSearchPluginViewModel gspvm = new(
                    name: cells[0].InnerText.Trim(),
                    author: SanatizeAnchors(cells[1]),
                    version: cells[2].InnerText.Trim(),
                    lastUpdate: cells[3].InnerText.Trim(),
                    downloadUri: cells[4].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "",
                    infoUri: cells[4].SelectSingleNode(".//a[2]")?.GetAttributeValue("href", "") ?? null,
                    comments: cells[5].InnerText.Trim()
                );

                gitSearchPlugins.Add(gspvm);
            }

            if (gitSearchPlugins.Count > 0)
                AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Wiki table {logMessageId} parsed - {gitSearchPlugins.Count} plugins");
            else
                AppLoggerService.AddLogMessage(Splat.LogLevel.Error, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Wiki table {logMessageId} has no plugins");
        }

        private static string SanatizeAnchors(HtmlNode htmlNode)
        {
            var anchors = htmlNode.SelectNodes(".//a");
            if (anchors == null)
                return string.Empty;

            List<string> safeLinks = [];

            foreach (var a in anchors)
            {
                string href = a.GetAttributeValue("href", "").Trim();
                string text = a.InnerText.Trim();

                if (Uri.TryCreate(href, UriKind.Absolute, out Uri? uri))
                {
                    // Only allow schemes that would actually get used for an author
                    if (uri.Scheme == Uri.UriSchemeHttp ||
                        uri.Scheme == Uri.UriSchemeHttps ||
                        uri.Scheme == Uri.UriSchemeMailto)
                    {
                        safeLinks.Add($"<a href='{href}'>{text}</a>");
                    }
                }
            }

            // Return as comma separated
            return string.Join(",", safeLinks);
        }
    }
}

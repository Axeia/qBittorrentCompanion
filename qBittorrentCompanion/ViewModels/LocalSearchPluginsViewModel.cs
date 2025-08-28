using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using HtmlAgilityPack;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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

        protected void UninstallSearchPlugin(Unit unit)
        {
            //await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            //await Initialise()
        }

        protected void ToggleEnabledSearchPlugin(bool enable)
        {
            //if( SelectedSearchPlugin != null )
            //    SelectedSearchPlugin.IsEnabled = enable;

            //await Initialise();
        }

        public LocalSearchPluginsViewModel() : base() 
        {
            LocalSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPlugins_CollectionChanged;
            SearchPlugins_CollectionChanged(null, NotifyCollectionChangedEventArgs.Empty); // Initial populate

            if (!Design.IsDesignMode)
                _ = FetchDataAsync();

            HideGithubPluginWarningCommand = 
                ReactiveCommand.Create(() => { ShowGithubPluginWarning = false;  });
            HideLocalPluginCopyrightWarningCommand = 
                ReactiveCommand.Create(() => { ShowLocalPluginCopyrightWarning = false; });
        }

        private void SearchPlugins_CollectionChanged(object? sender, EventArgs e)
        {
            SearchPlugins.Clear();
            var updatedSearchPlugins = LocalSearchPluginService.Instance.SearchPlugins
                .Where(sp => sp.Name != SearchPlugin.All && sp.Name != SearchPlugin.Enabled);

            foreach (var searchPlugin in updatedSearchPlugins)
                SearchPlugins.Add(new SearchPluginViewModel(searchPlugin));
        }

        protected override async Task FetchDataAsync()
        {
            GitPublicSearchPlugins.Clear();

            StatusMessage = "Attempting to fetch SearchPlugin data from github...";
            using var client = new HttpClient();
            string html = string.Empty;

            try
            {
                html = await client.GetStringAsync(SearchPluginWikiLink);
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Problem connecting to github. Internet problems?";
                return;
            }

            StatusMessage = "Contacted github - attempting to process HTML";

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var xPathedTables = doc.DocumentNode.SelectNodes("//div[@id='wiki-wrapper']//table");
            if (xPathedTables is null)
            {
                AppLoggerService.AddLogMessage(
                    Splat.LogLevel.Warn,
                    GetFullTypeName<LocalSearchPluginsViewModel>(),
                    "Unable to find tables on github unofficial-search-plugins page",
                    html,
                    "github.com"
                );
                return;
            }

            TableToSearchPlugins(xPathedTables[0], GitPublicSearchPlugins);
            TableToSearchPlugins(xPathedTables[1], GitPrivateSearchPlugins);

            if (GitPublicSearchPlugins.Count > 0)
                StatusMessage = "Succesfully retrieved plugins from github";
            else
                StatusMessage = "Could not process github page. Contact QBC developer";
        }

        private static void TableToSearchPlugins(HtmlNode pluginsTable, ObservableCollection<GitSearchPluginViewModel> gitSearchPlugins)
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

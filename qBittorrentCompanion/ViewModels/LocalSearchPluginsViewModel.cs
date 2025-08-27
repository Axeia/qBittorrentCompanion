using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using HtmlAgilityPack;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class LocalSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        public static string SearchPluginWikiLink => "https://github.com/qbittorrent/search-plugins/wiki/Unofficial-search-plugins";

        [AutoPropertyChanged]
        private ObservableCollection<GitSearchPluginViewModel> _gitSearchPlugins = [];
        [AutoPropertyChanged]
        private string _statusMessage = string.Empty;

        protected override async Task<Unit> UninstallSearchPluginAsync(Unit unit)
        {
            //await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            //await Initialise();
            return Unit.Default;
        }

        protected override async Task<Unit> ToggleEnabledSearchPluginAsync(bool enable)
        {
            //if( SelectedSearchPlugin != null )
            //    SelectedSearchPlugin.IsEnabled = enable;

            //await Initialise();
            return Unit.Default;
        }

        public LocalSearchPluginsViewModel() : base() 
        {
            LocalSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPlugins_CollectionChanged;
            SearchPlugins_CollectionChanged(null, NotifyCollectionChangedEventArgs.Empty); // Initial populate

            if (!Design.IsDesignMode)
                _ = FetchDataAsync();

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
            GitSearchPlugins.Clear();

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

            HtmlNode publicGitPlugins = xPathedTables.First();
            IEnumerable<HtmlNode> rows = publicGitPlugins.SelectNodes(".//tr").Skip(1);
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//td");

                if (cells is null || cells.Count < 5)
                    continue;

                GitSearchPluginViewModel gspvm = new(
                    name: cells[0].InnerText.Trim(),
                    author: cells[1].InnerText.Trim(),
                    version: cells[2].InnerText.Trim(),
                    lastUpdate: cells[3].InnerText.Trim(),
                    downloadUri: cells[4].SelectSingleNode(".//a")?.GetAttributeValue("href", "") ?? "",
                    infoUrl: cells[4].SelectSingleNode(".//a[1]")?.GetAttributeValue("href", "") ?? "",
                    comments: cells[5].InnerText.Trim()
                );

                GitSearchPlugins.Add(gspvm);
            }

            if (GitSearchPlugins.Count > 0)
                StatusMessage = "Succesfully retrieved plugins from github";
            else
                StatusMessage = "Could not process github page. Contact QBC developer";
        }
    }
}

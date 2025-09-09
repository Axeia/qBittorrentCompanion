using AutoPropertyChangedGenerator;
using Avalonia.Controls;
using HtmlAgilityPack;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public static class SearchPluginUriValidator
    {
        public static ValidationResult ValidateRemoteOrLocalUri(string? input, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult("URI cannot be empty");
            }

            if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeFile || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult("Must be a file:/// or http(s):// URI");
        }
        public static ValidationResult ValidateRemoteUri(string? input, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult("URL cannot be empty");
            }

            if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeFile || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult("Must be a http(s):// URL");
        }
    }
    /// <summary>
    /// Serves as a common base for <see cref="LocalSearchPluginsViewModel"/> and <see cref="RemoteSearchPluginsViewModel"/>
    /// to combine what they have in common.
    /// <br/><br/>
    /// [25/09/07] Tried adding a <c>MarkAsInstalled</c> property on <see cref="GitSearchPluginViewModel"/> and setting it on collection change to any of the 
    /// plugin lists. However the check would have to be quite thorough and wouldn't be reliable as the wiki name column is under no obiligation to actually match 
    /// the plugin name. Discrepencies turned out to be very common.
    /// </summary>
    public abstract partial class SearchPluginsViewModelBase : ViewModelBase
    {
        [CustomValidation(typeof(SearchPluginsViewModelBase), nameof(ValidateAddSearchPluginUri))]
        public string AddSearchPluginUri
        {
            get => _addRemoteSearchPluginUri;
            set => this.RaiseAndSetIfChanged(ref _addRemoteSearchPluginUri, value);
        }

        public static ValidationResult ValidateAddSearchPluginUri(string? input, ValidationContext context)
        {
            if (context.ObjectInstance is SearchPluginsViewModelBase vm)
            {
                return vm.ValidateUri(input);
            }

            return new ValidationResult("Invalid context");
        }

        protected virtual ValidationResult ValidateUri(string? input)
        {
            // Default fallback if not overridden
            return new ValidationResult("Validation not implemented");
        }

        /// <summary>
        /// Call SearchPlugins.Clear() and .Add from the relevant service
        /// (skip 'All' and 'Enabled' as they won't be relevant in this context)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void SearchPlugins_CollectionChanged(object? sender, EventArgs e);

        public static string SearchPluginWikiLink => "https://github.com/qbittorrent/search-plugins/wiki/Unofficial-search-plugins";
        protected string _addRemoteSearchPluginUri = string.Empty;

        public string WindowTitle { get; }
        [AutoPropertyChanged]
        private ObservableCollection<RemoteSearchPluginViewModel> _searchPlugins = [];
        [AutoPropertyChanged]
        private RemoteSearchPluginViewModel? _selectedSearchPlugin = null;
        [AutoPropertyChanged]
        private ObservableCollection<GitSearchPluginViewModel> _gitPublicSearchPlugins = [];
        [AutoPropertyChanged]
        private ObservableCollection<GitSearchPluginViewModel> _gitPrivateSearchPlugins = [];
        [AutoPropertyChanged]
        private GitSearchPluginViewModel? _selectedGitHubSearchPlugin = null;
        [AutoPropertyChanged]
        private bool _fetchingOrParsingWiki = false;

        public ReactiveCommand<Unit, Unit> HideGithubSearchPluginWarningCommand { get; }
        public ReactiveCommand<Unit, Unit> HideSearchPluginCopyrightWarningCommand { get; }
        public ReactiveCommand<Unit, Unit> InstallGitSearchPluginCommand { get; }
        public ReactiveCommand<Unit, Unit> InstallSearchPluginCommand { get; }

        public IObservable<bool> IsValidPluginUriObservable;

        [AutoPropertyChanged]
        private bool _expandAddRemoteSearchPluginUri = false;
        [AutoPropertyChanged]
        private string _addRemoteSearchPluginUriErrorMessage = string.Empty;


        private bool _showGitHubSearchPluginDetailLabelText = Design.IsDesignMode || ConfigService.ShowGitHubSearchPluginDetailLabelText;
        public bool ShowGitHubSearchPluginDetailLabelText
        {
            get => _showGitHubSearchPluginDetailLabelText;
            set
            {
                if (_showGitHubSearchPluginDetailLabelText != value)
                {
                    _showGitHubSearchPluginDetailLabelText = value;
                    ConfigService.ShowGitHubSearchPluginDetailLabelText = value;
                    this.RaisePropertyChanged(nameof(ShowGitHubSearchPluginDetailLabelText));
                }
            }
        }

        private bool _showGitHubSearchPluginAllDetails = Design.IsDesignMode || ConfigService.ShowGitHubSearchPluginAllDetails;
        public bool ShowGitHubSearchPluginAllDetails
        {
            get => _showGitHubSearchPluginAllDetails;
            set
            {
                if (_showGitHubSearchPluginAllDetails != value)
                {
                    _showGitHubSearchPluginAllDetails = value;
                    ConfigService.ShowGitHubSearchPluginAllDetails = value;
                    this.RaisePropertyChanged(nameof(ShowGitHubSearchPluginAllDetails));
                }
            }
        }

        private bool _showGitHubSearchPluginWarning = Design.IsDesignMode || ConfigService.ShowGithubSearchPluginWarning;
        public bool ShowGitHubSearchPluginWarning
        {
            get => _showGitHubSearchPluginWarning;
            set
            {
                if (_showGitHubSearchPluginWarning != value)
                {
                    _showGitHubSearchPluginWarning = value;
                    ConfigService.ShowGithubSearchPluginWarning = value;
                    this.RaisePropertyChanged(nameof(ShowGitHubSearchPluginWarning));
                }
            }
        }

        private bool _showSearchPluginCopyrightWarning = Design.IsDesignMode || ConfigService.ShowSearchPluginCopyrightWarning;
        public bool ShowSearchPluginCopyrightWarning
        {
            get => _showSearchPluginCopyrightWarning;
            set
            {
                if (_showSearchPluginCopyrightWarning != value)
                {
                    _showSearchPluginCopyrightWarning = value;
                    ConfigService.ShowSearchPluginCopyrightWarning = value;
                    this.RaisePropertyChanged(nameof(ShowSearchPluginCopyrightWarning));
                }
            }
        }

        public ReactiveCommand<Unit, Unit> UninstallSearchPluginCommand { get; set; }
        public bool IsPopulating { get; set; } = false;

        public ReactiveCommand<Unit, Unit> RefreshGitHubSearchPluginsCommand { get; }
        // Surpressing non-nullable warnings, enheriting class should set it.
        // This is regarding UninstalSearchPluginCommand and RefreshCommand
#pragma warning disable CS8618 
        public SearchPluginsViewModelBase(string windowTitle = "Base Search Plugin Manager")
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            HideGithubSearchPluginWarningCommand =
                ReactiveCommand.Create(() => { ShowGitHubSearchPluginWarning = false; });
            HideSearchPluginCopyrightWarningCommand =
                ReactiveCommand.Create(() => { ShowSearchPluginCopyrightWarning = false; });
            InstallGitSearchPluginCommand = ReactiveCommand.CreateFromTask(
                DownloadGitSearchPluginAsync,
                this.WhenAnyValue(x => x.SelectedGitHubSearchPlugin).Select(plugin => plugin != null)
            );

            RefreshGitHubSearchPluginsCommand = ReactiveCommand.CreateFromTask(FetchGitHubPluginDataSync);

            InstallSearchPluginCommand =
                ReactiveCommand.CreateFromTask(InstallSearchPluginAsync, IsValidPluginUriObservable);

            WindowTitle = windowTitle;

            if(!Design.IsDesignMode)
                _ = FetchGitHubPluginDataSync();

            this.WhenAnyValue(x => x.ExpandAddRemoteSearchPluginUri)
                .Subscribe((b) => { if (b == false) { AddRemoteSearchPluginUriErrorMessage = string.Empty; AddSearchPluginUri = string.Empty; } });
        }

        protected abstract Task InstallSearchPluginAsync();

        private async Task FetchGitHubPluginDataSync()
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

        /// <summary>
        /// Upon success set ExpandAddRemoteSearchPluginUri to false
        /// and AddSearchPluginUri to string.empty.
        /// That will cause the UI to reset and hide the controls.
        /// 
        /// If it fails, set AddRemoteSearchPluginUriErrorMessage to the error message
        /// </summary>
        /// <returns></returns>
        protected abstract Task DownloadGitSearchPluginAsync();

        private static void TableToSearchPlugins(HtmlNode pluginsTable, ObservableCollection<GitSearchPluginViewModel> gitSearchPlugins, string logMessageId)
        {
            // Skip header row
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
                AppLoggerService.AddLogMessage(Splat.LogLevel.Error, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Wiki table {logMessageId} has no plugins", pluginsTable.OuterHtml);
        }

        /// <summary>
        /// Never trust anything. 
        /// The only thing we need is the href and the inner text.
        /// 
        /// If the href is not http://, https:// or mailto:// it might be malicious and will be ignored.
        /// 
        /// </summary>
        /// <param name="htmlNode"></param>
        /// <returns></returns>
        private static string SanatizeAnchors(HtmlNode htmlNode)
        {
            var anchors = htmlNode.SelectNodes(".//a");
            if (anchors == null)
                return string.Empty;

            List<string> sanatizedLinks = [];

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
                        sanatizedLinks.Add($"<a href='{href}'>{text}</a>");
                    }
                }
            }

            // Return as comma separated
            return string.Join(",", sanatizedLinks);
        }
    }
}

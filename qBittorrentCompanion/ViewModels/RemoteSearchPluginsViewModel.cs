using Avalonia.Controls;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    public partial class RemoteSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        protected override ValidationResult ValidateUri(string? input)
            => SearchPluginUriValidator.ValidateRemoteOrLocalUri(input, new ValidationContext(this));

        public ReactiveCommand<Unit, Unit> RefreshRemoteSearchPluginsCommand { get; }

        private async Task UninstallSearchPluginAsync()
        {
            await QBittorrentService.UninstallSearchPluginAsync(SelectedSearchPlugin!.Name);
            await WaitForQbittorrentPluginSyncAsync();
            await FetchDataAsync();
        }

        public RemoteSearchPluginsViewModel() : base("Remote Search Plugins Manager")
        {
            UninstallSearchPluginCommand = 
                ReactiveCommand.CreateFromTask(UninstallSearchPluginAsync);
            RefreshRemoteSearchPluginsCommand = ReactiveCommand.CreateFromTask(FetchDataAsync);

            if (!Design.IsDesignMode)
                _ = FetchDataAsync();

            IsValidPluginUriObservable =
                this.WhenAnyValue(x => x.AddSearchPluginUri)
                    .Select(uri =>
                    Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
                    && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps));
        }

        protected override async Task InstallSearchPluginAsync()
        {
            Uri uri = new(AddSearchPluginUri);
            await QBittorrentService.InstallSearchPluginAsync(uri);
            // Only set on success - TODO: Figure out success.
            await WaitForQbittorrentPluginSyncAsync();
            AddSearchPluginUri = string.Empty;
            ExpandAddRemoteSearchPluginUri = false;
            await FetchDataAsync();
        }

        protected async Task FetchDataAsync()
        {
            IsPopulating = true;
            SearchPlugins.Clear();

            IReadOnlyList<SearchPlugin>? plugins = await QBittorrentService.GetSearchPluginsAsync();
            if (plugins != null)
            {
                foreach (SearchPlugin plugin in plugins)
                    SearchPlugins.Add(new RemoteSearchPluginViewModel(plugin));
            }

            SelectedSearchPlugin = SearchPlugins.FirstOrDefault();
            IsPopulating = false;
        }

        protected override async Task DownloadGitSearchPluginAsync()
        {
            await QBittorrentService.InstallSearchPluginAsync(SelectedGitHubSearchPlugin!.DownloadUri!);
            await WaitForQbittorrentPluginSyncAsync();
            await FetchDataAsync();
        }

        /// <summary>
        /// There's some delay between qBittorrent responding to a SearchPlugins changed (deleting or adding one) request
        /// and it actually have processed the change.
        /// Presumably this is due python running async and needing a little bit of extra time to spin up
        /// 
        /// This method gives qBittorrent a little bit of extra time to process the changes.
        /// </summary>
        /// <returns></returns>
        private static async Task WaitForQbittorrentPluginSyncAsync()
        {
            await Task.Delay(200);
        }
    }
}

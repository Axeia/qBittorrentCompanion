using DynamicData;
using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// Manages locally installed search plugins (those installed on this system) using 
    /// <see cref="LocalSearchPluginService"/>, <see cref="LocalSearchPluginViewModel"/> and <see cref="PythonSearchBridge"/>
    /// </summary>
    public partial class LocalSearchPluginsViewModel : SearchPluginsViewModelBase
    {
        protected override ValidationResult ValidateUri(string? input)
            => SearchPluginUriValidator.ValidateRemoteUri(input, new ValidationContext(this));

        public ReactiveCommand<Unit, Unit> RefreshLocalSearchPluginsCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearOutNonPluginPyFilesCommand { get; }

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

        public LocalSearchPluginsViewModel() : base("Local Search Plugin Manager") 
        {
            LocalSearchPluginService.Instance.SearchPlugins.CollectionChanged += SearchPlugins_CollectionChanged;
            SearchPlugins_CollectionChanged(null, EventArgs.Empty); // Initial populate
            LocalSearchPluginService.Instance.NonPluginPythonFilesChanged += NonPluginPythonFiles_Change;
            NonPluginPythonFiles_Change(LocalSearchPluginService.Instance.NonSearchPluginPythonFileNames);

            ClearOutNonPluginPyFilesCommand = ReactiveCommand.Create(
                LocalSearchPluginService.Instance.ClearOutNonPluginPyFiles,
                this.WhenAnyValue(vm => vm.HasNonSearchPluginPythonFile)
                    .Select(b => b)
            );

            RefreshLocalSearchPluginsCommand = ReactiveCommand.CreateFromTask(LocalSearchPluginService.Instance.UpdateSearchPluginsAsync);
            UninstallSearchPluginCommand = ReactiveCommand.Create(
                () => UninstallSearchPlugin(),
                this.WhenAnyValue(vm => vm.SelectedSearchPlugin)
                    .Select(plugin => plugin != null)
            );

            IsValidPluginUriObservable =
                this.WhenAnyValue(x => x.AddSearchPluginUri)
                    .Select(uri =>
                    Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
                    && (parsed.Scheme == Uri.UriSchemeFile || parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps));
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

        protected override void SearchPlugins_CollectionChanged(object? sender, EventArgs e)
        {
            SearchPlugins.Clear();
            // Only get actual plugins, not 'All' and 'Enabled'
            SearchPlugins.Add(LocalSearchPluginService.Instance.SearchPlugins.Skip(2));
        }

        protected override async Task DownloadGitSearchPluginAsync()
        {
            // Ensure a plugin is selected
            if (SelectedGitHubSearchPlugin is null || SelectedGitHubSearchPlugin.DownloadUri is null)
            {
                throw new InvalidOperationException("No valid plugin or download URI selected.");
            }

            await DownloadPluginAsync(SelectedGitHubSearchPlugin.DownloadUri);
        }

        private static async Task<string> DownloadPluginAsync(Uri downloadUri)
        {
            using var client = new HttpClient();
            string pythonContent;
            HttpResponseMessage? response = null;

            try
            {
                response = await client.GetAsync(downloadUri);
                response.EnsureSuccessStatusCode(); // This will throw HttpRequestException for 4xx/5xx status codes.

                pythonContent = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                AppLoggerService.AddLogMessage(Splat.LogLevel.Error, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Encountered {e.StatusCode} trying to download plugin", e.Message, $"Error {e.StatusCode}");
                return $"{e.StatusCode} - {e.Message}";
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
                fileName = Path.GetFileName(downloadUri.LocalPath);

                // No luck getting a file name - give up
                if (string.IsNullOrEmpty(fileName))
                {
                    return "Could not determine file name";
                }
            }

            fileName = Path.GetFileName(fileName); // Sanatize the file name

            if (!fileName.EndsWith(".py"))
            {
                return $"filename does not end on .py (\"{fileName}\")";
            }

            string filePath = Path.Combine(LocalSearchPluginService.SearchEngineDirectory, fileName);
            await File.WriteAllTextAsync(filePath, pythonContent);

            AppLoggerService.AddLogMessage(Splat.LogLevel.Info, GetFullTypeName<LocalSearchPluginsViewModel>(), $"Successfully downloaded plugin to {filePath}");

            return string.Empty;
        }

        protected override async Task InstallSearchPluginAsync()
        {
            var errorText = await DownloadPluginAsync(new Uri(AddSearchPluginUri));
            if (errorText == string.Empty)
            {
                ExpandAddRemoteSearchPluginUri = false;
            }
            else
            {
                AddRemoteSearchPluginUriErrorMessage = errorText;
            }
        }
    }
}

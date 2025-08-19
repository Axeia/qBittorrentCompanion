using Avalonia.Controls;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using ReactiveUI;
using RssPlugins;
using Splat;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// <summary>
    /// ViewModel for managing RSS rule plugins. Coordinates between plugin selection,
    /// input processing, and UI updates in a clean, testable way.
    /// </summary>
    public class RssPluginSupportBaseViewModel : ViewModelBase
    {
        private readonly List<RssRulePluginBase> _plugins;
        private RssRulePluginBase? _selectedPlugin;
        private string _pluginInput = string.Empty;
        private PluginResult? _currentResult;

        public RssPluginSupportBaseViewModel()
        {
            _plugins = LoadAvailablePlugins();
            _selectedPlugin = Plugins[0]; // Set default, might get overwritten below

            if (!Design.IsDesignMode
                && ConfigService.LastSelectedRssPlugin != string.Empty
                && Plugins.FirstOrDefault(p => p.Name == ConfigService.LastSelectedRssPlugin) is RssRulePluginBase rrpb)
            {
                _selectedPlugin = rrpb;
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    GetFullTypeName<RssPluginSupportBaseViewModel>(),
                    $"Restoring RSS Plugin selection from config",
                    $"Plugin with the name {rrpb.Name} was found and restored as selected entry"
                );
            }

            // Process initial state if we have a default plugin
            ProcessCurrentInput();
        }

        /// <summary>
        /// All available plugins for the user to choose from
        /// </summary>
        public IReadOnlyList<RssRulePluginBase> Plugins => _plugins;
        public IReadOnlyList<RssRulePluginBase> PluginsWithoutWizard => _plugins.Where(p => p.Name != "Wizard").ToList().AsReadOnly();

        /// <summary>
        /// Currently selected plugin. When changed, immediately reprocesses the current input.
        /// </summary>
        public RssRulePluginBase? SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                if (value != _selectedPlugin)
                {
                    _selectedPlugin = value;
                    this.RaisePropertyChanged(nameof(SelectedPlugin));
                    ProcessCurrentInput();

                    if (!Design.IsDesignMode && _selectedPlugin != null)
                    {
                        AppLoggerService.AddLogMessage(
                            LogLevel.Info,
                            GetFullTypeName<RssPluginSupportBaseViewModel>(),
                            $"Saving RSS plugin {_selectedPlugin.Name} selection config file"                            
                        );
                        ConfigService.LastSelectedRssPlugin = _selectedPlugin.Name;
                    }
                }
            }
        }

        /// <summary>
        /// The input text that the user wants to create a regex rule from.
        /// When changed, immediately processes with the selected plugin.
        /// </summary>
        public string PluginInput
        {
            get => _pluginInput;
            set
            {
                if (value != _pluginInput)
                {
                    _pluginInput = value;
                    this.RaisePropertyChanged(nameof(PluginInput));
                    ProcessCurrentInput();
                }
            }
        }

        /// <summary>
        /// Other than <see cref="RssRuleWizard"/> there should be no reason to call this directly.
        /// </summary>
        public void ProcessCurrentInput()
        {
            if (_selectedPlugin == null || string.IsNullOrEmpty(_pluginInput))
            {
                _currentResult = null;
                // Clear cache when no input
                _pluginResultCache.Clear();
            }
            else
            {
                _currentResult = _selectedPlugin.ProcessTarget(_pluginInput);

                // Update cache for all plugins (for menu display)
                UpdatePluginCache();
            }

            // Notify UI of all property changes at once
            NotifyAllPropertiesChanged();
        }


        #region Result Properties (for UI binding)

        /// <summary>
        /// Whether the plugin successfully processed the input
        /// </summary>
        public bool PluginIsSuccess => _currentResult?.IsSuccess ?? false;

        /// <summary>
        /// The generated regex pattern from the plugin
        /// </summary>
        public string PluginResult => _currentResult?.RegexPattern ?? string.Empty;

        /// <summary>
        /// The extracted rule title (typically the series/content name)
        /// </summary>
        public string PluginRuleTitle => _currentResult?.RuleTitle ?? string.Empty;

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string PluginErrorText => _currentResult?.ErrorMessage ?? string.Empty;

        /// <summary>
        /// Warning message for successful processing with caveats
        /// </summary>
        public string PluginWarningText => _currentResult?.WarningMessage ?? string.Empty;

        /// <summary>
        /// Informational message about what the plugin detected/handled
        /// </summary>
        public string PluginInfoText => _currentResult?.InfoMessage ?? string.Empty;

        /// <summary>
        /// Whether the primary action button should be enabled (input provided and processing succeeded)
        /// </summary>
        public bool PluginPrimaryButtonEnabled => !string.IsNullOrEmpty(PluginInput) && PluginIsSuccess;

        #endregion

        public void PluginForceUiUpdate()
        {
            NotifyAllPropertiesChanged();
        }

        private void NotifyAllPropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(PluginInput));
            this.RaisePropertyChanged(nameof(PluginIsSuccess));
            this.RaisePropertyChanged(nameof(PluginResult));
            this.RaisePropertyChanged(nameof(PluginRuleTitle));
            this.RaisePropertyChanged(nameof(PluginErrorText));
            this.RaisePropertyChanged(nameof(PluginWarningText));
            this.RaisePropertyChanged(nameof(PluginInfoText));
            this.RaisePropertyChanged(nameof(PluginPrimaryButtonEnabled));
        }

        /// <summary>
        /// 
        /// </summary>
        private static List<RssRulePluginBase> LoadAvailablePlugins()
        {
            //TODO load from dlls
            var plugins = new List<RssRulePluginBase>
            {
                new SeriesRssPlugin.SeriesRssPlugin(),
                new FossRssPlugin.FossRssPlugin(),
                new RssRuleWizard(),
            };

            /* 
            var pluginAssemblies = Directory.GetFiles("RssPlugins", "*.dll");
            Debug.WriteLine($"Found {pluginAssemblies.Count()} .dll files");

            foreach (var assemblyPath in pluginAssemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var pluginTypes = assembly.GetTypes().Where(t => typeof(RssRulePluginBase).IsAssignableFrom(t) && !t.IsInterface);
                foreach (var pluginType in pluginTypes)
                {
                    if (Activator.CreateInstance(pluginType, [""]) is RssRulePluginBase pluginInstance)
                    {
                        // Add plugin to your collection or process it
                        Plugins.Add(pluginInstance);
                        Debug.WriteLine(pluginInstance.RuleTitle);
                    }
                }
            }
            */

            return plugins;
        }

        private readonly Dictionary<RssRulePluginBase, PluginResult?> _pluginResultCache = [];

        /// <summary>
        /// Updates the cache with results from all plugins for the current input
        /// This enables menu items to show success/failure status
        /// </summary>
        private void UpdatePluginCache()
        {
            if (string.IsNullOrEmpty(_pluginInput))
            {
                _pluginResultCache.Clear();
                return;
            }

            foreach (var plugin in _plugins)
            {
                try
                {
                    _pluginResultCache[plugin] = plugin.ProcessTarget(_pluginInput);
                }
                catch
                {
                    // If a plugin fails completely, mark it as failed
                    _pluginResultCache[plugin] = RssPlugins.PluginResult.Error("Plugin error");
                }
            }
        }

        /// <summary>
        /// Gets whether a specific plugin would successfully process the current input.
        /// Used by menu items to show success/failure icons.
        /// </summary>
        /// <param name="plugin">The plugin to check</param>
        /// <returns>True if the plugin would successfully process the current input</returns>
        public bool GetPluginSuccess(RssRulePluginBase plugin)
        {
            if (string.IsNullOrEmpty(_pluginInput))
                return false;

            return _pluginResultCache.TryGetValue(plugin, out var result) && (result?.IsSuccess ?? false);
        }
    }
}
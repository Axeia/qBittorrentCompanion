using QBittorrent.Client;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using System;
using System.Linq;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// LocalSearchPluginViewModel only differs from RemoteSearchPluginViewModel in three aspects.
    /// 1) It has a FileName property
    /// 2) Its version can be set after the fact.
    ///   This is because <see cref="PythonSearchBridge "/> cannot parse versions and they're set in separate step
    /// 3) IsEnabled writes to <see cref="AppConfig"/>
    /// <param name="searchPlugin"></param>
    /// <param name="fileName"></param>
    public partial class LocalSearchPluginViewModel(SearchPlugin searchPlugin, string fileName) : RemoteSearchPluginViewModel(searchPlugin)
    {
        [RaiseChange]
        private string _fileName = fileName;

        public new Version Version
        {
            get => _searchPlugin.Version;
            set
            {
                if (value != _searchPlugin.Version)
                {
                    _searchPlugin.Version = value;
                    this.RaisePropertyChanged(nameof(Version));
                }
            }
        }

        public new bool IsEnabled
        {
            get => _searchPlugin.IsEnabled;
            set
            {
                if (value != _searchPlugin.IsEnabled)
                {
                    IsProcessing = true;
                    _searchPlugin.IsEnabled = value;

                    // As list for LINQ
                    var disabledLocalSearchPlugins = ConfigService.DisabledLocalSearchPlugins.ToList();

                    if (value == true)
                    {
                        // Remove from disabled list
                        if (disabledLocalSearchPlugins.Remove(FileName)) // Only re-assign if something was actually removed                            
                            ConfigService.DisabledLocalSearchPlugins = [.. disabledLocalSearchPlugins];
                    }
                    else
                    {
                        if (!disabledLocalSearchPlugins.Contains(FileName))
                        {
                            disabledLocalSearchPlugins.Add(FileName);
                            ConfigService.DisabledLocalSearchPlugins = [.. disabledLocalSearchPlugins];
                        }
                    }

                    this.RaisePropertyChanged(nameof(IsEnabled));
                    IsProcessing = false;
                }
            }
        }
    }
}

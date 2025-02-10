using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public abstract class RssPluginSupportBaseViewModel : ViewModelBase
    {
        public RssPluginsViewModel RssPluginsViewModel { get; } = new RssPluginsViewModel();

        public bool PluginIsSuccess
        {
            get => RssPluginsViewModel.SelectedPlugin.IsSuccess;
            set => this.RaisePropertyChanged(nameof(PluginIsSuccess));
        }

        public string PluginRuleTitle
        {
            get => RssPluginsViewModel.SelectedPlugin.RuleTitle;
            set => this.RaisePropertyChanged(nameof(PluginRuleTitle));
        }

        public string PluginResult
        {
            get => RssPluginsViewModel.SelectedPlugin.Result;
            set => this.RaisePropertyChanged(nameof(PluginResult));
        }

        public string PluginErrorText
        {
            get => RssPluginsViewModel.SelectedPlugin.ErrorText;
            set => this.RaisePropertyChanged(nameof(PluginErrorText));
        }

        public string PluginWarningText
        {
            get => RssPluginsViewModel.SelectedPlugin.WarningText;
            set => this.RaisePropertyChanged(nameof(PluginWarningText));
        }
    }
}
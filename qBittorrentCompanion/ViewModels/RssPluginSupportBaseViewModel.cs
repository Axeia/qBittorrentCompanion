using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    public abstract class RssPluginSupportBaseViewModel : ViewModelBase
    {
        public RssPluginsViewModel RssPluginsViewModel { get; } = new RssPluginsViewModel();

        public bool PluginIsSuccess
            => RssPluginsViewModel.SelectedPlugin.IsSuccess;

        public string PluginRuleTitle
            => RssPluginsViewModel.SelectedPlugin.RuleTitle;

        public string PluginResult
            => RssPluginsViewModel.SelectedPlugin.Result;

        public string PluginErrorText
            => RssPluginsViewModel.SelectedPlugin.ErrorText;

        public string PluginWarningText
            => RssPluginsViewModel.SelectedPlugin.WarningText;

        public string PluginInfoText
            => RssPluginsViewModel.SelectedPlugin.InfoText;

        public void PluginForceUiUpdate()
        {
            this.RaisePropertyChanged(nameof(PluginIsSuccess));
            this.RaisePropertyChanged(nameof(PluginRuleTitle));
            this.RaisePropertyChanged(nameof(PluginResult));
            this.RaisePropertyChanged(nameof(PluginWarningText));
            this.RaisePropertyChanged(nameof(PluginErrorText));
        }

        private string _pluginInput = string.Empty;
        public string PluginInput
        {
            get => _pluginInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _pluginInput, value);
                RssPluginsViewModel.SelectedPlugin.RevalidateOn(_pluginInput);
                PluginForceUiUpdate();
            }
        }
    }
}
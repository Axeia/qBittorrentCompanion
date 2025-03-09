using ReactiveUI;

namespace qBittorrentCompanion.ViewModels
{
    /// <summary>
    /// Technically this class should be abstract as its not used in the app itself,
    /// however not having it abstract allows it to be used for <see cref="qBittorrentCompanion.Views.RssPluginInfoView"/> 
    /// in designmode (for the xaml previewer)
    /// </summary>
    public class RssPluginSupportBaseViewModel : ViewModelBase
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
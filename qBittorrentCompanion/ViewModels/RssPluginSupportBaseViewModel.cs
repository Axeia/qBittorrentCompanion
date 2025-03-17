using ReactiveUI;
using System.Diagnostics;

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
        
        public RssPluginSupportBaseViewModel()
        {
            RssPluginsViewModel.PropertyChanged += RssPluginsViewModel_PropertyChanged;
        }

        private void RssPluginsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PluginForceUiUpdate();
        }

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

        public bool PluginPrimaryButtonEnabled
            => PluginInput != string.Empty && PluginIsSuccess;

        public void PluginForceUiUpdate()
        {
            this.RaisePropertyChanged(nameof(PluginIsSuccess));
            this.RaisePropertyChanged(nameof(PluginRuleTitle));
            this.RaisePropertyChanged(nameof(PluginResult));
            this.RaisePropertyChanged(nameof(PluginWarningText));
            this.RaisePropertyChanged(nameof(PluginErrorText));
            this.RaisePropertyChanged(nameof(PluginPrimaryButtonEnabled));
        }

        private string _pluginInput = string.Empty;
        public string PluginInput
        {
            get => _pluginInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _pluginInput, value);

                foreach(var plugin in RssPluginsViewModel.Plugins)
                    plugin.RevalidateOn(_pluginInput);
                PluginForceUiUpdate();
            }
        }
    }
}
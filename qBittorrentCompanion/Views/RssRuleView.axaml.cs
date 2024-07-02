using Avalonia.Controls;
using Avalonia.Interactivity;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleView : UserControl
    {
        public RssRuleView()
        {
            InitializeComponent();
            DataContext = new RssAutoDownloadingRulesViewModel();
            if (!Design.IsDesignMode)
                Loaded += RssRuleView_Loaded;

            this.SizeChanged += RssRulesView_SizeChanged;
        }

        private void RssRulesView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine(this.Bounds.Width);
            // Check if the Width is greater than 280
            if (this.Bounds.Width > 520)
                DockPanel.SetDock(AddPausedComboBox, Dock.Left);
            else
                DockPanel.SetDock(AddPausedComboBox, Dock.Top);
        }

        private void RssRuleView_Loaded(object? sender, RoutedEventArgs e)
        {
            if(DataContext is RssAutoDownloadingRulesViewModel rssRules)
                rssRules.Initialise(); // Fetches data from the QBittorrent WebUI.
        }



        /*
        private void UseRegexCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssAutoDownloadingRulesViewModel rulesViewModel)
            {
         //       rulesViewModel.FilterUseRegex = UseRegexCheckBox.IsChecked == true;
            }
        }*/
    }
}

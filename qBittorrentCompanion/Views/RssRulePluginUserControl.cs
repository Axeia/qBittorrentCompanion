using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using qBittorrentCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Adds <see cref="RssRulePluginUserControl.SuggestedFeeds"/> and <see cref="GenerateRssRuleSplitButton_Click(object?, RoutedEventArgs)"/>
    /// To be used for usercontrols that use a <see cref="RssPluginSupportBaseViewModel"/> DataContext and host a <see cref="RssPluginButtonView"/>
    /// </summary>
    public partial class RssRulePluginUserControl : UserControl
    {
        protected List<Uri> SuggestedFeeds = [];

        protected void GenerateRssRuleSplitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssPluginSupportBaseViewModel rpsbvm)
            {
                var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
                mainWindow.MainTabStrip.SelectedIndex = 3;

                Dispatcher.UIThread.Post(() =>
                {
                    mainWindow.RssRulesView.AddNewRule(
                        rpsbvm.PluginRuleTitle,
                        rpsbvm.PluginResult,
                        SuggestedFeeds
                    );
                }, DispatcherPriority.Background);
            }
        }
    }
}

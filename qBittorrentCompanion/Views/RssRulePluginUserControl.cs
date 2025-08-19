using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
using RssPlugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public RssRulePluginUserControl()
        {
            DataContextChanged += RssRulePluginUserControl_DataContextChanged;
        }

        private void RssRulePluginUserControl_DataContextChanged(object? sender, EventArgs e)
        {

            if (DataContext is RssPluginSupportBaseViewModel rpsbvm)
            {


            }
        }

        protected void GenerateRssRuleSplitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is RssPluginSupportBaseViewModel rpsbvm)
            {
                var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
                mainWindow.MainTabStrip.SelectedIndex = 3;
                Debug.WriteLine($"Adding new rule with title: {rpsbvm.PluginRuleTitle} and result: {rpsbvm.PluginResult}");
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

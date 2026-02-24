using Avalonia.Interactivity;
using System;

namespace qBittorrentCompanion.Views
{
    public partial class OwnAboutWindow : IcoWindow
    {
        public OwnAboutWindow()
        {
            InitializeComponent();
            FirstParagraphSimpleHtmlTextBlock.Text = qBittorrentCompanion.Resources.Resources.OwnAboutWindow_HtmlParagraph1;
            SecondParaphSimpleHtmlTextBlock.Text = qBittorrentCompanion.Resources.Resources.OwnAboutWindow_HtmlParagraph2;
            ThirdParagraphSimpleHtmlTextBlock.Text = qBittorrentCompanion.Resources.Resources.OwnAboutWindow_HtmlParagraph3;
            
        }
        public void OnAvaloniaUIClicked(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://avaloniaui.net/"));
        }

        public void OnQBittorentCompanionClicked(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://github.com/axeia/qBittorentCompanion"));
        }

        public void OnFlagpediaClicked(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("https://github.com/fedarovich/qbittorrent-net-client"));
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

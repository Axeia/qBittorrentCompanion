using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace qBittorrentCompanion.Views
{
    public partial class OwnAboutWindow : IcoWindow
    {
        public OwnAboutWindow()
        {
            InitializeComponent();
            FirstParagraphSimpleHtmlTextBlock.Text = "This application is written in C# by Pascal Bakhuis using the <a href='https://avaloniaui.net/'>Avalonia UI</a> framework and can be freely downloaded from <a href='https://github.com/Axeia/qBittorrentCompanion/'>github.com/Axeia/qBittorrentCompanion</a> (including the source code).<br/>";
            FirstParagraphSimpleHtmlTextBlock.Text += "It leans heavily on the <a href='https://github.com/fedarovich/qbittorrent-net-client'>qbittorrent-net-client</a> project by fedarovich.";
            SecondParaphSimpleHtmlTextBlock.Text = "For bug reports and feature requests please make use of the previously mentioned <a href='https://github.com/Axeia/qBittorrentCompanion/'>GitHub</a> page.";
            ThirdParagraphSimpleHtmlTextBlock.Text = "The flag icons are by <a href='https://flagpedia.net'>flagpedia.net</a> - all the other icons are by <a href='https://github.com/microsoft/fluentui-system-icons'>Microsoft</a> ";
            
        }
        public void OnAvaloniaUIClicked(object sender, RoutedEventArgs e)
        {
            LaunchUrl("https://avaloniaui.net/");
        }

        public void OnQBittorentCompanionClicked(object sender, RoutedEventArgs e)
        {
            LaunchUrl("https://github.com/axeia/qBittorentCompanion");
        }

        public void OnFlagpediaClicked(object sender, RoutedEventArgs e)
        {
            LaunchUrl("https://github.com/fedarovich/qbittorrent-net-client");
        }

        private void LaunchUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

        }

    }
}

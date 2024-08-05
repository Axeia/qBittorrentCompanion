using Avalonia.Controls;
using System.Diagnostics;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class WebUiView : UserControl
    {
        public WebUiView()
        {
            InitializeComponent();
        }

        private void DynamicDnsServiceComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DynamicDnsServiceComboBox == null)
                return;

            if (DynamicDnsServiceComboBox.SelectedIndex == 1)
                DnsUrlPreviewTextBox.Text = "noip.com";
            else if (DynamicDnsServiceComboBox.SelectedIndex == 2)
                DnsUrlPreviewTextBox.Text = "account.dyn.com";
            else
                DnsUrlPreviewTextBox.Text = "";

            LaunchBrowserButton.IsEnabled = DynamicDnsServiceComboBox.SelectedIndex != 0;
        }

        private void LaunchBrowserButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string href = "https://github.com/Axeia/qBittorrentCompanion";

            if (DynamicDnsServiceComboBox.SelectedIndex == 1)
                href = "https://www.noip.com/";
            else if (DynamicDnsServiceComboBox.SelectedIndex == 2)
                href = "https://account.dyn.com/";

            Process.Start(new ProcessStartInfo{ FileName = href, UseShellExecute = true });
        }
    }
}

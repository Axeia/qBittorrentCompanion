using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QBittorrent.Client;
using qBittorrentCompanion.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using qBittorrentCompanion.AvaloniaEditor;

namespace qBittorrentCompanion.Views
{
    public partial class RssRuleView : UserControl
    {
        public RssRuleView()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
            {
                DataContext = new RssAutoDownloadingRuleViewModel(
                    new RssAutoDownloadingRule(),
                    "",
                    new List<string>().AsReadOnly()
                );
            }
            RuleDefinitionScrollViewer.SizeChanged += RuleDefinitionScrollViewer_SizeChanged;
            PreventNewlines(MustContainTextEditor);
            MustContainTextEditor.Loaded += MustContainTextEditor_Loaded;
        }
        private void MustContainTextEditor_Loaded(object? sender, RoutedEventArgs e)
        {
            var registryOptions = new RegexRegistryOptions();
            var textMateInstallation = MustContainTextEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar("source.regexp");
        }

        private void PreventNewlines(TextEditor textEditor)
        {
            textEditor.TextArea.KeyDown += TextArea_TextEntering;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
        }

        private void TextArea_TextEntering(object? sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Enter || e.Key == Key.Return;
        }


        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            // If the text being entered contains newlines
            if (e.Text?.Contains('\n') == true || e.Text?.Contains('\r') == true)
            {
                // Replace newlines with spaces or just remove them
                string singleLine = Regex.Replace(e.Text, @"\r\n?|\n", "");

                // Insert the cleaned text instead
                MustContainTextEditor.Document.Replace(
                    MustContainTextEditor.SelectionStart,
                    MustContainTextEditor.SelectionLength,
                    singleLine);

                // Prevent the original text with newlines from being entered
                e.Handled = true;
            }
        }

        private void RuleDefinitionScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("Change afoot");
        }

        private void SwitchToFeedsButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = this.GetVisualAncestors().OfType<MainWindow>().First();
            mainWindow.MainTabStrip.SelectedIndex = 2;
        }

        private void PluginSourceTextBox_PastingFromClipboard(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    textBox.Text = textBox.Text.Trim();
                });
            }
        }
    }
}
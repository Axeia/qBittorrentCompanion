using AvaloniaEdit;
using Avalonia.Data;
using Avalonia.Metadata;
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Threading;
using qBittorrentCompanion.AvaloniaEditor;
using AvaloniaEdit.TextMate;
using Avalonia.Input;
using System.Text.RegularExpressions;
using AvaloniaEdit.Indentation;
using System.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Controls;
using AvaloniaEdit.Editing;

namespace qBittorrentCompanion.CustomControls
{
    public class BindableRegexEditor : TextEditor, INotifyPropertyChanged
    {
        public static readonly StyledProperty<string> BoundTextProperty =
            AvaloniaProperty.Register<BindableRegexEditor, string>(nameof(BoundText), defaultBindingMode: BindingMode.TwoWay);

        [Content]
        public string BoundText
        {
            get { return GetValue(BoundTextProperty); }
            set { SetValue(BoundTextProperty, value); }
        }

        public BindableRegexEditor()
        {
            this.PropertyChanged += OnSelfPropertyChanged;
            this.TextChanged += OnEditorTextChanged;
            this.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
            this.ShowLineNumbers = false;
            this.WordWrap = true;
            this.TextArea.IndentationStrategy = null;

            //this.Options.ShowSpacesGlyph = Options.ShowSpacesGlyph.;
            DataContextChanged += BindableRegexEditor_DataContextChanged;
            this.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);

            this.GetObservable(BoundTextProperty).Subscribe(text => {
                UpdateTextEditor();
            });

            var textMateInstallation = this.InstallTextMate(new RegexRegistryOptions());
            textMateInstallation.SetGrammar("source.regexp");

            this.TextArea.TextEntering += TextArea_TextEntering;
        }

        private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            { // Don't insert tabs
                e.Handled = true;
                // Focus next control somehow
            }
            else // Prevent new lines from being entered (filtered out in TextArea_TextEntering as well)
                e.Handled = e.Key == Key.Enter || e.Key == Key.Return;
        }

        private void TextArea_KeyDown(object? sender, KeyEventArgs e)
        {
            Debug.WriteLine("Keypresed");
        }

        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            // If the text being entered contains newlines
            if (e.Text?.Contains('\n') == true || e.Text?.Contains('\r') == true)
            {
                // Replace newlines with spaces or just remove them
                string singleLine = Regex.Replace(e.Text, @"\r\n?|\n", "");

                // Insert the cleaned text instead
                Document.Replace(SelectionStart, SelectionLength, singleLine);

                // Prevent the original text with newlines from being entered
                e.Handled = true;
            }
        }

        protected override Type StyleKeyOverride => typeof(TextEditor);

        private void BindableRegexEditor_DataContextChanged(object? sender, EventArgs e)
        {
            // The binding might not have been applied yet, so use Dispatcher to delay
            Dispatcher.UIThread.Post(() => {
                UpdateTextEditor();
            }, DispatcherPriority.Background);
        }

        private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BoundText))
            {
                UpdateTextEditor();
            }
        }

        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            if (Document != null)
            {
                BoundText = Document.Text;
            }
        }

        private void UpdateTextEditor()
        {
            if (Document != null && BoundText != Document.Text)
            {
                int caretOffset = CaretOffset;
                Document.Replace(0, Document.TextLength, BoundText ?? "");
                CaretOffset = Math.Min(caretOffset, Document.TextLength);
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
    }
}
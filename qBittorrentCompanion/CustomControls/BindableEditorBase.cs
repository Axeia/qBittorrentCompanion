using AvaloniaEdit;
using Avalonia.Data;
using Avalonia.Metadata;
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Input;
using System.Text.RegularExpressions;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Controls.Primitives;

namespace qBittorrentCompanion.CustomControls
{
    public partial class BindableEditorBase : TextEditor, INotifyPropertyChanged
    {
        public static readonly StyledProperty<string> BoundTextProperty =
            AvaloniaProperty.Register<BindableEditorBase, string>(
                nameof(BoundText),
                defaultBindingMode: BindingMode.TwoWay,
                defaultValue: string.Empty);

        // Add new property for validation error display
        public static readonly StyledProperty<string> ValidationErrorProperty =
            AvaloniaProperty.Register<BindableEditorBase, string>(nameof(ValidationError));

        // Add property for error border brush
        public static readonly StyledProperty<IBrush> ErrorBorderBrushProperty =
            AvaloniaProperty.Register<BindableEditorBase, IBrush>(
                nameof(ErrorBorderBrush),
                Brushes.Red);

        // Add property to control error border visibility
        public static readonly StyledProperty<bool> HasValidationErrorProperty =
            AvaloniaProperty.Register<BindableEditorBase, bool>(
                nameof(HasValidationError),
                false);

        [GeneratedRegex(@"\r\n?|\n")]
        private static partial Regex NewLine();

        [Content]
        public string BoundText
        {
            get => GetValue(BoundTextProperty);
            set => SetValue(BoundTextProperty, value);
        }

        public string ValidationError
        {
            get => GetValue(ValidationErrorProperty);
            private set => SetValue(ValidationErrorProperty, value);
        }

        public IBrush ErrorBorderBrush
        {
            get => GetValue(ErrorBorderBrushProperty);
            set => SetValue(ErrorBorderBrushProperty, value);
        }

        public bool HasValidationError
        {
            get => GetValue(HasValidationErrorProperty);
            private set => SetValue(HasValidationErrorProperty, value);
        }

        public BindableEditorBase()
        {
            this.PropertyChanged += OnSelfPropertyChanged;
            this.TextChanged += OnEditorTextChanged;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            this.ShowLineNumbers = false;
            this.WordWrap = true;
            this.TextArea.IndentationStrategy = null;
            this.FontFamily = FontFamily.Parse("Inconsolata, Consolas, Monospace, Courier");

            DataContextChanged += BindableEditorBase_DataContextChanged;
            this.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);

            // Subscribe to BoundText changes to update editor
            this.GetObservable(BoundTextProperty).Subscribe(text => {
                UpdateTextEditor();
            });

            // Subscribe to validation changes on the BoundText property
            BoundTextProperty.Changed.Subscribe(e => {
                if (e.Sender == this)
                {
                    CheckForValidationError(e);
                }
            });

            this.TextArea.TextEntering += TextArea_TextEntering;

            // Override the default style key to use our custom style
            this.TemplateApplied += BindableEditorBase_TemplateApplied;

            // Set the style class for proper styling
            this.Classes.Add("bindable-editor");
        }

        private void BindableEditorBase_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            // Optional: If we need any post-template initialization
        }

        private void CheckForValidationError(AvaloniaPropertyChangedEventArgs e)
        {
            // Check if there's a binding error in the change args
            var bindingNotification = e.NewValue as BindingNotification;
            if (bindingNotification != null && bindingNotification.ErrorType == BindingErrorType.DataValidationError)
            {
                // We have a validation error
                ValidationError = bindingNotification.Error?.Message ?? "Validation error";
                HasValidationError = true;
            }
            else
            {
                // Clear any validation errors
                ValidationError = string.Empty;
                HasValidationError = false;
            }
        }

        private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            { // Don't insert tabs
                e.Handled = true;
                // TODO Focus next control somehow (should be easier to do in a future Avalonia version)
            }
            else // Prevent new lines from being entered (filtered out in TextArea_TextEntering as well)
                e.Handled = e.Key == Key.Enter || e.Key == Key.Return;
        }

        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            // If the text being entered contains newlines
            if (e.Text?.Contains('\n') == true || e.Text?.Contains('\r') == true)
            {
                // Replace newlines with spaces or just remove them
                string singleLine = NewLine().Replace(e.Text, "");

                // Insert the cleaned text instead
                Document.Replace(SelectionStart, SelectionLength, singleLine);

                // Prevent the original text with newlines from being entered
                e.Handled = true;
            }
        }

        // Use our specific type for styling
        protected override Type StyleKeyOverride => typeof(TextEditor);

        private void BindableEditorBase_DataContextChanged(object? sender, EventArgs e)
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
                // Update bound text when editor changes
                SetCurrentValue(BoundTextProperty, Document.Text);
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using qBittorrentCompanion.AvaloniaEditor;
using System.Diagnostics;

namespace qBittorrentCompanion.Views;

public partial class TextBoxLikeEditor : Border
{
    public static readonly StyledProperty<string> BoundTextProperty =
    AvaloniaProperty.Register<TextBoxLikeEditor, string>(
        nameof(BoundText),
        defaultBindingMode: BindingMode.TwoWay);

    public string BoundText
    {
        get => GetValue(BoundTextProperty);
        set => SetValue(BoundTextProperty, value);
    }

    private bool _textMateInitialized = false;

    public static readonly StyledProperty<string?> HighlightAsProperty =
        AvaloniaProperty.Register<TextBoxLikeEditor, string?>("HighlightAs");

    public string? HighlightAs
    {
        get => GetValue(HighlightAsProperty);
        set => SetValue(HighlightAsProperty, value);
    }

    private readonly SolidColorBrush ErrorBrushFocused = new(Colors.Red);
    private readonly SolidColorBrush ErrorBrushUnfocusedPointerOver = new(Colors.Red, 0.6);
    private readonly SolidColorBrush ErrorBrushUnfocused = new(Colors.Red, 0.4);

    private bool IsErrored => Classes.Contains("Error");

    public TextBoxLikeEditor()
    {
        InitializeComponent();

        Loaded += RegexTextBoxLikeEditor_Loaded;
        PointerEntered += RegexTextBoxLikeEditor_PointerEntered;
        PointerExited += RegexTextBoxLikeEditor_PointerExited;
        Focusable = true;
        GotFocus += TextBoxLikeEditor_GotFocus; // Pass along

        // Bind the local BoundText property to the inner BindableRegexEditor's BoundText
        this.FindControl<CustomControls.BindableEditorBase>(nameof(EditorBase))
            !.Bind(CustomControls.BindableRegexEditor.BoundTextProperty, new Binding(nameof(BoundText)) { Source = this, Mode = BindingMode.TwoWay });

        this.PropertyChanged += HiglightAs_PropertyChanged;
    }

    private void HiglightAs_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!_textMateInitialized && e.Property == HighlightAsProperty && HighlightAs != null)
        {
            string? grammarFilePath = null;
            string? scopeName = null;

            if (HighlightAs.ToLower() == "regex")
            {
                grammarFilePath = "regex.tmLanguage.json";
                scopeName = "source.regexp";
            }
            else if (HighlightAs.ToLower() == "episodefilter")
            {
                grammarFilePath = "qbepfilter.tmLanguage.json";
                scopeName = "source.qbittorrent.episodefilter";
            }

            if (grammarFilePath != null && scopeName != null)
            {
                // Initialize TextMate
                var textMateInstallation = EditorBase.InstallTextMate(
                    new CustomRegistryOptions(grammarFilePath, scopeName)
                );
                textMateInstallation.SetGrammar(scopeName);
                _textMateInitialized = true;
            }
            else
            {
                Debug.WriteLine($"Unable to locate {grammarFilePath} {scopeName}");
            }
        }
    }

    private void RegexTextBoxLikeEditor_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        EditorBase.TextArea.GotFocus += TextArea_GotFocus;
        EditorBase.TextArea.LostFocus += TextArea_LostFocus;
        EditorBase.TextChanged += EditorBase_TextChanged;
    }

    private void EditorBase_TextChanged(object? sender, System.EventArgs e)
    {
        SetFocusedBorderBrush();
    }

    private void TextArea_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetDefaultBorderBrush();
    }

    private void TextArea_GotFocus(object? sender, GotFocusEventArgs e)
    {
        SetFocusedBorderBrush();
    }

    private void RegexTextBoxLikeEditor_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (!EditorBase.TextArea.IsFocused)
        {
            App.Current!.TryGetResource("TextControlBorderBrushPointerOver", ActualThemeVariant, out var brush);
            if (brush is ISolidColorBrush scb)
            {
                BorderBrush = IsErrored ? ErrorBrushUnfocusedPointerOver : scb;
            }
        }
    }

    private void RegexTextBoxLikeEditor_PointerExited(object? sender, PointerEventArgs e)
    {
        if (!EditorBase.TextArea.IsFocused)
        {
            SetDefaultBorderBrush();
        }
    }

    private void SetDefaultBorderBrush()
    {
        
        App.Current!.TryGetResource("TextControlBorderBrush", ActualThemeVariant, out var brush);
        if (brush is ISolidColorBrush scb)
        {
            BorderBrush = IsErrored ? ErrorBrushUnfocused : scb;
        }
    }

    private void SetFocusedBorderBrush()
    {
        App.Current!.TryGetResource("TextControlBorderBrushFocused", ActualThemeVariant, out var brush);
        if (brush is ISolidColorBrush scb)
        {
            BorderBrush = IsErrored ? ErrorBrushFocused : scb;
        }
    }

    private void TextBoxLikeEditor_GotFocus(object? sender, GotFocusEventArgs e)
    {
        EditorBase.Focus();
    }
}
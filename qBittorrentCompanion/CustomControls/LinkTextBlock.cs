using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.CustomControls
{
    public class LinkTextBlock : TextBlock
    {
        public static readonly new StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<LinkTextBlock, string>(nameof(Text), default(string)!);
        public new string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<IBrush> LinkColorProperty =
            AvaloniaProperty.Register<LinkTextBlock, IBrush>(nameof(LinkColor), Brushes.Blue);
        public IBrush LinkColor
        {
            get => GetValue(LinkColorProperty);
            set => SetValue(LinkColorProperty, value);
        }

        public LinkTextBlock()
        {
            this.GetObservable(TextProperty).Subscribe(_ => ParseAndRenderText());
            this.GetObservable(LinkColorProperty).Subscribe(_ => ParseAndRenderText());
        }

        private void ParseAndRenderText()
        {
            Inlines!.Clear();

            if (string.IsNullOrEmpty(Text))
                return;

            Debug.WriteLine($"Parsing text: {Text}");

            var regex = new Regex(@"(http[s]?://[^\s]+)");
            var matches = regex.Matches(Text);

            Debug.WriteLine($"Found {matches.Count} matches");

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                Debug.WriteLine($"Match: {match.Value} at index {match.Index}");

                if (match.Index > lastIndex)
                {
                    Inlines.Add(new Run { Text = Text.Substring(lastIndex, match.Index - lastIndex) });
                }

                var linkText = new TextBlock
                {
                    Text = match.Value,
                    Foreground = LinkColor,
                    Cursor = new Cursor(StandardCursorType.Hand)
                };

                linkText.PointerReleased += (s, e) =>
                {
                    var url = match.Value.TrimEnd('.');
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = uriResult.ToString(),
                            UseShellExecute = true
                        });
                    }
                };

                Inlines.Add(new InlineUIContainer { Child = linkText });
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < Text.Length)
            {
                Inlines.Add(new Run { Text = Text.Substring(lastIndex) });
            }
        }
    }
}
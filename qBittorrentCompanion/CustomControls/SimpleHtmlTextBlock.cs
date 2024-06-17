using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;

namespace qBittorrentCompanion.CustomControls
{
    public class SimpleHtmlTextBlock : TextBlock
    {
        public static readonly new StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<SimpleHtmlTextBlock, string>(nameof(Text), default(string));
        public new string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<IBrush> LinkColorProperty =
            AvaloniaProperty.Register<SimpleHtmlTextBlock, IBrush>(nameof(LinkColor), Brushes.Lavender);
        public IBrush LinkColor
        {
            get => GetValue(LinkColorProperty);
            set => SetValue(LinkColorProperty, value);
        }

        public SimpleHtmlTextBlock()
        {
            this.GetObservable(TextProperty).Subscribe(_ => ParseAndRenderHtml());
        }

        private void ParseAndRenderHtml()
        {
            Inlines!.Clear();

            if (string.IsNullOrEmpty(Text))
                return;

            var doc = new HtmlDocument();
            doc.LoadHtml(Text);

            var elements = ParseHtml(doc.DocumentNode);

            foreach (var element in elements)
            {
                Inlines.Add(element);
            }
        }

        private IEnumerable<Inline> ParseHtml(HtmlNode node)
        {
            var inlines = new List<Inline>();

            foreach (var child in node.ChildNodes)
            {
                var contentText = HtmlEntity.DeEntitize(child.InnerText);
                if (child.NodeType == HtmlNodeType.Element)
                {

                    switch (child.Name.ToLower())
                    {
                        case "b":
                        case "strong":
                            inlines.Add(new Run
                            {
                                Text = contentText,
                                FontWeight = FontWeight.Bold
                            });
                            break;

                        case "i":
                        case "em":
                            inlines.Add(new Run
                            {
                                Text = contentText,
                                FontStyle = FontStyle.Italic
                            });
                            break;

                        case "br":
                            inlines.Add(new LineBreak());
                            break;

                        case "a":
                            var href = child.GetAttributeValue("href", null);
                            if (href != null)
                            {
                                var button = new Button
                                {
                                    Content = new TextBlock { Text = contentText, Foreground = LinkColor },
                                    Background = Brushes.Transparent,
                                    BorderBrush = Brushes.Transparent,
                                    Padding = new Thickness(0),
                                    [ToolTip.TipProperty] = href
                                };

                                

                                button.Click += (s, e) =>
                                {
                                    if (Uri.TryCreate(href, UriKind.Absolute, out var uriResult)
                                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                                    {
                                        Process.Start(new ProcessStartInfo
                                        {
                                            FileName = href,
                                            UseShellExecute = true
                                        });
                                    }
                                };

                                button.PointerEntered += (sender, args) =>
                                {
                                    Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        button.Background = Brushes.Transparent;
                                    });
                                };


                                inlines.Add(new InlineUIContainer { Child = button });
                            }
                            break;

                        default:
                            inlines.Add(new Run { Text = contentText });
                            break;
                    }
                }
                else if (child.NodeType == HtmlNodeType.Text)
                {
                    // Remove newline characters from text nodes <br> to be treated as such.
                    var text = contentText.Replace("\r", "").Replace("\n", "").Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        inlines.Add(new Run { Text = text });
                    }
                }
            }

            return inlines;
        }
    }
}
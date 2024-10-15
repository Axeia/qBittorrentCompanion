using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentIcons.Avalonia;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.ViewModels;
using System;
using System.Globalization;

namespace qBittorrentCompanion.Converters
{
    public class AddTrackerOptionToLayoutConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is AddTrackerOption replaceOption)
            {
                return replaceOption switch
                {
                    AddTrackerOption.AddToBottom => 
                        CreateDockPanel(FluentIcons.Common.Symbol.LayoutRowFourFocusBottom, "Add", "Add tracker on a new tier all the way at the bottom"),
                    AddTrackerOption.AddToSameTier =>
                        CreateDockPanel(FluentIcons.Common.Symbol.AddCircle, "Add to same tier", "Add tracker to the same tier as the currently selected item"),
                    AddTrackerOption.InsertAbove => 
                        CreateDockPanel(FluentIcons.Common.Symbol.TableAdd, "Add & insert above selected (creates new tier)", "Add tracker above the currently selected item", true),
                    AddTrackerOption.InsertBelow =>
                        CreateDockPanel(FluentIcons.Common.Symbol.TableAdd, "Add & insert below selected (creates new tier)", "Add tracker below the currently selected item"),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return null;
        }

        private DockPanel CreateDockPanel(FluentIcons.Common.Symbol symbol, string title, string subText, bool vMirror = false)
        {
            var symbolIcon = new SymbolIcon { Symbol = symbol };
            if (vMirror)
                symbolIcon.RenderTransform = new ScaleTransform(1, -1);

            return new DockPanel
            {
                Children = {
                    symbolIcon,
                    new StackPanel{
                        Children = {
                            new TextBlock{ Text = title, FontWeight = FontWeight.Bold, Margin = Thickness.Parse("0 2 8 2") },
                            new TextBlock{ Text = subText, Opacity = 0.7, Margin = Thickness.Parse("0 2 8 2")  }
                        }
                    }
                }
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return BindingOperations.DoNothing;
        }

    }
}

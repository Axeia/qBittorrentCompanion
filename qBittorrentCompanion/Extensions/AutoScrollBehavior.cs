using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using System;
using System.Collections.Specialized;
using Avalonia.VisualTree;

namespace qBittorrentCompanion.Extensions
{
    public static class AutoScrollBehavior
    {
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<ListBox, bool>("IsEnabled", typeof(AutoScrollBehavior));

        public static bool GetIsEnabled(ListBox listBox) => listBox.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(ListBox listBox, bool value) => listBox.SetValue(IsEnabledProperty, value);

        static AutoScrollBehavior()
        {
            IsEnabledProperty.Changed.Subscribe(OnIsEnabledChanged);
        }

        private static void OnIsEnabledChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is ListBox listBox)
            {
                if ((bool)e.NewValue!)
                {
                    listBox.Loaded += OnListBoxLoaded;
                    listBox.Unloaded += OnListBoxUnloaded;
                }
                else
                {
                    listBox.Loaded -= OnListBoxLoaded;
                    listBox.Unloaded -= OnListBoxUnloaded;
                }
            }
        }

        private static void OnListBoxLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                AttachScrollBehavior(listBox);
            }
        }

        private static void OnListBoxUnloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                DetachScrollBehavior(listBox);
            }
        }

        private static void AttachScrollBehavior(ListBox listBox)
        {
            // Find ScrollViewer in the visual tree
            var scrollViewer = listBox.FindDescendantOfType<ScrollViewer>();
            if (scrollViewer == null) return;

            bool isAutoScrollEnabled = true;

            void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
            {
                const double threshold = 10;
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null)
                {
                    isAutoScrollEnabled = scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height - threshold;
                }
            }

            void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (isAutoScrollEnabled && e.Action == NotifyCollectionChangedAction.Add)
                {
                    // Use Dispatcher to ensure UI has updated
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        scrollViewer.ScrollToEnd();
                    });
                }
            }

            scrollViewer.ScrollChanged += OnScrollChanged;

            if (listBox.Items is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += OnCollectionChanged;
            }

            // Store references for cleanup
            listBox.SetValue(ScrollViewerProperty, scrollViewer);
            listBox.SetValue(ScrollHandlerProperty, (EventHandler<ScrollChangedEventArgs>)OnScrollChanged);
            listBox.SetValue(CollectionHandlerProperty, (NotifyCollectionChangedEventHandler)OnCollectionChanged);
        }

        private static void DetachScrollBehavior(ListBox listBox)
        {
            if (listBox.GetValue(ScrollViewerProperty) is ScrollViewer scrollViewer &&
                listBox.GetValue(ScrollHandlerProperty) is EventHandler<ScrollChangedEventArgs> scrollHandler)
            {
                scrollViewer.ScrollChanged -= scrollHandler;
            }

            if (listBox.Items is INotifyCollectionChanged collection &&
                listBox.GetValue(CollectionHandlerProperty) is NotifyCollectionChangedEventHandler collectionHandler)
            {
                collection.CollectionChanged -= collectionHandler;
            }
        }

        private static readonly AttachedProperty<ScrollViewer?> ScrollViewerProperty =
            AvaloniaProperty.RegisterAttached<ListBox, ScrollViewer?>("ScrollViewer", typeof(AutoScrollBehavior));

        private static readonly AttachedProperty<EventHandler<ScrollChangedEventArgs>?> ScrollHandlerProperty =
            AvaloniaProperty.RegisterAttached<ListBox, EventHandler<ScrollChangedEventArgs>?>("ScrollHandler", typeof(AutoScrollBehavior));

        private static readonly AttachedProperty<NotifyCollectionChangedEventHandler?> CollectionHandlerProperty =
            AvaloniaProperty.RegisterAttached<ListBox, NotifyCollectionChangedEventHandler?>("CollectionHandler", typeof(AutoScrollBehavior));
    }
}

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;

namespace qBittorrentCompanion.Views
{
    abstract public class TabControlsIcoWindow : IcoWindow
    {
        protected ReactiveCommand<Unit, Unit> NextTabCommand
            => ReactiveCommand.Create(NextTab);
        protected ReactiveCommand<Unit, Unit> PreviousTabCommand
            => ReactiveCommand.Create(PreviousTab);
        protected ReactiveCommand<int, Unit> FocusTabCommand
            => ReactiveCommand.Create<int>(FocusTab);

        protected TabStrip? TabStrip = null;
        private string _tabStripNotSetMessage = "The TabStrip wasn't set";


        public TabControlsIcoWindow()
        {
            var previousTabKeyBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control | KeyModifiers.Shift),
                Command = PreviousTabCommand
            };
            KeyBindings.Add(previousTabKeyBinding);

            var nextTabKeyBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control),
                Command = NextTabCommand
            };
            KeyBindings.Add(nextTabKeyBinding);
        }

        protected bool IsTabVisible(int index)
        {
            if (TabStrip?.Items[index] is Visual visual)
            {
                return visual.IsVisible;
            }
            return true;
        }

        private void NextTab()
        {
            if (TabStrip == null) return;

            int current = TabStrip.SelectedIndex;
            int count = TabStrip.ItemCount;

            for (int i = 1; i <= count; i++)
            {
                int checkIndex = (current + i) % count;
                if (IsTabVisible(checkIndex))
                {
                    TabStrip.SelectedIndex = checkIndex;
                    return;
                }
            }
        }

        private void PreviousTab()
        {
            if (TabStrip == null) return;

            int current = TabStrip.SelectedIndex;
            int count = TabStrip.ItemCount;

            for (int i = 1; i <= count; i++)
            {
                int checkIndex = (current - i + count) % count;
                if (IsTabVisible(checkIndex))
                {
                    TabStrip.SelectedIndex = checkIndex;
                    return;
                }
            }
        }

        protected virtual void FocusTab(int tabIndex)
        {
            if (TabStrip == null)
            {
                Debug.WriteLine(_tabStripNotSetMessage);
                return;
            }

            TabStrip.SelectedIndex = tabIndex;
        }

        /// <summary>
        /// Safe to assume there's at least two tabs - if not, why use tabs?
        /// </summary>
        protected void SetKeyBindings()
        {
            var focusFirstTabBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.D1, KeyModifiers.Control),
                Command = FocusTabCommand,
                CommandParameter = 0
            };
            KeyBindings.Add(focusFirstTabBinding);

            var focusSecondTabBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.D2, KeyModifiers.Control),
                Command = FocusTabCommand,
                CommandParameter = 1
            };
            KeyBindings.Add(focusSecondTabBinding);
        }
    }
}

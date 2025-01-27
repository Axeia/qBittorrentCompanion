using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;

namespace qBittorrentCompanion.Views
{
    abstract public class TabControlsIcoWindow : IcoWindow
    {
        protected ReactiveCommand<Unit, Unit> NextTabCommand { get; }
        protected ReactiveCommand<Unit, Unit> PreviousTabCommand { get; }
        protected ReactiveCommand<int, Unit> FocusTabCommand { get; }

        protected TabStrip? TabStrip = null;
        private string TabStripNotSetMessage = "The TabStrip wasn't set";


        public TabControlsIcoWindow()
        {
            NextTabCommand = ReactiveCommand.Create(NextTab);
            PreviousTabCommand = ReactiveCommand.Create(PreviousTab);
            FocusTabCommand = ReactiveCommand.Create<int>(FocusTab);

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

        private void NextTab()
        {
            if (TabStrip == null)
            {
                Debug.WriteLine(TabStripNotSetMessage);
                return;
            }

            int nextTabIndex = TabStrip.SelectedIndex + 1;
            if (nextTabIndex > TabStrip.ItemCount)
                nextTabIndex = 0;

            TabStrip.SelectedIndex = nextTabIndex;
        }

        private void PreviousTab()
        {
            if (TabStrip == null)
            {
                Debug.WriteLine(TabStripNotSetMessage);
                return;
            }

            int prevTabIndex = TabStrip.SelectedIndex - 1;
            if (prevTabIndex < 0)
                prevTabIndex = TabStrip.ItemCount - 1;

            TabStrip.SelectedIndex = prevTabIndex;
        }

        private void FocusTab(int tabIndex)
        {
            if (TabStrip == null)
            {
                Debug.WriteLine(TabStripNotSetMessage);
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

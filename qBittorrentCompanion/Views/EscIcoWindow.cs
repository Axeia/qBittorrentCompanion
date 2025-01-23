using Avalonia.Input;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Linq;
using System.Reactive;

namespace qBittorrentCompanion.Views
{
    /// <summary>
    /// Extends IcoWindow and adds the option to close the window using the escape key
    /// </summary>
    public class EscIcoWindow : IcoWindow
    {
        public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }

        public EscIcoWindow()
        {
            CloseWindowCommand = ReactiveCommand.Create(this.Close);

            Loaded += EscIcoWindow_Loaded;
        }

        private void EscIcoWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var closeWindowKeyBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.Escape, KeyModifiers.None),
                Command = CloseWindowCommand
            };
            KeyBindings.Add(closeWindowKeyBinding);

            // For whatever reason a control must have focus for the KeyBinding to work.
            FocusFirstFocusableControl();
        }

        private void FocusFirstFocusableControl()
        {
            if (this.GetVisualDescendants().OfType<IInputElement>().Any(c => c.IsFocused))
                return;

            var focusableControl = this.GetVisualDescendants()
                .OfType<IInputElement>()
                .FirstOrDefault(c => c.IsEnabled && c.Focusable);

            focusableControl?.Focus();
        }
    }
}
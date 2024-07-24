using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Text.RegularExpressions;


namespace qBittorrentCompanion.CustomControls
{
    internal class IpAddressTextBox : TextBox
    {
        private static readonly Regex IpRegex = new Regex(@"^(\d{0,3}\.){0,3}\d{0,3}$");

        public IpAddressTextBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FontFamily = Avalonia.Media.FontFamily.Parse("Consolas, Monospace");
        }

        public string IpAddress
        {
            get => GetValue(IpAddressProperty);
            set => SetValue(IpAddressProperty, value);
        }

        public static readonly StyledProperty<string> IpAddressProperty =
            AvaloniaProperty.Register<IpAddressTextBox, string>(nameof(IpAddress), "0.0.0.0");

        private void IpTextBox_TextInput(object sender, TextInputEventArgs e)
        {
            if (!IsValidInput(e.Text))
            {
                e.Handled = true;
            }
        }

        private void IpTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                MoveToNextSection();
                e.Handled = true;
            }
        }

        private bool IsValidInput(string input)
        {
            var textBox = this.FindControl<TextBox>("IpTextBox");
            string newText = textBox.Text + input;
            return IpRegex.IsMatch(newText);
        }

        private void MoveToNextSection()
        {
            var textBox = this.FindControl<TextBox>("IpTextBox");
            int position = textBox.CaretIndex;
            string text = textBox.Text;

            for (int i = position; i < text.Length; i++)
            {
                if (text[i] == '.')
                {
                    textBox.CaretIndex = i + 1;
                    return;
                }
            }

            for (int i = position; i >= 0; i--)
            {
                if (text[i] == '.')
                {
                    textBox.CaretIndex = i + 1;
                    return;
                }
            }
        }
    }
}
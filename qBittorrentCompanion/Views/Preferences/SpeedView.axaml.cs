using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;
using System.Diagnostics;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class SpeedView : UserControl
    {
        private const int MaxIntValue = int.MaxValue;

        public SpeedView()
        {
            InitializeComponent();
            if (Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
            }
            Loaded += SpeedView_Loaded;
        }

        private void SchedulerDaysComboBox_DataContextChanged(object? sender, System.EventArgs e)
        {
            SchedulerDaysComboBox.SelectedIndex = 0;
        }

        private void SpeedView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            InitializeComboBox(UploadLimitComboBox, 1);
            InitializeNumericUpDown(UploadLimitNumericUpDown, DisplayUploadLimitNumericUpDown, UploadLimitComboBox);

            InitializeComboBox(DownloadLimitComboBox, 1);
            InitializeNumericUpDown(DownloadLimitNumericUpDown, DisplayDownloadLimitNumericUpDown, DownloadLimitComboBox);

            InitializeComboBox(AlternativeUploadLimitComboBox, 1);
            InitializeNumericUpDown(AlternativeUploadLimitNumericUpDown, DisplayAlternativeUploadLimitNumericUpDown, AlternativeUploadLimitComboBox);

            InitializeComboBox(AlternativeDownloadLimitComboBox, 1);
            InitializeNumericUpDown(AlternativeDownloadLimitNumericUpDown, DisplayAlternativeDownloadLimitNumericUpDown, AlternativeDownloadLimitComboBox);

            RecalculateDisplayValues(); // Trigger once to initiate values
        }

        private void InitializeComboBox(ComboBox comboBox, int selectedIndex)
        {
            comboBox.SelectedIndex = selectedIndex;
            comboBox.SelectionChanged += LimitComboBox_SelectionChanged;
        }

        private void InitializeNumericUpDown(NumericUpDown numericUpDown, NumericUpDown displayNumericUpDown, ComboBox comboBox)
        {
            numericUpDown.ValueChanged += (sender, e) => RecalculateDisplayValue(numericUpDown, displayNumericUpDown, comboBox);
            displayNumericUpDown.ValueChanged += (sender, e) => RecalculateValue(numericUpDown, displayNumericUpDown, comboBox);
        }

        private void LimitComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox == UploadLimitComboBox)
                    RecalculateValue(UploadLimitNumericUpDown, DisplayUploadLimitNumericUpDown, UploadLimitComboBox);
                else if (comboBox == DownloadLimitComboBox)
                    RecalculateValue(DownloadLimitNumericUpDown, DisplayDownloadLimitNumericUpDown, DownloadLimitComboBox);
                else if (comboBox == AlternativeUploadLimitComboBox)
                    RecalculateValue(AlternativeUploadLimitNumericUpDown, DisplayAlternativeUploadLimitNumericUpDown, AlternativeUploadLimitComboBox);
                else if (comboBox == AlternativeDownloadLimitComboBox)
                    RecalculateValue(AlternativeDownloadLimitNumericUpDown, DisplayAlternativeDownloadLimitNumericUpDown, AlternativeDownloadLimitComboBox);
            }
        }

        private void DisplayLimitNumericUpDown_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (sender is NumericUpDown numericUpDown)
            {
                if (numericUpDown == DisplayUploadLimitNumericUpDown)
                    RecalculateValue(UploadLimitNumericUpDown, DisplayUploadLimitNumericUpDown, UploadLimitComboBox);
                else if (numericUpDown == DisplayDownloadLimitNumericUpDown)
                    RecalculateValue(DownloadLimitNumericUpDown, DisplayDownloadLimitNumericUpDown, DownloadLimitComboBox);
                else if (numericUpDown == DisplayAlternativeUploadLimitNumericUpDown)
                    RecalculateValue(AlternativeUploadLimitNumericUpDown, DisplayAlternativeUploadLimitNumericUpDown, AlternativeUploadLimitComboBox);
                else if (numericUpDown == DisplayAlternativeDownloadLimitNumericUpDown)
                    RecalculateValue(AlternativeDownloadLimitNumericUpDown, DisplayAlternativeDownloadLimitNumericUpDown, AlternativeDownloadLimitComboBox);
            }
        }

        private void RecalculateValue(NumericUpDown limitNumericUpDown, NumericUpDown displayLimitNumericUpDown, ComboBox limitComboBox)
        {
            if (DataContext is PreferencesWindowViewModel pwvm)
            {
                var multiplier = pwvm.GetMultiplierForUnit(limitComboBox.SelectedItem!.ToString()!);
                var value = displayLimitNumericUpDown.Value * multiplier;
                limitNumericUpDown.Value = value > MaxIntValue ? MaxIntValue : value;
            }
        }

        private void UploadLimitNumericUpDown_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            RecalculateDisplayValue(UploadLimitNumericUpDown, DisplayUploadLimitNumericUpDown, UploadLimitComboBox);
        }

        private void RecalculateDisplayValue(NumericUpDown limitNumericUpDown, NumericUpDown displayLimitNumericUpDown, ComboBox limitComboBox)
        {
            if (DataContext is PreferencesWindowViewModel pwvm)
            {
                var multiplier = pwvm.GetMultiplierForUnit(limitComboBox.SelectedItem!.ToString()!);
                var value = limitNumericUpDown.Value / multiplier;
                displayLimitNumericUpDown.Value = value > MaxIntValue ? MaxIntValue : value;
            }
        }

        private void RecalculateDisplayValues()
        {
            RecalculateDisplayValue(UploadLimitNumericUpDown, DisplayUploadLimitNumericUpDown, UploadLimitComboBox);
            RecalculateDisplayValue(DownloadLimitNumericUpDown, DisplayDownloadLimitNumericUpDown, DownloadLimitComboBox);
            RecalculateDisplayValue(AlternativeUploadLimitNumericUpDown, DisplayAlternativeUploadLimitNumericUpDown, AlternativeUploadLimitComboBox);
            RecalculateDisplayValue(AlternativeDownloadLimitNumericUpDown, DisplayAlternativeDownloadLimitNumericUpDown, AlternativeDownloadLimitComboBox);
        }
    }
}

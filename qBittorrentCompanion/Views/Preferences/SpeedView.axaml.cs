using Avalonia.Controls;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class SpeedView : UserControl
    {
        public SpeedView()
        {
            InitializeComponent();
        }

        private void SchedulerDaysComboBox_DataContextChanged(object? sender, System.EventArgs e)
        {
            SchedulerDaysComboBox.SelectedIndex = 0;
        }
    }
}

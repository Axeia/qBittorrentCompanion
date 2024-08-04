using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views.Preferences
{
    public partial class SpeedView : UserControl
    {
        public SpeedView()
        {
            InitializeComponent();
            if(Design.IsDesignMode)
            {
                DataContext = new PreferencesWindowViewModel();
            }
        }

        private void SchedulerDaysComboBox_DataContextChanged(object? sender, System.EventArgs e)
        {
            SchedulerDaysComboBox.SelectedIndex = 0;
        }
    }
}

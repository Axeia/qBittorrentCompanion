using Avalonia.Controls;
using qBittorrentCompanion.ViewModels;

namespace qBittorrentCompanion.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
            this.Loaded += SearchView_Loaded;
            this.DataContext = new SearchViewModel();
        }

        private void SearchView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(DataContext is SearchViewModel searchVm)
            {
                searchVm.Initialise();
            }
        }
    }
}

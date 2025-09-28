using AutoPropertyChangedGenerator;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public partial class DirectoriesViewModel() : ViewModelBase
    {

        [AutoPropertyChanged]
        private string _customName = string.Empty;

    }
}

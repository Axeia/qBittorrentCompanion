using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace qBittorrentCompanion.ViewModels
{
    public class CategoryCountViewModel(QBittorrent.Client.Category category) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private QBittorrent.Client.Category _category = category;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name
        {
            get { return _category.Name; }
            set
            {
                if (value != _category.Name)
                {
                    _category.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string SavePath
        {
            get { return _category.SavePath; }
            set
            {
                if (value != _category.SavePath)
                {
                    _category.SavePath = value;
                    OnPropertyChanged(nameof(SavePath));
                }
            }
        }

        public bool HasPath => _category != null && !string.IsNullOrEmpty(_category.SavePath);

        public override string ToString()
        {
            return SavePath ?? string.Empty;
        }

        private int _count = 0;
        public int Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        public bool IsEditable { get; set; } = true;
    }
}

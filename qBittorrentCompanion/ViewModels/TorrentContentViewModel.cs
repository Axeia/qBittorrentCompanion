
using Avalonia;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a DataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     * Basically if it doesn't work as it should wor
     */
    public class TorrentContentViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        private TorrentContent? _torrentContent;

        public ObservableCollection<TorrentContentViewModel> Contents { get; set; } = [];

        public string DisplayName { get; } //Set is ommitted - immutable 
        public bool IsFile = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public TorrentContentViewModel(TorrentContent torrentContent)
        {
            _torrentContent = torrentContent;
            DisplayName = torrentContent.Name;
            IsFile = true;
        }

        public TorrentContentViewModel(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public double? Availability
        {
            get => _torrentContent?.Availability ?? 0;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Availability)
                {
                    _torrentContent.Availability = value;
                    OnPropertyChanged();
                }
            }
        }

        public FluentIcons.Common.Symbol icon
        {
            get
            {
                if (_torrentContent is null)
                    return FluentIcons.Common.Symbol.Folder;

                string extension = Path.GetExtension(_torrentContent.Name).ToLower();
                switch (extension)
                {
                    // Compressed files
                    case ".zip":
                    case ".rar":
                    case ".7z":
                    case ".tar":
                    case ".gz":
                    case ".bz2":
                    case ".xz":
                        return FluentIcons.Common.Symbol.FolderZip;

                    // Video files
                    case ".mp4":
                    case ".mkv":
                    case ".avi":
                    case ".mov":
                    case ".wmv":
                    case ".flv":
                    case ".webm":
                    case ".m4v":
                        return FluentIcons.Common.Symbol.Video;

                    // Image files
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                    case ".tiff":
                    case ".svg":
                    case ".webp":
                        return FluentIcons.Common.Symbol.Image;

                    // Audio files
                    case ".mp3":
                    case ".wav":
                    case ".flac":
                    case ".aac":
                    case ".ogg":
                    case ".m4a":
                    case ".wma":
                    case ".aiff":
                        return FluentIcons.Common.Symbol.MusicNote1;

                    // Document files
                    case ".pdf":
                        return FluentIcons.Common.Symbol.DocumentPdf;
                    case ".doc":
                    case ".docx":
                    case ".odt":
                    case ".rtf":
                    case ".txt":
                        return FluentIcons.Common.Symbol.DocumentText;

                    // Spreadsheet files
                    case ".xls":
                    case ".xlsx":
                    case ".ods":
                    case ".csv":
                        return FluentIcons.Common.Symbol.LayoutCellFourFocusBottomLeft;

                    // Code files
                    case ".css":
                        return FluentIcons.Common.Symbol.DocumentCss;
                    case ".js":
                        return FluentIcons.Common.Symbol.DocumentJavascript;
                    case ".java":
                        return FluentIcons.Common.Symbol.DocumentJava;
                    case ".html":
                    case ".ts":
                    case ".json":
                    case ".xml":
                    case ".yml":
                    case ".yaml":
                    case ".cs":
                    case ".py":
                    case ".cpp":
                    case ".c":
                    case ".h":
                    case ".php":
                    case ".rb":
                    case ".swift":
                    case ".go":
                    case ".rs":
                        return FluentIcons.Common.Symbol.Code;

                    // Disk image files
                    case ".iso":
                    case ".img":
                    case ".dmg":
                        return FluentIcons.Common.Symbol.DocumentData;

                    default:
                        return FluentIcons.Common.Symbol.Document;

                }
            }
        }


        public int? Index
        {
            get => _torrentContent?.Index ?? 0;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Index)
                {
                    _torrentContent.Index = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IdForPost));
                }
            }
        }

        public bool IsSeed
        {
            get => _torrentContent?.IsSeeding ?? false;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.IsSeeding)
                {
                    _torrentContent.IsSeeding = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _folderName = "";

        public string Name
        {
            get => _torrentContent?.Name ?? _folderName;
            set
            {
                if (_torrentContent is not null)
                {
                    if (value != _torrentContent.Name)
                    {
                        _torrentContent.Name = value;
                        OnPropertyChanged();
                    }
                }
                else if (value != _folderName)
                {
                    _folderName = value;
                    OnPropertyChanged();
                }
            }
        }

        public Range PieceRange
        {
            get => _torrentContent?.PieceRange ?? new Range();
            set
            {
                if (_torrentContent is not null && !_torrentContent.PieceRange.Equals(value))
                {
                    _torrentContent.PieceRange = value;
                    OnPropertyChanged();
                }
            }
        }

        private TorrentContentPriority _folderPriority = TorrentContentPriority.Normal;
        public TorrentContentPriority Priority
        {
            get => _torrentContent?.Priority ?? _folderPriority;
            set
            {
                if (_torrentContent is not null)
                {
                    if (value != _torrentContent.Priority)
                    {
                        _torrentContent.Priority = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(ComboBoxIndex));
                    }
                }
                else if (_torrentContent is null && value != _folderPriority)
                {
                    _folderPriority = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ComboBoxIndex));
                }

            }
        }

        public int ComboBoxIndex
        {
            get
            {
                switch (Priority)
                {
                    default:
                    //case DownloadPriority.Mixed:
                        return 4;
                    case TorrentContentPriority.Skip:
                        return 0;
                    case TorrentContentPriority.Normal:
                        return 1;
                    case TorrentContentPriority.High:
                        return 2;
                    case TorrentContentPriority.Maximal:
                        return 3;
                }
            }
        }

        private double CalculateProgress()
        {
            double totalSize = 0;
            double completedSize = 0;

            foreach (TorrentContentViewModel tcvm in Contents)
            {
                totalSize += tcvm.Size;
                completedSize += tcvm.Size * tcvm.Progress;
            }

            if (totalSize == 0) return 0;

            return completedSize / totalSize;
        }

        public double Progress
        {
            get => _torrentContent?.Progress ?? CalculateProgress();
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Progress)
                {
                    _torrentContent.Progress = value;
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(Remaining));
                }
            }
        }

        public long Size
        {
            get => _torrentContent?.Size ?? Contents.Sum<TorrentContentViewModel>(t => t.Size);
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Size)
                {
                    _torrentContent.Size = value;
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(SizeHr));
                    OnPropertyChanged(nameof(Remaining));
                }
            }
        }

        private long CalculateRemaining()
        {
            long remaining = 0;

            foreach (TorrentContentViewModel tcvm in Contents)
            {
                remaining += tcvm.Remaining;
            }

            return remaining;
        }

        public long Remaining
        {
            get 
            {
                if (_torrentContent is not null)
                {
                    return Size - (long)(Size * Progress);
                }
                else
                {
                    return CalculateRemaining();
                }
            }

        }

        public string SizeHr => DataConverter.BytesToHumanReadable(Size);

        public string RemainingHr => DataConverter.BytesToHumanReadable(Remaining);
        public string IdForPost => Index.ToString();

        public void Update(TorrentContent tc)
        {
            Availability = tc.Availability;
            Index = tc.Index;
            IsSeed = tc.IsSeeding;
            Name = tc.Name;
            PieceRange = tc.PieceRange;
            Priority = tc.Priority;
            Progress = tc.Progress;
            Size = tc.Size;
        }

    }

}

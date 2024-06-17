using Avalonia;
using Avalonia.Media;
using QBittorrent.Client;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public static Geometry fileIcon = Geometry.Parse("");
        public static Geometry folderIcon = Geometry.Parse("");

        private TorrentContent? _torrentContent;
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
            LeftOffset = CalcLeftOffset(_torrentContent.Name);
        }

        public TorrentContentViewModel(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
            LeftOffset = CalcLeftOffset(Name);
        }

        public Thickness LeftOffset { get; }
        private Thickness CalcLeftOffset(string name)
        {
            int calculated = 8 + name.Count(c => c == '/') * 8;
            if (_torrentContent is null)
                calculated -= 8;
            //Debug.WriteLine($"{calculated} : {name}");
            return new Thickness(calculated, 0, 8, 0);
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

        public Geometry icon
        {
            get => _torrentContent is not null ? fileIcon : folderIcon;
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

        private double _folderProgress = 0;
        public double Progress
        {
            get => _torrentContent?.Progress ?? _folderProgress;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Progress)
                {
                    _torrentContent.Progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Remaining));
                }
                else if (_torrentContent is null && value != _folderProgress)
                {
                    _folderProgress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Remaining));
                }
            }
        }

        private long _folderSize = 0;
        public long Size
        {
            get => _torrentContent?.Size ?? _folderSize;
            set
            {
                if (_torrentContent is not null && value != _torrentContent.Size)
                {
                    _torrentContent.Size = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SizeHr));
                    OnPropertyChanged(nameof(Remaining));
                }
                else if (_torrentContent is null && value != _folderSize)
                {
                    _folderSize = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SizeHr));
                    OnPropertyChanged(nameof(Remaining));
                }
            }
        }

        private long _folderRemaining = 0;
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
                    return _folderRemaining;
                }
            }
            set
            {
                _folderRemaining = Size - (long)(Size * Progress);
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingHr));
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

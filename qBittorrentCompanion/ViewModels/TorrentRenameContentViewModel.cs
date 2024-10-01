using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;


namespace qBittorrentCompanion.ViewModels
{
    /**
     * Avalonia seems to run into problems displaying a TreeDataGrid with multiple classes even if 
     * they enherit from the same baseclass/follow the same blueprint. That means that this class
     * is trying to fulfill the role of two classes (file and folder viewmodels).
     */
    public class TorrentRenameContentViewModel : TorrentContentViewModel
    {
        private ObservableCollection<TorrentRenameContentViewModel> _children = [];

        public new IReadOnlyList<TorrentRenameContentViewModel> Children => _children;
        public void AddChild(TorrentRenameContentViewModel child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            child.PropertyChanged += Child_PropertyChanged;

            _children.Add(child);
            folderPriority = _children.All(c => c.Priority == child.Priority)
                ? child.Priority
                : null;
            OnPropertyChanged(nameof(Children));
        }

        private TorrentRenameContentViewModel? _parent;
        public TorrentRenameContentViewModel? Parent { get => _parent; }

        public TorrentRenameContentViewModel(string infoHash, TorrentContent torrentContent, TorrentRenameContentViewModel? parent) : base(infoHash, torrentContent)
        {
            _parent = parent;
        }

        public TorrentRenameContentViewModel(string infoHash, string name, TorrentRenameContentViewModel? parent) : base(infoHash, name)
        {
            _parent = parent;
        }


        private bool _checkForMatch = true;
        public bool IsChecked
        {
            get => _checkForMatch;
            set
            {
                if (_checkForMatch != value)
                {
                    _checkForMatch = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        /// <summary>
        /// TODO: check if it's a file if temporary/download directory is set
        /// Note: Will not work properly for files that use a period as a separator and have no extension
        /// Note: Until todo is implemented it will not work properly for folders with periods in their name
        /// </summary>
        public string FileExtension
        {
            get
            {
                var lastPeriodPos = DisplayName.LastIndexOf('.');
                return lastPeriodPos == -1
                    ? string.Empty
                    : DisplayName.Substring(lastPeriodPos + 1);
            }
        }

        /// <summary>
        /// Basically cuts off the last period and everything behind it to 
        /// return the file name and the name only.
        /// TODO: check if it's a file if temporary/download directory is set
        /// Note: Will not work properly for files that use a period as a separator and have no extension
        /// Note: Until todo is implemented it will not work properly for folders with periods in their name
        /// </summary>
        public string FileName
        {
            get
            {
                var lastPeriodPos = DisplayName.LastIndexOf('.');
                return lastPeriodPos == -1
                    ? DisplayName
                    : DisplayName.Substring(0, lastPeriodPos);
            }
        }

        private string _renameTo = string.Empty;
        public string RenameTo
        {
            get => _renameTo;
            set
            {
                if (_renameTo != value)
                {
                    _renameTo = value;
                    OnPropertyChanged(nameof(RenameTo));
                }
            }
        }

        public void GetAll(List<TorrentRenameContentViewModel> tcvrml)
        {
            tcvrml.Add(this);
            foreach (var child in Children)
                child.GetAll(tcvrml);
        }

        public void GetAllToBeRenamed(List<TorrentRenameContentViewModel> tcvrml)
        {
            if (RenameTo != string.Empty)
                tcvrml.Add(this);

            foreach (var child in Children)
                child.GetAllToBeRenamed(tcvrml);
        }


        /// <summary>
        /// Name contains the path of how this instance was created.
        /// For renaming purposes (where the parent may have been renamed) recreate the
        /// path by iterating over the parents and getting their displayName.
        /// </summary>
        /// <returns></returns>
        public string NameToRenameTo()
        {
            List<string> pathParts = [];

            var node = this;
            do
            {
                pathParts.Add(node.RenameTo == string.Empty ? node.DisplayName : RenameTo);
                node = node._parent;
            }
            while (node != null);

            pathParts.Reverse();

            return string.Join('/', pathParts);
        }


        /// <summary>
        /// <inheritdoc cref="TorrentContentViewModel"/>
        /// Calls the <see cref="TorrentContentViewModel.Name"> base implementation</see> but also calls 
        /// <see cref="CascadeNameChangesDownwards(string, string)">CascadeNameChangesDownwards</see>
        /// </summary>
        public new string Name
        {
            get => base.Name;
            set
            {
                var oldValue = base.Name;
                base.Name = value;

                if ((!IsFile && value != oldValue))
                {
                    CascadeNameChangesDownwards(oldValue, value);
                }
            }
        }

        /// <summary>
        /// Will be called recursively as it's called by changes to <see cref="Name">Name</see> 
        /// and changes <see cref="Name">Name</see> properties of it <see cref="Children">Children</see>
        /// (and its children's children etc through recursion)
        /// </summary>
        /// <param name="oldName"/>
        /// <param name="newName"/>
        private void CascadeNameChangesDownwards(string oldName, string newName)
        {
            foreach (var child in Children)
            {
                if (!child.Name.StartsWith(newName))
                {
                    var newChildName = newName + '/' + child.DisplayName;
                    //Debug.WriteLine($"{this.Name} renaming child to {newChildName}");
                    child.Name = newChildName;
                }
            }
        }
    }
}

using Avalonia.Controls;
using DynamicData;
using qBittorrentCompanion.Helpers;
using qBittorrentCompanion.Services;
using RaiseChangeGenerator;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.ViewModels.LocalSettings
{
    public enum MonitoredDirectoryAction
    {
        Move,
        ChangeExtension,
        Delete
    }

    /// <summary>
    /// Simplistic class just used to hold values and store/restore from config. 
    /// <see cref="SelectableMonitoredDirectoryViewModel"/> for the class used in the UI.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="whatToDo"></param>
    public class MonitoredDirectory(string path, MonitoredDirectoryAction whatToDo)
    {
        public string PathToMonitor = path;
        public MonitoredDirectoryAction Action = whatToDo;
        public string PathToMoveTo = string.Empty;

        public AddTorrentRequestBaseDto? Optionals;
    }

    public partial class MonitorDirectoriesViewModel : ViewModelBase
    {
        private readonly Dictionary<SelectableMonitoredDirectoryViewModel, PropertyChangedEventHandler> _changeHandlers = [];
        private readonly Dictionary<SelectableMonitoredDirectoryViewModel, IDisposable> _selectableDirectorySubscriptions = [];


        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance", 
            "CA1822:Mark members as static", 
            Justification = "XAML problems if made static")
        ]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Supressing a warning about supressing")]
        public List<MonitoredDirectoryAction> MonitoredDirectoryActionOptions
            => [.. Enum.GetValues(typeof(MonitoredDirectoryAction)).Cast<MonitoredDirectoryAction>()];

        [RaiseChange]
        private ObservableCollection<SelectableMonitoredDirectoryViewModel> _monitoredDirectories = 
            Design.IsDesignMode 
            ? [
                new SelectableMonitoredDirectoryViewModel("C:\\Very\\Long\\Path\\To\\Be\\Trimmed\\Trim\\Me\\Already\\Please", MonitoredDirectoryAction.Move){ PathToMoveTo = "C:\\temp\\OldTorrents\\" },
                new SelectableMonitoredDirectoryViewModel("C:\\temp\\", MonitoredDirectoryAction.ChangeExtension),
                new SelectableMonitoredDirectoryViewModel("C:\\temp\\other\\", MonitoredDirectoryAction.Delete)
            ]
            : [];

        private MonitoredDirectoryAction _addDirectoryDefaultAction = Design.IsDesignMode
            ? MonitoredDirectoryAction.ChangeExtension
            : ConfigService.AddDirectoryDefaultAction;
        public MonitoredDirectoryAction AddDirectoryDefaultAction
        {
            get => _addDirectoryDefaultAction;
            set
            {
                if (value != _addDirectoryDefaultAction)
                {
                    ConfigService.AddDirectoryDefaultAction = value;
                    _addDirectoryDefaultAction = value;
                    this.RaisePropertyChanged(nameof(AddDirectoryDefaultAction));
                }
            }
        }

        [RaiseChange]
        private bool _deleteModeIsEnabled = false;

        public ReactiveCommand<Unit, Unit> ToggleDeleteModeCommand
            => ReactiveCommand.Create(() => { DeleteModeIsEnabled = !DeleteModeIsEnabled; });


        /// <summary>
        /// Intended to be used as a 'control all' value for a checkbox controlling the state of all individual values
        /// <c>null</c> Only occurs when <see cref="SelectableMonitoredDirectoryViewModel.IsSelected"/> changes. 
        /// The checkbox this value is bound to should be two-state (threestate="false") 
        /// that way it will only cycle through true/false when uses directly.
        /// </summary>
        public bool? SelectAll
        {
            get
            {
                if (_monitoredDirectories.All(md => md.IsSelected))
                    return true;
                if (_monitoredDirectories.All(md => !md.IsSelected))
                    return false;

                return null;
            }
            set
            {
                if (value is bool b)
                {
                    foreach (var item in MonitoredDirectories)
                    {
                        item.IsSelected = b;
                    }
                }
            }
        }

        /// <summary>
        /// Enables/disables delete command
        /// </summary>
        public bool AnyDirectorySelected 
            => MonitoredDirectories.Any(md => md.IsSelected);
        // Enable/Disable the SaveAndApply command (and its associated button)
        public bool HasUnsavedChanges 
            => MonitoredDirectories.Any(md => md.HasUnsavedChanges);

        public ReactiveCommand<Unit, Unit> SaveAndApplyCommand
            => ReactiveCommand.Create(SaveAndApply, this.WhenAnyValue(vm => vm.HasUnsavedChanges));
        /// <summary>
        /// Only store directories of which <see cref="SelectableMonitoredDirectoryViewModel.MonitoredDirectory"/><br/>
        /// A) Is empty and the action is something other than <see cref="MonitoredDirectoryAction.Move"/><br/>
        /// or<br/>
        /// B) Is a valid directory path and the action is <see cref="MonitoredDirectoryAction.Move"/>
        /// </summary>
        public void SaveAndApply()
        {
            /// Retrieve the base <see cref="MonitoreDirectory"/> that <see cref="ConfigService"/> can store.
            var monitoredDirectories = MonitoredDirectories
                .Select(slmd => slmd.MonitoredDirectory)
                .Where(slmd => slmd.Action.Equals(MonitoredDirectoryAction.Move) && Directory.Exists(slmd.PathToMoveTo)
                || !slmd.Action.Equals(MonitoredDirectoryAction.Move) && string.IsNullOrEmpty(slmd.PathToMoveTo));

            // Finally, store to config.
            ConfigService.MonitoredDirectories = [.. monitoredDirectories];

            foreach(var item in MonitoredDirectories)
               item.MarkAsSaved();

            // Restart service so it picks up on the changes.
            DirectoryMonitorService.Instance.Initialize();
        }

        public ReactiveCommand<Unit, Unit> DeleteSelectedDirectoriesCommand
            => ReactiveCommand.Create(
                DeleteSelectedDirectories, 
                this.WhenAnyValue(vm=>vm.AnyDirectorySelected)
            );

        public void DeleteSelectedDirectories()
        {
            // Collect monitored paths to act as the identifier
            List<string> selectedDirectories = [.. MonitoredDirectories
                .Where(md => md.IsSelected)
                .Select(md => md.PathToMonitor)];
            
            // Get fresh collection straight from config (to avoid applying unsaved changes)
            var configMonitoredDirectories = ConfigService.MonitoredDirectories;
            // Remove all entries matching the selected monitored paths
            configMonitoredDirectories.RemoveAll(cmd => selectedDirectories.Contains(cmd.PathToMonitor));
            // Store the directories in config
            ConfigService.MonitoredDirectories = configMonitoredDirectories;

            // Remove the same entries from the collection used for the UI
            MonitoredDirectories.RemoveMany(
                MonitoredDirectories.Where(md => selectedDirectories.Contains(md.PathToMonitor))
            );

            // If there's no directories remaining, disable delete mode
            if (MonitoredDirectories.Count == 0)
                DeleteModeIsEnabled = false;

            this.RaisePropertyChanged(nameof(HasUnsavedChanges));
        }
        
        public void CheckAllPaths()
        {
            foreach(var monitoredDirectory in MonitoredDirectories)
            {
                monitoredDirectory.PathToMonitorIsValid = Directory.Exists(monitoredDirectory.PathToMonitor);
                monitoredDirectory.PathToMoveToIsValid = Directory.Exists(monitoredDirectory.PathToMoveTo);
            }
        }

        public MonitorDirectoriesViewModel()
        {
            MonitoredDirectories.CollectionChanged += MonitoredDirectories_CollectionChanged;

            // Back up ConfigService so it can be compared to to see if there's any changes.
            List<MonitoredDirectory> mds = [.. ConfigService.MonitoredDirectories
                .Select(md => new MonitoredDirectory(md.PathToMonitor, md.Action)
                {
                    PathToMoveTo = md.PathToMoveTo,
                    Optionals = md.Optionals // if needed
                })];


            // Restore previous entries from config file
            foreach (var md in mds)
                MonitoredDirectories.Add(new SelectableMonitoredDirectoryViewModel(md));

            CheckAllPaths();
        }

        private void MonitoredDirectories_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var smd in e.OldItems.OfType<SelectableMonitoredDirectoryViewModel>())
                {
                    smd.StopTrackingSelection();

                    // Unsubscribe from property changes
                    if (_changeHandlers.TryGetValue(smd, out var handler))
                    {
                        smd.PropertyChanged -= handler;
                        _changeHandlers.Remove(smd);
                    }
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var smd in e.NewItems.OfType<SelectableMonitoredDirectoryViewModel>())
                {
                    if (smd is null)
                        continue;

                    smd.TrackSelection(() =>
                    {
                        this.RaisePropertyChanged(nameof(SelectAll));
                        this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                        this.RaisePropertyChanged(nameof(AnyDirectorySelected));
                    });

                    _selectableDirectorySubscriptions.Add(smd,
                        smd.WhenAnyValue(md => md.HasUnsavedChanges)
                        .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUnsavedChanges)))
                    );
                }
            }
        }

        private string _dotTorrentRenameExtensionPostfix = Design.IsDesignMode 
            ? ".qbcd" 
            : ConfigService.DotTorrentRenameExtensionPostfix;
        public string DotTorrentRenameExtensionPostfix
        {
            get => _dotTorrentRenameExtensionPostfix[1..];
            set
            {
                var sanitized = AsciiAlphaNumericOnlyRegex().Replace(value ?? string.Empty, "");
                var full = "." + sanitized;

                ExtensionPostfixErrorMessage = sanitized.Length == 0
                    ? "Not saved, too short"
                    : string.Empty;

                // If valid & different
                if (full != _dotTorrentRenameExtensionPostfix)
                {
                    ConfigService.DotTorrentRenameExtensionPostfix = full;
                    _dotTorrentRenameExtensionPostfix = full;
                    this.RaisePropertyChanged(nameof(DotTorrentRenameExtensionPostfix));
                }
            }
        }

        [RaiseChange]
        private string _extensionPostfixErrorMessage = string.Empty;

        [GeneratedRegex(@"[^a-zA-Z0-9]")]
        private static partial Regex AsciiAlphaNumericOnlyRegex();
    }
}

using QBittorrent.Client;
using qBittorrentCompanion.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System;
using Avalonia.Controls.Documents;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using DynamicData;
using System.Dynamic;
using qBittorrentCompanion.Models;

namespace qBittorrentCompanion.ViewModels
{
    public enum AddTrackerOption { AddToBottom, AddToSameTier, InsertAbove, InsertBelow }
    public enum AddTrackerError { NoSelection, WrongFormat }

    public class EditTrackersWindowViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        public string[] TrackerStarts => ["https://", "http://", "udp://"];
        public static AddTrackerOption[] AddTrackerOptions =>
            [AddTrackerOption.AddToBottom, AddTrackerOption.AddToSameTier, AddTrackerOption.InsertAbove, AddTrackerOption.InsertBelow];

        private AddTrackerOption _addTrackerOption = AddTrackerOption.AddToBottom;
        public AddTrackerOption SelectedTrackerOption
        {
            get => _addTrackerOption;
            set
            {
                this.RaiseAndSetIfChanged(ref _addTrackerOption, value);
                ValidateTracker(TrackerToAdd);
            }
        }

        private string _infoHash;

        public bool HasChanged => ToDeleteCount > 0 || ToAddCount > 0 || ToEditCount > 0;

        private int _deletedCount = 0;
        public int ToDeleteCount
        {
            get => Trackers.Count(t=>t.DeleteMe);
        }
        public int ToAddCount => Trackers.Count(t=>t.IsNew);

        public int ToEditCount => Trackers.Count(t => t.IsModified && !t.DeleteMe);
        public int DupedCount => 0;
        public bool HasDupes => DupedCount > 0;

        public int NewTier => Tiers.Last();

        private ObservableCollection<EditTorrentTrackerViewModel> _trackers = [];

        /// <summary>
        /// Do **NOT** use .add Directly on Trackers! Use <see cref="AddTracker(TorrentTracker, bool)">
        /// or things like <see cref="Tiers"/> will go awry!
        /// </summary>
        public ObservableCollection<EditTorrentTrackerViewModel> Trackers 
            => _trackers;

        private EditTorrentTrackerViewModel? _selectedTracker = null;
        public EditTorrentTrackerViewModel? SelectedTracker
        {
            get => _selectedTracker;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTracker, value);
                ValidateTracker(TrackerToAdd);

                this.RaisePropertyChanged(nameof(CanMergeSelectedTrackerIntoTierAbove));
                this.RaisePropertyChanged(nameof(CanMergeSelectedTrackerIntoTierBelow));
                this.RaisePropertyChanged(nameof(CanMoveUpSelectedTracker));
                this.RaisePropertyChanged(nameof(CanMoveDownSelectedTracker));
                this.RaisePropertyChanged(nameof(CanMergeSelection));
                this.RaisePropertyChanged(nameof(CanSplitSelection));
            }
        }

        private ObservableCollection<EditTorrentTrackerViewModel> _selectedTrackers = [];
        public ObservableCollection<EditTorrentTrackerViewModel> SelectedTrackers => _selectedTrackers;

        public bool CanMergeSelectedTrackerIntoTierAbove
        {
            get
            {
                if(SelectedTracker != null)
                {
                    var trackerAboveIndex = Trackers.IndexOf(SelectedTracker) - 1;
                    if(trackerAboveIndex > -1)
                    {
                        if(Trackers[trackerAboveIndex].NewTier < SelectedTracker.NewTier)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool CanMergeSelectedTrackerIntoTierBelow
        {
            get
            {
                if (SelectedTracker != null)
                {
                    var trackerBelowIndex = Trackers.IndexOf(SelectedTracker) + 1;
                    if (trackerBelowIndex < Trackers.Count())
                    {
                        if (Trackers[trackerBelowIndex].NewTier > SelectedTracker.NewTier)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool CanMergeSelection => SelectedTrackers.Count() >= 2
            && !SelectedTrackers.All(ettvm => ettvm.NewTier == SelectedTrackers.First().NewTier);
        public bool CanSplitSelection => SelectedTrackers.Count() >= 2 
            && SelectedTrackers.All(ettvm => ettvm.NewTier == SelectedTrackers.First().NewTier);

        public bool CanMoveUpSelectedTracker =>
            SelectedTracker != null && Trackers.IndexOf(SelectedTracker) > 0;
        public bool CanMoveDownSelectedTracker =>
            SelectedTracker != null && Trackers.IndexOf(SelectedTracker) < Trackers.Count() - 1;

        private ObservableCollection<int> _tiers = [];
        public ObservableCollection<int> Tiers
        {
            get => _tiers;
            set
            {
                this.RaiseAndSetIfChanged(ref _tiers, value);
                this.RaisePropertyChanged(nameof(NewTier));
            }
        }

        /* Validation */
        private void ValidateTracker(string value)
        {
            ClearErrors(nameof(TrackerToAdd));

            if (value != string.Empty)
            {
                Uri? uri = null;

                if (!value.StartsWith("http://") && !value.StartsWith("udp://") && !value.StartsWith("https://"))
                {
                    AddError(nameof(TrackerToAdd), "Trackers have to start with http://, https:// or udp://");
                }
                else if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
                {
                    AddError(nameof(TrackerToAdd), "Malformatted tracker Url");
                }
                else if ((SelectedTrackerOption == AddTrackerOption.InsertAbove || SelectedTrackerOption == AddTrackerOption.InsertBelow)
                    && SelectedTracker == null)
                {
                    AddError(nameof(TrackerToAdd), "No tracker is selected (select one below)");
                }
                else if (SelectedTracker != null && SelectedTrackerOption == AddTrackerOption.InsertAbove || SelectedTrackerOption == AddTrackerOption.InsertBelow)
                {
                    var selectedIndex = Trackers.IndexOf(SelectedTracker!);

                    if (SelectedTrackerOption == AddTrackerOption.InsertAbove
                        && selectedIndex > 0 && SelectedTracker!.NewTier == Trackers.ElementAt(selectedIndex - 1).NewTier)
                    {
                        AddError(nameof(TrackerToAdd),
                            "Cannot add new tracker 'above' if the tracker above is of the same tier (due to API limitations). "
                            + "Please select the top item of this tier group or select the `Add to same tier` option instead.");
                    }
                    else if(SelectedTrackerOption == AddTrackerOption.InsertBelow
                        && selectedIndex+1 < Trackers.Count() && SelectedTracker!.NewTier == Trackers.ElementAt(selectedIndex + 1).NewTier)
                    {
                        AddError(nameof(TrackerToAdd),
                            "Cannot add new tracker 'below' if the item under it belongs to the same tier (due to API limitations). "
                            + "Please select bottom item of this tier group or select the `Add to the same tier` option instead");
                    }
                }
            }

            this.RaisePropertyChanged(nameof(TrackerToAddIsValid));
        }

        public bool TrackerToAddIsValid => TrackerToAdd != string.Empty
            && !_errors.ContainsKey(nameof(TrackerToAdd))
            || (_errors.ContainsKey(nameof(TrackerToAdd)) && _errors[nameof(TrackerToAdd)].Count == 0);

        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors[propertyName] = new List<string>();
            }
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        public bool HasErrors => _errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable? GetErrors(string propertyName)
        {
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }
        /* /Validation */


        private string _trackerToAdd = string.Empty;
        public string TrackerToAdd
        {
            get => _trackerToAdd;
            set
            {
                this.RaiseAndSetIfChanged(ref _trackerToAdd, value);
                ValidateTracker(value);
            }
        }

        private ObservableCollection<Inline> _errorDescription = [];
        public ObservableCollection<Inline> ErrorDescription
        {
            get => _errorDescription;
            set => this.RaiseAndSetIfChanged(ref _errorDescription, value);
        }

        public EditTrackersWindowViewModel(string hash, string? torrentName)
        {
            _infoHash = hash;
            _ = FetchDataAsync();

            if(Design.IsDesignMode)
            {
                AddTracker(new TorrentTracker() { Tier = 0, Url = new Uri("http://p.com") });
                AddTracker(new TorrentTracker() { Tier = 1, Url = new Uri("http://placeholder1.com") });
                AddTracker(new TorrentTracker() { Tier = 2, Url = new Uri("http://placeholder2.com") });
                AddTracker(new TorrentTracker() { Tier = 3, Url = new Uri("http://placeholder3.com") });
            }

            SetSelectedAddTrackerOptionCommand = ReactiveCommand.Create<AddTrackerOption>(SetSelectedAddTrackerOption);
            AddTrackerCommand = ReactiveCommand.Create(AddTracker);
            SaveTrackersCommand = ReactiveCommand.CreateFromTask(SaveTrackers);

            SelectedTrackers.CollectionChanged += SelectedTrackers_CollectionChanged;
        }

        private void SelectedTrackers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(CanMergeSelection));
            this.RaisePropertyChanged(nameof(CanSplitSelection));
        }

        public ReactiveCommand<AddTrackerOption, Unit> SetSelectedAddTrackerOptionCommand { get; }
        private void SetSelectedAddTrackerOption(AddTrackerOption addTrackerOption)
        {
            SelectedTrackerOption = addTrackerOption;
        }
        public ReactiveCommand<Unit, Unit> AddTrackerCommand { get; }

        private void AddTracker()
        {
            Uri? uri = new Uri(TrackerToAdd);
            switch (SelectedTrackerOption)
            {
                case AddTrackerOption.AddToBottom:
                    var tt = new TorrentTracker { Url = uri, Tier = NewTier };
                    AddTracker(tt, true);
                    break;
                case AddTrackerOption.AddToSameTier:
                    AddNewTrackerToSameTier(uri!);
                    break;
                case AddTrackerOption.InsertAbove:
                    AddNewTrackerAbove(uri!);
                    break;
                case AddTrackerOption.InsertBelow:
                    AddNewTrackerBelow(uri!);
                    break;
            }

            TrackerToAdd = "";
            this.RaisePropertyChanged(nameof(ToAddCount));
            this.RaisePropertyChanged(nameof(HasChanged));
        }

        private void AddNewTrackerToSameTier(Uri uri)
        {
            var lastTrackerOfSameTier = Trackers.Where(t => t.Tier == SelectedTracker!.NewTier).Last();
            var ettvm = new EditTorrentTrackerViewModel(new TorrentTracker { Url = uri, Tier = lastTrackerOfSameTier.Tier }, true);
            AddTrackerAfter(lastTrackerOfSameTier, ettvm);
        }

        private void AddTrackerAfter(EditTorrentTrackerViewModel toInsertAfter, EditTorrentTrackerViewModel newTracker)
        {
            Trackers.Insert(Trackers.IndexOf(toInsertAfter)+1, newTracker);
        }
        private void AddNewTrackerAbove(Uri uri)
        {
            var selectedIndex = Trackers.IndexOf(SelectedTracker!);

            var newEttvm = new EditTorrentTrackerViewModel(
                new TorrentTracker {Tier = SelectedTracker!.Tier, Url = uri}, 
                true
            );

            Trackers.Insert(Trackers.IndexOf(SelectedTracker!), newEttvm);
            PlusOneTiersAfter(newEttvm);
        }

        private void AddNewTrackerBelow(Uri uri)
        {
            var selectedIndex = Trackers.IndexOf(SelectedTracker!);

            // Use second last tier if it's to be added all the way at the end.
            if (selectedIndex == Trackers.Count() - 1)
            {
                AddTracker(new TorrentTracker() { Url = uri, Tier = _trackers.Last().Tier + 1 });
            }
            else if(selectedIndex + 1 < _trackers.Count())
            {
                var nextTracker = Trackers.ElementAt(selectedIndex + 1);
                var newEttvm = new EditTorrentTrackerViewModel(
                    new TorrentTracker { Url = uri, Tier = nextTracker.Tier }, 
                    true
                );
                Trackers.Insert(Trackers.IndexOf(nextTracker), newEttvm);
                PlusOneTiersAfter(newEttvm);
            }
        }

        public ReactiveCommand<Unit, Unit> SaveTrackersCommand { get; }

        public async Task SaveTrackers()
        {
            // Delete trackers
            var TrackersToDelete = Trackers.Where(t => t.DeleteMe);
            if (TrackersToDelete.Count() > 0)
            {
                TrackersToDelete.ToList().ForEach(t => t.IsInProgress = true);
                try
                {
                    await QBittorrentService.QBittorrentClient.DeleteTrackersAsync(_infoHash, TrackersToDelete.Select(t => t.Url));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    Trackers.RemoveMany(TrackersToDelete);
                }
            }

            var TrackersToModify = Trackers.Where(t => t.IsModified);
            if(TrackersToModify.Count() > 0)
            {
                foreach(var trackerToModify in TrackersToModify)
                {
                    try
                    {
                        trackerToModify.IsInProgress = true;
                        await QBittorrentService.QBittorrentClient.EditTrackerAsync(
                            _infoHash, trackerToModify.Url, new Uri(trackerToModify.NewUrl)
                        );

                        trackerToModify.Url = new Uri(trackerToModify.NewUrl);
                        trackerToModify.OnPropertyChanged(nameof(EditTorrentTrackerViewModel.Url));

                        trackerToModify.Tier = NewTier;
                        trackerToModify.OnPropertyChanged(nameof(EditTorrentTrackerViewModel.Tier));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        trackerToModify.IsInProgress = false;
                    }
                }
            }

            var trackersToAdd = Trackers.Where(t => t.IsNew);
            if (trackersToAdd.Count() > 0)
            {
                await QBittorrentService.QBittorrentClient.AddTrackersAsync(
                    _infoHash, trackersToAdd.ToList().Select(t => new Uri(t.NewUrl))
                );
            }
        }

        protected async Task FetchDataAsync()
        {
            var rawTrackers = await QBittorrentService.QBittorrentClient.GetTorrentTrackersAsync(_infoHash);
            var trackers = rawTrackers.Where(t => t.Tier > -1).ToList();
            Intialise(trackers);
        }

        private void Intialise(List<TorrentTracker> trackers)
        {
            foreach (var tracker in trackers)
                AddTracker(tracker);
        }

        /// <summary>
        /// To ensure the tiers are kept up to date and 
        /// the PropertyChanged event is handled this method has to be useds rather
        /// than _Trackers.Add()
        /// </summary>
        /// <param name="tt"></param>
        /// <param name="isNew"></param>
        public void AddTracker(TorrentTracker tt, bool isNew = false)
        {
            int tierMinusOne = (tt.Tier ?? default) - 1;
            int tierPlusOne = (tt.Tier ?? default) + 1;

            if (tierMinusOne >= 0 && !Tiers.Contains(tierMinusOne))
                Tiers.Add(tierMinusOne);

            if (!Tiers.Contains(tt.Tier ?? default))
                Tiers.Add(tt.Tier ?? default);

            if (!Tiers.Contains(tierPlusOne))
                Tiers.Add(tierPlusOne);


            var ettvm = new EditTorrentTrackerViewModel(tt, isNew);
            ettvm.PropertyChanged += Ettvm_PropertyChanged;

            Trackers.Add(ettvm);
            this.RaisePropertyChanged(nameof(HasChanged));
        }

        private void Ettvm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is EditTorrentTrackerViewModel ettvm)
            {
                if (e.PropertyName == nameof(EditTorrentTrackerViewModel.DeleteMe))
                {
                    this.RaisePropertyChanged(nameof(ToDeleteCount));
                    RedoTiersAfterDeleteChangeOn(ettvm);
                }

                if (e.PropertyName == nameof(EditTorrentTrackerViewModel.NewTier))
                    ChangeTierAndReorder(ettvm);

                if (e.PropertyName == nameof(EditTorrentTrackerViewModel.IsModified))
                    this.RaisePropertyChanged(nameof(ToEditCount));

                this.RaisePropertyChanged(nameof(HasChanged));


                Debug.WriteLine(e.PropertyName);

                // Could be optimised but the operation should be cheap so there's no need.
                // If there's an operation that **may** affect the tier count
                // (item getting deleted, tiers getting merged etc)
                // check if there's an excess of tiers now and delete if needed.
                switch (e.PropertyName)
                {
                    case nameof(EditTorrentTrackerViewModel.DeleteMe):
                    case nameof(EditTorrentTrackerViewModel.Tier):
                        int lastTier = Trackers.Last().NewTier;

                        Debug.WriteLine($"last {lastTier}");

                        // Iterate in reverse to prevent errors from modifying whilst iterating over it
                        for (int i = Tiers.Count - 1; i >= 0; i--)
                        {
                            if (Tiers[i] > lastTier+1)
                                Tiers.RemoveAt(i);
                        }
                        break;
                }

                this.RaisePropertyChanged(nameof(CanMergeSelectedTrackerIntoTierAbove));
                this.RaisePropertyChanged(nameof(CanMergeSelectedTrackerIntoTierBelow));
            }
        }

        private void ChangeTierAndReorder(EditTorrentTrackerViewModel tracker)
        {
            if(tracker.NewTier - tracker.Tier != 0) // Tier has definitely changed. Need to reorder
            {
                MoveToNewPosition(tracker);
            }
        }

        private void MoveToNewPosition(EditTorrentTrackerViewModel tracker)
        {
            // Get other trackers of this tier 
            var trackersOfThisTier = Trackers.Where(t => t.NewTier == tracker.NewTier && t != tracker);

            // Tier is currently in use?
            if (trackersOfThisTier.Count() > 0)
            {
                int firstIndexOfTierHolder = Trackers.IndexOf(trackersOfThisTier.First());
                int toBeChangedIndex = Trackers.IndexOf(tracker);

                // Tracker is getting moved up
                if(firstIndexOfTierHolder < toBeChangedIndex)
                {
                    // Increase the tier of all items from the new position up to just before the old position
                    int indexBeforeToBeChanged =  toBeChangedIndex - 1;
                    var trackersToIncrease = Trackers.ToList().GetRange(firstIndexOfTierHolder, indexBeforeToBeChanged);
                    trackersToIncrease.ForEach(t => t.NewTier++);

                    // Other tracker tiers now make sense, move into desired position
                    Trackers.Move(toBeChangedIndex, firstIndexOfTierHolder);
                }
                // Tracker is getting moved down
                else
                {
                    // Decrease the tier of all items from the old position up to just before the new position
                    int indexAfterToBeChanged = toBeChangedIndex + 1;
                    var trackersToDecrease = Trackers.ToList().GetRange(indexAfterToBeChanged, firstIndexOfTierHolder - indexAfterToBeChanged + 1);
                    trackersToDecrease.ForEach(t => t.NewTier--);

                    // Other tracker tiers now make sense, move into desired position
                    Trackers.Move(toBeChangedIndex, firstIndexOfTierHolder);
                }

            }
            
        }

        /// <summary>
        /// After a tracker is marked for deletion all that were in a higher tier after it should be moved up one tier
        /// If the tracker is 'undeleted' increase the tier of everything after it that is on a different tier
        /// </summary>
        /// <param name="ettvm"></param>
        private void RedoTiersAfterDeleteChangeOn(EditTorrentTrackerViewModel ettvm)
        {
            // Get the index of the trigger as items after it are affected
            var ettvmIndex = Trackers.IndexOf(ettvm);
            // Need to know the tier of the trigger, subsequent trackers might be in the same tier and should not get altered.
            var triggerTier = ettvm.NewTier;

            // Ensure the trigger is found
            if (ettvmIndex != -1)
            {
                // If delete is true, decrease the tier of everything after the trigger by 1
                if (ettvm.DeleteMe)
                    MinusOneTiersAfter(ettvm);
                // If delete is false, increase the tier of everything after the trigger by 1
                else
                    PlusOneTiersAfter(ettvm);
            }
        }

        private void MinusOneTiersAfter(EditTorrentTrackerViewModel ettvm)
        {
            var ettvmIndex = Trackers.IndexOf(ettvm);
            for (var i = ettvmIndex + 1; i < Trackers.Count; i++)
            {
                if (Trackers[i].NewTier > ettvm.NewTier)
                {
                    Trackers[i].NewTier--;
                }
            }
        }

        private void PlusOneTiersAfter(EditTorrentTrackerViewModel ettvm)
        {
            var ettvmIndex = Trackers.IndexOf(ettvm);
            // Need to know the tier of the trigger, subsequent trackers might be in the same tier and should not get altered.
            var triggerTier = ettvm.NewTier;

            for (var i = ettvmIndex + 1; i < Trackers.Count; i++)
            {
                if (Trackers[i].NewTier >= ettvm.NewTier)
                {
                    Trackers[i].NewTier++;
                }
            }
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using qBittorrentCompanion.CustomControls;
using qBittorrentCompanion.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace qBittorrentCompanion.Views;

public partial class RssPluginInfoView : UserControl
{
    private readonly ObservableCollection<RegexWizardViewModel> _regexifiedEntries = [];

    public RssPluginInfoView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new RssPluginSupportBaseViewModel();
            RuleTitlePersistentSelectionTextBlock.Text = "Test test test test test";
            ShowWizardMode();
        }

        DataContextChanged += RssPluginInfoView_DataContextChanged;
        Loaded += RssPluginInfoView_Loaded;
    }

    private void RssPluginInfoView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RegexifyDataGrid.DataContext = null;
        RegexifyDataGrid.ItemsSource = _regexifiedEntries;
        BadgeItemsControl.ItemsSource = _regexifiedEntries;
        _regexifiedEntries.CollectionChanged += RegexifiedEntries_CollectionChanged;
        if (Design.IsDesignMode)
            _regexifiedEntries.Add(
                new RegexWizardViewModel(new Run("Test test"), 1, 0)
            );

        // Only allow clicking the button if a selection is made.
        RuleTitlePersistentSelectionTextBlock.WhenAnyValue(
            x => x.SelectionStart,
            x => x.SelectionEnd,
            (start, end) => (start, end))
            .Subscribe(tuple => { 
                var (start, end) = tuple;
                if (end > start)
                {
                    if (RuleTitlePersistentSelectionTextBlock.GetRunWithSelection == null)
                    {
                        FlashMessage("Selection invalid");
                        RegexifyButton.IsEnabled = false;
                    }
                    else
                    {
                        FlashMessage($"Selected '{RuleTitlePersistentSelectionTextBlock.SelectedText}'");
                        RegexifyButton.IsEnabled = true;
                    }
                }
            });
    }

    private void RegexifiedEntries_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateOutput();
        var entriesExist = _regexifiedEntries.Count > 0;

        DataGridPlaceHolderDockPanel.IsVisible = !entriesExist;
        RegexifyDataGrid.IsVisible = entriesExist;
    }

    private void RssPluginInfoView_DataContextChanged(object? sender, EventArgs e)
    {
        MonitorInputChanges();
    }

    private void MonitorInputChanges()
    {
        if (DataContext is RssPluginSupportBaseViewModel rpsbvm
            && rpsbvm.RssPluginsViewModel is RssPluginsViewModel rpvm)
        {
            if (rpvm.SelectedPlugin is RssRuleWizard rrw)
            {
                rpsbvm
                    .WhenAnyValue(r => r.PluginInput)
                    .Subscribe(newValue => InputChanged(newValue));
            }
        }
    }

    private void ShowAppropriateLayout()
    {
        if (DataContext is RssPluginSupportBaseViewModel rpsbvm
            && rpsbvm.RssPluginsViewModel is RssPluginsViewModel rpvm)
        {
            if (rpsbvm.PluginInput == string.Empty)
                ShowLongDescription();
            else
            {
                if (rpvm.SelectedPlugin is RssRuleWizard)
                    ShowWizardMode();
                else
                    ShowPluginMode();
            }
        }
        else
            Debug.WriteLine("Something going horribly wrong, somehow the DataContext isn't" +
                " RssPluginSupportBaseViewModel or the RssPluginsViewModel wasn't found" );
    }

    private void ShowLongDescription()
    {
        PluginContentStackPanel.IsVisible = false;
        WizardStackPanel.IsVisible = false;
        LongDescriptionSimpleHtmlTextBlock.IsVisible = true;
    }

    private void ShowPluginMode()
    {
        PluginContentStackPanel.IsVisible = true;
        WizardStackPanel.IsVisible = false;
        LongDescriptionSimpleHtmlTextBlock.IsVisible = false;
    }

    private void ShowWizardMode()
    {
        PluginContentStackPanel.IsVisible = false;
        WizardStackPanel.IsVisible = true;
        LongDescriptionSimpleHtmlTextBlock.IsVisible = false;
    }

    int count = 0;
    private void InputChanged(string newValue)
    {
        ShowAppropriateLayout();

        // First, set the ItemsSource to null to detach it from the collection
        RegexifyDataGrid.ItemsSource = null;
        // Clear the collection
        _regexifiedEntries.Clear();
        // Then reattach the collection 
        RegexifyDataGrid.ItemsSource = _regexifiedEntries;

        count = 0;

        RuleTitlePersistentSelectionTextBlock.Text = "";
        RuleTitlePersistentSelectionTextBlock.Inlines = [];
        RuleTitlePersistentSelectionTextBlock.Inlines?.Add(new Run() { Text = newValue });
    }

    private void RegexifyButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(RuleTitlePersistentSelectionTextBlock.GetRunWithSelection() is Run run)
        {
            var selectedText = RuleTitlePersistentSelectionTextBlock.SelectedText;

            // If the selected text doesn't take up the whole Run
            if (run.Text != selectedText)
            {
                int runWithSelectionIndex = RuleTitlePersistentSelectionTextBlock.Inlines!.IndexOf(run);

                // If the run starts with the selection - only an insertion after it is needed
                if (run.Text!.StartsWith(selectedText))
                {
                    var runAfter = new Run() { Text = run.Text[selectedText.Length..] };
                    run.Text = run.Text[..selectedText.Length];
                    RuleTitlePersistentSelectionTextBlock.Inlines.Insert(runWithSelectionIndex + 1, runAfter);
                }
                // If the run ends with the selection only an insertion before it is needed
                else if (run.Text!.EndsWith(selectedText))
                {
                    var runBefore = new Run() { Text = run.Text[..(run.Text.Length - selectedText.Length)] };
                    RuleTitlePersistentSelectionTextBlock.Inlines.Insert(runWithSelectionIndex, runBefore);
                    run.Text = run.Text[selectedText.Length..];

                }
                else // No short cuts, need to insert before and after.
                {
                    int selectionStart = run.Text.IndexOf(selectedText);

                    var runBefore = new Run() { Text = run.Text[..selectionStart] };
                    int selectionEnd = selectionStart + selectedText.Length;
                    var runAfter = new Run() { Text = run.Text[selectionEnd..] };

                    RuleTitlePersistentSelectionTextBlock.Inlines.Insert(runWithSelectionIndex, runBefore);
                    run.Text = selectedText; // // Modify the original run to just be the selected text
                    RuleTitlePersistentSelectionTextBlock.Inlines.Insert(runWithSelectionIndex + 2, runAfter);
                }
            }
            // no `else` needed, run was the whole thing and can be marked entirely

            run.Background = PersistentSelectionTextBlock.MarkedBrush;
            RuleTitlePersistentSelectionTextBlock.ClearSelection();

            count++;
            // 5 = half of the width of the rectangle in the xaml
            var rwvm = new RegexWizardViewModel(run, count, GetBoundsForBadge(run).X-5);
            rwvm.WhenAnyValue(r => r.ReplaceWith).Subscribe(_ => UpdateOutput());
            _regexifiedEntries.Add(rwvm);
            UpdateOutput();
        }
        else
        {
            FlashMessage("Selection not found or invalid");
        }
    }

    private Point GetBoundsForBadge(Run run)
    {
        var bounds = RuleTitlePersistentSelectionTextBlock.GetBoundsForRun(run);

        return RuleTitlePersistentSelectionTextBlock.TranslatePoint(
            new Point(bounds.Left + bounds.Width / 2, bounds.Bottom), BadgeItemsControl
        )
        ?? new Point(0, 0);
    }

    private void UpdateOutput()
    {
        var txt = string.Empty;
        foreach (var run in RuleTitlePersistentSelectionTextBlock.Inlines!.OfType<Run>())
        {
            var ar = _regexifiedEntries.Where(e => e.AssociatedRun == run).FirstOrDefault();
            txt += ar != null ? ar.ReplaceWith : Regex.Escape(run.Text!);
        }

        OutputEditor.Text = txt;
    }

    private void UpdatePluginData()
    {
        if (DataContext is RssPluginSupportBaseViewModel rpsbvm
            && rpsbvm.RssPluginsViewModel is RssPluginsViewModel rpvm
            && rpvm.SelectedPlugin is RssRuleWizard rrw)
        {
            rrw.SetResult(OutputEditor.Text);
            rrw.SetTitle(PluginRuleTitleTextBox.Text ?? "");
        }
    }


    /// <summary>
    /// For convenience, just connects straight to <see cref="MainWindow.ShowFlashMessage(string)"/>
    /// </summary>
    /// <param name="message"></param>
    private void FlashMessage(string message)
    {
        if (Application.Current is not null
            && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null
            && desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ShowFlashMessage(message);
        }
    }

    private void ShowOptionsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(sender is Button button
            && button.ContextMenu is ContextMenu cm)
        {
            cm.Open();
        }
    }

    private void DeleteRowButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is RegexWizardViewModel viewModel)
        {
            var stringId = viewModel.Id.ToString();
            var entryToDelete = _regexifiedEntries
                .Where(r => r.Id.ToString() == stringId)
                .First();
            var associatedRun = entryToDelete.AssociatedRun;
            int idOfRemovedEntry = entryToDelete.Id;
            var indexOf = RuleTitlePersistentSelectionTextBlock.Inlines!.IndexOf(associatedRun);

            // remove from the collection
            // - Removes datagrid row
            // - Removes badge
            _regexifiedEntries.Remove(entryToDelete); 
            // Restore runs by merging into neighbour (if possible)
            MergeIntoAdjacentRun(indexOf, associatedRun);
            //Substract one from all subsequent ids
            _regexifiedEntries
                .Where(re => re.Id > idOfRemovedEntry)
                .ToList()
                .ForEach(re => re.Id = re.Id - 1);
            // Reduce count by one for future entries
            count--;
        }
    }

    private void MergeIntoAdjacentRun(int indexOf, Run associatedRun)
    {
        //Check left
        if (indexOf > 0
            // If there's a run on the left
            && RuleTitlePersistentSelectionTextBlock.Inlines![indexOf - 1] is Run runLeftOfCurrent
            // And it's not marked 
            && runLeftOfCurrent.Background != PersistentSelectionTextBlock.MarkedBrush)
        {
            runLeftOfCurrent!.Text += associatedRun.Text;
            RuleTitlePersistentSelectionTextBlock.Inlines.Remove(associatedRun);
        }
        //Check right
        else if (indexOf < RuleTitlePersistentSelectionTextBlock.Inlines!.Count
            // If there's a run on the right
            && RuleTitlePersistentSelectionTextBlock.Inlines[indexOf + 1] is Run runRightOfCurrent
            // And it's not marked 
            && runRightOfCurrent.Background != PersistentSelectionTextBlock.MarkedBrush)
        {
            runRightOfCurrent!.Text = associatedRun.Text + runRightOfCurrent.Text;
            RuleTitlePersistentSelectionTextBlock.Inlines.Remove(associatedRun);
        }
        //No suitable neighbour to merge into - undo brush on self
        else
        {
            associatedRun.Background = Brushes.Transparent;
        }
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdatePluginData();
    }

    private void BindableRegexEditor_TextChanged(object? sender, EventArgs e)
    {
        UpdatePluginData();
    }
}
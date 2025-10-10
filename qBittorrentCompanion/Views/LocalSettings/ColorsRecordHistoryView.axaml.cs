using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using qBittorrentCompanion.Models;
using qBittorrentCompanion.ViewModels.LocalSettings;
using System;

namespace qBittorrentCompanion.Views.LocalSettings
{
    public partial class ColorsRecordHistoryView : UserControl
    {
        /// <summary>
        /// Set to True for Redo, False for Undo
        /// </summary>
        public static readonly StyledProperty<bool> IsForRedoProperty =
            AvaloniaProperty.Register<ColorsRecordHistoryView, bool>(nameof(IsForRedo));

        public bool IsForRedo
        {
            get => GetValue(IsForRedoProperty);
            set => SetValue(IsForRedoProperty, value);
        }

        public ColorsRecordHistoryView()
        {
            InitializeComponent();

            if(Design.IsDesignMode)
            {
                var icvm = new IconCustomizationViewModel(true, LogoDataRecord.DarkModeDefault);
                icvm.B_Color = Colors.Orange;
                icvm.AddLogoDataRecordToHistory();
                icvm.B_Color = Colors.Teal;
                icvm.AddLogoDataRecordToHistory();
                DataContext = icvm;
            }

            this.GetObservable(IsForRedoProperty)
                .Subscribe((Action<bool>)(isRedo =>
                {
                    if (isRedo)
                    {
                        HistoryListBox.FlowDirection = FlowDirection.LeftToRight;
                    }
                }));

            IsForRedo = false;
        }
    }
}
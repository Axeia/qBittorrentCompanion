using Avalonia;
using Avalonia.Controls;
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
                var icvm = new IconCustomizationViewModel(true, LogoColorsRecord.DarkModeDefault);
                icvm.UseLogoColorsCommand.Execute(LogoColorsRecord.LightModeDefault);
                icvm.UseLogoColorsCommand.Execute(LogoColorsRecord.DarkModeDefault);
                icvm.UseLogoColorsCommand.Execute(LogoColorsRecord.LightModeDefault);
                icvm.UseLogoColorsCommand.Execute(LogoColorsRecord.DarkModeDefault);
                DataContext = icvm;
            }

            this.GetObservable(IsForRedoProperty)
                .Subscribe((Action<bool>)(isRedo =>
                {
                    if (isRedo)
                        if (DataContext is IconCustomizationViewModel icvm)
                            HistoryListBox.ItemsSource = icvm.LogoColorsRecordRedoHistory;
                }));
        }
    }
}
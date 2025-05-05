using AvaloniaEdit.TextMate;
using qBittorrentCompanion.AvaloniaEditor;

namespace qBittorrentCompanion.CustomControls
{
    public class BindableRegexEditor : BindableEditorBase
    {
        public BindableRegexEditor() : base()
        {
            // Set up TextMate integration
            var textMateInstallation = this.InstallTextMate(
                new CustomRegistryOptions("regex.tmLanguage.json", "source.regexp")
            );
            textMateInstallation.SetGrammar("source.regexp");
        }
    }
}

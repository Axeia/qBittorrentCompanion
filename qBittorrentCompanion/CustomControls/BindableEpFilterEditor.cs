using AvaloniaEdit.TextMate;
using qBittorrentCompanion.AvaloniaEditor;

namespace qBittorrentCompanion.CustomControls
{
    public class BindableEpFilterEditor : BindableEditorBase
    {
        public BindableEpFilterEditor() : base()
        {
            // Set up TextMate integration
            var textMateInstallation = this.InstallTextMate(
                new CustomRegistryOptions("qbepfilter.tmLanguage.json", "source.qbittorrent.episodefilter")
            );
            textMateInstallation.SetGrammar("source.qbittorrent.episodefilter");
        }
    }
}

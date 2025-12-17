using RaiseChangeGenerator;
using ReactiveUI;
using System;

namespace qBittorrentCompanion.ViewModels
{
    public partial class GitSearchPluginViewModel(string name, string author, string version, string lastUpdate, string downloadUri, string? infoUri, string comments) : ReactiveObject
    {
        [RaiseChange]
        private string _name = name;
        [RaiseChange]
        private string _author = author;
        [RaiseChange]
        private Version _version = new (version);
        [RaiseChange]
        private string _lastUpdate = lastUpdate;
        [RaiseChange]
        private Uri _downloadUri = new(downloadUri);
        [RaiseChange]
        private Uri? _infoUri = string.IsNullOrEmpty(infoUri) ? null : new Uri(infoUri!);
        [RaiseChange]
        private string _comments = comments;
    }
}

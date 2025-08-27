using AutoPropertyChangedGenerator;
using ReactiveUI;
using System;

namespace qBittorrentCompanion.ViewModels
{
    public partial class GitSearchPluginViewModel(string name, string author, string version, string lastUpdate, string downloadUri, string infoUrl, string comments) : ReactiveObject
    {
        [AutoPropertyChanged]
        private string _name = name;
        [AutoPropertyChanged]
        private string _author = author;
        [AutoPropertyChanged]
        private Version _version = new (version);
        [AutoPropertyChanged]
        private string _lastUpdate = lastUpdate;
        [AutoPropertyChanged]
        private Uri _downloadUri = new(downloadUri);
        [AutoPropertyChanged]
        private string _infoUrl = infoUrl;
        [AutoPropertyChanged]
        private string _comments = comments;
    }
}

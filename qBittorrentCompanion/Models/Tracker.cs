using Newtonsoft.Json;
using System.ComponentModel;

namespace qBittorrentCompanion.Models
{
    /** 
     * Some fields are declared nullable (the questionmark) so the default value is null rather
     * than an actual value. This is due to how the WebUI works sending all information in the 
     * initial response but ommitting fields in later responses.
     * 
     * Having the fields which (such as num_leechs) default to null rather than 0 it allows 
     * comparing of the value and a difference can be told between no update on it (null) and the 
     * default long value (0). 
     */
    public class Tracker : INotifyPropertyChanged
    {
        [JsonProperty("infohash")]
        public string? InfoHash { get; set; }

        [JsonProperty("added_on")]
        public long? AddedOn { get; set; }

        [JsonProperty("amount_left")]
        public long? AmountLeft { get; set; }

        [JsonProperty("auto_tmm")]
        public bool? AutoTmm { get; set; }

        [JsonProperty("availability")]
        public long? Availability { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("completed")]
        public long? Completed { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
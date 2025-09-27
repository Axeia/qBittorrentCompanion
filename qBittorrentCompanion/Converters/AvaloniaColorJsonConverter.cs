using Avalonia.Media;
using Newtonsoft.Json;
using System;

namespace qBittorrentCompanion.Converters
{
    public class AvaloniaColorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Color);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var color = (Color)value!;
            writer.WriteValue(color.ToString()); 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var colorString = (string?)reader.Value ?? "#000000";
            return Color.Parse(colorString);
        }
    }
}

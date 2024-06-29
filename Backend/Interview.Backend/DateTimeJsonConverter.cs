using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Interview.Backend;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public const string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString() ?? string.Empty;
        return DateTime.ParseExact(str, DefaultDateTimeFormat, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var str = value.ToString(DefaultDateTimeFormat, CultureInfo.InvariantCulture);
        writer.WriteStringValue(str);
    }
}

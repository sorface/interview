using System.Text.Json;
using System.Text.Json.Serialization;

namespace Interview.Domain.Dates;

public sealed class DateConverter : JsonConverter<Date>
{
    public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt64();
        return new Date(value);
    }

    public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.UnixTimeSeconds);
    }
}

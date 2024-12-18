using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Ardalis.SmartEnum.SystemTextJson;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.Events.Serializers;

public sealed class JsonEventSerializer : IEventSerializer, IEventDeserializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            new SmartEnumNameConverter<RoomQuestionState, int>(),
        },
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public string SerializeAsString(IRoomEvent? @event) => SerializePayloadAsString(@event);

    public byte[] SerializeToUtf8Bytes<T>(T? payload)
    {
        JsonSerializer.Serialize(new { });
        return payload is null
            ? JsonSerializer.SerializeToUtf8Bytes(new { }, _options)
            : JsonSerializer.SerializeToUtf8Bytes(payload, payload.GetType(), _options);
    }

    public string SerializePayloadAsString<T>(T? payload)
    {
        return payload is null ? "{}" : JsonSerializer.Serialize(payload, payload.GetType(), _options);
    }

    public T? Deserialize<T>(string? payload)
    {
        return payload is null ? default : JsonSerializer.Deserialize<T>(payload, _options);
    }

    public T? Deserialize<T>(byte[]? payload)
    {
        return payload is null ? default : JsonSerializer.Deserialize<T>(payload, _options);
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken)
    {
        return JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
    }
}

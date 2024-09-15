using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Ardalis.SmartEnum.SystemTextJson;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Events.Events.Serializers;

public sealed class JsonRoomEventSerializer : IRoomEventSerializer
{
    private static readonly JsonSerializerOptions _options = new()
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

    public string SerializePayloadAsString<T>(T? payload)
    {
        return payload is null ? "{}" : JsonSerializer.Serialize(payload, payload.GetType(), _options);
    }
}

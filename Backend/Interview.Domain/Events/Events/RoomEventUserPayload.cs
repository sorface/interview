using System.Text.Json;
using System.Text.Json.Serialization;
using Interview.Domain.Events.Events.Serializers;

namespace Interview.Domain.Events.Events;

public sealed class RoomEventUserPayload(Guid userId, Dictionary<string, object>? additionalData = null) : IPayloadBuilder
{
    public Guid UserId { get; } = userId;

    public Dictionary<string, object>? AdditionalData { get; } = additionalData;

    public string? BuildPayload(IRoomEventSerializer serializer)
    {
        return serializer.SerializePayloadAsString(AdditionalData);
    }
}

public sealed class RoomUserEvent : RoomEvent<RoomEventUserPayload>
{
}

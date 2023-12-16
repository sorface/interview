using System.Text.Json;

namespace Interview.Domain.Events.Events;

public sealed class RoomEventUserPayload : IPayloadBuilder
{
    public Guid UserId { get; }

    public Dictionary<string, object>? AdditionalData { get; }

    public RoomEventUserPayload(Guid userId, Dictionary<string, object>? additionalData = null)
    {
        UserId = userId;
        AdditionalData = additionalData;
    }

    public string? BuildPayload()
    {
        return AdditionalData is null ? null : JsonSerializer.Serialize(AdditionalData);
    }
}

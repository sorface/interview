using Interview.Domain.Events.Events;

namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomEventRequest(Guid roomId, Guid userId, string type, Dictionary<string, object>? additionalData)
    : IEventRequest
{
    public Guid RoomId { get; } = roomId;

    public Guid UserId { get; } = userId;

    public string Type { get; } = type;

    public Dictionary<string, object>? AdditionalData { get; } = additionalData;

    public IRoomEvent ToRoomEvent(bool stateful) => new RoomUserEvent
    {
        RoomId = RoomId,
        Type = Type,
        Value = new RoomEventUserPayload(UserId, AdditionalData),
        Stateful = stateful,
        CreatedById = UserId,
    };
}

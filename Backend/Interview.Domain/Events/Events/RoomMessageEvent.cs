using System.Text.Json.Serialization;

namespace Interview.Domain.Events.Events;

public sealed class RoomMessageEvent : RoomEvent<UserMessageEventPayload>
{
}

public sealed class UserMessageEventPayload
{
    public required string Message { get; init; }

    public required string Nickname { get; init; }
}

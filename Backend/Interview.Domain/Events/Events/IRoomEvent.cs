using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Events.Events;

public interface IRoomEvent
{
    Guid Id { get; }

    Guid RoomId { get; }

    string Type { get; }

    bool Stateful { get; }

    DateTime CreatedAt { get; }

    IRoomEventFilter Filter { get; }

    string? BuildStringPayload();
}

public interface IRoomEvent<out T> : IRoomEvent
{
    T? Value { get; }
}

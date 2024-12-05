using Interview.Domain.Events.Events.Serializers;

namespace Interview.Domain.Events.Events;

public interface IRoomEvent
{
    Guid Id { get; }

    Guid RoomId { get; }

    string Type { get; }

    bool Stateful { get; }

    DateTime CreatedAt { get; }

    Guid CreatedById { get; }

    string? BuildStringPayload(IRoomEventSerializer serializer);
}

public interface IRoomEvent<out T> : IRoomEvent
{
    T? Value { get; }
}

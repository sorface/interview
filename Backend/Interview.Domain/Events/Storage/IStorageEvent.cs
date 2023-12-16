namespace Interview.Domain.Events.Storage;

public interface IStorageEvent
{
    Guid Id { get; }

    Guid RoomId { get; }

    string Type { get; }

    string? Payload { get; }

    bool Stateful { get; }

    DateTime CreatedAt { get; }
}

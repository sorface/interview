namespace Interview.Domain.Events.Storage;

public class StorageEvent : IStorageEvent
{
    public required Guid Id { get; set; }

    public required Guid RoomId { get; set; }

    public required string Type { get; set; }

    public required string? Payload { get; set; }

    public required bool Stateful { get; set; }

    public required DateTime CreatedAt { get; init; }
}

using Interview.Domain.Repository;
using Interview.Domain.Rooms;

namespace Interview.Domain.Events;

public class DbRoomEvent : Entity
{
    public required Guid RoomId { get; init; }

    public required string Type { get; init; }

    public required bool Stateful { get; init; }

    public required string? Payload { get; init; }

    public Room? Room { get; init; }
}

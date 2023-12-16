using Interview.Domain.Repository;

namespace Interview.Domain.Rooms;

public class RoomState : Entity
{
    public required Guid RoomId { get; init; }

    public required string Type { get; init; }

    public required string Payload { get; set; }

    public required Room? Room { get; init; }
}

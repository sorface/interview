using Interview.Domain.Repository;

namespace Interview.Domain.Rooms;

public class QueuedRoomEvent : Entity
{
    public required Guid RoomId { get; set; }
}

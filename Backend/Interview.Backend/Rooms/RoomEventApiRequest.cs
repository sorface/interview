using Interview.Domain.Rooms.Records.Request;

namespace Interview.Backend.Rooms;

public class RoomEventApiRequest
{
    public Guid RoomId { get; set; }

    public string? Type { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }

    public IEventRequest ToDomainRequest(Guid userId) => new RoomEventRequest(RoomId, userId, Type ?? string.Empty, AdditionalData);
}

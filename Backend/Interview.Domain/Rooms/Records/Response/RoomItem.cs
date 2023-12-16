using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Rooms.Service.Records.Response;

public class RoomItem
{
    public Guid Id { get; init; }

    public string? Name { get; set; }

    public required List<TagItem> Tags { get; set; }
}

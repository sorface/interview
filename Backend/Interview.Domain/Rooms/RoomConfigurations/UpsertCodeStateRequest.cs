namespace Interview.Domain.Rooms.RoomConfigurations;

public class UpsertCodeStateRequest
{
    public required Guid RoomId { get; init; }

    public required string CodeEditorContent { get; init; }
}

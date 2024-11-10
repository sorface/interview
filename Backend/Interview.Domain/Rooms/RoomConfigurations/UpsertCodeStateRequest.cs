namespace Interview.Domain.Rooms.RoomConfigurations;

public class UpsertCodeStateRequest
{
    public required Guid RoomId { get; init; }

    public required string CodeEditorContent { get; init; }

    public required EVRoomCodeEditorChangeSource ChangeCodeEditorContentSource { get; init; }

    public bool SaveChanges { get; set; } = true;
}

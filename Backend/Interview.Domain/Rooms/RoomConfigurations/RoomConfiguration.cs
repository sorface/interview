using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomConfigurations;

public class RoomConfiguration : Entity
{
    private string? _codeEditorContent;
    private EVRoomCodeEditorChangeSource _codeEditorChangeSource;

    public bool CodeEditorEnabled { get; set; }

    public required string? CodeEditorContent { get => _codeEditorContent; init => _codeEditorContent = value; }

    public Room? Room { get; set; }

    public required EVRoomCodeEditorChangeSource CodeEditorChangeSource { get => _codeEditorChangeSource; init => _codeEditorChangeSource = value; }

    public void ChangeCodeEditor(string? content, EVRoomCodeEditorChangeSource changeSource)
    {
        _codeEditorContent = content;
        _codeEditorChangeSource = changeSource;
    }
}

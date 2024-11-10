using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomConfigurations;

namespace Interview.Domain.Questions.CodeEditors;

public class QuestionCodeEditor : Entity
{
    private string _content = string.Empty;
    private EVRoomCodeEditorChangeSource _source;

    public required string Content { get => _content; init => _content = value; }

    public required EVRoomCodeEditorChangeSource Source { get => _source; init => _source = value; }

    public required string Lang { get; set; }

    public void UpdateContent(string content, EVRoomCodeEditorChangeSource source)
    {
        _content = content;
        _source = source;
    }
}

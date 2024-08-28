using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomConfigurations;

public class RoomConfiguration : Entity
{
    public bool CodeEditorEnabled { get; set; }

    public string? CodeEditorContent { get; set; }

    public Room? Room { get; set; }
}

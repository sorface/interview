namespace Interview.Domain.Connections;

public interface IActiveRoomSource
{
    IReadOnlyCollection<Guid> ActiveRooms { get; }
}

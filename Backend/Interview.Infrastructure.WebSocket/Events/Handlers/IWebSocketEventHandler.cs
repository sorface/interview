using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public interface IWebSocketEventHandler
{
    int Order { get; }

    Task<bool> HandleAsync(SocketEventDetail detail, CancellationToken cancellationToken);
}

public record SocketEventDetail(
    IServiceProvider ScopedServiceProvider,
    System.Net.WebSockets.WebSocket WebSocket,
    WebSocketEvent Event,
    User User,
    Room Room,
    EVRoomParticipantType ParticipantType)
{
    public Guid UserId => User.Id;

    public Guid RoomId => Room.Id;
}

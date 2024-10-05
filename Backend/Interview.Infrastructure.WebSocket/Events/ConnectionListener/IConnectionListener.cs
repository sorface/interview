using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;

namespace Interview.Infrastructure.WebSocket.Events.ConnectionListener;

public interface IConnectionListener
{
    Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken);

    Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken);
}

public record WebSocketConnectDetail(System.Net.WebSockets.WebSocket WebSocket, Room Room, User User, EVRoomParticipantType ParticipantType);

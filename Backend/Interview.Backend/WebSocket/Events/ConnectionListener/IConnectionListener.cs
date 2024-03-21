using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Backend.WebSocket.Events.ConnectionListener;

public interface IConnectionListener
{
    Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken);

    Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken);
}

public record WebSocketConnectDetail(System.Net.WebSockets.WebSocket WebSocket, Room Room, User User, EVRoomParticipantType ParticipantType);

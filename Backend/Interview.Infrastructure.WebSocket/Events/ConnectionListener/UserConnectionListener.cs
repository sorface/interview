using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;

namespace Interview.Infrastructure.WebSocket.Events.ConnectionListener;

public class UserConnectionListener : IConnectionListener, IUserWebSocketConnectionProvider
{
    private readonly ConcurrentDictionary<Guid, ImmutableList<ConnectionInfo>> _store = new();

    public Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        _store.AddOrUpdate(
            detail.Room.Id,
            _ => ImmutableList.Create(new ConnectionInfo(detail)),
            (_, list) => list.Add(new ConnectionInfo(detail)));
        return Task.CompletedTask;
    }

    public Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        _store.AddOrUpdate(
            detail.Room.Id,
            _ => ImmutableList<ConnectionInfo>.Empty,
            (_, list) => list.RemoveAll(e => ReferenceEquals(e.WebSocket, detail.WebSocket)));
        return Task.CompletedTask;
    }

    public bool TryGetConnections(Guid userId, Guid roomId, [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections)
    {
        if (_store.TryGetValue(roomId, out var details))
        {
            connections = details.Where(e => e.UserId == userId).Select(e => e.WebSocket).ToList();
            return true;
        }

        connections = null;
        return false;
    }

    public bool TryGetConnections(
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<(User User, EVRoomParticipantType ParticipantType, System.Net.WebSockets.WebSocket WebSocket, bool ScreenShareEnabled)>? connections)
    {
        if (_store.TryGetValue(roomId, out var details))
        {
            connections = details.Select(e => (e.User, e.ParticipantType, e.WebSocket, e.ScreenShareEnabled)).ToList();
            return true;
        }

        connections = null;
        return false;
    }

    public bool StartScreenShare(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket)
    {
        var anyChanges = false;
        foreach (var sharingConnection in GetSharingConnections(userId, roomId, webSocket))
        {
            if (sharingConnection.ScreenShareEnabled)
            {
                continue;
            }

            sharingConnection.ScreenShareEnabled = true;
            anyChanges = true;
        }

        return anyChanges;
    }

    public bool StopScreenShare(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket)
    {
        var anyChanges = false;
        foreach (var sharingConnection in GetSharingConnections(userId, roomId, webSocket))
        {
            if (!sharingConnection.ScreenShareEnabled)
            {
                continue;
            }

            sharingConnection.ScreenShareEnabled = false;
            anyChanges = true;
        }

        return anyChanges;
    }

    private IEnumerable<ConnectionInfo> GetSharingConnections(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket)
    {
        if (!_store.TryGetValue(roomId, out var details))
        {
            yield break;
        }

        foreach (var detail in details)
        {
            if (detail.UserId != userId)
            {
                continue;
            }

            if (detail.ParticipantType != EVRoomParticipantType.Expert && detail.ParticipantType != EVRoomParticipantType.Examinee)
            {
                continue;
            }

            if (!ReferenceEquals(detail.WebSocket, webSocket))
            {
                continue;
            }

            yield return detail;
        }
    }

    private record ConnectionInfo(User User, Room Room, EVRoomParticipantType ParticipantType, System.Net.WebSockets.WebSocket WebSocket)
    {
        public Guid UserId => User.Id;

        public Guid RoomId => Room.Id;

        public bool ScreenShareEnabled { get; set; }

        public ConnectionInfo(WebSocketConnectDetail detail)
            : this(detail.User, detail.Room, detail.ParticipantType, detail.WebSocket)
        {
        }
    }
}

public interface IUserWebSocketConnectionProvider
{
    bool TryGetConnections(
        Guid userId,
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections);

    bool TryGetConnections(
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<(User User, EVRoomParticipantType ParticipantType, System.Net.WebSockets.WebSocket WebSocket, bool ScreenShareEnabled)>? connections);

    bool StartScreenShare(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket);

    bool StopScreenShare(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket);
}

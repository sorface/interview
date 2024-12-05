using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Interview.Backend.WebSocket.Events.ConnectionListener;

public class UserConnectionListener : IConnectionListener, IUserWebSocketConnectionProvider
{
    private readonly ConcurrentDictionary<(Guid UserId, Guid RoomId), ImmutableList<System.Net.WebSockets.WebSocket>> _store = new();

    public Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        _store.AddOrUpdate(
            (detail.User.Id, detail.Room.Id),
            _ => ImmutableList.Create(detail.WebSocket),
            (_, list) => list.Add(detail.WebSocket));
        return Task.CompletedTask;
    }

    public Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        _store.AddOrUpdate(
            (detail.User.Id, detail.Room.Id),
            _ => ImmutableList<System.Net.WebSockets.WebSocket>.Empty,
            (_, list) => list.Remove(detail.WebSocket));
        return Task.CompletedTask;
    }

    public bool TryGetConnections(Guid userId, Guid roomId, [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections)
    {
        if (_store.TryGetValue((userId, roomId), out var details))
        {
            connections = details;
            return true;
        }

        connections = null;
        return false;
    }
}

public interface IUserWebSocketConnectionProvider
{
    bool TryGetConnections(
        Guid userId,
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections);
}

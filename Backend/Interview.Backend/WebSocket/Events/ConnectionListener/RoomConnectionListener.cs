using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Interview.Backend.Auth;
using Interview.Domain.Connections;
using Interview.Domain.Events;
using Microsoft.Extensions.Options;

namespace Interview.Backend.WebSocket.Events.ConnectionListener;

public class RoomConnectionListener : IActiveRoomSource, IConnectionListener, IWebSocketConnectionSource
{
    private readonly ConcurrentDictionary<Guid, ImmutableList<WebSocketConnectDetail>> _activeRooms = new();

    public IReadOnlyCollection<Guid> ActiveRooms => _activeRooms.Where(e => e.Value.Count > 0).Select(e => e.Key).ToList();

    public async Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        await Task.Yield();
        _ = _activeRooms.AddOrUpdate(
            detail.Room.Id,
            roomId =>
            {
                return ImmutableList.Create(detail);
            },
            (roomId, list) =>
            {
                var newList = list.Add(detail);

                return newList;
            });
    }

    public async Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        await Task.Yield();
        _activeRooms.AddOrUpdate(
            detail.Room.Id,
            s => ImmutableList<WebSocketConnectDetail>.Empty,
            (roomId, list) =>
            {
                var newList = list.Remove(detail);
                return newList;
            });
    }

    public bool TryGetConnections(Guid roomId, [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections)
    {
        if (!_activeRooms.TryGetValue(roomId, out var details) || details.Count == 0)
        {
            connections = default;
            return false;
        }

        connections = details.Select(e => e.WebSocket).ToList();
        return true;
    }

    public bool TryGetConnectionsByPredicate(Guid roomId, Func<WebSocketConnectDetail, bool> predicate, [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections)
    {
        if (!_activeRooms.TryGetValue(roomId, out var details) || details.Count == 0)
        {
            connections = default;
            return false;
        }

        connections = details.Where(predicate).Select(e => e.WebSocket).ToList();
        return true;
    }
}

public interface IWebSocketConnectionSource
{
    bool TryGetConnections(
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections);
}

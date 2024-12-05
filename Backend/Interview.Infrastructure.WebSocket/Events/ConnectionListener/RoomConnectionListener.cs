using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Interview.Domain.Connections;

namespace Interview.Infrastructure.WebSocket.Events.ConnectionListener;

public class RoomConnectionListener : IActiveRoomSource, IConnectionListener
{
    private readonly ConcurrentDictionary<Guid, ImmutableList<WebSocketConnectDetail>> _activeRooms = new();

    public IReadOnlyCollection<Guid> ActiveRooms => _activeRooms.Where(e => e.Value.Count > 0).Select(e => e.Key).ToList();

    public async Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        await Task.Yield();
        _ = _activeRooms.AddOrUpdate(
            detail.Room.Id,
            _ => ImmutableList.Create(detail),
            (_, list) => list.Add(detail));
    }

    public async Task OnDisconnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        await Task.Yield();
        _activeRooms.AddOrUpdate(
            detail.Room.Id,
            _ => ImmutableList<WebSocketConnectDetail>.Empty,
            (_, list) => list.Remove(detail));
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

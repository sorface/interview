using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Interview.Backend.Auth;
using Interview.Domain.Connections;
using Interview.Domain.Events;
using Interview.Infrastructure.Chat;
using Microsoft.Extensions.Options;

namespace Interview.Backend.WebSocket.Events.ConnectionListener;

public class RoomConnectionListener : IActiveRoomSource, IConnectionListener, IWebSocketConnectionSource
{
    private readonly ConcurrentDictionary<Guid, ImmutableList<WebSocketConnectDetail>> _activeRooms = new();

    private readonly ChatBotAccount _chatBotAccount;
    private readonly ILogger<RoomConnectionListener> _logger;
    private readonly ConcurrentDictionary<Guid, TwitchChatClient> _twitchClients = new();

    public RoomConnectionListener(IOptions<ChatBotAccount> chatBotAccount, IRoomEventDispatcher roomEventDispatcher, ILogger<RoomConnectionListener> logger)
    {
        _chatBotAccount = chatBotAccount.Value;
        _logger = logger;
    }

    public IReadOnlyCollection<Guid> ActiveRooms => _activeRooms.Where(e => e.Value.Count > 0).Select(e => e.Key).ToList();

    public async Task OnConnectAsync(WebSocketConnectDetail detail, CancellationToken cancellationToken)
    {
        await Task.Yield();
        var list = _activeRooms.AddOrUpdate(
            detail.Room.Id,
            roomId =>
            {
                ConnectToTwitch(detail, roomId);
                return ImmutableList.Create(detail);
            },
            (roomId, list) =>
            {
                var newList = list.Add(detail);
                if (newList.Count == 1)
                {
                    ConnectToTwitch(detail, roomId);
                }

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
                if (newList.Count == 0 && _twitchClients.TryRemove(roomId, out var client))
                {
                    try
                    {
                        client.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

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

    private void ConnectToTwitch(WebSocketConnectDetail detail, Guid roomId)
    {
        try
        {
            var client = new TwitchChatClient(roomId);
            client.Connect(_chatBotAccount.Username, _chatBotAccount.AccessToken);
            _twitchClients.TryAdd(roomId, client);
            _logger.LogInformation("Start listen new room {RoomId}", roomId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable connect to twitch {RoomId}", roomId);
        }
    }
}

public interface IWebSocketConnectionSource
{
    bool TryGetConnections(
        Guid roomId,
        [NotNullWhen(true)] out IReadOnlyCollection<System.Net.WebSockets.WebSocket>? connections);
}

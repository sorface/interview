using System.Buffers;
using System.Net.WebSockets;
using System.Text.Json;
using Interview.Backend.WebSocket.Events.Handlers;
using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.IO;

namespace Interview.Backend.WebSocket.Events;

public class WebSocketReader
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IWebSocketEventHandler[] _handlers;
    private readonly IEventStorage _eventStorage;
    private readonly ILogger<WebSocketReader> _logger;

    public WebSocketReader(
        RecyclableMemoryStreamManager manager,
        IEnumerable<IWebSocketEventHandler> handlers,
        IEventStorage eventStorage,
        ILogger<WebSocketReader> logger)
    {
        _manager = manager;
        _eventStorage = eventStorage;
        _logger = logger;
        _handlers = handlers.OrderBy(e => e.Order).ToArray();
    }

    public async Task ReadAsync(
        User user,
        Room room,
        EVRoomParticipantType participantType,
        IServiceProvider scopedServiceProvider,
        System.Net.WebSockets.WebSocket webSocket,
        CancellationToken ct)
    {
        while (!webSocket.ShouldCloseWebSocket())
        {
            try
            {
                await HandleAsync(user, room, participantType, scopedServiceProvider, webSocket, ct);
            }
            catch (Exception e)
            {
                if (!webSocket.ShouldCloseWebSocket())
                {
                    _logger.LogError(e, "During read events");
                }
            }
        }
    }

    private async Task HandleAsync(
        User user,
        Room room,
        EVRoomParticipantType participantType,
        IServiceProvider scopedServiceProvider,
        System.Net.WebSockets.WebSocket webSocket,
        CancellationToken ct)
    {
        var deserializeResult = await DeserializeResultAsync(webSocket, ct);
        if (webSocket.ShouldCloseWebSocket())
        {
            return;
        }

        if (deserializeResult is null)
        {
            return;
        }

        await SaveEventAsync(room, deserializeResult, ct);
        var socketEventDetail = new SocketEventDetail(
            scopedServiceProvider,
            webSocket,
            deserializeResult,
            user,
            room,
            participantType);
        var handleAny = await HandleAsync(socketEventDetail, ct);
        if (!handleAny)
        {
            _logger.LogWarning(
                "Not found handler for {Type} {Value}",
                socketEventDetail.Event.Type,
                socketEventDetail.Event.Value);
        }
    }

    private async Task<bool> HandleAsync(SocketEventDetail socketEventDetail, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
        {
            if (await handler.HandleAsync(socketEventDetail, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<WebSocketEvent?> DeserializeResultAsync(System.Net.WebSockets.WebSocket webSocket, CancellationToken ct)
    {
        WebSocketEvent? deserializeResult = null;
        using var buffer = new PoolItem(8192);
        using var ms = _manager.GetStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer.Buffer, ct);
            await ms.WriteAsync(buffer.Buffer.AsMemory(0, result.Count), ct);
        }
        while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);

        try
        {
            deserializeResult = await JsonSerializer.DeserializeAsync<WebSocketEvent>(ms, JsonSerializerOptions.Default, ct);
        }
        catch (Exception e)
        {
            if (!webSocket.ShouldCloseWebSocket())
            {
                _logger.LogError(e, "During read event");
            }
        }

        return deserializeResult;
    }

    private ValueTask SaveEventAsync(Room room, WebSocketEvent deserializeResult, CancellationToken ct)
    {
        var storageEvent = new StorageEvent
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Payload = deserializeResult.Value,
            RoomId = room.Id,
            Stateful = false,
            Type = deserializeResult.Type,
        };
        return _eventStorage.AddAsync(storageEvent, ct);
    }

    private class PoolItem : IDisposable
    {
        public byte[] Buffer { get; }

        public PoolItem(int size)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(size);
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Buffer);
        }
    }
}

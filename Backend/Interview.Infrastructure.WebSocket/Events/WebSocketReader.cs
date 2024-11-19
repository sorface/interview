using System.Buffers;
using System.Net.WebSockets;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Events.Storage;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.Service;
using Interview.Domain.Users;
using Interview.Infrastructure.WebSocket.Events.Handlers;
using Interview.Infrastructure.WebSocket.PubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Interview.Infrastructure.WebSocket.Events;

public class WebSocketReader
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IWebSocketEventHandler[] _handlers;
    private readonly IHotEventStorage _hotEventStorage;
    private readonly ILogger<WebSocketReader> _logger;
    private readonly IEventBusSubscriberFactory _eventBusSubscriberFactory;
    private readonly IEventBusPublisherFactory _publisherFactory;
    private readonly IRoomEventDispatcher _dispatcher;
    private readonly IRoomEventSerializer _serializer;
    private readonly IRoomEventSerializer _roomEventSerializer;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;

    public WebSocketReader(
        RecyclableMemoryStreamManager manager,
        IEnumerable<IWebSocketEventHandler> handlers,
        IHotEventStorage hotEventStorage,
        ILogger<WebSocketReader> logger,
        IEventBusSubscriberFactory eventBusSubscriberFactory,
        IEventBusPublisherFactory publisherFactory,
        IRoomEventDispatcher dispatcher,
        IRoomEventSerializer serializer,
        IRoomEventSerializer roomEventSerializer,
        ILogger<WebSocketEventSender> webSocketEventSender,
        IEventSenderAdapter eventSenderAdapter)
    {
        _manager = manager;
        _hotEventStorage = hotEventStorage;
        _logger = logger;
        _eventBusSubscriberFactory = eventBusSubscriberFactory;
        _publisherFactory = publisherFactory;
        _dispatcher = dispatcher;
        _serializer = serializer;
        _roomEventSerializer = roomEventSerializer;
        _webSocketEventSender = webSocketEventSender;
        _eventSenderAdapter = eventSenderAdapter;
        _handlers = handlers.OrderBy(e => e.Order).ToArray();
    }

    public async Task ReadAsync(
        User user,
        Room room,
        EVRoomParticipantType participantType,
        IServiceScopeFactory serviceScopeFactory,
        System.Net.WebSockets.WebSocket webSocket,
        CancellationToken ct)
    {
        var subscriber = await _eventBusSubscriberFactory.CreateAsync(ct);
        var publisher = await _publisherFactory.CreateAsync(ct);
        var eventBusRoomEventKey = new EventBusRoomEventKey(room.Id);
        await using var roomEventSubscriber = await subscriber.SubscribeAsync(eventBusRoomEventKey, (key, @event) =>
        {
            if (@event is null)
            {
                return;
            }

            var task = @event.Match<Task>(
                async busEvent =>
                {
                    var webSocketEvent = JsonSerializer.Deserialize<WebSocketEvent>(busEvent.Event);
                    if (webSocketEvent is null)
                    {
                        return;
                    }

                    await ProcessWebSocketEventAsync(user, room, participantType, serviceScopeFactory, webSocket, webSocketEvent, ct);
                },
                async busEvent =>
                {
                    var @event = JsonSerializer.Deserialize<IRoomEvent>(busEvent.Event);
                    var statefulEvents = new List<IRoomEvent>();
                    await ProcessEventAsync(@event, statefulEvents, webSocket, ct);
                    await UpdateRoomStateAsync(serviceScopeFactory, statefulEvents, ct);

                    // return _dispatcher.WriteDirectlyAsync(@event, ct);
                });
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }, ct);

        while (!webSocket.ShouldCloseWebSocket())
        {
            try
            {
                var deserializeResult = await DeserializeResultAsync(webSocket, ct);
                if (deserializeResult is null)
                {
                    continue;
                }

                await ProcessWebSocketEventAsync(user, room, participantType, serviceScopeFactory, webSocket, deserializeResult, ct);
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

    private async Task ProcessWebSocketEventAsync(User user,
                                                  Room room,
                                                  EVRoomParticipantType participantType,
                                                  IServiceScopeFactory serviceScopeFactory,
                                                  System.Net.WebSockets.WebSocket webSocket,
                                                  WebSocketEvent webSocketEvent,
                                                  CancellationToken ct)
    {
        using var scope = serviceScopeFactory.CreateScope();
        await SaveEventAsync(room, user, webSocketEvent, ct);
        var socketEventDetail = new SocketEventDetail(
            scope.ServiceProvider,
            webSocket,
            webSocketEvent,
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
        await using var ms = _manager.GetStream();
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

    private ValueTask SaveEventAsync(Room room, User user, WebSocketEvent deserializeResult, CancellationToken ct)
    {
        var storageEvent = new StorageEvent
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Payload = deserializeResult.Value,
            RoomId = room.Id,
            Stateful = false,
            Type = deserializeResult.Type,
            CreatedById = user.Id,
        };
        return _hotEventStorage.AddAsync(storageEvent, ct);
    }

    private async Task ProcessEventAsync(
        IRoomEvent currentEvent,
        List<IRoomEvent> statefulEvents,
        System.Net.WebSockets.WebSocket socket,
        CancellationToken cancellationToken)
    {
        if (currentEvent.Stateful)
        {
            statefulEvents.Add(currentEvent);
        }

        var provider = new CachedRoomEventProvider(currentEvent, _roomEventSerializer);
        _logger.LogDebug("Start sending {@Event}", currentEvent);
        var sender = new WebSocketEventSender(_webSocketEventSender, socket);
        await _eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
    }

    private async Task UpdateRoomStateAsync(IServiceScopeFactory serviceScopeFactory, List<IRoomEvent> statefulEvents, CancellationToken cancellationToken)
    {
        if (statefulEvents.Count <= 0)
        {
            return;
        }

        try
        {
            await using var dbScope = serviceScopeFactory.CreateAsyncScope();
            var service = dbScope.ServiceProvider.GetRequiredService<IRoomServiceWithoutPermissionCheck>();
            foreach (var roomEvent in statefulEvents)
            {
                try
                {
                    var payload = roomEvent.BuildStringPayload(_serializer);
                    await service.UpsertRoomStateAsync(
                        roomEvent.RoomId,
                        roomEvent.Type,
                        payload ?? string.Empty,
                        cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "During update {Type} room state", roomEvent.Type);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fails to update room states");
        }
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

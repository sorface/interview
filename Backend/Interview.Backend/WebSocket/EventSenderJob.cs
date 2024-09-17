using Interview.Backend.WebSocket.Events;
using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.Service;

namespace Interview.Backend.WebSocket;

public class EventSenderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRoomEventDispatcher _roomEventDispatcher;
    private readonly IWebSocketConnectionSource _webSocketConnectionSource;
    private readonly ILogger<EventSenderJob> _logger;
    private readonly IRoomEventSerializer _roomEventSerializer;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;
    private readonly IRoomEventSerializer _serializer;

    public EventSenderJob(
        IRoomEventDispatcher roomEventDispatcher,
        IWebSocketConnectionSource webSocketConnectionSource,
        ILogger<EventSenderJob> logger,
        IRoomEventSerializer roomEventSerializer,
        IServiceScopeFactory scopeFactory,
        ILogger<WebSocketEventSender> webSocketEventSender,
        IEventSenderAdapter eventSenderAdapter,
        IRoomEventSerializer serializer)
    {
        _roomEventDispatcher = roomEventDispatcher;
        _webSocketConnectionSource = webSocketConnectionSource;
        _logger = logger;
        _roomEventSerializer = roomEventSerializer;
        _scopeFactory = scopeFactory;
        _webSocketEventSender = webSocketEventSender;
        _eventSenderAdapter = eventSenderAdapter;
        _serializer = serializer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Start sending");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await SendEventsAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stop sending");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "During sending events");
            }
        }

        _logger.LogDebug("Stop sending");
    }

    private async Task SendEventsAsync(CancellationToken cancellationToken)
    {
        ILookup<Guid, IRoomEvent> lookup;

        try
        {
            lookup = _roomEventDispatcher.Read().ToLookup(e => e.RoomId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "During read events");
            return;
        }

        try
        {
            // not found actual events
            if (lookup.Count == 0)
            {
                _logger.LogDebug("Before wait async");
                await _roomEventDispatcher.WaitAsync(cancellationToken);
                _logger.LogDebug("After wait async");
                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "During wait");
        }

        try
        {
            var statefulEvents = new List<IRoomEvent>();
            foreach (var group in lookup)
            {
                if (!_webSocketConnectionSource.TryGetConnections(group.Key, out var subscribers) ||
                    subscribers.Count == 0)
                {
                    continue;
                }

                foreach (var currentEvent in group)
                {
                    await ProcessEventAsync(currentEvent, statefulEvents, subscribers, cancellationToken);
                }
            }

            await UpdateRoomStateAsync(statefulEvents, cancellationToken);
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _logger.LogError(e, "Read events");
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }
    }

    private async Task ProcessEventAsync(
        IRoomEvent currentEvent,
        List<IRoomEvent> statefulEvents,
        IReadOnlyCollection<System.Net.WebSockets.WebSocket> subscribers,
        CancellationToken cancellationToken)
    {
        if (currentEvent.Stateful)
        {
            statefulEvents.Add(currentEvent);
        }

        var provider = new CachedRoomEventProvider(currentEvent, _roomEventSerializer);
        _logger.LogDebug("Start sending {@Event}", currentEvent);
        await Parallel.ForEachAsync(subscribers, cancellationToken, async (entry, token) =>
        {
            var sender = new WebSocketEventSender(_webSocketEventSender, entry);
            await _eventSenderAdapter.SendAsync(provider, sender, token);
        });
    }

    private async Task UpdateRoomStateAsync(List<IRoomEvent> statefulEvents, CancellationToken cancellationToken)
    {
        if (statefulEvents.Count > 0)
        {
            try
            {
                await using var dbScope = _scopeFactory.CreateAsyncScope();
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
    }
}

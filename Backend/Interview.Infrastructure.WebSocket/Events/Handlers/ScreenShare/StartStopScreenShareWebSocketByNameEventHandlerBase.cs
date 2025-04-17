using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.Events.Handlers.VideoChat;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.ScreenShare;

public abstract class StartStopScreenShareWebSocketByNameEventHandlerBase(
    ILogger<StartStopScreenShareWebSocketByNameEventHandlerBase> logger,
    IUserWebSocketConnectionProvider userWebSocketConnectionProvider,
    ILogger<StartStopScreenShareWebSocketByNameEventHandlerBase> startStopScreenShareWebSocketByNameEventHandlerBaseLogger,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase(logger, deserializer)
{
    protected IUserWebSocketConnectionProvider UserWebSocketConnectionProvider => userWebSocketConnectionProvider;

    protected abstract string SendUserEventType { get; }

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        try
        {
            if (!PerformAction(detail.UserId, detail.RoomId, detail.WebSocket))
            {
                return;
            }

            await SendEventsAsync(detail, cancellationToken);
        }
        catch (Exception e)
        {
            startStopScreenShareWebSocketByNameEventHandlerBaseLogger.LogError(e, "During {SendUserEventType}", SendUserEventType);
        }
    }

    protected abstract bool PerformAction(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket);

    private async Task SendEventsAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        if (!userWebSocketConnectionProvider.TryGetConnections(detail.RoomId, out var subscribers))
        {
            return;
        }

        var payloadForSerialization = new
        {
            UserId = detail.User.Id,
            ParticipantType = detail.ParticipantType,
        };

        var payloadStr = serializer.SerializePayloadAsString(payloadForSerialization);
        var sendEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = SendUserEventType,
            Value = payloadStr,
            CreatedById = detail.UserId,
        };
        var provider = new CachedRoomEventProvider(sendEvent, serializer);
        foreach (var subscriber in subscribers)
        {
            var sender = new WebSocketEventSender(webSocketEventSender, subscriber.WebSocket);
            await eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
        }
    }
}

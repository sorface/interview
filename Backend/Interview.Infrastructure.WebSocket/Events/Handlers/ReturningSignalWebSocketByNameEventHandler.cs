using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class ReturningSignalWebSocketByNameEventHandler(
    ILogger<WebSocketByNameEventHandlerBase<ReturningSignalWebSocketByNameEventHandler.ReceivePayload>> logger,
    IVideChatConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase<ReturningSignalWebSocketByNameEventHandler.ReceivePayload>(logger, deserializer)
{
    protected override string SupportType => "returning signal";

    protected override async Task HandleEventAsync(SocketEventDetail detail, ReceivePayload? payload, CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            return;
        }

        if (!userWebSocketConnectionProvider.TryGetConnections(payload.To, detail.RoomId, out var connections))
        {
            Logger.LogWarning("Not found {To} user connections. {RoomId} current {UserId}", payload.To, detail.RoomId, detail.UserId);
            return;
        }

        var receivingReturnedSignalPayload = new { Signal = payload.Signal, From = detail.UserId, ScreenShare = payload.ScreenShare };
        var strPayload = serializer.SerializePayloadAsString(receivingReturnedSignalPayload);
        var sendEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = "receiving returned signal",
            Value = strPayload,
            CreatedById = detail.UserId,
        };
        var provider = new CachedRoomEventProvider(sendEvent, serializer);
        foreach (var webSocket in connections)
        {
            var sender = new WebSocketEventSender(webSocketEventSender, webSocket);
            await eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
        }
    }

    public sealed class ReceivePayload
    {
        public Guid To { get; set; }

        public string? Signal { get; set; }

        public bool? ScreenShare { get; set; }
    }
}

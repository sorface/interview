using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.ScreenShare;

public class StopScreenShareWebSocketByNameEventHandler(
    ILogger<StopScreenShareWebSocketByNameEventHandler> logger,
    IUserWebSocketConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : StartStopScreenShareWebSocketByNameEventHandlerBase(logger, userWebSocketConnectionProvider, webSocketEventSender, eventSenderAdapter, serializer, deserializer)
{
    protected override string SupportType => "screen share stop";

    protected override bool PerformAction(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket)
        => UserWebSocketConnectionProvider.StopScreenShare(userId, roomId, webSocket);
}

using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.ScreenShare;

public class StartScreenShareWebSocketByNameEventHandler(
    ILogger<StartScreenShareWebSocketByNameEventHandler> logger,
    IUserWebSocketConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    ILogger<StartScreenShareWebSocketByNameEventHandler> startStopScreenShareWebSocketByNameEventHandlerBaseLogger,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : StartStopScreenShareWebSocketByNameEventHandlerBase(logger, userWebSocketConnectionProvider, startStopScreenShareWebSocketByNameEventHandlerBaseLogger, webSocketEventSender, eventSenderAdapter, serializer, deserializer)
{
    protected override string SupportType => "start screen share";

    protected override string SendUserEventType => "screen share started";

    protected override bool PerformAction(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket)
        => UserWebSocketConnectionProvider.StartScreenShare(userId, roomId, webSocket);
}

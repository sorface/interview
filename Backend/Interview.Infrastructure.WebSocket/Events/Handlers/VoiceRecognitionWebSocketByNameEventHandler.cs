using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class VoiceRecognitionWebSocketByNameEventHandler(
    IRoomEventDispatcher eventDispatcher,
    ILogger<WebSocketByNameEventHandlerBase> logger,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase(logger, deserializer)
{
    protected override string SupportType => "voice-recognition";

    protected override Task HandleEventAsync(SocketEventDetail detail, string? message, CancellationToken cancellationToken)
    {
        var payload = new UserMessageEventPayload
        {
            Message = message ?? string.Empty,
            Nickname = detail.User.Nickname,
        };
        var @event = new RoomMessageEvent
        {
            RoomId = detail.RoomId,
            Type = EventType.VoiceRecognition,
            Value = payload,
            CreatedById = detail.UserId,
        };
        return eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}

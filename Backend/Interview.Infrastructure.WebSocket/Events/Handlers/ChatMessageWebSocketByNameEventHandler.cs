using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class ChatMessageWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public ChatMessageWebSocketByNameEventHandler(
        IRoomEventDispatcher eventDispatcher,
        ILogger<WebSocketByNameEventHandlerBase> logger)
        : base(logger)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override string SupportType => "chat-message";

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
            Type = EventType.ChatMessage,
            Value = payload,
            CreatedById = detail.UserId,
        };
        return _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}


using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Infrastructure.Chat;

namespace Interview.Backend.WebSocket.Events.Handlers;

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

    protected override Task HandleEventAsync(SocketEventDetail detail, string message, CancellationToken cancellationToken)
    {
        var payload = new UserMessageEventPayload(message, detail.User.Nickname);
        var @event = new RoomEvent<UserMessageEventPayload>(detail.RoomId, EventType.ChatMessage, payload, false);
        return _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}

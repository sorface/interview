using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Infrastructure.Chat;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class VoiceRecognitionWebSocketEventHandler : WebSocketEventHandlerBase
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public VoiceRecognitionWebSocketEventHandler(
        IRoomEventDispatcher eventDispatcher,
        ILogger<WebSocketEventHandlerBase> logger)
        : base(logger)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override string SupportType => "voice-recognition";

    protected override Task HandleEventAsync(SocketEventDetail detail, string message, CancellationToken cancellationToken)
    {
        var payload = new UserMessageEventPayload(message, detail.User.Nickname);
        var @event = new RoomEvent<UserMessageEventPayload>(detail.RoomId, EventType.VoiceRecognition, payload, false);
        return _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}

public sealed class UserMessageEventPayload
{
    public string Message { get; }

    public string Nickname { get; }

    public UserMessageEventPayload(string message, string nickname)
    {
        Message = message;
        Nickname = nickname;
    }
}

using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Infrastructure.Chat;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class VoiceRecognitionWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public VoiceRecognitionWebSocketByNameEventHandler(
        IRoomEventDispatcher eventDispatcher,
        ILogger<WebSocketByNameEventHandlerBase> logger)
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

#pragma warning disable SA1402
public sealed class UserMessageEventPayload
#pragma warning restore SA1402
{
    public string Message { get; }

    public string Nickname { get; }

    public UserMessageEventPayload(string message, string nickname)
    {
        Message = message;
        Nickname = nickname;
    }
}

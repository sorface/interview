using Interview.Domain.Events;
using Interview.Domain.Events.Events;

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

    protected override Task HandleEventAsync(SocketEventDetail detail, string? message, CancellationToken cancellationToken)
    {
        var payload = new UserMessageEventPayload
        {
            Message = message ?? string.Empty,
            Nickname = detail.User.Nickname,
        };
        var @event = new RoomEvent<UserMessageEventPayload>(detail.RoomId, EventType.VoiceRecognition, payload, false, detail.UserId);
        return _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}

#pragma warning disable SA1402
public sealed class UserMessageEventPayload
#pragma warning restore SA1402
{
    public required string Message { get; init; }

    public required string Nickname { get; init; }
}

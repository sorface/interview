using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class ExpertReviewTypingWebSocketEventHandler : WebSocketByNameEventHandlerBase
{
    private readonly ILogger<ExpertReviewTypingWebSocketEventHandler> _logger;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;
    private readonly RoomConnectionListener _roomConnectionListener;
    private readonly IRoomEventSerializer _serializer;

    public ExpertReviewTypingWebSocketEventHandler(ILogger<ExpertReviewTypingWebSocketEventHandler> logger,
                                                   IEventSenderAdapter eventSenderAdapter,
                                                   RoomConnectionListener roomConnectionListener,
                                                   ILogger<WebSocketEventSender> webSocketEventSender,
                                                   IRoomEventSerializer serializer) : base(logger)
    {
        _logger = logger;
        _eventSenderAdapter = eventSenderAdapter;
        _roomConnectionListener = roomConnectionListener;
        _webSocketEventSender = webSocketEventSender;
        _serializer = serializer;
    }

    protected override string SupportType => "expert-review-typing";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        if (detail.ParticipantType != EVRoomParticipantType.Expert)
        {
            return;
        }

        if (!_roomConnectionListener.TryGetConnectionsByPredicate(
                detail.RoomId,
                e => e.ParticipantType == EVRoomParticipantType.Expert && e.User.Id != detail.UserId,
                out var connections))
        {
            return;
        }

        var payloadForSerialization = new UserDetail
        {
            Id = detail.User.Id,
            Nickname = detail.User.Nickname,
            Avatar = detail.User.Avatar,
            ParticipantType = detail.ParticipantType,
        };

        var payloadStr = _serializer.SerializePayloadAsString(payloadForSerialization);
        var sendEvent = new RoomEvent(detail.RoomId, "review-typing", payloadStr, false, detail.UserId);
        var provider = new CachedRoomEventProvider(sendEvent, _serializer);
        foreach (var webSocket in connections)
        {
            var sender = new WebSocketEventSender(_webSocketEventSender, webSocket);
            await _eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
        }
    }
}

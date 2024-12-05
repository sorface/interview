using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class SendingSignalWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase<SendingSignalWebSocketByNameEventHandler.ReceivePayload>
{
    private readonly IVideChatConnectionProvider _userWebSocketConnectionProvider;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;
    private readonly IRoomEventSerializer _serializer;

    public SendingSignalWebSocketByNameEventHandler(
        ILogger<WebSocketByNameEventHandlerBase<ReceivePayload>> logger,
        IVideChatConnectionProvider userWebSocketConnectionProvider,
        ILogger<WebSocketEventSender> webSocketEventSender,
        IEventSenderAdapter eventSenderAdapter,
        IRoomEventSerializer serializer)
        : base(logger)
    {
        _userWebSocketConnectionProvider = userWebSocketConnectionProvider;
        _webSocketEventSender = webSocketEventSender;
        _eventSenderAdapter = eventSenderAdapter;
        _serializer = serializer;
    }

    protected override string SupportType => "sending signal";

    protected override async Task HandleEventAsync(SocketEventDetail detail, ReceivePayload? payload, CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            return;
        }

        if (!_userWebSocketConnectionProvider.TryGetConnections(payload.To, detail.RoomId, out var connections))
        {
            Logger.LogWarning("Not found {To} user connections. {RoomId} {From}", payload.To, detail.RoomId, detail.UserId);
            return;
        }

        var payloadForSerialization = new UserDetailResponse
        {
            From = new UserDetail
            {
                Id = detail.User.Id,
                Nickname = detail.User.Nickname,
                Avatar = detail.User.Avatar,
                ParticipantType = detail.ParticipantType,
            },
            Signal = payload.Signal,
            ScreenShare = payload.ScreenShare,
        };
        var payloadStr = _serializer.SerializePayloadAsString(payloadForSerialization);
        var sendEvent = new RoomEvent(detail.RoomId, "user joined", payloadStr, false, detail.UserId);
        var provider = new CachedRoomEventProvider(sendEvent, _serializer);
        foreach (var webSocket in connections)
        {
            var sender = new WebSocketEventSender(_webSocketEventSender, webSocket);
            await _eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
        }
    }

    public sealed class ReceivePayload
    {
        public Guid To { get; set; }

        public string? Signal { get; set; }

        public bool? ScreenShare { get; set; }
    }
}

#pragma warning disable SA1402
public class UserDetailResponse
#pragma warning restore SA1402
{
    public required UserDetail From { get; init; }

    public required string? Signal { get; init; }

    public required bool? ScreenShare { get; init; }
}

#pragma warning disable SA1402
public class UserDetail
#pragma warning restore SA1402
{
    public required Guid Id { get; init; }

    public required string Nickname { get; init; }

    public required string? Avatar { get; init; }

    public required EVRoomParticipantType ParticipantType { get; init; }
}

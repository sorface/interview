using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class ReturningSignalWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase<ReturningSignalWebSocketByNameEventHandler.ReceivePayload>
{
    private readonly IVideChatConnectionProvider _userWebSocketConnectionProvider;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;
    private readonly IRoomEventSerializer _serializer;

    public ReturningSignalWebSocketByNameEventHandler(
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

    protected override string SupportType => "returning signal";

    protected override async Task HandleEventAsync(SocketEventDetail detail, ReceivePayload? payload, CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            return;
        }

        if (!_userWebSocketConnectionProvider.TryGetConnections(payload.To, detail.RoomId, out var connections))
        {
            Logger.LogWarning("Not found {To} user connections. {RoomId} current {UserId}", payload.To, detail.RoomId, detail.UserId);
            return;
        }

        var receivingReturnedSignalPayload = new { Signal = payload.Signal, From = detail.UserId, ScreenShare = payload.ScreenShare };
        var strPayload = _serializer.SerializePayloadAsString(receivingReturnedSignalPayload);
        var sendEvent = new RoomEvent(detail.RoomId, "receiving returned signal", strPayload, false, detail.UserId);
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

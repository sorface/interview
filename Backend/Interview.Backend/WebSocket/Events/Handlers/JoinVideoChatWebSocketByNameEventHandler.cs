using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Interview.Backend.WebSocket.Events.ConnectionListener;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class JoinVideoChatWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase
{
    private readonly IVideChatConnectionProvider _videChatConnectionProvider;
    private readonly ILogger<WebSocketEventSender> _webSocketEventSender;
    private readonly IEventSenderAdapter _eventSenderAdapter;
    private readonly IRoomEventSerializer _serializer;

    public JoinVideoChatWebSocketByNameEventHandler(
        ILogger<JoinVideoChatWebSocketByNameEventHandler> logger,
        IVideChatConnectionProvider videChatConnectionProvider,
        ILogger<WebSocketEventSender> webSocketEventSender,
        IEventSenderAdapter eventSenderAdapter,
        IRoomEventSerializer serializer)
        : base(logger)
    {
        _videChatConnectionProvider = videChatConnectionProvider;
        _webSocketEventSender = webSocketEventSender;
        _eventSenderAdapter = eventSenderAdapter;
        _serializer = serializer;
    }

    protected override string SupportType => "join video chat";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        var successConnectResult = await _videChatConnectionProvider.TryConnectAsync(detail, cancellationToken);
        if (!successConnectResult)
        {
            return;
        }

        try
        {
            await SendEventsAsync(detail, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "During send join video chat event");
        }
    }

    private async Task SendEventsAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        if (!_videChatConnectionProvider.TryGetConnections(detail.RoomId, out var subscribers))
        {
            return;
        }

        var users = subscribers
            .DistinctBy(e => e.User.Id)
            .Select(e => new UserDetail
            {
                Id = e.User.Id,
                Nickname = e.User.Nickname,
                Avatar = e.User.Avatar,
                ParticipantType = e.ParticipantType,
            }).ToList();
        var strPayload = _serializer.SerializePayloadAsString(users);
        var newEvent = new RoomEvent(detail.RoomId, "all users", strPayload, false);
        var provider = new CachedRoomEventProvider(newEvent, _serializer);
        var sender = new WebSocketEventSender(_webSocketEventSender, detail.WebSocket);
        await _eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
    }
}

using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.VideoChat;

public class JoinVideoChatWebSocketByNameEventHandler(
    ILogger<JoinVideoChatWebSocketByNameEventHandler> logger,
    IVideChatConnectionProvider videChatConnectionProvider,
    IUserWebSocketConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase(logger, deserializer)
{
    protected override string SupportType => "join video chat";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        var successConnectResult = await videChatConnectionProvider.TryConnectAsync(detail, cancellationToken);
        if (!successConnectResult)
        {
            logger.LogDebug("Unable to connect to video chat");
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
        if (!videChatConnectionProvider.TryGetConnections(detail.RoomId, out var subscribers))
        {
            logger.LogError("Unable to get video chat connections {RoomId}", detail.RoomId);
            return;
        }

        userWebSocketConnectionProvider.TryGetConnections(detail.RoomId, out var connections);

        var users = subscribers
            .DistinctBy(e => e.User.Id)
            .Select(e => new AllUserDetail
            {
                Id = e.User.Id,
                Nickname = e.User.Nickname,
                Avatar = e.User.Avatar,
                ParticipantType = e.ParticipantType,
                ScreenShareEnabled = connections != null && connections.Any(cc => ReferenceEquals(cc.WebSocket, detail.WebSocket)),
            }).ToList();
        var strPayload = serializer.SerializePayloadAsString(users);
        var newEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = EventType.AllUsers,
            Value = strPayload,
            CreatedById = detail.UserId,
        };
        var provider = new CachedRoomEventProvider(newEvent, serializer);
        var sender = new WebSocketEventSender(webSocketEventSender, detail.WebSocket);
        await eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
    }
}

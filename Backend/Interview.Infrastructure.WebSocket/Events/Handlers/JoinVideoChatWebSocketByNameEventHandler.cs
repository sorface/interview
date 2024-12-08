using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class JoinVideoChatWebSocketByNameEventHandler(
    ILogger<JoinVideoChatWebSocketByNameEventHandler> logger,
    IVideChatConnectionProvider videChatConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IRoomEventSerializer serializer)
    : WebSocketByNameEventHandlerBase(logger)
{
    protected override string SupportType => "join video chat";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        var successConnectResult = await videChatConnectionProvider.TryConnectAsync(detail, cancellationToken);
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
        if (!videChatConnectionProvider.TryGetConnections(detail.RoomId, out var subscribers))
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
        var strPayload = serializer.SerializePayloadAsString(users);
        var newEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = "all users",
            Value = strPayload,
            CreatedById = detail.UserId,
        };
        var provider = new CachedRoomEventProvider(newEvent, serializer);
        var sender = new WebSocketEventSender(webSocketEventSender, detail.WebSocket);
        await eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
    }
}

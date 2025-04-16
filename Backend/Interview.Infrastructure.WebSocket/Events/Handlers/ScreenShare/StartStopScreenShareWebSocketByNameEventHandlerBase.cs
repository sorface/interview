using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.Events.Handlers.VideoChat;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.ScreenShare;

public abstract class StartStopScreenShareWebSocketByNameEventHandlerBase(
    ILogger<StartStopScreenShareWebSocketByNameEventHandlerBase> logger,
    IUserWebSocketConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase(logger, deserializer)
{
    protected IUserWebSocketConnectionProvider UserWebSocketConnectionProvider => userWebSocketConnectionProvider;

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        if (!PerformAction(detail.UserId, detail.RoomId, detail.WebSocket))
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

    protected abstract bool PerformAction(Guid userId, Guid roomId, System.Net.WebSockets.WebSocket webSocket);

    private async Task SendEventsAsync(SocketEventDetail detail, CancellationToken cancellationToken)
    {
        if (!userWebSocketConnectionProvider.TryGetConnections(detail.RoomId, out var subscribers))
        {
            return;
        }

        var users = subscribers
            .DistinctBy(e => e.User.Id)
            .Select(e => new AllUserDetail
            {
                Id = e.User.Id,
                Nickname = e.User.Nickname,
                Avatar = e.User.Avatar,
                ParticipantType = e.ParticipantType,
                ScreenShareEnabled = e.ScreenShareEnabled,
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

/*
Make event 'start screen share' similar to 'join video chat'.
Make event 'screen share started' similar to 'user joined'.
The event adds a screen share to the list of room participants. A separate indicator is needed that this is a screen share.
Add 'screen share stop'
 */

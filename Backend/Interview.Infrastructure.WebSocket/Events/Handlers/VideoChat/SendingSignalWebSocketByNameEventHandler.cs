using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers.VideoChat;

public class SendingSignalWebSocketByNameEventHandler(
    ILogger<WebSocketByNameEventHandlerBase<SendingSignalWebSocketByNameEventHandler.ReceivePayload>> logger,
    IVideChatConnectionProvider userWebSocketConnectionProvider,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSenderAdapter eventSenderAdapter,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase<SendingSignalWebSocketByNameEventHandler.ReceivePayload>(logger, deserializer)
{
    protected override string SupportType => "sending signal";

    protected override async Task HandleEventAsync(SocketEventDetail detail, ReceivePayload? payload, CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            logger.LogDebug("Payload should not be null");
            return;
        }

        if (!userWebSocketConnectionProvider.TryGetConnections(payload.To, detail.RoomId, out var connections))
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
        var payloadStr = serializer.SerializePayloadAsString(payloadForSerialization);
        var sendEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = "user joined",
            Value = payloadStr,
            CreatedById = detail.UserId,
        };
        var provider = new CachedRoomEventProvider(sendEvent, serializer);
        foreach (var webSocket in connections)
        {
            var sender = new WebSocketEventSender(webSocketEventSender, webSocket);
            await eventSenderAdapter.SendAsync(provider, sender, cancellationToken);
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

#pragma warning disable SA1402
public class AllUserDetail : UserDetail
#pragma warning restore SA1402
{
    public required bool ScreenShareEnabled { get; init; }
}

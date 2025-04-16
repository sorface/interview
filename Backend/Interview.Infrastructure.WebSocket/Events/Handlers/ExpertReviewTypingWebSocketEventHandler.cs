using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Sender;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Infrastructure.WebSocket.Events.ConnectionListener;
using Interview.Infrastructure.WebSocket.Events.Handlers.VideoChat;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class ExpertReviewTypingWebSocketEventHandler(
    ILogger<ExpertReviewTypingWebSocketEventHandler> logger,
    IEventSenderAdapter eventSenderAdapter,
    RoomConnectionListener roomConnectionListener,
    ILogger<WebSocketEventSender> webSocketEventSender,
    IEventSerializer serializer,
    IEventDeserializer deserializer)
    : WebSocketByNameEventHandlerBase(logger, deserializer)
{
    protected override string SupportType => "expert-review-typing";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string? payload, CancellationToken cancellationToken)
    {
        if (detail.ParticipantType != EVRoomParticipantType.Expert)
        {
            return;
        }

        if (!roomConnectionListener.TryGetConnectionsByPredicate(
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

        var payloadStr = serializer.SerializePayloadAsString(payloadForSerialization);
        var sendEvent = new RoomEvent
        {
            RoomId = detail.RoomId,
            Type = "review-typing",
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
}

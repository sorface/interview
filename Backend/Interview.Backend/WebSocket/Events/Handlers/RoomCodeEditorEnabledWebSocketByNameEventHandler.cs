using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class RoomCodeEditorEnabledWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase<RoomCodeEditorEnabledEvent.Payload>
{
    public RoomCodeEditorEnabledWebSocketByNameEventHandler(ILogger<RoomCodeEditorEnabledWebSocketByNameEventHandler> logger)
        : base(logger)
    {
    }

    protected override string SupportType => EventType.RoomCodeEditorEnabled;

    protected override Task HandleEventAsync(SocketEventDetail detail, RoomCodeEditorEnabledEvent.Payload? message, CancellationToken cancellationToken)
    {
        if (detail.ParticipantType is not (EVRoomParticipantType.Expert or EVRoomParticipantType.Examinee))
        {
            return Task.CompletedTask;
        }

        var handler = detail.ScopedServiceProvider.GetRequiredService<RoomCodeEditorChangeEventHandler>();
        return handler.HandleAsync(detail.RoomId, message?.Enabled ?? false, cancellationToken);
    }
}

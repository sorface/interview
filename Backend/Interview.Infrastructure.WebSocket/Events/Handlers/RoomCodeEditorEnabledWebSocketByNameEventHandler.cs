using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.WebSocket.Events.Handlers;

public class RoomCodeEditorEnabledWebSocketByNameEventHandler(ILogger<RoomCodeEditorEnabledWebSocketByNameEventHandler> logger)
    : WebSocketByNameEventHandlerBase<RoomCodeEditorEnabledEvent.Payload>(logger)
{
    protected override string SupportType => EventType.RoomCodeEditorEnabled;

    protected override Task HandleEventAsync(SocketEventDetail detail, RoomCodeEditorEnabledEvent.Payload? message, CancellationToken cancellationToken)
    {
        if (detail.ParticipantType is not (EVRoomParticipantType.Expert or EVRoomParticipantType.Examinee))
        {
            return Task.CompletedTask;
        }

        var handler = detail.ScopedServiceProvider.GetRequiredService<RoomCodeEditorChangeEventHandler>();
        var request = new RoomCodeEditorChangeEventHandler.Request(detail.RoomId, message?.Enabled ?? false, EVRoomCodeEditorChangeSource.User);
        return handler.HandleAsync(request, cancellationToken);
    }
}

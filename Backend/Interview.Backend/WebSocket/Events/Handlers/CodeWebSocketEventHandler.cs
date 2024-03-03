using System.Net.WebSockets;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class CodeWebSocketEventHandler : WebSocketEventHandlerBase
{
    public CodeWebSocketEventHandler(ILogger<WebSocketEventHandlerBase> logger)
        : base(logger)
    {
    }

    protected override string SupportType => "code";

    protected override async Task HandleEventAsync(SocketEventDetail detail, string payload, CancellationToken cancellationToken)
    {
        var participantRepository = detail.ScopedServiceProvider.GetRequiredService<IRoomParticipantRepository>();
        var roomParticipant = await participantRepository.FindByRoomIdAndUserIdDetailedAsync(detail.RoomId, detail.UserId, cancellationToken);
        if (roomParticipant is null)
        {
            Logger.LogWarning("Not found room participant {RoomId} {UserId}", detail.RoomId, detail.UserId);
            return;
        }

        if (roomParticipant.Type != SERoomParticipantType.Examinee &&
            roomParticipant.Type != SERoomParticipantType.Expert)
        {
            Logger.LogWarning("Not enough permissions to send an event {RoomId} {UserId}", detail.RoomId, detail.UserId);
            return;
        }

        var repository = detail.ScopedServiceProvider.GetRequiredService<IRoomConfigurationRepository>();
        var request = new UpsertCodeStateRequest { CodeEditorContent = payload, RoomId = detail.RoomId, };
        await repository.UpsertCodeStateAsync(request, cancellationToken);
    }
}

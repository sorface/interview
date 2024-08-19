using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.Events;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;

namespace Interview.Backend.WebSocket.Events.Handlers;

public class RoomCodeEditorEnabledWebSocketByNameEventHandler : WebSocketByNameEventHandlerBase<RoomCodeEditorEnabledWebSocketByNameEventHandler.Payload>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomCodeEditorEnabledWebSocketByNameEventHandler(
        IRoomEventDispatcher eventDispatcher,
        ILogger<RoomCodeEditorEnabledWebSocketByNameEventHandler> logger)
        : base(logger)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override string SupportType => EventType.RoomCodeEditorEnabled;

    protected override async Task HandleEventAsync(SocketEventDetail detail, Payload? message, CancellationToken cancellationToken)
    {
        if (detail.ParticipantType is not (EVRoomParticipantType.Expert or EVRoomParticipantType.Examinee))
        {
            return;
        }

        var db = detail.ScopedServiceProvider.GetRequiredService<AppDbContext>();
        var count = await db.RoomConfiguration
            .Include(e => e.Room)
            .Where(e => e.Room!.Id == detail.RoomId)
            .ExecuteUpdateAsync(calls => calls.SetProperty(e => e.CodeEditorEnabled, message!.Enabled), cancellationToken);
        if (count == 0)
        {
            var roomConfiguration = new RoomConfiguration
            {
                Room = await db.Rooms.FirstAsync(e => e.Id == detail.RoomId, cancellationToken),
                CodeEditorEnabled = message!.Enabled,
            };
            await db.RoomConfiguration.AddAsync(roomConfiguration, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        var @event = new RoomEvent<Payload>(detail.RoomId, EventType.RoomCodeEditorEnabled, message, false);
        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }

    public sealed class Payload
    {
        public bool Enabled { get; set; }
    }
}

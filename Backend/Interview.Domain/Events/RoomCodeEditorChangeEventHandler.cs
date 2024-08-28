using Interview.Domain.Database;
using Interview.Domain.Events.DatabaseProcessors.Records.Question;
using Interview.Domain.Rooms.RoomConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events;

public class RoomCodeEditorChangeEventHandler
{
    private readonly AppDbContext _db;
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomCodeEditorChangeEventHandler(AppDbContext db, IRoomEventDispatcher eventDispatcher)
    {
        _db = db;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(Guid roomId, bool enabled, CancellationToken cancellationToken)
    {
        var roomConfiguration = await _db.RoomConfiguration.Include(e => e.Room)
            .FirstOrDefaultAsync(e => e.Room!.Id == roomId, cancellationToken);
        if (roomConfiguration is null)
        {
            roomConfiguration = new RoomConfiguration
            {
                Room = await _db.Rooms.FirstAsync(e => e.Id == roomId, cancellationToken),
                CodeEditorEnabled = enabled,
            };
            await _db.RoomConfiguration.AddAsync(roomConfiguration, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (roomConfiguration.CodeEditorEnabled == enabled)
            {
                return;
            }

            roomConfiguration.CodeEditorEnabled = enabled;
            await _db.SaveChangesAsync(cancellationToken);
        }

        var payload = new RoomCodeEditorEnabledEvent.Payload
        {
            Enabled = enabled,
        };
        var @event = new RoomCodeEditorEnabledEvent(roomId, payload);
        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}

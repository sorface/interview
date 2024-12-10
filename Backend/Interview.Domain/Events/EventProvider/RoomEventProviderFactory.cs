using Interview.Domain.Database;
using Interview.Domain.Events.Storage;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events.EventProvider;

public class RoomEventProviderFactory(IHotEventStorage eventStorage, AppDbContext db) : ISelfScopeService
{
    public async Task<IRoomEventProvider> CreateProviderAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var hasDbEvents = await db.Rooms
            .Include(e => e.QueuedRoomEvent)
            .AnyAsync(e => e.Id == roomId && e.QueuedRoomEvent != null, cancellationToken);
        return hasDbEvents
            ? new DbRoomEventProvider(db, roomId)
            : new HotRoomEventStorageRoomEventProvider(eventStorage, roomId);
    }
}

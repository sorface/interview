using Interview.Domain.Database;
using Interview.Domain.Events.Storage;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Events.EventProvider;

public class RoomEventProviderFactory : ISelfScopeService
{
    private readonly IHotEventStorage _eventStorage;
    private readonly AppDbContext _db;

    public RoomEventProviderFactory(IHotEventStorage eventStorage, AppDbContext db)
    {
        _eventStorage = eventStorage;
        _db = db;
    }

    public async Task<IRoomEventProvider> CreateProviderAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var hasDbEvents = await _db.Rooms
            .Include(e => e.QueuedRoomEvent)
            .AnyAsync(e => e.Id == roomId && e.QueuedRoomEvent != null, cancellationToken);
        return hasDbEvents
            ? new DbRoomEventProvider(_db, roomId)
            : new HotRoomEventStorageRoomEventProvider(_eventStorage, roomId);
    }
}

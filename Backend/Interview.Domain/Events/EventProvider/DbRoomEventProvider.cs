using Interview.Domain.Database;
using Microsoft.EntityFrameworkCore;
using NSpecifications;

namespace Interview.Domain.Events.EventProvider;

public sealed class DbRoomEventProvider : IRoomEventProvider
{
    private readonly AppDbContext _db;
    private readonly Guid _roomId;

    public DbRoomEventProvider(AppDbContext db, Guid roomId)
    {
        _db = db;
        _roomId = roomId;
    }

    public async Task<IEnumerable<EPStorageEvent>> GetEventsAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        var res = await _db.RoomEvents.AsNoTracking()
            .Where(spec)
            .OrderBy(e => e.CreateDate)
            .Select(e => new EPStorageEvent
            {
                Id = e.Id,
                Payload = e.Payload,
                CreatedAt = e.CreateDate,
                CreatedById = e.EventSenderId,
            })
            .ToListAsync(cancellationToken);
        return res;
    }

    public Task<EPStorageEvent?> GetLatestEventAsync(EPStorageEventRequest request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        return _db.RoomEvents.AsNoTracking()
            .Where(spec)
            .OrderByDescending(e => e.CreateDate)
            .Select(e => new EPStorageEvent
            {
                Id = e.Id,
                Payload = e.Payload,
                CreatedAt = e.CreateDate,
                CreatedById = e.EventSenderId,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private ASpec<DbRoomEvent> BuildSpecification(EPStorageEventRequest request)
    {
        ASpec<DbRoomEvent> spec = new Spec<DbRoomEvent>(e => e.RoomId == _roomId && e.Type == request.Type);
        if (request.From is not null)
        {
            var dateFrom = request.From.Value;
            spec &= new Spec<DbRoomEvent>(e => e.CreateDate >= dateFrom);
        }

        if (request.To is not null)
        {
            var dateTo = request.To.Value;
            spec &= new Spec<DbRoomEvent>(e => e.CreateDate <= dateTo);
        }

        return spec;
    }
}

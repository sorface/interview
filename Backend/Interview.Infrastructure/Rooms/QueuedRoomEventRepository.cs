using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Infrastructure.Rooms;

public class QueuedRoomEventRepository : EfRepository<QueuedRoomEvent>, IQueuedRoomEventRepository
{
    public QueuedRoomEventRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<IPagedList<Guid>> GetNotProcessedRoomsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return Db.Rooms.AsNoTracking()
            .Include(e => e.QueuedRoomEvent)
            .Where(e => e.Status == SERoomStatus.Close && e.QueuedRoomEvent == null)
            .Select(e => e.Id)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }
}

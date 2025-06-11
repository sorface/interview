using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Infrastructure.Rooms;

public class QueuedRoomEventRepository(AppDbContext db) : EfRepository<QueuedRoomEvent>(db), IQueuedRoomEventRepository
{
    public Task<IPagedList<Guid>> GetNotProcessedRoomsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var closeStatuses = SERoomStatus.CloseStatuses;
        return Db.Rooms.AsNoTracking()
            .Include(e => e.QueuedRoomEvent)
            .Where(e => (closeStatuses.Contains(e.Status) || e.Status == SERoomStatus.Review) && e.QueuedRoomEvent == null)
            .Select(e => e.Id)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }
}

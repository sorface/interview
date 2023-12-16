using Interview.Domain.Repository;
using X.PagedList;

namespace Interview.Domain.Rooms;

public interface IQueuedRoomEventRepository : IRepository<QueuedRoomEvent>
{
    Task<IPagedList<Guid>> GetNotProcessedRoomsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}

using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews;

public interface IRoomReviewRepository : IRepository<RoomReview>
{
    Task<IPagedList<RoomReviewPageDetail>> GetDetailedPageAsync(ISpecification<RoomReview> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

using Interview.Domain.Repository;
using Interview.Domain.Rooms.Service.Records.Response.Page;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.RoomReviews;

public interface IRoomReviewRepository : IRepository<RoomReview>
{
    Task<IPagedList<RoomReviewPageDetail>> GetDetailedPageAsync(ISpecification<RoomReview> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

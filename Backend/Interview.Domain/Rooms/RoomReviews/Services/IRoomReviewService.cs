using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews.Services;

public interface IRoomReviewService : IService
{
    Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(
        RoomReviewPageRequest request,
        CancellationToken cancellationToken = default);

    Task<RoomReviewDetail> CreateAsync(
        RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<RoomReviewDetail> UpdateAsync(
        Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default);
}

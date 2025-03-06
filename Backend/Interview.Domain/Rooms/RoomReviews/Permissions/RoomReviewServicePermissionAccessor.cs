using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews.Permissions;

public class RoomReviewServicePermissionAccessor(IRoomReviewService roomReviewService, ISecurityService securityService, IRoomReviewRepository repository)
    : IRoomReviewService, IServiceDecorator
{
    public Task<UserRoomReviewResponse?> GetUserRoomReviewAsync(UserRoomReviewRequest request, CancellationToken cancellationToken)
    {
        return roomReviewService.GetUserRoomReviewAsync(request, cancellationToken);
    }

    public async Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(RoomReviewPageRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.RoomReviewFindPage, cancellationToken);
        return await roomReviewService.FindPageAsync(request, cancellationToken);
    }

    public async Task<RoomReviewDetail> CreateAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewCreate, cancellationToken);
        return await roomReviewService.CreateAsync(request, userId, cancellationToken);
    }

    public async Task<RoomReviewDetail> UpdateAsync(Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.FindByIdDetailedAsync(id, cancellationToken);
        await securityService.EnsureRoomPermissionAsync(entity?.Participant.RoomId, SEPermission.RoomReviewUpdate, cancellationToken);
        return await roomReviewService.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<UpsertReviewResponse> UpsertAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewUpsert, cancellationToken);
        return await roomReviewService.UpsertAsync(request, userId, cancellationToken);
    }

    public async Task<RoomCompleteResponse> CompleteAsync(RoomReviewCompletionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewCompletion, cancellationToken);
        return await roomReviewService.CompleteAsync(request, userId, cancellationToken);
    }
}

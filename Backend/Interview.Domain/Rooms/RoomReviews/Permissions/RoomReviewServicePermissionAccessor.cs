using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services;
using Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;
using X.PagedList;

namespace Interview.Domain.Rooms.RoomReviews.Permissions;

public class RoomReviewServicePermissionAccessor : IRoomReviewService, IServiceDecorator
{
    private readonly IRoomReviewService _roomReviewService;
    private readonly ISecurityService _securityService;
    private readonly IRoomReviewRepository _repository;

    public RoomReviewServicePermissionAccessor(IRoomReviewService roomReviewService, ISecurityService securityService, IRoomReviewRepository repository)
    {
        _roomReviewService = roomReviewService;
        _securityService = securityService;
        _repository = repository;
    }

    public Task<UserRoomReviewResponse?> GetUserRoomReviewAsync(UserRoomReviewRequest request, CancellationToken cancellationToken)
    {
        return _roomReviewService.GetUserRoomReviewAsync(request, cancellationToken);
    }

    public async Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(RoomReviewPageRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.RoomReviewFindPage, cancellationToken);
        return await _roomReviewService.FindPageAsync(request, cancellationToken);
    }

    public async Task<RoomReviewDetail> CreateAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewCreate, cancellationToken);
        return await _roomReviewService.CreateAsync(request, userId, cancellationToken);
    }

    public async Task<RoomReviewDetail> UpdateAsync(Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.FindByIdDetailedAsync(id, cancellationToken);
        await _securityService.EnsureRoomPermissionAsync(entity?.Participant.RoomId, SEPermission.RoomReviewUpdate, cancellationToken);
        return await _roomReviewService.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<UpsertReviewResponse> UpsertAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewUpsert, cancellationToken);
        return await _roomReviewService.UpsertAsync(request, userId, cancellationToken);
    }

    public async Task CompleteAsync(RoomReviewCompletionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomReviewCompletion, cancellationToken);
        await _roomReviewService.CompleteAsync(request, userId, cancellationToken);
    }
}

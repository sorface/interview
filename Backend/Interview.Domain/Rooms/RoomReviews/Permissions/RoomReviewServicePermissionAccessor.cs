using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomReviews.Records;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Interview.Domain.Rooms.RoomReviews.Services;
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

    public Task<IPagedList<RoomReviewPageDetail>> FindPageAsync(RoomReviewPageRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomReviewFindPage);
        return _roomReviewService.FindPageAsync(request, cancellationToken);
    }

    public Task<RoomReviewDetail> CreateAsync(RoomReviewCreateRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomReviewCreate);
        return _roomReviewService.CreateAsync(request, userId, cancellationToken);
    }

    public async Task<RoomReviewDetail> UpdateAsync(Guid id, RoomReviewUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.FindByIdDetailedAsync(id, cancellationToken);
        _securityService.EnsureRoomPermission(entity?.Room?.Id, SEPermission.RoomReviewUpdate);
        return await _roomReviewService.UpdateAsync(id, request, cancellationToken);
    }
}

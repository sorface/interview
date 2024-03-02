using Interview.Domain.Events.Storage;
using Interview.Domain.Permissions;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.Service;
using X.PagedList;

namespace Interview.Domain.Rooms.Permissions;

public class RoomServicePermissionAccessor : IRoomService, IServiceDecorator
{
    private readonly IRoomService _roomService;
    private readonly ISecurityService _securityService;

    public RoomServicePermissionAccessor(IRoomService roomService, ISecurityService securityService)
    {
        _roomService = roomService;
        _securityService = securityService;
    }

    public Task<IPagedList<RoomPageDetail>> FindPageAsync(RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomFindPage);
        return _roomService.FindPageAsync(filter, pageNumber, pageSize, cancellationToken);
    }

    public Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _securityService.EnsureRoomPermission(id, SEPermission.RoomFindById);
        return _roomService.FindByIdAsync(id, cancellationToken);
    }

    public Task<Room> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomCreate);
        return _roomService.CreateAsync(request, cancellationToken);
    }

    public Task<RoomItem> UpdateAsync(Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.RoomUpdate);
        return _roomService.UpdateAsync(roomId, request, cancellationToken);
    }

    public Task<(Room, RoomParticipant)> AddParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.RoomAddParticipant);
        return _roomService.AddParticipantAsync(roomId, userId, cancellationToken);
    }

    public Task SendEventRequestAsync(IEventRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomSendEventRequest);
        return _roomService.SendEventRequestAsync(request, cancellationToken);
    }

    public Task CloseAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.RoomClose);
        return _roomService.CloseAsync(roomId, cancellationToken);
    }

    public Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.RoomStartReview);
        return _roomService.StartReviewAsync(roomId, cancellationToken);
    }

    public Task<ActualRoomStateResponse> GetActualStateAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.RoomGetState);
        return _roomService.GetActualStateAsync(roomId, cancellationToken);
    }

    public Task UpsertRoomStateAsync(Guid roomId, string type, string payload, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.UpsertRoomState);
        return _roomService.UpsertRoomStateAsync(roomId, type, payload, cancellationToken);
    }

    public Task DeleteRoomStateAsync(Guid roomId, string type, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(roomId, SEPermission.DeleteRoomState);
        return _roomService.DeleteRoomStateAsync(roomId, type, cancellationToken);
    }

    public Task<Analytics> GetAnalyticsAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomGetAnalytics);
        return _roomService.GetAnalyticsAsync(request, cancellationToken);
    }

    public Task<RoomInviteDetail> ApplyInvite(Guid roomId, Guid? invite, CancellationToken cancellationToken = default)
    {
        if (_securityService.CurrentUser() is null)
        {
            throw AccessDeniedException.CreateForAction("use invite");
        }

        return _roomService.ApplyInvite(roomId, invite, cancellationToken);
    }

    public Task<AnalyticsSummary> GetAnalyticsSummaryAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomGetAnalyticsSummary);
        return _roomService.GetAnalyticsSummaryAsync(request, cancellationToken);
    }

    public Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.TranscriptionGet);
        return _roomService.GetTranscriptionAsync(request, cancellationToken);
    }
}

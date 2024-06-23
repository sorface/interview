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

    public async Task<IPagedList<RoomPageDetail>> FindPageAsync(RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.RoomFindPage, cancellationToken);
        return await _roomService.FindPageAsync(filter, pageNumber, pageSize, cancellationToken);
    }

    public async Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await _securityService.EnsureRoomPermissionAsync(id, SEPermission.RoomFindById, cancellationToken);
        return await _roomService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<RoomPageDetail> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default)
    {
        var permission = request.AccessType == SERoomAccessType.Public
            ? SEPermission.PublicRoomCreate
            : SEPermission.RoomCreate;
        await _securityService.EnsurePermissionAsync(permission, cancellationToken);
        return await _roomService.CreateAsync(request, cancellationToken);
    }

    public async Task<RoomItem> UpdateAsync(Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomUpdate, cancellationToken);
        return await _roomService.UpdateAsync(roomId, request, cancellationToken);
    }

    public async Task<(Room, RoomParticipant)> AddParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomAddParticipant, cancellationToken);
        return await _roomService.AddParticipantAsync(roomId, userId, cancellationToken);
    }

    public async Task SendEventRequestAsync(IEventRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomSendEventRequest, cancellationToken);
        await _roomService.SendEventRequestAsync(request, cancellationToken);
    }

    public async Task CloseAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomClose, cancellationToken);
        await _roomService.CloseAsync(roomId, cancellationToken);
    }

    public async Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomStartReview, cancellationToken);
        await _roomService.StartReviewAsync(roomId, cancellationToken);
    }

    public async Task<ActualRoomStateResponse> GetActualStateAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomGetState, cancellationToken);
        return await _roomService.GetActualStateAsync(roomId, cancellationToken);
    }

    public async Task UpsertRoomStateAsync(Guid roomId, string type, string payload, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.UpsertRoomState, cancellationToken);
        await _roomService.UpsertRoomStateAsync(roomId, type, payload, cancellationToken);
    }

    public async Task DeleteRoomStateAsync(Guid roomId, string type, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.DeleteRoomState, cancellationToken);
        await _roomService.DeleteRoomStateAsync(roomId, type, cancellationToken);
    }

    public async Task<Analytics> GetAnalyticsAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomGetAnalytics, cancellationToken);
        return await _roomService.GetAnalyticsAsync(request, cancellationToken);
    }

    public Task<RoomInviteResponse> ApplyInvite(Guid roomId, Guid? invite, CancellationToken cancellationToken = default)
    {
        if (_securityService.CurrentUser() is null)
        {
            throw AccessDeniedException.CreateForAction("use invite");
        }

        return _roomService.ApplyInvite(roomId, invite, cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GetInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomInviteGet, cancellationToken);
        return await _roomService.GetInvitesAsync(roomId, cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GenerateInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomInviteGenerate, cancellationToken);

        return await _roomService.GenerateInvitesAsync(roomId, cancellationToken);
    }

    public async Task<RoomInviteResponse> GenerateInviteAsync(
        RoomInviteGeneratedRequest roomInviteGenerated,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(roomInviteGenerated.RoomId, SEPermission.RoomInviteGenerate, cancellationToken);

        return await _roomService.GenerateInviteAsync(roomInviteGenerated, cancellationToken);
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomGetAnalyticsSummary, cancellationToken);
        return await _roomService.GetAnalyticsSummaryAsync(request, cancellationToken);
    }

    public async Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.TranscriptionGet, cancellationToken);
        return await _roomService.GetTranscriptionAsync(request, cancellationToken);
    }
}

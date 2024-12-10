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

public class RoomServicePermissionAccessor(IRoomService roomService, ISecurityService securityService) : IRoomService, IServiceDecorator
{
    public async Task<IPagedList<RoomPageDetail>> FindPageAsync(RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.RoomFindPage, cancellationToken);
        return await roomService.FindPageAsync(filter, pageNumber, pageSize, cancellationToken);
    }

    public async Task<List<RoomCalendarItem>> GetCalendarAsync(RoomCalendarRequest filter, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.GetRoomCalendar, cancellationToken);
        return await roomService.GetCalendarAsync(filter, cancellationToken);
    }

    public async Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(id, SEPermission.RoomFindById, cancellationToken);
        return await roomService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<RoomPageDetail> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default)
    {
        var permission = request.AccessType == SERoomAccessType.Public
            ? SEPermission.PublicRoomCreate
            : SEPermission.RoomCreate;
        await securityService.EnsurePermissionAsync(permission, cancellationToken);
        return await roomService.CreateAsync(request, cancellationToken);
    }

    public async Task<RoomItem> UpdateAsync(Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomUpdate, cancellationToken);
        return await roomService.UpdateAsync(roomId, request, cancellationToken);
    }

    public async Task<(Room, RoomParticipant)> AddParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomAddParticipant, cancellationToken);
        return await roomService.AddParticipantAsync(roomId, userId, cancellationToken);
    }

    public async Task SendEventRequestAsync(IEventRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomSendEventRequest, cancellationToken);
        await roomService.SendEventRequestAsync(request, cancellationToken);
    }

    public async Task CloseAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomClose, cancellationToken);
        await roomService.CloseAsync(roomId, cancellationToken);
    }

    public async Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomStartReview, cancellationToken);
        await roomService.StartReviewAsync(roomId, cancellationToken);
    }

    public async Task<ActualRoomStateResponse> GetActualStateAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomGetState, cancellationToken);
        return await roomService.GetActualStateAsync(roomId, cancellationToken);
    }

    public async Task UpsertRoomStateAsync(Guid roomId, string type, string payload, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.UpsertRoomState, cancellationToken);
        await roomService.UpsertRoomStateAsync(roomId, type, payload, cancellationToken);
    }

    public async Task DeleteRoomStateAsync(Guid roomId, string type, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.DeleteRoomState, cancellationToken);
        await roomService.DeleteRoomStateAsync(roomId, type, cancellationToken);
    }

    public async Task<Analytics> GetAnalyticsAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomGetAnalytics, cancellationToken);
        return await roomService.GetAnalyticsAsync(request, cancellationToken);
    }

    public Task<RoomInviteResponse> ApplyInvite(Guid roomId, Guid? invite, CancellationToken cancellationToken = default)
    {
        if (securityService.CurrentUser() is null)
        {
            throw AccessDeniedException.CreateForAction("use invite");
        }

        return roomService.ApplyInvite(roomId, invite, cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GetInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomInviteGet, cancellationToken);
        return await roomService.GetInvitesAsync(roomId, cancellationToken);
    }

    public async Task<List<RoomInviteResponse>> GenerateInvitesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.RoomInviteGenerate, cancellationToken);

        return await roomService.GenerateInvitesAsync(roomId, cancellationToken);
    }

    public async Task<RoomInviteResponse> GenerateInviteAsync(
        RoomInviteGeneratedRequest roomInviteGenerated,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomInviteGenerated.RoomId, SEPermission.RoomInviteGenerate, cancellationToken);

        return await roomService.GenerateInviteAsync(roomInviteGenerated, cancellationToken);
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(RoomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomGetAnalyticsSummary, cancellationToken);
        return await roomService.GetAnalyticsSummaryAsync(request, cancellationToken);
    }

    public async Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.TranscriptionGet, cancellationToken);
        return await roomService.GetTranscriptionAsync(request, cancellationToken);
    }
}

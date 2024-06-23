using Interview.Domain.Events.Storage;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Request.Transcription;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.Records.Response.RoomStates;
using Interview.Domain.Rooms.RoomParticipants;
using X.PagedList;

namespace Interview.Domain.Rooms.Service;

public interface IRoomService : IService
{
    Task<IPagedList<RoomPageDetail>> FindPageAsync(
        RoomPageDetailRequestFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<RoomDetail> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RoomPageDetail> CreateAsync(RoomCreateRequest request, CancellationToken cancellationToken = default);

    Task<RoomItem> UpdateAsync(
        Guid roomId, RoomUpdateRequest? request, CancellationToken cancellationToken = default);

    Task<(Room Room, RoomParticipant Participant)> AddParticipantAsync(
        Guid roomId, Guid userId, CancellationToken cancellationToken = default);

    Task SendEventRequestAsync(
        IEventRequest request, CancellationToken cancellationToken = default);

    Task CloseAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task StartReviewAsync(Guid roomId, CancellationToken cancellationToken);

    Task<ActualRoomStateResponse> GetActualStateAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task UpsertRoomStateAsync(
        Guid roomId,
        string type,
        string payload,
        CancellationToken cancellationToken = default);

    Task DeleteRoomStateAsync(
        Guid roomId,
        string type,
        CancellationToken cancellationToken = default);

    Task<Analytics> GetAnalyticsAsync(
        RoomAnalyticsRequest request,
        CancellationToken cancellationToken = default);

    Task<RoomInviteResponse> ApplyInvite(Guid room, Guid? invite, CancellationToken cancellationToken = default);

    Task<List<RoomInviteResponse>> GetInvitesAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task<List<RoomInviteResponse>> GenerateInvitesAsync(Guid roomId, CancellationToken cancellationToken = default);

    public Task<RoomInviteResponse> GenerateInviteAsync(
        RoomInviteGeneratedRequest roomInviteGenerated,
        CancellationToken cancellationToken = default);

    Task<AnalyticsSummary> GetAnalyticsSummaryAsync(
        RoomAnalyticsRequest request, CancellationToken cancellationToken = default);

    Task<Dictionary<string, List<IStorageEvent>>> GetTranscriptionAsync(
        TranscriptionRequest request, CancellationToken cancellationToken = default);
}

public interface IRoomServiceWithoutPermissionCheck : IRoomService
{
}

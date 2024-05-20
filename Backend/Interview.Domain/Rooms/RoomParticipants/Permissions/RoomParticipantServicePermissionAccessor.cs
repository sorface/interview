using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Records.Response;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomParticipants.Permissions;

public class RoomParticipantServicePermissionAccessor : IRoomParticipantService, IServiceDecorator
{
    private readonly IRoomParticipantService _roomParticipantService;
    private readonly ISecurityService _securityService;

    public RoomParticipantServicePermissionAccessor(
        IRoomParticipantService roomParticipantService,
        ISecurityService securityService)
    {
        _securityService = securityService;
        _roomParticipantService = roomParticipantService;
    }

    public async Task<RoomParticipantDetail> FindByRoomIdAndUserIdAsync(
        RoomParticipantGetRequest request,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantFindByRoomIdAndUserId, cancellationToken);
        return await _roomParticipantService.FindByRoomIdAndUserIdAsync(request, cancellationToken);
    }

    public async Task<RoomParticipantDetail> ChangeStatusAsync(
        RoomParticipantChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantChangeStatus, cancellationToken);
        return await _roomParticipantService.ChangeStatusAsync(request, cancellationToken);
    }

    public async Task<RoomParticipantDetail> CreateAsync(
        RoomParticipantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantCreate, cancellationToken);
        return await _roomParticipantService.CreateAsync(request, cancellationToken);
    }

    public Task<IReadOnlyCollection<RoomParticipant>> CreateAsync(
        Guid roomId,
        IReadOnlyCollection<(User User, Room Room, SERoomParticipantType Type)> participants,
        CancellationToken cancellationToken = default)
    {
        return _roomParticipantService.CreateAsync(roomId, participants, cancellationToken);
    }
}

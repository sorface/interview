using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Records.Response;
using Interview.Domain.Rooms.RoomParticipants.Service;

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

    public Task<RoomParticipantDetail> FindByRoomIdAndUserIdAsync(
        RoomParticipantGetRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomParticipantFindByRoomIdAndUserId);

        return _roomParticipantService.FindByRoomIdAndUserIdAsync(request, cancellationToken);
    }

    public Task<RoomParticipantDetail> ChangeStatusAsync(
        RoomParticipantChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomParticipantChangeStatus);

        return _roomParticipantService.ChangeStatusAsync(request, cancellationToken);
    }

    public Task<RoomParticipantDetail> CreateAsync(
        RoomParticipantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsureRoomPermission(request.RoomId, SEPermission.RoomParticipantCreate);

        return _roomParticipantService.CreateAsync(request, cancellationToken);
    }
}

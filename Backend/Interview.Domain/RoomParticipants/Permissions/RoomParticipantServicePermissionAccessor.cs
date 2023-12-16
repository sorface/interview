using Interview.Domain.Permissions;
using Interview.Domain.RoomParticipants.Records.Request;
using Interview.Domain.RoomParticipants.Records.Response;
using Interview.Domain.RoomParticipants.Service;

namespace Interview.Domain.RoomParticipants.Permissions;

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
        _securityService.EnsurePermission(SEPermission.RoomParticipantFindByRoomIdAndUserId);

        return _roomParticipantService.FindByRoomIdAndUserIdAsync(request, cancellationToken);
    }

    public Task<RoomParticipantDetail> ChangeStatusAsync(
        RoomParticipantChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomParticipantChangeStatus);

        return _roomParticipantService.ChangeStatusAsync(request, cancellationToken);
    }

    public Task<RoomParticipantDetail> CreateAsync(
        RoomParticipantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomParticipantCreate);

        return _roomParticipantService.CreateAsync(request, cancellationToken);
    }
}

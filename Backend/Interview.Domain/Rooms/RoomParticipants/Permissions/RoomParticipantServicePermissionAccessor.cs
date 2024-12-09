using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Records.Response;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomParticipants.Permissions;

public class RoomParticipantServicePermissionAccessor(
    IRoomParticipantService roomParticipantService,
    ISecurityService securityService)
    : IRoomParticipantService, IServiceDecorator
{
    public async Task<RoomParticipantDetail> FindByRoomIdAndUserIdAsync(
        RoomParticipantGetRequest request,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantFindByRoomIdAndUserId, cancellationToken);
        return await roomParticipantService.FindByRoomIdAndUserIdAsync(request, cancellationToken);
    }

    public async Task<RoomParticipantDetail> ChangeStatusAsync(
        RoomParticipantChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantChangeStatus, cancellationToken);
        return await roomParticipantService.ChangeStatusAsync(request, cancellationToken);
    }

    public async Task<RoomParticipantDetail> CreateAsync(
        RoomParticipantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomParticipantCreate, cancellationToken);
        return await roomParticipantService.CreateAsync(request, cancellationToken);
    }

    public Task<IReadOnlyCollection<RoomParticipant>> CreateAsync(
        Guid roomId,
        IReadOnlyCollection<(User User, Room Room, SERoomParticipantType Type)> participants,
        CancellationToken cancellationToken = default)
    {
        return roomParticipantService.CreateAsync(roomId, participants, cancellationToken);
    }
}

public interface IRoomParticipantServiceWithoutPermissionCheck : IRoomParticipantService
{
}

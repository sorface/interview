using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Records.Response;
using Interview.Domain.Users;
using NSpecifications;

namespace Interview.Domain.Rooms.RoomParticipants.Service;

public class RoomParticipantService : IRoomParticipantService
{
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAvailableRoomPermissionRepository _availableRoomPermissionRepository;

    public RoomParticipantService(
        IRoomParticipantRepository roomParticipantRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IAvailableRoomPermissionRepository availableRoomPermissionRepository)
    {
        _roomParticipantRepository = roomParticipantRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _availableRoomPermissionRepository = availableRoomPermissionRepository;
    }

    public async Task<RoomParticipantDetail> FindByRoomIdAndUserIdAsync(
        RoomParticipantGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var participant =
            await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(request.RoomId, request.UserId, cancellationToken);

        if (participant is null)
        {
            throw new NotFoundException($"Participant with userId found in the room" +
                                        $"Room id = {request.RoomId} User id = {request.UserId}");
        }

        return new RoomParticipantDetail
        {
            Id = participant.Id,
            RoomId = participant.Room.Id,
            UserId = participant.User.Id,
            UserType = participant.Type.Name,
        };
    }

    public async Task<RoomParticipantDetail> ChangeStatusAsync(
        RoomParticipantChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (SERoomParticipantType.TryFromName(request.UserType, out var participantType) is false)
        {
            throw new UserException("Type user not valid");
        }

        var participant =
            await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(request.RoomId, request.UserId, cancellationToken);

        if (participant is null)
        {
            throw new UserException("The user not found in the room");
        }

        participant.Type = participantType;
        await AddDefaultPermissionsAsync(new[] { participant }, cancellationToken);
        await _roomParticipantRepository.UpdateAsync(participant, cancellationToken);

        return new RoomParticipantDetail
        {
            Id = participant.Id,
            RoomId = participant.Room.Id,
            UserId = participant.User.Id,
            UserType = participant.Type.Name,
        };
    }

    /// <summary>
    /// Adding a new member to a room.
    /// </summary>
    /// <param name="request">Data for adding a new participant to the room.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Data of the new room participant.</returns>
    public async Task<RoomParticipantDetail> CreateAsync(
        RoomParticipantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (SERoomParticipantType.TryFromName(request.Type, out var participantType) is false)
        {
            throw new UserException("Type user not valid");
        }

        var existingParticipant = await _roomParticipantRepository.IsExistsByRoomIdAndUserIdAsync(
            request.RoomId, request.UserId, cancellationToken);

        if (existingParticipant)
        {
            throw new UserException("Participant already exists. " +
                                    $"Room id = {request.RoomId} User id = {request.UserId}");
        }

        var room = await _roomRepository.FindByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var user = await _userRepository.FindByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw NotFoundException.Create<Room>(request.UserId);
        }

        var roomParticipants = await CreateAsync(new[] { (user, room, participantType) }, cancellationToken);
        var roomParticipant = roomParticipants.First();
        await _roomParticipantRepository.CreateAsync(roomParticipant, cancellationToken);

        return new RoomParticipantDetail
        {
            Id = roomParticipant.Id,
            RoomId = roomParticipant.Room.Id,
            UserId = roomParticipant.User.Id,
            UserType = roomParticipant.Type.Name,
        };
    }

    public async Task<IReadOnlyCollection<RoomParticipant>> CreateAsync(
        IReadOnlyCollection<(User User, Room Room, SERoomParticipantType Type)> participants,
        CancellationToken cancellationToken = default)
    {
        var createdParticipants = participants.Select(e => new RoomParticipant(e.User, e.Room, e.Type)).ToList();
        await AddDefaultPermissionsAsync(createdParticipants, cancellationToken);
        return createdParticipants;
    }

    private async Task AddDefaultPermissionsAsync(
        IReadOnlyCollection<RoomParticipant> participants,
        CancellationToken cancellationToken)
    {
        var participantTypes = participants
            .Select(e => e.Type)
            .ToHashSet();
        var requiredPermissions = participantTypes
            .SelectMany(e => e.DefaultRoomPermission)
            .Select(e => e.Value)
            .ToHashSet();
        var specification = new Spec<AvailableRoomPermission>(e => requiredPermissions.Contains(e.Id));
        var availablePermissions = await _availableRoomPermissionRepository.FindAsync(specification, cancellationToken);
        var availablePermissionMap = availablePermissions
            .ToDictionary(e => e.Id);
        var defaultPermissionsMap = participantTypes
            .Select(e =>
            {
                var availableRoomPermissions = e.DefaultRoomPermission.Select(p => availablePermissionMap[p.Value]).ToList();
                return (ParticipantType: e, DefaultPermissions: availableRoomPermissions);
            })
            .ToDictionary(e => e.ParticipantType, e => e.DefaultPermissions);
        foreach (var roomParticipant in participants)
        {
            roomParticipant.Permissions.Clear();
            roomParticipant.Permissions.AddRange(defaultPermissionsMap[roomParticipant.Type]);
        }
    }
}

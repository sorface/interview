using Interview.Domain.Database;
using Interview.Domain.Invites;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Rooms.RoomInvites;

public class RoomInviteService : IRoomInviteService
{
    private readonly AppDbContext _db;
    private readonly IRoomParticipantService _roomParticipantService;
    private readonly ILogger<RoomInviteService> _logger;

    public RoomInviteService(AppDbContext db, IRoomParticipantService roomParticipantService, ILogger<RoomInviteService> logger)
    {
        _db = db;
        _roomParticipantService = roomParticipantService;
        _logger = logger;
    }

    public async Task<RoomInviteResponse> ApplyInvite(
        Guid inviteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var invite = await _db.Invites.Where(invite => invite.Id == inviteId).FirstOrDefaultAsync(cancellationToken);

        if (invite is null)
        {
            _logger.LogError("invite not found with id {inviteId}", inviteId);
            throw NotFoundException.Create<Invite>(inviteId);
        }

        _logger.LogInformation("invite found with id {inviteId}", inviteId);
        if (invite.UsesCurrent >= invite.UsesMax)
        {
            _logger.LogError("invite with id {inviteId} has max count used {inviteUseCurrent}/{inviteUseMax}", inviteId, invite.UsesCurrent, invite.UsesMax);
            throw new UserException("The invitation has already been used");
        }

        var roomInvite = await _db.RoomInvites
            .Where(roomInviteItem => roomInviteItem.InviteId == inviteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (roomInvite is null)
        {
            _logger.LogError("room invite not found by invite id {inviteId}", inviteId);
            throw new NotFoundException("Invite not found for any rooms");
        }

        _logger.LogInformation("room invite found by invite id {inviteId}", inviteId);

        if (roomInvite.Room is null)
        {
            _logger.LogError("room invite not sync with something room's {inviteId}", inviteId);
            throw new NotFoundException("The invitation no longer belongs to the room");
        }

        _logger.LogInformation("found room [id -> {roomId}] which joined for invite [id -> {inviteId}]", roomInvite.RoomId, inviteId);

        var user = await _db.Users.Where(user => user.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The current user was not found");
        }

        _logger.LogInformation("User with id [{userId}] for invite found {inviteId}", user.Id, inviteId);

        return await _db.RunTransactionAsync(async _ =>
        {
            var participant = await _db.RoomParticipants
                .Include(e => e.Room)
                .Where(participant => participant.User.Id == userId && participant.Room.Id == roomInvite.RoomId)
                .FirstOrDefaultAsync(cancellationToken);

            if (participant is null)
            {
                _logger.LogInformation("Room participant not found in room [id -> {roomId}] by user [id -> {userId}]", user.Id, inviteId);

                var participants = await _roomParticipantService.CreateAsync(
                    roomInvite.Room.Id,
                    new[] { (user, roomInvite.Room, roomInvite.ParticipantType ?? SERoomParticipantType.Viewer) },
                    cancellationToken);
                var roomParticipant = participants.First();

                _logger.LogInformation(
                    "Created participant room [id -> {participantId}, type -> {participantType}] for room [id -> {roomId}] and user [id -> {userId}]",
                    roomParticipant.Id,
                    roomParticipant.Type,
                    roomInvite.RoomId,
                    userId);

                await UpdateInviteLimit(roomInvite, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                return new RoomInviteResponse
                {
                    InviteId = invite.Id,
                    ParticipantType = roomInvite.ParticipantType!.EnumValue,
                    Used = invite.UsesCurrent,
                    Max = invite.UsesMax,
                };
            }

            if (participant.Type != roomInvite.ParticipantType && roomInvite.ParticipantType is not null)
            {
                var roomParticipantChangeStatusRequest = new RoomParticipantChangeStatusRequest
                {
                    RoomId = roomInvite.RoomId,
                    UserId = participant.UserId,
                    UserType = roomInvite.ParticipantType.EnumValue,
                };
                await _roomParticipantService.ChangeStatusAsync(roomParticipantChangeStatusRequest, cancellationToken);
            }

            // await UpdateInviteLimit(roomInvite, cancellationToken);
            return new RoomInviteResponse
            {
                InviteId = invite.Id,
                ParticipantType = roomInvite.ParticipantType!.EnumValue,
                Used = invite.UsesCurrent,
                Max = invite.UsesMax,
            };
        },
        cancellationToken);
    }

    public async Task<RoomInviteResponse> GenerateAsync(
        Guid roomId,
        SERoomParticipantType participantType,
        int inviteMaxCount,
        CancellationToken cancellationToken)
    {
        await _db.RoomInvites
            .Where(roomInvite => roomInvite.RoomId == roomId && participantType == roomInvite.ParticipantType)
            .ExecuteDeleteAsync(cancellationToken);

        var invite = new Invite(inviteMaxCount);

        await _db.Invites.AddAsync(invite, cancellationToken);

        var newRoomInvite = new RoomInvite(invite.Id, roomId, participantType);

        await _db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new RoomInviteResponse
        {
            InviteId = invite.Id,
            ParticipantType = newRoomInvite.ParticipantType!.EnumValue,
            Used = invite.UsesCurrent,
            Max = invite.UsesMax,
        };
    }

    private async Task UpdateInviteLimit(RoomInvite roomInvite, CancellationToken cancellationToken = default)
    {
        roomInvite.Invite!.UsesCurrent += 1;

        if (roomInvite.Invite!.UsesCurrent < roomInvite.Invite!.UsesMax)
        {
            _logger.LogInformation("room invite [id -> {roomInviteId}] increment use count to {currentCount}", roomInvite.Id, roomInvite.Invite!.UsesCurrent);

            _db.Invites.Update(roomInvite.Invite);

            await _db.SaveChangesAsync(cancellationToken);

            return;
        }

        _logger.LogInformation("generate new 5 invites for room [id -> {roomId}]", roomInvite.RoomId);

        var regenerateInvite = new Invite(5);

        _logger.LogInformation("remove old invite for room [id -> {roomId}]", roomInvite.RoomId);

        _db.Invites.Remove(roomInvite.Invite);

        _logger.LogInformation("add new room invite for room [id -> {roomId}]", roomInvite.RoomId);

        await _db.Invites.AddAsync(regenerateInvite, cancellationToken);

        var newRoomInvite = new RoomInvite(regenerateInvite, roomInvite.Room!, roomInvite.ParticipantType!);

        await _db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        _db.RoomInvites.Remove(roomInvite);

        await _db.SaveChangesAsync(cancellationToken);
    }
}

using Interview.Domain.Database;
using Interview.Domain.Invites;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Permissions;
using Interview.Domain.Rooms.RoomParticipants.Records.Request;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Rooms.RoomInvites;

public class RoomInviteService(
    AppDbContext db,
    IRoomParticipantServiceWithoutPermissionCheck roomParticipantService,
    ILogger<RoomInviteService> logger)
    : IRoomInviteService
{
    public async Task<RoomInviteResponse> ApplyInvite(
        Guid inviteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var invite = await db.Invites.Where(invite => invite.Id == inviteId).FirstOrDefaultAsync(cancellationToken);

        if (invite is null)
        {
            logger.LogError("invite not found with id {inviteId}", inviteId);
            throw NotFoundException.Create<Invite>(inviteId);
        }

        logger.LogInformation("invite found with id {inviteId}", inviteId);
        if (invite.UsesCurrent >= invite.UsesMax)
        {
            logger.LogError("invite with id {inviteId} has max count used {inviteUseCurrent}/{inviteUseMax}", inviteId, invite.UsesCurrent, invite.UsesMax);
            throw new UserException("The invitation has already been used");
        }

        var roomInvite = await db.RoomInvites
            .Include(inviteItem => inviteItem.Room)
            .Where(roomInviteItem => roomInviteItem.InviteId == inviteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (roomInvite is null)
        {
            logger.LogError("room invite not found by invite id {inviteId}", inviteId);
            throw new NotFoundException("Invite not found for any rooms");
        }

        logger.LogInformation("room invite found by invite id {inviteId}", inviteId);

        if (roomInvite.Room is null)
        {
            logger.LogError("room invite not sync with something room's {inviteId}", inviteId);
            throw new NotFoundException("The invitation no longer belongs to the room");
        }

        if (roomInvite.Room.CreatedById == userId)
        {
            logger.LogError("You cannot apply an invite {inviteId} to a room in which you are the creator", inviteId);
            throw new UserException("You cannot apply an invite to a room in which you are the creator");
        }

        logger.LogInformation("found room [id -> {roomId}] which joined for invite [id -> {inviteId}]", roomInvite.RoomId, inviteId);

        var user = await db.Users.Where(user => user.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The current user was not found");
        }

        logger.LogInformation("User with id [{userId}] for invite found {inviteId}", user.Id, inviteId);

        return await db.RunTransactionAsync(async _ =>
            {
                var participant = await db.RoomParticipants
                    .Include(e => e.Room)
                    .Where(participant => participant.User.Id == userId && participant.Room.Id == roomInvite.RoomId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (participant is null)
                {
                    logger.LogInformation("Room participant not found in room [id -> {roomId}] by user [id -> {userId}]", user.Id, inviteId);

                    var participants = await roomParticipantService.CreateAsync(
                        roomInvite.Room.Id,
                        new[] { (user, roomInvite.Room, roomInvite.ParticipantType ?? SERoomParticipantType.Viewer) },
                        cancellationToken);
                    var roomParticipant = participants.First();

                    logger.LogInformation(
                        "Created participant room [id -> {participantId}, type -> {participantType}] for room [id -> {roomId}] and user [id -> {userId}]",
                        roomParticipant.Id,
                        roomParticipant.Type,
                        roomInvite.RoomId,
                        userId);

                    await UpdateInviteLimit(roomInvite, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);

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
                    await roomParticipantService.ChangeStatusAsync(roomParticipantChangeStatusRequest, cancellationToken);
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
        await db.RoomInvites
            .Where(roomInvite => roomInvite.RoomId == roomId && participantType == roomInvite.ParticipantType)
            .ExecuteDeleteAsync(cancellationToken);

        var invite = new Invite(inviteMaxCount);

        await db.Invites.AddAsync(invite, cancellationToken);

        var newRoomInvite = new RoomInvite(invite.Id, roomId, participantType);

        await db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

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
            logger.LogInformation("room invite [id -> {roomInviteId}] increment use count to {currentCount}", roomInvite.Id, roomInvite.Invite!.UsesCurrent);

            db.Invites.Update(roomInvite.Invite);

            await db.SaveChangesAsync(cancellationToken);

            return;
        }

        logger.LogInformation("generate new 5 invites for room [id -> {roomId}]", roomInvite.RoomId);

        var regenerateInvite = new Invite(5);

        logger.LogInformation("remove old invite for room [id -> {roomId}]", roomInvite.RoomId);

        db.Invites.Remove(roomInvite.Invite);

        logger.LogInformation("add new room invite for room [id -> {roomId}]", roomInvite.RoomId);

        await db.Invites.AddAsync(regenerateInvite, cancellationToken);

        var newRoomInvite = new RoomInvite(regenerateInvite, roomInvite.Room!, roomInvite.ParticipantType!);

        await db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        db.RoomInvites.Remove(roomInvite);

        await db.SaveChangesAsync(cancellationToken);
    }
}

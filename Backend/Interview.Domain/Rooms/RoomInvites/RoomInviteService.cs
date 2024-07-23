using Interview.Domain.Database;
using Interview.Domain.Invites;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomParticipants.Service;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.RoomInvites;

public class RoomInviteService : IRoomInviteService
{
    private readonly AppDbContext _db;
    private readonly IRoomParticipantService _roomParticipantService;

    public RoomInviteService(AppDbContext db, IRoomParticipantService roomParticipantService)
    {
        _db = db;
        _roomParticipantService = roomParticipantService;
    }

    public async Task<RoomInviteResponse> ApplyInvite(
        Guid inviteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var databaseContextTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var invite = await _db.Invites.Where(invite => invite.Id == inviteId)
                .FirstOrDefaultAsync(cancellationToken);

            if (invite is null)
            {
                throw NotFoundException.Create<Invite>(inviteId);
            }

            if (invite.UsesCurrent >= invite.UsesMax)
            {
                throw new UserException("The invitation has already been used");
            }

            var roomInvite = await _db.RoomInvites
                .Where(roomInviteItem => roomInviteItem.InviteById == inviteId)
                .FirstOrDefaultAsync(cancellationToken);

            if (roomInvite is null)
            {
                throw new NotFoundException("Invite not found for any rooms");
            }

            if (roomInvite.Room is null)
            {
                throw new Exception("The invitation no longer belongs to the room");
            }

            var user = await _db.Users.Where(user => user.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                throw new NotFoundException("The current user was not found");
            }

            var participant = await _db.RoomParticipants
                .Include(e => e.Room)
                .Where(participant => participant.User.Id == userId && participant.Room.Id == roomInvite.RoomById)
                .FirstOrDefaultAsync(cancellationToken);

            if (participant is null)
            {
                var participants = await _roomParticipantService.CreateAsync(
                    roomInvite.Room.Id,
                    new[] { (user, roomInvite.Room, roomInvite.ParticipantType ?? SERoomParticipantType.Viewer) },
                    cancellationToken);
                var roomParticipant = participants.First();

                await UpdateInviteLimit(roomInvite, cancellationToken);
                await _db.RoomParticipants.AddAsync(roomParticipant, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                await databaseContextTransaction.CommitAsync(cancellationToken);

                return new RoomInviteResponse
                {
                    InviteId = invite.Id,
                    ParticipantType = roomInvite.ParticipantType!.EnumValue,
                    Used = invite.UsesCurrent,
                    Max = invite.UsesMax,
                };
            }

            // await UpdateInviteLimit(roomInvite, cancellationToken);
            await databaseContextTransaction.CommitAsync(cancellationToken);

            return new RoomInviteResponse
            {
                InviteId = invite.Id,
                ParticipantType = roomInvite.ParticipantType!.EnumValue,
                Used = invite.UsesCurrent,
                Max = invite.UsesMax,
            };
        }
        catch
        {
            await databaseContextTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<RoomInviteResponse> GenerateAsync(
        Guid roomId,
        SERoomParticipantType participantType,
        int inviteMaxCount,
        CancellationToken cancellationToken)
    {
        await _db.RoomInvites
            .Where(roomInvite => roomInvite.RoomById == roomId && participantType == roomInvite.ParticipantType)
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
            _db.Invites.Update(roomInvite.Invite);

            await _db.SaveChangesAsync(cancellationToken);

            return;
        }

        var regenerateInvite = new Invite(5);

        _db.Invites.Remove(roomInvite.Invite);

        await _db.Invites.AddAsync(regenerateInvite, cancellationToken);

        var newRoomInvite = new RoomInvite(regenerateInvite, roomInvite.Room!, roomInvite.ParticipantType!);

        await _db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        _db.RoomInvites.Remove(roomInvite);

        await _db.SaveChangesAsync(cancellationToken);
    }
}

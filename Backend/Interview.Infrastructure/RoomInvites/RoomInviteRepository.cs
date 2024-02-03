using System.Runtime.InteropServices.ComTypes;
using System.Transactions;
using Interview.Domain;
using Interview.Domain.Invites;
using Interview.Domain.RoomInvites;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Service.Records.Response.Detail;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.RoomInvites;

public class RoomInviteRepository : EfRepository<RoomInvite>, IRoomInviteRepository
{
    public RoomInviteRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<RoomInviteDetail> ApplyInvite(
        Guid inviteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var databaseContextTransaction = await Db.Database.BeginTransactionAsync(cancellationToken);

        var invite = await Db.Invites.Where(invite => invite.Id == inviteId).FirstOrDefaultAsync(cancellationToken);

        if (invite is null)
        {
            throw NotFoundException.Create<Invite>(inviteId);
        }

        if (invite.UsesCurrent >= invite.UsesMax)
        {
            throw new UserException("The invitation has already been used");
        }

        var roomInvite = await Db.RoomInvites
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

        var user = await Db.Users.Where(user => user.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The current user was not found");
        }

        var participant = await Db.RoomParticipants
            .Where(participant => participant.User.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (participant is null)
        {
            var roomParticipant = new RoomParticipant(user, roomInvite.Room, roomInvite.ParticipantType!);

            await UpdateInviteLimit(roomInvite, cancellationToken);

            await Db.RoomParticipants.AddAsync(roomParticipant, cancellationToken);

            await Db.SaveChangesAsync(cancellationToken);

            await databaseContextTransaction.CommitAsync(cancellationToken);

            return new RoomInviteDetail
            {
                ParticipantId = roomParticipant.Id,
                ParticipantType = roomParticipant.Type,
                RoomId = roomInvite.Room.Id,
            };
        }

        // await UpdateInviteLimit(roomInvite, cancellationToken);
        await databaseContextTransaction.CommitAsync(cancellationToken);

        return new RoomInviteDetail
        {
            ParticipantId = participant.Id, ParticipantType = participant.Type, RoomId = roomInvite.Room.Id,
        };
    }

    private async Task UpdateInviteLimit(RoomInvite roomInvite, CancellationToken cancellationToken = default)
    {
        roomInvite.Invite!.UsesCurrent += 1;

        if (roomInvite.Invite!.UsesCurrent < roomInvite.Invite!.UsesMax)
        {
            Db.Invites.Update(roomInvite.Invite);

            await Db.SaveChangesAsync(cancellationToken);

            return;
        }

        var regenerateInvite = new Invite(5);

        Db.Invites.Remove(roomInvite.Invite);

        await Db.Invites.AddAsync(regenerateInvite, cancellationToken);

        var newRoomInvite = new RoomInvite(regenerateInvite, roomInvite.Room!, roomInvite.ParticipantType!);

        await Db.RoomInvites.AddAsync(newRoomInvite, cancellationToken);

        Db.RoomInvites.Remove(roomInvite);

        await Db.SaveChangesAsync(cancellationToken);
    }
}

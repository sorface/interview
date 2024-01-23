using Interview.Domain;
using Interview.Domain.Invites;
using Interview.Domain.RoomInvites;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public class RoomInviteRepository : EfRepository<RoomInvite>, IRoomInviteRepository
{
    public RoomInviteRepository(AppDbContext db)
    : base(db)
    {
    }

    public async Task<RoomInvite> FindFirstByInviteId(Guid inviteId, CancellationToken cancellationToken = default)
    {
        var transcription = await Db.Database.BeginTransactionAsync(cancellationToken);

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
            .Where(roomInviteItem => roomInviteItem.InviteById == inviteId && roomInviteItem.Invite != null && roomInviteItem.Invite.UsesCurrent < roomInviteItem.Invite.UsesMax)
            .FirstOrDefaultAsync(cancellationToken);

        if (roomInvite is null)
        {
            throw NotFoundException.Create<RoomInvite>(inviteId);
        }

        await transcription.CommitAsync(cancellationToken);

        return roomInvite;
    }
}
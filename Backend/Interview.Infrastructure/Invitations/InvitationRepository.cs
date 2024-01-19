using System.Data;
using Interview.Domain;
using Interview.Domain.Invitations;
using Interview.Domain.Invitations.Records.Response;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace Interview.Infrastructure.Invitations;

public class InvitationRepository : EfRepository<Invitation>, IInvitationRepository
{
    public InvitationRepository(AppDbContext db)
        : base(db)
    {
    }

    public async Task<Invitation?> CreateAsync(string hash, CancellationToken cancellationToken = default)
    {
        var invitation = new Invitation { Hash = hash, };

        Set.Add(invitation);

        await Db.SaveChangesAsync(cancellationToken);

        return invitation;
    }

    public async Task<Invitation?> RegenerationAsync(
        Guid id,
        string hash,
        CancellationToken cancellationToken = default)
    {
        var transaction = await Db.Database.BeginTransactionAsync(cancellationToken);

        var invitation = await FindByIdAsync(id, cancellationToken);

        if (invitation is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw NotFoundException.Create<Invitation>(id);
        }

        invitation.Hash = hash;

        await transaction.CommitAsync(cancellationToken);

        return invitation;
    }
}

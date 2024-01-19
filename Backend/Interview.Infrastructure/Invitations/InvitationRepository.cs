using System.Data;
using Interview.Domain;
using Interview.Domain.Invitations;
using Interview.Domain.Invitations.Records.Response;
using Interview.Domain.Repository;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;
using NSpecifications;

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

    public async Task<InvitationItem?> RegenerationAsync(
        Guid id,
        string hash,
        IMapper<Invitation, InvitationItem> mapper,
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

        return mapper.Map(invitation);
    }
}

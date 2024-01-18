using Interview.Domain.Invitations;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Invitations;

public class InvitationRepository : EfRepository<Invitation>, IInvitationRepository
{
    public InvitationRepository(AppDbContext db)
        : base(db)
    {
    }
}

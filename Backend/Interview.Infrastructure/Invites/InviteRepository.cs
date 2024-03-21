using Interview.Domain.Database;
using Interview.Domain.Invites;

namespace Interview.Infrastructure.Invites;

public class InviteRepository : EfRepository<Invite>, IInviteRepository
{
    public InviteRepository(AppDbContext db)
        : base(db)
    {
    }
}

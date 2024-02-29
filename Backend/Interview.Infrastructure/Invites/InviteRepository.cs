using Interview.Domain.Invites;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Invites;

public class InviteRepository : EfRepository<Invite>, IInviteRepository
{
    public InviteRepository(AppDbContext db)
        : base(db)
    {
    }
}

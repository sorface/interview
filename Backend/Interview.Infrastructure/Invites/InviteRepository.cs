using Interview.Domain.Database;
using Interview.Domain.Invites;

namespace Interview.Infrastructure.Invites;

public class InviteRepository(AppDbContext db) : EfRepository<Invite>(db), IInviteRepository;

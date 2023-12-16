using Interview.Domain.Events;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Events;

public class DbRoomEventRepository : EfRepository<DbRoomEvent>, IDbRoomEventRepository
{
    public DbRoomEventRepository(AppDbContext db)
        : base(db)
    {
    }
}

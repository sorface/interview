using Interview.Domain.Database;
using Interview.Domain.Events;

namespace Interview.Infrastructure.Events;

public class DbRoomEventRepository : EfRepository<DbRoomEvent>, IDbRoomEventRepository
{
    public DbRoomEventRepository(AppDbContext db)
        : base(db)
    {
    }
}

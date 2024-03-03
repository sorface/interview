using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Rooms;

public class RoomStateRepository : EfRepository<RoomState>, IRoomStateRepository
{
    public RoomStateRepository(AppDbContext db)
        : base(db)
    {
    }

    protected override IQueryable<RoomState> ApplyIncludes(DbSet<RoomState> set)
        => set.Include(e => e.Room);
}

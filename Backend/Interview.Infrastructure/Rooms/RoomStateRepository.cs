using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Rooms;

public class RoomStateRepository(AppDbContext db) : EfRepository<RoomState>(db), IRoomStateRepository
{
    protected override IQueryable<RoomState> ApplyIncludes(DbSet<RoomState> set)
        => set.Include(e => e.Room);
}

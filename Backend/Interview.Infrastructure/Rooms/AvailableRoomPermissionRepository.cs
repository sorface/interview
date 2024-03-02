using Interview.Domain.Rooms;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Rooms;

public class AvailableRoomPermissionRepository : EfRepository<AvailableRoomPermission>, IAvailableRoomPermissionRepository
{
    public AvailableRoomPermissionRepository(AppDbContext db)
        : base(db)
    {
    }
}

using Interview.Domain.Database;
using Interview.Domain.Rooms;

namespace Interview.Infrastructure.Rooms;

public class AvailableRoomPermissionRepository : EfRepository<AvailableRoomPermission>, IAvailableRoomPermissionRepository
{
    public AvailableRoomPermissionRepository(AppDbContext db)
        : base(db)
    {
    }
}

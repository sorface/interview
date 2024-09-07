using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.RoomParticipants;

public class RoomParticipantRepository : EfRepository<RoomParticipant>, IRoomParticipantRepository
{
    public RoomParticipantRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<RoomParticipant?> FindByRoomIdAndUserIdDetailedAsync(
        Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .Where(participant => participant.Room.Id == roomId && participant.User.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsExistsByRoomIdAndUserIdAsync(
        Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .AnyAsync(participant => participant.Room.Id == roomId && participant.User.Id == userId, cancellationToken);
    }

    protected override IQueryable<RoomParticipant> ApplyIncludes(DbSet<RoomParticipant> set) => set
        .Include(participant => participant.Room)
        .Include(participant => participant.User)
        .Include(participant => participant.Review)
        .Include(participant => participant.Permissions);
}

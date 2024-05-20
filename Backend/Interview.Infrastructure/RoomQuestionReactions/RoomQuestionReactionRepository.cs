using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.RoomQuestionReactions;

public class RoomQuestionReactionRepository : EfRepository<RoomQuestionReaction>, IRoomQuestionReactionRepository
{
    public RoomQuestionReactionRepository(AppDbContext db)
        : base(db)
    {
    }

    protected override IQueryable<RoomQuestionReaction> ApplyIncludes(DbSet<RoomQuestionReaction> set) => Set
        .Include(roomQuestionReaction => roomQuestionReaction.Reaction)
        .Include(roomQuestionReaction => roomQuestionReaction.RoomQuestion)
            .ThenInclude(roomQuestion => roomQuestion!.Room);
}

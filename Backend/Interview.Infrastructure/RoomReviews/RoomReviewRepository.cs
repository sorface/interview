using Interview.Domain.Database;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Rooms.RoomReviews.Response.Page;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Infrastructure.RoomReviews;

public class RoomReviewRepository : EfRepository<RoomReview>, IRoomReviewRepository
{
    public RoomReviewRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<IPagedList<RoomReviewPageDetail>> GetDetailedPageAsync(ISpecification<RoomReview> specification,
                                                                       int pageNumber,
                                                                       int pageSize,
                                                                       CancellationToken cancellationToken = default)
    {
        return Set
            .AsNoTracking()
            .Include(e => e.Participant)
            .Where(specification.Expression)
            .Select(e => new RoomReviewPageDetail
            {
                Id = e.Id,
                RoomId = e.Participant.RoomId,
                User = new RoomUserDetail { Id = e.Participant.User.Id, Nickname = e.Participant.User.Nickname, Type = null, Avatar = e.Participant.User.Avatar },
                Review = e.Review,
                State = e.State.EnumValue,
            })
            .OrderBy(e => e.Id)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<RoomReview?> FindByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).FirstOrDefaultAsync(review => review.Id == participantId, cancellationToken);
    }

    protected override IQueryable<RoomReview> ApplyIncludes(DbSet<RoomReview> set)
    {
        return set.Include(it => it.Participant)
            .Include(it => it.Participant.Room);
    }
}

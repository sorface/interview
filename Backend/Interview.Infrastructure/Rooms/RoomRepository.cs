using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Tags.Records.Response;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Infrastructure.Rooms;

public class RoomRepository : EfRepository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<bool> HasUserAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        return Set
            .Include(room => room.Participants)
            .AnyAsync(
                room => room.Id == roomId && room.Participants.Any(participant => participant.User.Id == userId),
                cancellationToken);
    }

    public Task<RoomParticipant?> FindParticipantOrDefaultAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Db.RoomParticipants.FirstOrDefaultAsync(
            roomParticipant => roomParticipant.Room.Id == roomId && roomParticipant.User.Id == userId,
            cancellationToken);
    }

    public Task<IPagedList<RoomPageDetail>> GetDetailedPageAsync(
        RoomPageDetailRequestFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Room> queryable = Set
            .Include(e => e.Participants).ThenInclude(e => e.User)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.Tags)
            .OrderBy(e => e.Status == SERoomStatus.Active ? 1 :
                e.Status == SERoomStatus.Review ? 2 :
                e.Status == SERoomStatus.New ? 3 :
                4)
            .ThenByDescending(e => e.CreateDate);
        var filterName = filter.Name?.Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(filterName))
        {
            queryable = queryable.Where(e => e.Name.ToLower().Contains(filterName));
        }

        if (filter.Statuses is not null && filter.Statuses.Count > 0)
        {
            var mapStatuses = filter.Statuses.Join(
                SERoomStatus.List,
                status => status,
                status => status.EnumValue,
                (_, roomStatus) => roomStatus).ToList();
            queryable = queryable.Where(e => mapStatuses.Contains(e.Status));
        }

        if (filter.Participants is not null && filter.Participants.Count > 0)
        {
            queryable = queryable.Where(e => e.Participants.Any(p => filter.Participants.Contains(p.User.Id)));
        }

        return queryable
            .Select(e => new RoomPageDetail
            {
                Id = e.Id,
                Name = e.Name,
                Questions = e.Questions.OrderBy(rq => rq.Order)
                    .Select(question => new RoomQuestionDetail { Id = question.Question!.Id, Value = question.Question.Value, Order = question.Order, })
                    .ToList(),
                Participants = e.Participants.Select(participant =>
                        new RoomUserDetail
                        {
                            Id = participant.User.Id,
                            Nickname = participant.User.Nickname,
                            Avatar = participant.User.Avatar,
                        })
                    .ToList(),
                RoomStatus = e.Status.EnumValue,
                Tags = e.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
            })
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    protected override IQueryable<Room> ApplyIncludes(DbSet<Room> set)
        => Set
            .Include(e => e.Tags)
            .Include(e => e.Participants)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.RoomStates);
}

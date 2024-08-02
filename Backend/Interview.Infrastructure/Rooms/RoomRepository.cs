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

    public async Task<AnalyticsSummary?> GetAnalyticsSummaryAsync(
        RoomAnalyticsRequest request,
        CancellationToken cancellationToken = default)
    {
        var questions = await Db.RoomQuestions.AsNoTracking()
            .Include(e => e.Question)
            .Include(e => e.Room)
            .Where(e => e.Room!.Id == request.RoomId)
            .Select(e => new { e.Question!.Id, e.Question!.Value })
            .ToListAsync(cancellationToken);

        var reactions = await Db.RoomQuestionReactions.AsNoTracking()
            .Include(e => e.RoomQuestion).ThenInclude(e => e!.Question)
            .Include(e => e.RoomQuestion).ThenInclude(e => e!.Room)
            .Include(e => e.Reaction)
            .Include(e => e.Sender)
            .Where(e => e.RoomQuestion!.Room!.Id == request.RoomId)
            .ToListAsync(cancellationToken);

        var participants = await Db.RoomParticipants.AsNoTracking()
            .Include(e => e.Room)
            .Include(e => e.User)
            .Where(e => e.Room.Id == request.RoomId)
            .ToDictionaryAsync(e => e.User.Id, e => e.Type, cancellationToken);

        var summary = new AnalyticsSummary { Questions = new List<AnalyticsSummaryQuestion>(questions.Count), };
        foreach (var question in questions)
        {
            var reactionQuestions = reactions
                .Where(e => e.RoomQuestion!.Question!.Id == question.Id)
                .ToList();

            var viewers = reactionQuestions
                .Where(e => participants[e.Sender!.Id] == SERoomParticipantType.Viewer)
                .GroupBy(e => participants[e.Sender!.Id])
                .Select(e => new AnalyticsSummaryViewer
                {
                    ReactionsSummary = e.GroupBy(t => (t.Reaction!.Id, t.Reaction.Type))
                        .Select(t => new Analytics.AnalyticsReactionSummary { Id = t.Key.Id, Type = t.Key.Type.Name, Count = t.Count(), })
                        .ToList(),
                })
                .ToList();

            var experts = reactionQuestions
                .Where(e => participants[e.Sender!.Id] == SERoomParticipantType.Expert)
                .GroupBy(e => (e.Sender!.Id, e.Sender!.Nickname))
                .Select(e => new AnalyticsSummaryExpert
                {
                    Nickname = e.Key.Nickname,
                    ReactionsSummary = e.GroupBy(t => (t.Reaction!.Id, t.Reaction.Type))
                        .Select(t => new Analytics.AnalyticsReactionSummary { Id = t.Key.Id, Type = t.Key.Type.Name, Count = t.Count(), })
                        .ToList(),
                })
                .ToList();

            var noReactions = viewers.Count == 0 && experts.Count == 0;
            if (noReactions)
            {
                continue;
            }

            summary.Questions.Add(new AnalyticsSummaryQuestion
            {
                Id = question.Id,
                Value = question.Value,
                Experts = experts,
                Viewers = viewers,
            });
        }

        return summary;
    }

    public async Task<Analytics?> GetAnalyticsAsync(
        RoomAnalyticsRequest request,
        CancellationToken cancellationToken = default)
    {
        var analytics = await GetAnalyticsCoreAsync(request.RoomId, cancellationToken);
        if (analytics == null)
        {
            return null;
        }

        foreach (var analyticsQuestion in analytics.Questions!)
        {
            analyticsQuestion.Users = await GetUsersByQuestionIdAsync(analyticsQuestion.Id, cancellationToken);
        }

        return analytics;

        Task<Analytics?> GetAnalyticsCoreAsync(Guid roomId, CancellationToken ct)
        {
            return Set.AsNoTracking()
                .Include(e => e.Questions).ThenInclude(e => e.Question)
                .Include(e => e.Participants)
                .Where(e => e.Id == roomId)
                .Select(e => new Analytics
                {
                    Questions = e.Questions.OrderBy(rq => rq.Order).Select(q => new Analytics.AnalyticsQuestion
                    {
                        Id = q.Question!.Id,
                        Status = q.State!.Name,
                        Value = q.Question.Value,
                        Users = null,
                    }).ToList(),
                })
                .FirstOrDefaultAsync(ct);
        }

        Task<List<Analytics.AnalyticsUser>> GetUsersByQuestionIdAsync(Guid questionId, CancellationToken ct)
        {
            return Db.RoomParticipants.AsNoTracking()
                .Include(e => e.Room)
                .Include(e => e.User).ThenInclude(e =>
                    e.RoomQuestionEvaluations.Where(rqe => rqe.RoomQuestion!.RoomId == request.RoomId && rqe.RoomQuestion!.QuestionId == questionId))
                .Where(e => e.Room.Id == request.RoomId)
                .Select(e => new Analytics.AnalyticsUser
                {
                    Id = e.User.Id,
                    Avatar = string.Empty,
                    Nickname = e.User.Nickname,
                    ParticipantType = e.Type.Name ?? string.Empty,
                    Evaluation = e.User.RoomQuestionEvaluations
                        .Where(rqe => rqe.RoomQuestion!.RoomId == request.RoomId && rqe.RoomQuestion!.QuestionId == questionId)
                        .Select(rqe => new Analytics.AnalyticsUserQuestionEvaluation { Review = rqe.Review, Mark = rqe.Mark, })
                        .FirstOrDefault(),
                })
                .ToListAsync(ct);
        }
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
                        new RoomUserDetail { Id = participant.User.Id, Nickname = participant.User.Nickname, Avatar = participant.User.Avatar, })
                    .ToList(),
                RoomStatus = e.Status.EnumValue,
                Tags = e.Tags.Select(t => new TagItem { Id = t.Id, Value = t.Value, HexValue = t.HexColor, }).ToList(),
            })
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<RoomDetail?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var res = await Set
            .Include(e => e.Participants)
            .Include(e => e.Configuration)
            .Include(e => e.Timer)
            .Select(e => new
            {
                Id = e.Id,
                Name = e.Name,
                Owner = new RoomUserDetail { Id = e.CreatedBy!.Id, Nickname = e.CreatedBy!.Nickname, Avatar = e.CreatedBy!.Avatar, },
                Participants = e.Participants.Select(participant =>
                        new RoomUserDetail
                        {
                            Id = participant.User.Id,
                            Nickname = participant.User.Nickname,
                            Avatar = participant.User.Avatar,
                            Type = participant.Type.Name,
                        })
                    .ToList(),
                Status = e.Status.EnumValue,
                Invites = e.Invites.Select(roomInvite => new RoomInviteResponse()
                {
                    InviteId = roomInvite.InviteById!.Value,
                    ParticipantType = roomInvite.ParticipantType!.EnumValue,
                    Max = roomInvite.Invite!.UsesMax,
                    Used = roomInvite.Invite.UsesCurrent,
                })
                    .ToList(),
                Type = e.AccessType.EnumValue,
                Timer = e.Timer == null ? null : new { Duration = e.Timer.Duration, ActualStartTime = e.Timer.ActualStartTime, },
                ScheduledStartTime = e.ScheduleStartTime,
            })
            .FirstOrDefaultAsync(room => room.Id == roomId, cancellationToken: cancellationToken);
        if (res is null)
        {
            return null;
        }

        return new RoomDetail
        {
            Id = res.Id,
            Name = res.Name,
            Owner = res.Owner,
            Participants = res.Participants,
            Status = res.Status,
            Invites = res.Invites,
            Type = res.Type,
            Timer = res.Timer == null ? null : new RoomTimerDetail { DurationSec = (long)res.Timer.Duration.TotalSeconds, StartTime = res.Timer.ActualStartTime },
            ScheduledStartTime = res.ScheduledStartTime,
        };
    }

    protected override IQueryable<Room> ApplyIncludes(DbSet<Room> set)
        => Set
            .Include(e => e.Tags)
            .Include(e => e.Participants)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.RoomStates);
}

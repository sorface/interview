using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionReactions;
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
                        .Select(t => new Analytics.AnalyticsReactionSummary
                        {
                            Id = t.Key.Id,
                            Type = t.Key.Type.Name,
                            Count = t.Count(),
                        })
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
                        .Select(t => new Analytics.AnalyticsReactionSummary
                        {
                            Id = t.Key.Id,
                            Type = t.Key.Type.Name,
                            Count = t.Count(),
                        })
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

        var reactions = await GetRoomQuestionReactionAsync(request.RoomId, cancellationToken);

        analytics.Reactions = GetReactions(reactions);

        var questionReaction = reactions.ToLookup(e => e.RoomQuestion!.Question!.Id);
        foreach (var analyticsQuestion in analytics.Questions!)
        {
            if (!questionReaction[analyticsQuestion.Id].Any())
            {
                continue;
            }

            analyticsQuestion.Users = await GetUsersAsync(questionReaction[analyticsQuestion.Id]);
        }

        return analytics;

        Task<List<RoomQuestionReaction>> GetRoomQuestionReactionAsync(Guid roomId, CancellationToken ct)
        {
            return Db.RoomQuestionReactions.AsNoTracking()
                .Include(e => e.Sender)
                .Include(e => e.Reaction)
                .Include(e => e.RoomQuestion)
                .ThenInclude(e => e!.Question)
                .Where(e => e.RoomQuestion!.Room!.Id == roomId)
                .ToListAsync(ct);
        }

        Task<Analytics?> GetAnalyticsCoreAsync(Guid roomId, CancellationToken ct)
        {
            return Set.AsNoTracking()
                .Include(e => e.Questions)
                .ThenInclude(e => e.Question)
                .Include(e => e.Participants)
                .Where(e => e.Id == roomId)
                .Select(e => new Analytics
                {
                    Questions = e.Questions.Select(q => new Analytics.AnalyticsQuestion
                    {
                        Id = q.Question!.Id,
                        Status = q.State!.Name,
                        Value = q.Question.Value,
                    }).ToList(),
                })
                .FirstOrDefaultAsync(ct);
        }

        List<Analytics.AnalyticsReactionSummary> GetReactions(List<RoomQuestionReaction> roomQuestionReactions)
        {
            return roomQuestionReactions
                .Select(e => e.Reaction)
                .GroupBy(e => (e!.Id, e.Type))
                .Select(e => new Analytics.AnalyticsReactionSummary
                {
                    Id = e.Key.Id,
                    Count = e.Count(),
                    Type = e.Key.Type.Name,
                })
                .ToList();
        }

        async Task<List<Analytics.AnalyticsUser>> GetUsersAsync(IEnumerable<RoomQuestionReaction> roomReactions)
        {
            var users = reactions.Select(e => e.Sender!.Id).Distinct();

            var participants = await Db.RoomParticipants.AsNoTracking()
                .Include(e => e.Room)
                .Include(e => e.User)
                .Where(e => e.Room.Id == request.RoomId && users.Contains(e.User.Id))
                .ToDictionaryAsync(e => e.User.Id, cancellationToken);

            return roomReactions
                .GroupBy(e => e.Sender!.Id).Select(e =>
                {
                    var sender = e.First().Sender;
                    participants.TryGetValue(sender!.Id, out var participant);
                    return new Analytics.AnalyticsUser
                    {
                        Id = sender.Id,
                        Avatar = string.Empty,
                        Nickname = sender.Nickname,
                        ParticipantType = participant?.Type.Name ?? string.Empty,
                        Reactions = ToAnalyticsReaction(e),
                        ReactionsSummary = ToAnalyticsReactionSummary(e),
                    };
                }).ToList();
        }

        static List<Analytics.AnalyticsReaction> ToAnalyticsReaction(IGrouping<Guid, RoomQuestionReaction> e)
        {
            return e.Select(roomQuestionReaction => new Analytics.AnalyticsReaction
            {
                Id = roomQuestionReaction.Reaction!.Id,
                Type = roomQuestionReaction.Reaction.Type.Name,
                CreatedAt = roomQuestionReaction.CreateDate,
            }).ToList();
        }

        static List<Analytics.AnalyticsReactionSummary> ToAnalyticsReactionSummary(
            IGrouping<Guid, RoomQuestionReaction> e)
        {
            return e.GroupBy(roomQuestionReaction =>
                    (roomQuestionReaction.Reaction!.Id, roomQuestionReaction.Reaction.Type))
                .Select(roomQuestionReactions => new Analytics.AnalyticsReactionSummary
                {
                    Id = roomQuestionReactions.Key.Id,
                    Count = roomQuestionReactions.Count(),
                    Type = roomQuestionReactions.Key.Type.Name,
                }).ToList();
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
            .Include(e => e.Participants)
            .ThenInclude(e => e.User)
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
                Questions = e.Questions.Select(question => question.Question)
                    .Select(question => new RoomQuestionDetail { Id = question!.Id, Value = question.Value, })
                    .ToList(),
                Users = e.Participants.Select(participant =>
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

    public Task<RoomDetail?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return Set
            .Include(e => e.Participants)
            .Include(e => e.Configuration)
            .Include(e => e.Timer)
            .Select(e => new RoomDetail
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
                        })
                    .ToList(),
                Status = e.Status.EnumValue,
                Invites = e.Invites.Select(roomInvite => new RoomInviteResponse()
                {
                    InviteId = roomInvite.InviteById!.Value,
                    ParticipantType = roomInvite.ParticipantType!.EnumValue,
                    Max = roomInvite.Invite!.UsesMax,
                    Used = roomInvite.Invite.UsesCurrent,
                }).ToList(),
                Type = e.AccessType.EnumValue,
                Timer = e.Timer == null ? null : new RoomTimerDetail { DurationSec = (long)e.Timer.Duration.TotalSeconds, StartTime = e.Timer.ActualStartTime, },
            })
            .FirstOrDefaultAsync(room => room.Id == roomId, cancellationToken: cancellationToken);
    }

    protected override IQueryable<Room> ApplyIncludes(DbSet<Room> set)
        => Set
            .Include(e => e.Tags)
            .Include(e => e.Participants)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.RoomStates);
}

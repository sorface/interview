using System.Collections.Immutable;
using Interview.Domain.Database;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.Records.Request;
using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Rooms.Records.Response.Page;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
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

    public Task<bool> IsReadyToCloseAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var userAttendedAtLeast1QuestionSpec = new Spec<RoomParticipant>(e => e.User.RoomQuestionEvaluations.Any(rq => rq.RoomQuestion!.RoomId == roomId));
        var roomExpertsSpec = new Spec<RoomParticipant>(participant => participant.RoomId == roomId && participant.Type == SERoomParticipantType.Expert);
        return Db.RoomParticipants
            .Include(participant => participant.Review)
            .Include(participant => participant.User)
            .ThenInclude(e => e.RoomQuestionEvaluations)
            .ThenInclude(e => e.RoomQuestion)
            .Where(roomExpertsSpec & userAttendedAtLeast1QuestionSpec)
            .AllAsync(participant => participant.Review != null && participant.Review.State == SERoomReviewState.Closed, cancellationToken);
    }

    protected override IQueryable<Room> ApplyIncludes(DbSet<Room> set)
        => Set
            .Include(e => e.Tags)
            .Include(e => e.Participants)
            .Include(e => e.Questions)
            .Include(e => e.Configuration)
            .Include(e => e.RoomStates);
}

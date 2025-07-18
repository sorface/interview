using System.Linq.Expressions;
using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

public class RoomQuestionEvaluationService(
    IRoomQuestionRepository roomQuestionRepository,
    IRoomParticipantRepository roomParticipantRepository,
    AppDbContext db,
    IRoomMembershipChecker membershipChecker)
    : IRoomQuestionEvaluationService
{
    public async Task<List<RoomQuestionEvaluationResponse>> GetUserRoomQuestionEvaluationsAsync(UserRoomQuestionEvaluationsRequest request, CancellationToken cancellationToken)
    {
        await membershipChecker.EnsureUserMemberOfRoomAsync(request.UserId, request.RoomId, cancellationToken);
        return await db.RoomQuestions
            .Include(e => e.Question)
            .Include(e => e.Evaluations)
            .Where(e => e.RoomId == request.RoomId && e.State == RoomQuestionState.Closed)
            .OrderBy(e => e.Order)
            .Select(e => new RoomQuestionEvaluationResponse
            {
                Id = e.Question!.Id,
                Value = e.Question!.Value,
                Order = e.Order,
                Evaluation = e.Evaluations
                    .Where(ev => ev.CreatedById == request.UserId)
                    .Select(ev => new QuestionEvaluationDetail
                    {
                        Id = ev.Id,
                        Mark = ev.Mark,
                        Review = ev.Review,
                    })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionEvaluationDetail> FindByRoomIdAndQuestionIdAsync(QuestionEvaluationGetRequest request, CancellationToken cancellationToken)
    {
        await ValidateQuestionEvaluation(request.RoomId, request.UserId, cancellationToken);
        var evaluation = await db.RoomQuestionEvaluation
            .Include(e1 => e1.RoomQuestion)
            .Where(e3 => e3.RoomQuestion!.QuestionId == request.QuestionId && e3.RoomQuestion!.RoomId == request.RoomId && e3.CreatedById == request.UserId)
            .Select((Expression<Func<RoomQuestionEvaluation, QuestionEvaluationDetail>>)(e => new QuestionEvaluationDetail
            {
                Id = e.Id,
                Mark = e.Mark != null ? e.Mark!.Value : null,
                Review = e.Review,
            }))
            .FirstOrDefaultAsync(cancellationToken);
        if (evaluation is null)
        {
            throw new NotFoundException($"Evaluation not found by question id [{request.QuestionId}] and room id [{request.RoomId}]");
        }

        return evaluation;
    }

    public async Task<QuestionEvaluationDetail> MergeAsync(QuestionEvaluationMergeRequest mergeRequest, CancellationToken cancellationToken)
    {
        await ValidateQuestionEvaluation(mergeRequest.RoomId, mergeRequest.UserId, cancellationToken);
        var questionEvaluation = await db.RoomQuestionEvaluation
            .Include(e1 => e1.RoomQuestion)
            .ThenInclude(e => e!.Room)
            .Where(e3 => e3.RoomQuestion!.QuestionId == mergeRequest.QuestionId && e3.RoomQuestion!.RoomId == mergeRequest.RoomId && e3.CreatedById == mergeRequest.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (questionEvaluation is null)
        {
            var roomQuestion = await roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(mergeRequest.QuestionId, mergeRequest.RoomId, cancellationToken);
            if (roomQuestion is null)
            {
                throw new NotFoundException("Not found question in the room");
            }

            questionEvaluation = new RoomQuestionEvaluation
            {
                RoomQuestion = roomQuestion,
                Mark = mergeRequest.Mark,
                Review = mergeRequest.Review,
                State = SERoomQuestionEvaluationState.Draft,
                RoomQuestionId = roomQuestion.Id,
            };
            await db.RoomQuestionEvaluation.AddAsync(questionEvaluation, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var roomStatus = questionEvaluation.RoomQuestion?.Room?.Status;
            if (roomStatus is not null && SERoomStatus.CloseStatuses.Contains(roomStatus))
            {
                throw new UserException("The user cannot change the evaluation because the room is closed.");
            }

            questionEvaluation.Review = mergeRequest.Review;
            questionEvaluation.Mark = mergeRequest.Mark;
            await db.SaveChangesAsync(cancellationToken);
        }

        return new QuestionEvaluationDetail { Id = questionEvaluation.Id, Mark = questionEvaluation.Mark, Review = questionEvaluation.Review, };
    }

    private async Task ValidateQuestionEvaluation(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var hasRoom = await db.Rooms.AnyAsync(e => e.Id == roomId, cancellationToken);
        if (!hasRoom)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var roomParticipant = await roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId, cancellationToken);
        if (roomParticipant is null)
        {
            throw new AccessDeniedException("The user is not a member of the room");
        }

        if (roomParticipant.Type != SERoomParticipantType.Expert)
        {
            throw new AccessDeniedException("The user is not an expert in the room");
        }
    }
}

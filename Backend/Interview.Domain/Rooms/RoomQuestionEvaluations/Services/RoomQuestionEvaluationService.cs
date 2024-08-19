using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

public class RoomQuestionEvaluationService : IRoomQuestionEvaluationService
{
    private readonly IRoomQuestionRepository _roomQuestionRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly AppDbContext _db;
    private readonly IRoomMembershipChecker _membershipChecker;

    public RoomQuestionEvaluationService(
        IRoomQuestionRepository roomQuestionRepository,
        IRoomParticipantRepository roomParticipantRepository,
        AppDbContext db,
        IRoomMembershipChecker membershipChecker)
    {
        _roomQuestionRepository = roomQuestionRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _db = db;
        _membershipChecker = membershipChecker;
    }

    public async Task<List<RoomQuestionEvaluationResponse>> GetUserRoomQuestionEvaluationsAsync(UserRoomQuestionEvaluationsRequest request, CancellationToken cancellationToken)
    {
        await _membershipChecker.EnsureUserMemberOfRoomAsync(request.UserId, request.RoomId, cancellationToken);
        return await _db.RoomQuestionEvaluation.AsNoTracking()
            .Include(e => e.RoomQuestion).ThenInclude(e => e!.Question)
            .Where(e => e.RoomQuestion!.RoomId == request.RoomId && e.CreatedById == request.UserId && e.State == SERoomQuestionEvaluationState.Submitted)
            .OrderBy(e => e.RoomQuestion!.Order)
            .Select(e => new RoomQuestionEvaluationResponse
            {
                Id = e.RoomQuestion!.Question!.Id,
                Value = e.RoomQuestion!.Question!.Value,
                Order = e.RoomQuestion!.Order,
                Evaluation = new QuestionEvaluationDetail
                {
                    Id = e.Id,
                    Mark = e.Mark,
                    Review = e.Review,
                },
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionEvaluationDetail> FindByRoomIdAndQuestionIdAsync(QuestionEvaluationGetRequest request, CancellationToken cancellationToken)
    {
        var evaluation = await FindQuestionEvaluation(request.RoomId, request.QuestionId, request.UserId, cancellationToken);
        if (evaluation is null)
        {
            throw new NotFoundException($@"Evaluation not found by question id [{request.QuestionId}] and room id [{request.RoomId}]");
        }

        return new QuestionEvaluationDetail { Id = evaluation.Id, Mark = evaluation.Mark != null ? evaluation.Mark!.Value : null, Review = evaluation.Review, };
    }

    public async Task<QuestionEvaluationDetail> MergeAsync(QuestionEvaluationMergeRequest mergeRequest, CancellationToken cancellationToken)
    {
        var questionEvaluation = await FindQuestionEvaluation(mergeRequest.RoomId, mergeRequest.QuestionId, mergeRequest.UserId, cancellationToken);
        if (questionEvaluation is null)
        {
            var roomQuestion = await _roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(mergeRequest.QuestionId, mergeRequest.RoomId, cancellationToken);
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
            await _db.RoomQuestionEvaluation.AddAsync(questionEvaluation, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (questionEvaluation.State == SERoomQuestionEvaluationState.Submitted)
            {
                throw new UserException("The evaluation cannot be changed, as it is approved");
            }

            questionEvaluation.Review = mergeRequest.Review;
            questionEvaluation.Mark = mergeRequest.Mark;
            _db.RoomQuestionEvaluation.Update(questionEvaluation);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new QuestionEvaluationDetail { Id = questionEvaluation.Id, Mark = questionEvaluation.Mark, Review = questionEvaluation.Review, };
    }

    private async Task<RoomQuestionEvaluation?> FindQuestionEvaluation(Guid roomId, Guid questionId, Guid userId, CancellationToken cancellationToken)
    {
        await ValidateQuestionEvaluation(roomId, userId, cancellationToken);
        return await _db.RoomQuestionEvaluation
            .Include(e => e.RoomQuestion)
            .Include(e => e.CreatedBy)
            .Where(e => e.RoomQuestion!.QuestionId == questionId && e.CreatedById == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateQuestionEvaluation(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var hasRoom = await _db.Rooms.AnyAsync(e => e.Id == roomId, cancellationToken);
        if (!hasRoom)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var roomParticipant = await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId, cancellationToken);
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

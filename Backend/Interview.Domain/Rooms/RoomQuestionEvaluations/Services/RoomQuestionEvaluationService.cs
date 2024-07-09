using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

public class RoomQuestionEvaluationService : IRoomQuestionEvaluationService
{
    private readonly IRoomQuestionRepository _roomQuestionRepository;

    private readonly IRoomQuestionEvaluationRepository _roomQuestionEvaluationRepository;

    private readonly IRoomRepository _roomRepository;

    private readonly IRoomParticipantRepository _roomParticipantRepository;

    public RoomQuestionEvaluationService(IRoomQuestionEvaluationRepository roomQuestionEvaluationRepository,
                                         IRoomQuestionRepository roomQuestionRepository,
                                         IRoomRepository roomRepository,
                                         IRoomParticipantRepository roomParticipantRepository)
    {
        _roomQuestionEvaluationRepository = roomQuestionEvaluationRepository;
        _roomQuestionRepository = roomQuestionRepository;
        _roomRepository = roomRepository;
        _roomParticipantRepository = roomParticipantRepository;
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

            await _roomQuestionEvaluationRepository.CreateAsync(questionEvaluation, cancellationToken);
        }
        else
        {
            if (questionEvaluation.State == SERoomQuestionEvaluationState.Submitted)
            {
                throw new UserException("The evaluation cannot be changed, as it is approved");
            }

            questionEvaluation.Review = mergeRequest.Review;
            questionEvaluation.Mark = mergeRequest.Mark;

            await _roomQuestionEvaluationRepository.UpdateAsync(questionEvaluation, cancellationToken);
        }

        return new QuestionEvaluationDetail { Id = questionEvaluation.Id, Mark = questionEvaluation.Mark, Review = questionEvaluation.Review, };
    }

    private async Task<RoomQuestionEvaluation?> FindQuestionEvaluation(Guid roomId, Guid questionId, Guid userId, CancellationToken cancellationToken)
    {
        await ValidateQuestionEvaluation(roomId, userId, cancellationToken);

        return await _roomQuestionEvaluationRepository.FindByQuestionIdAndRoomAsync(roomId, questionId, userId, cancellationToken);
    }

    private async Task ValidateQuestionEvaluation(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.FindByIdAsync(roomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var roomParticipant = await _roomParticipantRepository.FindByRoomIdAndUserIdDetailedAsync(roomId, userId, cancellationToken);

        if (roomParticipant is null)
        {
            throw new UserException("The user is not a member of the room");
        }

        if (roomParticipant.Type != SERoomParticipantType.Expert)
        {
            throw new UserException("The user is not an expert in the room");
        }
    }
}

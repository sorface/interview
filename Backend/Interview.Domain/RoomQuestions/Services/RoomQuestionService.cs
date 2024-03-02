using Interview.Domain.Questions;
using Interview.Domain.Questions.Services;
using Interview.Domain.Repository;
using Interview.Domain.RoomQuestions.Records;
using Interview.Domain.RoomQuestions.Records.Response;
using Interview.Domain.Rooms;
using NSpecifications;

namespace Interview.Domain.RoomQuestions.Services;

public class RoomQuestionService : IRoomQuestionService
{
    private readonly IRoomQuestionRepository _roomQuestionRepository;

    private readonly IQuestionRepository _questionRepository;

    private readonly IRoomRepository _roomRepository;

    private readonly IQuestionService _questionService;

    public RoomQuestionService(
        IRoomQuestionRepository roomQuestionRepository,
        IRoomRepository roomRepository,
        IQuestionRepository questionRepository,
        IQuestionService questionService)
    {
        _roomQuestionRepository = roomQuestionRepository;
        _roomRepository = roomRepository;
        _questionRepository = questionRepository;
        _questionService = questionService;
    }

    public async Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request, CancellationToken cancellationToken = default)
    {
        var roomQuestion = await _roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(
            request.QuestionId, request.RoomId, cancellationToken);

        if (roomQuestion is null)
        {
            throw new NotFoundException($"Question in room not found by id {request.QuestionId}");
        }

        if (roomQuestion.State == RoomQuestionState.Active)
        {
            throw new UserException("Question already has active state");
        }

        var specification = new Spec<Room>(r => r.Id == request.RoomId && r.Status == SERoomStatus.New);

        var room = await _roomRepository.FindFirstOrDefaultAsync(specification, cancellationToken);

        if (room is not null)
        {
            room.Status = SERoomStatus.Active;
            await _roomRepository.UpdateAsync(room, cancellationToken);
        }

        await _roomQuestionRepository.CloseActiveQuestionAsync(request.RoomId, cancellationToken);

        roomQuestion.State = RoomQuestionState.Active;

        await _roomQuestionRepository.UpdateAsync(roomQuestion, cancellationToken);

        return new RoomQuestionDetail
        {
            Id = roomQuestion.Id,
            RoomId = roomQuestion.Room!.Id,
            QuestionId = roomQuestion.Question!.Id,
            State = roomQuestion.State,
        };
    }

    /// <summary>
    /// Adding a question to a room.
    /// </summary>
    /// <param name="request">Request with data to add a question to the room.</param>
    /// <param name="cancellationToken">Task cancellation token.</param>
    /// <returns>The data of the new entry about the participant of the room.</returns>
    public async Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken)
    {
        Guid questionId;
        if (request.QuestionId is not null)
        {
            var roomQuestion = await _roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(
                request.QuestionId.Value, request.RoomId, cancellationToken);

            if (roomQuestion is not null)
            {
                throw new UserException($"The room {request.RoomId} with question {request.QuestionId} already exists");
            }

            questionId = request.QuestionId.Value;
        }
        else
        {
            if (request.Question is null)
            {
                throw new UserException("The expectation was to get data to create a question");
            }

            var createdQuestion = await _questionService.CreateAsync(request.Question, request.RoomId, cancellationToken);
            questionId = createdQuestion.Id;
        }

        var room = await _roomRepository.FindByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var question = await _questionRepository.FindByIdAsync(questionId, cancellationToken);

        if (question is null)
        {
            throw NotFoundException.Create<Question>(questionId);
        }

        var newRoomQuestion = new RoomQuestion { Room = room, Question = question, State = RoomQuestionState.Open };

        await _roomQuestionRepository.CreateAsync(newRoomQuestion, cancellationToken);

        return new RoomQuestionDetail
        {
            Id = newRoomQuestion.Id,
            QuestionId = question.Id,
            RoomId = room.Id,
            State = newRoomQuestion.State,
        };
    }

    public async Task<List<RoomQuestionResponse>> FindQuestionsAsync(
        RoomQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        var hasRoom =
            await _roomRepository.HasAsync(new Spec<Room>(room => room.Id == request.RoomId), cancellationToken);

        if (hasRoom is false)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var states = request.States.Select(e => RoomQuestionState.FromValue((int)e)).ToList();

        var specification = new Spec<RoomQuestion>(rq => rq.Room.Id == request.RoomId && states.Contains(rq.State));

        var mapper = Mapper<RoomQuestion>.Create(rq => new { Id = rq.Question.Id, State = rq.State, Value = rq.Question.Value, });
        var questions = await _roomQuestionRepository.FindAsync(specification, mapper, cancellationToken);
        return questions.ConvertAll(e => new RoomQuestionResponse
        {
            Id = e.Id,
            State = e.State.EnumValue,
            Value = e.Value,
        });
    }
}

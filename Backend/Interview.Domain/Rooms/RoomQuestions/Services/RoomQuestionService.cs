using System.Runtime.CompilerServices;
using System.Text.Json;
using Interview.Domain.Database;
using Interview.Domain.Events;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Events.Storage;
using Interview.Domain.Questions;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.Services;
using Interview.Domain.Rooms.RoomQuestions.AnswerDetail;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services.Update;
using Interview.Domain.ServiceResults.Success;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using NSpecifications;
using QuestionCodeEditorResponse = Interview.Domain.Rooms.Records.Response.Detail.QuestionCodeEditorResponse;

namespace Interview.Domain.Rooms.RoomQuestions.Services;

public class RoomQuestionService : IRoomQuestionService
{
    private readonly IRoomQuestionRepository _roomQuestionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IQuestionService _questionService;
    private readonly AppDbContext _db;
    private readonly IEventStorage _eventStorage;
    private readonly ISystemClock _clock;

    public RoomQuestionService(
        IRoomQuestionRepository roomQuestionRepository,
        IRoomRepository roomRepository,
        IQuestionRepository questionRepository,
        IQuestionService questionService,
        AppDbContext db,
        IEventStorage eventStorage,
        ISystemClock clock)
    {
        _roomQuestionRepository = roomQuestionRepository;
        _roomRepository = roomRepository;
        _questionRepository = questionRepository;
        _questionService = questionService;
        _db = db;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
    {
        const int DefaultChunkSize = 500;
        var roomQuestion = await _db.RoomQuestions
            .Include(e => e.Question)
                .ThenInclude(e => e!.CodeEditor)
            .Where(e => e.RoomId == request.RoomId && e.QuestionId == request.QuestionId)
            .Select(e => new RoomQuestionAnswerDetailResponse
            {
                CodeEditor = e.Question!.CodeEditor == null
                    ? null
                    : new QuestionCodeEditorResponse
                    {
                        Content = e.Question!.CodeEditor!.Content,
                        Lang = e.Question!.CodeEditor!.Lang,
                    },
                Details = new List<RoomQuestionAnswerDetailResponse.Detail>(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (roomQuestion == null)
        {
            throw NotFoundException.Create<RoomQuestion>((request.RoomId, "RoomId"), (request.QuestionId, "QuestionId"));
        }

        await foreach (var (startActiveDate, endActiveDate) in GetActiveQuestionDateAsync(request, _eventStorage, _clock, cancellationToken))
        {
            var voiceRecognition = EventType.VoiceRecognition;
            var codeEditorChangeEventType = EventType.CodeEditorChange;
            var res = await _eventStorage
                .GetBySpecAsync(new Spec<IStorageEvent>(e => e.RoomId == request.RoomId && e.Type == voiceRecognition && e.CreatedAt >= startActiveDate && e.CreatedAt <= endActiveDate), DefaultChunkSize, cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken);

            var lastCodeEditorState = await _eventStorage
                .GetLatestBySpecAsync(new Spec<IStorageEvent>(e => e.RoomId == request.RoomId && e.Type == codeEditorChangeEventType), 1, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            roomQuestion.Details.Add(new RoomQuestionAnswerDetailResponse.Detail
            {
                Transcription = res
                    .SelectMany(e => e)
                    .Select(e =>
                    {
                        using var payloadAsJson = JsonDocument.Parse(e.Payload ?? "{}");
                        return new QuestionDetailTranscriptionResponse
                        {
                            Id = e.Id,
                            Payload = payloadAsJson.RootElement.GetProperty("Message").GetString(),
                            CreatedAt = e.CreatedAt,
                            User = new QuestionDetailTranscriptionUserResponse
                            {
                                Id = e.CreatedById,
                                Nickname = payloadAsJson.RootElement.GetProperty("Nickname").GetString() ?? "Anonymous",
                            },
                        };
                    })
                    .ToList(),
                AnswerCodeEditorContent = lastCodeEditorState?.Count > 0 ? lastCodeEditorState.MaxBy(e => e.CreatedAt)?.Payload : null,
                StartActiveDate = startActiveDate,
                EndActiveDate = endActiveDate,
            });
        }

        return roomQuestion;

        static async IAsyncEnumerable<(DateTime StartActiveDate, DateTime EndActiveDate)> GetActiveQuestionDateAsync(
            RoomQuestionAnswerDetailRequest request, IEventStorage eventStorage, ISystemClock clock, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var changeRoomQuestionState = EventType.ChangeRoomQuestionState;
            var changedRooms = await eventStorage
                .GetBySpecAsync(new Spec<IStorageEvent>(e => e.RoomId == request.RoomId && e.Type == changeRoomQuestionState), DefaultChunkSize, cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken);

            var list = changedRooms.SelectMany(e => e)
                .Where(e => e.Payload is not null)
                .Select(e => new
                {
                    Payload = JsonSerializer.Deserialize<RoomQuestionChangeEventPayload>(e.Payload!),
                    CreateAt = e.CreatedAt,
                })
                .ToList();
            foreach (var e in list)
            {
                if (e.Payload is null || e.Payload.QuestionId != request.QuestionId || e.Payload.NewState != RoomQuestionStateType.Active)
                {
                    continue;
                }

                var minDate = e.CreateAt;
                var endActiveDate = list
                    .Where(e => e.Payload is not null && e.Payload.QuestionId == request.QuestionId && e.Payload.OldState == RoomQuestionStateType.Active && e.CreateAt > minDate)
                    .Select(e => (DateTime?)e.CreateAt)
                    .FirstOrDefault();

                yield return (minDate, endActiveDate ?? clock.UtcNow.UtcDateTime);
            }
        }
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

    public async Task<ServiceResult> UpdateAsync(
        Guid roomId,
        List<RoomQuestionUpdateRequest> request,
        CancellationToken cancellationToken = default)
    {
        var hasRoom =
            await _roomRepository.HasAsync(new Spec<Room>(room => room.Id == roomId), cancellationToken);

        if (hasRoom is false)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var requiredQuestions = request.Select(e => e.QuestionId).ToHashSet();
        var dbRoomQuestions = await _db.RoomQuestions.Where(e => requiredQuestions.Contains(e.QuestionId)).ToListAsync(cancellationToken);
        requiredQuestions.ExceptWith(dbRoomQuestions.Select(e => e.QuestionId));
        if (requiredQuestions.Count > 0)
        {
            throw NotFoundException.Create<RoomQuestion>(requiredQuestions);
        }

        foreach (var (dbQuestion, order) in dbRoomQuestions.Join(
                     request,
                     question => question.QuestionId,
                     e => e.QuestionId,
                     (dbQuestion, updateRequest) => (dbQuestion: dbQuestion, Order: updateRequest.Order)))
        {
            dbQuestion.Order = order;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
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

        var newRoomQuestion = new RoomQuestion
        {
            Room = room,
            Question = question,
            State = RoomQuestionState.Open,
            RoomId = default,
            QuestionId = default,
            Order = request.Order,
        };

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
        var questions = await _db.RoomQuestions
            .Include(e => e.Question).ThenInclude(e => e.Answers)
            .AsNoTracking()
            .Where(rq => rq.Room!.Id == request.RoomId && states.Contains(rq.State!))
            .OrderBy(e => e.Order)
            .Select(rq => new
            {
                Id = rq.Question!.Id,
                State = rq.State,
                Value = rq.Question.Value,
                Order = rq.Order,
                Answers = rq.Question!.Answers
                    .Select(a => new QuestionAnswerResponse
                    {
                        Id = a.Id,
                        Content = a.Content,
                        Title = a.Title,
                        CodeEditor = a.CodeEditor,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);
        return questions.ConvertAll(e => new RoomQuestionResponse
        {
            Id = e.Id,
            State = e.State!.EnumValue,
            Value = e.Value,
            Order = e.Order,
            Answers = e.Answers,
        });
    }
}

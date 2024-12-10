using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.Services;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;
using Interview.Domain.Rooms.RoomQuestions.Services.Update;
using Interview.Domain.ServiceResults.Success;
using Microsoft.EntityFrameworkCore;
using NSpecifications;

namespace Interview.Domain.Rooms.RoomQuestions.Services;

public class RoomQuestionService(
    IRoomQuestionRepository roomQuestionRepository,
    IRoomRepository roomRepository,
    IQuestionRepository questionRepository,
    IQuestionService questionService,
    AnswerDetailService roomQuestionService,
    AppDbContext db)
    : IRoomQuestionService
{
    public Task<RoomQuestionAnswerDetailResponse> GetAnswerDetailsAsync(RoomQuestionAnswerDetailRequest request, CancellationToken cancellationToken)
        => roomQuestionService.GetAnswerDetailsAsync(request, cancellationToken);

    public async Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request, CancellationToken cancellationToken = default)
    {
        var roomQuestion = await roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(
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

        var room = await roomRepository.FindFirstOrDefaultAsync(specification, cancellationToken);

        if (room is not null)
        {
            room.Status = SERoomStatus.Active;
            await roomRepository.UpdateAsync(room, cancellationToken);
        }

        await roomQuestionRepository.CloseActiveQuestionAsync(request.RoomId, cancellationToken);

        roomQuestion.State = RoomQuestionState.Active;

        await roomQuestionRepository.UpdateAsync(roomQuestion, cancellationToken);

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
            await roomRepository.HasAsync(new Spec<Room>(room => room.Id == roomId), cancellationToken);

        if (hasRoom is false)
        {
            throw NotFoundException.Create<Room>(roomId);
        }

        var requiredQuestions = request.Select(e => e.QuestionId).ToHashSet();
        var dbRoomQuestions = await db.RoomQuestions.Where(e => requiredQuestions.Contains(e.QuestionId)).ToListAsync(cancellationToken);
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

        await db.SaveChangesAsync(cancellationToken);
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
            var roomQuestion = await roomQuestionRepository.FindFirstByQuestionIdAndRoomIdAsync(
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

            var createdQuestion = await questionService.CreateAsync(request.Question, request.RoomId, cancellationToken);
            questionId = createdQuestion.Id;
        }

        var room = await roomRepository.FindByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var question = await questionRepository.FindByIdAsync(questionId, cancellationToken);

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

        await roomQuestionRepository.CreateAsync(newRoomQuestion, cancellationToken);

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
            await roomRepository.HasAsync(new Spec<Room>(room => room.Id == request.RoomId), cancellationToken);

        if (hasRoom is false)
        {
            throw NotFoundException.Create<Room>(request.RoomId);
        }

        var states = request.States.Select(e => RoomQuestionState.FromValue((int)e)).ToList();
        var questions = await db.RoomQuestions
            .Include(e => e.Question).ThenInclude(e => e!.Answers)
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

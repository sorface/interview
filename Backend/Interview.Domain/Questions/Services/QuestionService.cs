using CSharpFunctionalExtensions;
using Interview.Domain.Database;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Repository;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Questions.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuestionNonArchiveRepository _questionNonArchiveRepository;
    private readonly ArchiveService<Question> _archiveService;
    private readonly ITagRepository _tagRepository;
    private readonly IRoomMembershipChecker _roomMembershipChecker;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly AppDbContext _db;

    public QuestionService(
        IQuestionRepository questionRepository,
        IQuestionNonArchiveRepository questionNonArchiveRepository,
        ArchiveService<Question> archiveService,
        ITagRepository tagRepository,
        IRoomMembershipChecker roomMembershipChecker,
        ICurrentUserAccessor currentUserAccessor,
        AppDbContext db)
    {
        _questionRepository = questionRepository;
        _questionNonArchiveRepository = questionNonArchiveRepository;
        _archiveService = archiveService;
        _tagRepository = tagRepository;
        _roomMembershipChecker = roomMembershipChecker;
        _currentUserAccessor = currentUserAccessor;
        _db = db;
    }

    public async Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserAccessor.UserId ?? Guid.Empty;
        ASpec<Question> spec = new Spec<Question>(e => e.Type == SEQuestionType.Public || e.CreatedById == currentUserId);

        if (request.Tags is not null && request.Tags.Count > 0)
        {
            spec &= new Spec<Question>(e => e.Tags.Any(t => request.Tags.Contains(t.Id)));
        }

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            var questionValue = request.Value.Trim().ToLower();
            spec &= new Spec<Question>(e => e.Value.ToLower().Contains(questionValue));
        }

        var mapper =
            new Mapper<Question, QuestionItem>(
                question => new QuestionItem
                {
                    Id = question.Id,
                    Value = question.Value,
                    Tags = question.Tags
                        .Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, }).ToList(),
                    Answers = question.Answers.Select(q => new QuestionAnswerResponse
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Content = q.Content,
                        CodeEditor = q.CodeEditor,
                    }).ToList(),
                });
        return await _questionNonArchiveRepository.GetPageDetailedAsync(
            spec, mapper, request.Page.PageNumber, request.Page.PageSize, cancellationToken);
    }

    public Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var mapper = new Mapper<Question, QuestionItem>(question => new QuestionItem
        {
            Id = question.Id,
            Value = question.Value,
            Tags = question.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
            Answers = question.Answers.Select(q => new QuestionAnswerResponse
            {
                Id = q.Id,
                Title = q.Title,
                Content = q.Content,
                CodeEditor = q.CodeEditor,
            }).ToList(),
        });

        var isArchiveSpecification = new Spec<Question>(question => question.IsArchived);

        return _questionRepository
            .GetPageDetailedAsync(isArchiveSpecification, mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default)
    {
        if (roomId is not null)
        {
            await _roomMembershipChecker.EnsureCurrentUserMemberOfRoomAsync(roomId.Value, cancellationToken);
        }

        // QuestionAnswer.EnsureValid(request.Answers, request.CodeEditor);
        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);
        var result = new Question(request.Value)
        {
            Tags = tags,
            Type = GetQuestionType(),

            // CodeEditor = request.CodeEditor,
        };

        if (request.Answers is not null)
        {
            foreach (var questionAnswerCreateRequest in request.Answers)
            {
                result.Answers.Add(new QuestionAnswer
                {
                    Title = questionAnswerCreateRequest.Title,
                    Content = questionAnswerCreateRequest.Content,
                    CodeEditor = questionAnswerCreateRequest.CodeEditor,
                    QuestionId = default,
                    Question = null,
                });
            }
        }

        await _questionRepository.CreateAsync(result, cancellationToken);

        return new QuestionItem
        {
            Id = result.Id,
            Value = result.Value,
            Tags = result.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
            Answers = result.Answers.Select(QuestionAnswerResponse.Mapper.Map).ToList(),
        };

        SEQuestionType GetQuestionType()
        {
            if (request.Type == EVQuestionType.Public && !_currentUserAccessor.IsAdmin())
            {
                throw new AccessDeniedException("You cannot create a public question.");
            }

            return SEQuestionType.FromValue((int)request.Type);
        }
    }

    public async Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Questions
            .Include(e => e.Answers)
            .Where(e => !e.IsArchived && e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        var answersMap = request.ExistsAnswers?.ToDictionary(e => e.Id)
                              ?? new Dictionary<Guid, QuestionAnswerEditRequest>();
        entity.Answers.RemoveAll(e => !answersMap.ContainsKey(e.Id));
        foreach (var questionAnswer in entity.Answers)
        {
            var newAnswer = answersMap[questionAnswer.Id];
            questionAnswer.Title = newAnswer.Title;
            questionAnswer.CodeEditor = newAnswer.CodeEditor;
            questionAnswer.Content = newAnswer.Content;
        }

        if (request.NewAnswers is not null)
        {
            foreach (var questionAnswerCreateRequest in request.NewAnswers)
            {
                entity.Answers.Add(new QuestionAnswer
                {
                    Title = questionAnswerCreateRequest.Title,
                    Content = questionAnswerCreateRequest.Content,
                    CodeEditor = questionAnswerCreateRequest.CodeEditor,
                    QuestionId = default,
                    Question = null,
                });
            }
        }

        // QuestionAnswer.EnsureValid(request.NewAnswers, request.CodeEditor);
        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);

        // entity.CodeEditor = request.CodeEditor;
        entity.Value = request.Value;
        entity.Tags.Clear();
        entity.Tags.AddRange(tags);

        await _questionRepository.UpdateAsync(entity, cancellationToken);

        return new QuestionItem
        {
            Id = entity.Id,
            Value = entity.Value,
            Tags = entity.Tags.Select(e => new TagItem
            {
                Id = e.Id,
                Value = e.Value,
                HexValue = e.HexColor,
            })
                .ToList(),
            Answers = entity.Answers.Select(QuestionAnswerResponse.Mapper.Map).ToList(),
        };
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _questionNonArchiveRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (question is null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        return new QuestionItem
        {
            Id = question.Id,
            Value = question.Value,
            Tags = question.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
            Answers = question.Answers.Select(QuestionAnswerResponse.Mapper.Map).ToList(),
        };
    }

    /// <summary>
    /// Permanent deletion of a question with verification of its existence.
    /// </summary>
    /// <param name="id">Question's guid.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>ServiceResult with QuestionItem - success, ServiceError - error.</returns>
    public async Task<QuestionItem> DeletePermanentlyAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _questionRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        await _questionRepository.DeletePermanentlyAsync(question, cancellationToken);

        return new QuestionItem
        {
            Id = question.Id,
            Value = question.Value,
            Tags = question.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
            Answers = new List<QuestionAnswerResponse>(),
        };
    }

    public async Task<QuestionItem> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var archiveQuestion = await _archiveService.ArchiveAsync(id, cancellationToken);

        return new QuestionItem
        {
            Id = archiveQuestion.Id,
            Value = archiveQuestion.Value,
            Tags = archiveQuestion.Tags
                .Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, }).ToList(),
            Answers = new List<QuestionAnswerResponse>(),
        };
    }

    public async Task<QuestionItem> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unarchiveQuestion = await _archiveService.UnarchiveAsync(id, cancellationToken);

        return new QuestionItem
        {
            Id = unarchiveQuestion.Id,
            Value = unarchiveQuestion.Value,
            Tags = unarchiveQuestion.Tags
                .Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, }).ToList(),
            Answers = new List<QuestionAnswerResponse>(),
        };
    }
}

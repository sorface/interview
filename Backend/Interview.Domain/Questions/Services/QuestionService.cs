using Interview.Domain.Categories;
using Interview.Domain.Categories.Page;
using Interview.Domain.Database;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
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

        if (request.CategoryId is not null)
        {
            spec &= new Spec<Question>(e => e.CategoryId == request.CategoryId);
        }

        return await _questionNonArchiveRepository.GetPageDetailedAsync(
            spec, QuestionItem.Mapper, request.Page.PageNumber, request.Page.PageSize, cancellationToken);
    }

    public Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var isArchiveSpecification = new Spec<Question>(question => question.IsArchived);
        return _questionRepository
            .GetPageDetailedAsync(isArchiveSpecification, QuestionItem.Mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default)
    {
        if (roomId is not null)
        {
            await _roomMembershipChecker.EnsureCurrentUserMemberOfRoomAsync(roomId.Value, cancellationToken);
        }

        var value = EnsureValidQuestionValue(request.Value);
        QuestionAnswer.EnsureValid(request.Answers?.Select(e => new QuestionAnswer.Validate(e)), request.CodeEditor is not null);
        var categoryValidateResult = await Category.ValidateCategoryAsync(_db, request.CategoryId, cancellationToken);
        categoryValidateResult?.Throw();

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);
        var result = new Question(value)
        {
            Tags = tags,
            Type = GetQuestionType(),
            CategoryId = request.CategoryId,
            CodeEditor = request.CodeEditor is null
                ? null
                : new QuestionCodeEditor
                {
                    Content = request.CodeEditor.Content,
                    Lang = request.CodeEditor.Lang,
                    Source = EVRoomCodeEditorChangeSource.User,
                },
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
            Category = result.CategoryId is not null ?
                await _db.Categories.AsNoTracking()
                    .Where(e => e.Id == result.CategoryId)
                    .Select(e => new CategoryResponse
                    {
                        Id = e.Id,
                        Name = e.Name,
                        ParentId = e.ParentId,
                    })
                    .FirstOrDefaultAsync(cancellationToken)
                : null,
            Answers = result.Answers.Select(QuestionAnswerResponse.Mapper.Map).ToList(),
            CodeEditor = result.CodeEditor == null
                ? null
                : new QuestionCodeEditorResponse
                {
                    Content = result.CodeEditor.Content,
                    Lang = result.CodeEditor.Lang,
                },
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
        var value = EnsureValidQuestionValue(request.Value);
        var entity = await _db.Questions
            .Include(e => e.Answers)
            .Include(e => e.CodeEditor)
            .Include(e => e.Category)
            .Where(e => !e.IsArchived && e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        var answersMap = request.Answers?
            .Where(e => e.Id is not null)
            .ToDictionary(e => e.Id!.Value) ?? new Dictionary<Guid, QuestionAnswerEditRequest>();
        entity.Answers.RemoveAll(e => !answersMap.ContainsKey(e.Id));
        foreach (var questionAnswer in entity.Answers)
        {
            var newAnswer = answersMap[questionAnswer.Id];
            questionAnswer.Title = newAnswer.Title;
            questionAnswer.CodeEditor = newAnswer.CodeEditor;
            questionAnswer.Content = newAnswer.Content;
        }

        if (request.Answers is not null)
        {
            foreach (var questionAnswerCreateRequest in request.Answers.Where(e => e.Id is null))
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

        QuestionAnswer.EnsureValid(request.Answers?.Select(e => new QuestionAnswer.Validate(e)), request.CodeEditor is not null);

        var categoryValidateResult = await Category.ValidateCategoryAsync(_db, request.CategoryId, cancellationToken);
        categoryValidateResult?.Throw();

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);

        if (request.CodeEditor is not null && entity.CodeEditor is not null)
        {
            entity.CodeEditor.UpdateContent(request.CodeEditor.Content, EVRoomCodeEditorChangeSource.User);
            entity.CodeEditor.Lang = request.CodeEditor.Lang;
        }
        else
        {
            // remove exists code editor
            if (entity.CodeEditor is not null)
            {
                _db.QuestionCodeEditors.Remove(entity.CodeEditor);
                entity.CodeEditor = null;
            }

            // add new if needed
            if (request.CodeEditor is not null)
            {
                entity.CodeEditor = new QuestionCodeEditor
                {
                    Content = request.CodeEditor.Content,
                    Lang = request.CodeEditor.Lang,
                    Source = EVRoomCodeEditorChangeSource.User,
                };
            }
        }

        entity.Value = value;
        entity.CategoryId = request.CategoryId;
        entity.Tags.Clear();
        entity.Tags.AddRange(tags);

        await _questionRepository.UpdateAsync(entity, cancellationToken);

        return await ToQuestionItemAsync(entity, cancellationToken);
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions.AsNoTracking()
            .Include(e => e.Tags)
            .Include(e => e.Category)
            .Include(e => e.CodeEditor)
            .Include(e => e.Answers)
            .Where(e => !e.IsArchived && e.Id == id)
            .Select(QuestionItem.Mapper.Expression)
            .FirstOrDefaultAsync(cancellationToken);
        return question ?? throw NotFoundException.Create<Question>(id);
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
        return await ToQuestionItemAsync(question, cancellationToken);
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
            CodeEditor = null,
            Category = null,
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
            CodeEditor = null,
            Category = null,
        };
    }

    private static string EnsureValidQuestionValue(string value)
    {
        value = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
        {
            throw new UserException("Question value must be at least 3 characters long");
        }

        return value;
    }

    private async Task<QuestionItem> ToQuestionItemAsync(Question entity, CancellationToken cancellationToken)
    {
        var category = entity.Category;
        if (category is null && entity.CategoryId is not null)
        {
            category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.CategoryId, cancellationToken);
        }

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
            Category = category is not null
                ? new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    ParentId = category.ParentId,
                }
                : null,
            Answers = entity.Answers.Select(q => new QuestionAnswerResponse
            {
                Id = q.Id,
                Title = q.Title,
                Content = q.Content,
                CodeEditor = q.CodeEditor,
            })
                .ToList(),
            CodeEditor = entity.CodeEditor == null
                ? null
                : new QuestionCodeEditorResponse
                {
                    Content = entity.CodeEditor.Content,
                    Lang = entity.CodeEditor.Lang,
                },
        };
    }
}

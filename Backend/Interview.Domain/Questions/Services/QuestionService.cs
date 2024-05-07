using CSharpFunctionalExtensions;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Repository;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Questions.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IRoomQuestionRepository _roomQuestionRepository;
    private readonly IQuestionNonArchiveRepository _questionNonArchiveRepository;
    private readonly ArchiveService<Question> _archiveService;
    private readonly ITagRepository _tagRepository;
    private readonly IRoomMembershipChecker _roomMembershipChecker;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public QuestionService(
        IQuestionRepository questionRepository,
        IQuestionNonArchiveRepository questionNonArchiveRepository,
        ArchiveService<Question> archiveService,
        ITagRepository tagRepository,
        IRoomMembershipChecker roomMembershipChecker,
        ICurrentUserAccessor currentUserAccessor,
        IRoomQuestionRepository roomQuestionRepository)
    {
        _questionRepository = questionRepository;
        _questionNonArchiveRepository = questionNonArchiveRepository;
        _archiveService = archiveService;
        _tagRepository = tagRepository;
        _roomMembershipChecker = roomMembershipChecker;
        _currentUserAccessor = currentUserAccessor;
        _roomQuestionRepository = roomQuestionRepository;
    }

    public async Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserAccessor.UserId ?? Guid.Empty;
        ASpec<Question> spec = new Spec<Question>(e => e.RoomId == null || e.CreatedById == currentUserId);

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

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);
        var result = new Question(request.Value)
        {
            Tags = tags,
            RoomId = roomId,
        };

        await _questionRepository.CreateAsync(result, cancellationToken);
        if (roomId is not null)
        {
            await _roomQuestionRepository.CreateAsync(new RoomQuestion { QuestionId = result.Id, RoomId = roomId.Value, }, cancellationToken);
        }

        return new QuestionItem
        {
            Id = result.Id,
            Value = result.Value,
            Tags = result.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
        };
    }

    public async Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _questionNonArchiveRepository.FindByIdAsync(id, cancellationToken);

        if (entity == null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        var tags = await Tag.EnsureValidTagsAsync(_tagRepository, request.Tags, cancellationToken);

        entity.Value = request.Value;
        entity.Tags.Clear();
        entity.Tags.AddRange(tags);

        await _questionRepository.UpdateAsync(entity, cancellationToken);

        return new QuestionItem
        {
            Id = entity.Id,
            Value = entity.Value,
            Tags = entity.Tags.Select(e => new TagItem { Id = e.Id, Value = e.Value, HexValue = e.HexColor, })
                .ToList(),
        };
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _questionNonArchiveRepository.FindByIdAsync(id, cancellationToken);

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
        var question = await _questionRepository.FindByIdAsync(id, cancellationToken);

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
        };
    }
}

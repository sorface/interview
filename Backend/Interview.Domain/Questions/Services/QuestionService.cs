using Interview.Domain.Categories;
using Interview.Domain.Categories.Page;
using Interview.Domain.Database;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Questions.QuestionTreeById;
using Interview.Domain.Questions.QuestionTreePage;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.UpsertQuestionTree;
using Interview.Domain.Rooms.RoomConfigurations;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Questions.Services;

public class QuestionService(
    IQuestionRepository questionRepository,
    IQuestionNonArchiveRepository questionNonArchiveRepository,
    ArchiveService<Question> archiveService,
    ArchiveService<QuestionTree> archiveQuestionTreeService,
    ArchiveService<QuestionSubjectTree> archiveQuestionSubjectTreeService,
    ITagRepository tagRepository,
    IRoomMembershipChecker roomMembershipChecker,
    ICurrentUserAccessor currentUserAccessor,
    QuestionTreeUpsert questionTreeUpsert,
    AppDbContext db)
    : IQuestionService
{
    public async Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserAccessor.UserId ?? Guid.Empty;
        ASpec<Question> spec = new Spec<Question>(e => e.Type == SEQuestionType.Public || e.CreatedById == currentUserId);

        if (request.Tags is not null && request.Tags.Count > 0)
        {
            spec &= new Spec<Question>(e => e.Tags.Any(t => request.Tags.Contains(t.Id)));
        }

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            var questionValue = request.Value.Trim().ToLower();
#pragma warning disable CA1862
            spec &= new Spec<Question>(e => e.Value.ToLower().Contains(questionValue));
#pragma warning restore CA1862
        }

        if (request.CategoryId is not null)
        {
            spec &= new Spec<Question>(e => e.CategoryId == request.CategoryId);
        }

        return await questionNonArchiveRepository.GetPageDetailedAsync(
            spec, QuestionItem.Mapper, request.Page.PageNumber, request.Page.PageSize, cancellationToken);
    }

    public Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var isArchiveSpecification = new Spec<Question>(question => question.IsArchived);
        return questionRepository
            .GetPageDetailedAsync(isArchiveSpecification, QuestionItem.Mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default)
    {
        if (roomId is not null)
        {
            await roomMembershipChecker.EnsureCurrentUserMemberOfRoomAsync(roomId.Value, cancellationToken);
        }

        var value = EnsureValidQuestionValue(request.Value);
        QuestionAnswer.EnsureValid(request.Answers?.Select(e => new QuestionAnswer.Validate(e)), request.CodeEditor is not null);
        var categoryValidateResult = await Category.ValidateCategoryAsync(db, request.CategoryId, cancellationToken);
        categoryValidateResult?.Throw();

        var tags = await Tag.EnsureValidTagsAsync(tagRepository, request.Tags, cancellationToken);
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

        await questionRepository.CreateAsync(result, cancellationToken);

        return new QuestionItem
        {
            Id = result.Id,
            Value = result.Value,
            Tags = result.Tags.Select(e => new TagItem
            {
                Id = e.Id,
                Value = e.Value,
                HexValue = e.HexColor,
            })
                .ToList(),
            Category = result.CategoryId is not null
                ? await db.Categories.AsNoTracking()
                    .Include(e => e.Parent)
                    .Where(e => e.Id == result.CategoryId)
                    .OrderBy(e => e.Order)
                    .Select(CategoryResponse.Mapper.Expression)
                    .FirstOrDefaultAsync(cancellationToken)
                : null,
            Answers = result.Answers.Select(QuestionAnswerResponse.Mapper.Map)
                .ToList(),
            CodeEditor = result.CodeEditor == null
                ? null
                : new QuestionCodeEditorResponse
                {
                    Content = result.CodeEditor.Content,
                    Lang = result.CodeEditor.Lang,
                },
            Author = null,
            Type = result.Type.EnumValue,
        };

        SEQuestionType GetQuestionType()
        {
            if (request.Type == EVQuestionType.Public && !currentUserAccessor.IsAdmin())
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
        var entity = await db.Questions
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

        var categoryValidateResult = await Category.ValidateCategoryAsync(db, request.CategoryId, cancellationToken);
        categoryValidateResult?.Throw();

        var tags = await Tag.EnsureValidTagsAsync(tagRepository, request.Tags, cancellationToken);

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
                db.QuestionCodeEditors.Remove(entity.CodeEditor);
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

        if (request.Type is not null)
        {
            entity.Type = SEQuestionType.FromValue((int)request.Type.Value);
        }

        await questionRepository.UpdateAsync(entity, cancellationToken);

        return await ToQuestionItemAsync(entity, cancellationToken);
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var question = await db.Questions.AsNoTracking()
            .Include(e => e.Tags)
            .Include(e => e.Category)
            .Include(e => e.CodeEditor)
            .Include(e => e.Answers)
            .Include(e => e.CreatedBy)
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
        var question = await questionRepository.FindByIdDetailedAsync(id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<Question>(id);
        }

        await questionRepository.DeletePermanentlyAsync(question, cancellationToken);
        return await ToQuestionItemAsync(question, cancellationToken);
    }

    public async Task<QuestionItem> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var archiveQuestion = await archiveService.ArchiveAsync(id, cancellationToken);

        return new QuestionItem
        {
            Id = archiveQuestion.Id,
            Value = archiveQuestion.Value,
            Tags = archiveQuestion.Tags
                .Select(e => new TagItem
                {
                    Id = e.Id,
                    Value = e.Value,
                    HexValue = e.HexColor,
                })
                .ToList(),
            Answers =
            [
            ],
            CodeEditor = null,
            Category = null,
            Author = null,
            Type = archiveQuestion.Type.EnumValue,
        };
    }

    public async Task<QuestionItem> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unarchiveQuestion = await archiveService.UnarchiveAsync(id, cancellationToken);

        return new QuestionItem
        {
            Id = unarchiveQuestion.Id,
            Value = unarchiveQuestion.Value,
            Tags = unarchiveQuestion.Tags
                .Select(e => new TagItem
                {
                    Id = e.Id,
                    Value = e.Value,
                    HexValue = e.HexColor,
                })
                .ToList(),
            Answers =
            [
            ],
            CodeEditor = null,
            Category = null,
            Author = null,
            Type = unarchiveQuestion.Type.EnumValue,
        };
    }

    public Task<IPagedList<QuestionTreePageResponse>> FindQuestionTreePageAsync(QuestionTreePageRequest request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        return db.QuestionTree.AsNoTracking()
            .Where(spec)
            .OrderBy(e => e.Order)
            .ThenBy(e => e.CreateDate)
            .Select(e => new QuestionTreePageResponse { Id = e.Id, Name = e.Name, ParentQuestionTreeId = e.ParentQuestionTreeId, })
            .ToPagedListAsync(request.Page, cancellationToken);

        static ASpec<QuestionTree> BuildSpecification(QuestionTreePageRequest request)
        {
            var archived = request.Filter?.Archived == true;
            ASpec<QuestionTree>? res = new Spec<QuestionTree>(e => e.IsArchived == archived);
            if (request.Filter is null)
            {
                return res;
            }

            if (!string.IsNullOrWhiteSpace(request.Filter.Name))
            {
                var name = request.Filter.Name.Trim();
#pragma warning disable CA1862
                res &= new Spec<QuestionTree>(e => e.Name.ToLower().Contains(name));
#pragma warning restore CA1862
            }

            if (request.Filter.ParentQuestionTreeId is not null)
            {
                res &= new Spec<QuestionTree>(e => e.ParentQuestionTreeId == request.Filter.ParentQuestionTreeId);
            }
            else if (request.Filter.ParentlessOnly.GetValueOrDefault())
            {
                res &= new Spec<QuestionTree>(e => e.ParentQuestionTreeId == null);
            }

            return res;
        }
    }

    public Task<ServiceResult<Guid>> UpsertQuestionTreeAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken = default)
        => questionTreeUpsert.UpsertQuestionTreeAsync(request, cancellationToken);

    public async Task<QuestionTreeByIdResponse> GetQuestionTreeByIdAsync(Guid questionTreeId, bool archive, CancellationToken cancellationToken)
    {
        var questionTree = await db.QuestionTree.AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.ThemeAiDescription,
                e.RootQuestionSubjectTreeId,
                e.IsArchived,
            })
            .FirstOrDefaultAsync(e => e.Id == questionTreeId && e.IsArchived == archive, cancellationToken);
        if (questionTree is null)
        {
            throw NotFoundException.Create<QuestionTree>(questionTreeId);
        }

        var response = new QuestionTreeByIdResponse
        {
            Id = questionTree.Id,
            RootQuestionSubjectTreeId = questionTree.RootQuestionSubjectTreeId,
            Name = questionTree.Name,
            Tree = new List<QuestionTreeByIdResponseTree>(),
            ThemeAiDescription = questionTree.ThemeAiDescription,
        };
        await response.FillTreeAsync(db, cancellationToken);
        return response;
    }

    public Task ArchiveQuestionTreeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return db.RunTransactionAsync(async ct =>
        {
            var tree = await archiveQuestionTreeService.ArchiveAsync(id, false, ct);
            var nodes = await db.QuestionSubjectTree.GetAllChildrenAsync(tree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
            foreach (var subjectTreeId in nodes)
            {
                await archiveQuestionSubjectTreeService.ArchiveAsync(subjectTreeId, false, ct);
            }

            await db.SaveChangesAsync(ct);
            return DBNull.Value;
        },
            cancellationToken);
    }

    public Task UnarchiveQuestionTreeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return db.RunTransactionAsync(async ct =>
            {
                var tree = await archiveQuestionTreeService.UnarchiveAsync(id, false, ct);
                await QuestionTreeUpsert.EnsureNonDuplicateByNameAsync(db, tree.Id, tree.Name, tree.ParentQuestionTreeId, cancellationToken);
                var nodes = await db.QuestionSubjectTree.GetAllChildrenAsync(tree.RootQuestionSubjectTreeId, e => e.ParentQuestionSubjectTreeId, true, cancellationToken);
                foreach (var subjectTreeId in nodes)
                {
                    await archiveQuestionSubjectTreeService.UnarchiveAsync(subjectTreeId, false, ct);
                }

                await db.SaveChangesAsync(ct);
                return DBNull.Value;
            },
            cancellationToken);
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
            category = await db.Categories.Include(e => e.Parent).AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.CategoryId, cancellationToken);
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
            Category = category is null ? null : CategoryResponse.Mapper.Map(category),
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
            Author = entity.CreatedBy == null
                ? null
                : new QuestionItemAuthorResponse
                {
                    Nickname = entity.CreatedBy.Nickname,
                    UserId = entity.CreatedBy.Id,
                },
            Type = entity.Type.EnumValue,
        };
    }
}

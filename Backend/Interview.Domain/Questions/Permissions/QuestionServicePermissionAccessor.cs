using Interview.Domain.Permissions;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using X.PagedList;

namespace Interview.Domain.Questions.Permissions;

public class QuestionServicePermissionAccessor : IQuestionService, IServiceDecorator
{
    private readonly IQuestionService _questionService;
    private readonly ISecurityService _securityService;

    public QuestionServicePermissionAccessor(
        IQuestionService questionService, ISecurityService securityService)
    {
        _questionService = questionService;
        _securityService = securityService;
    }

    public Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.QuestionFindPage);

        return _questionService.FindPageAsync(request, cancellationToken);
    }

    public Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.QuestionFindPageArchive);

        return _questionService.FindPageArchiveAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionCreate);

        return _questionService.CreateAsync(request, cancellationToken);
    }

    public Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionUpdate);

        return _questionService.UpdateAsync(id, request, cancellationToken);
    }

    public Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionFindById);

        return _questionService.FindByIdAsync(id, cancellationToken);
    }

    public Task<QuestionItem> DeletePermanentlyAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionDeletePermanently);

        return _questionService.DeletePermanentlyAsync(id, cancellationToken);
    }

    public Task<QuestionItem> ArchiveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionArchive);

        return _questionService.ArchiveAsync(id, cancellationToken);
    }

    public Task<QuestionItem> UnarchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.QuestionUnarchive);

        return _questionService.UnarchiveAsync(id, cancellationToken);
    }
}

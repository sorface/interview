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

    public async Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionFindPage, cancellationToken);
        return await _questionService.FindPageAsync(request, cancellationToken);
    }

    public async Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionFindPageArchive, cancellationToken);
        return await _questionService.FindPageArchiveAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionCreate, cancellationToken);
        return await _questionService.CreateAsync(request, cancellationToken);
    }

    public async Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionUpdate, cancellationToken);
        return await _questionService.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionFindById, cancellationToken);
        return await _questionService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> DeletePermanentlyAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionDeletePermanently, cancellationToken);
        return await _questionService.DeletePermanentlyAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> ArchiveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionArchive, cancellationToken);
        return await _questionService.ArchiveAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> UnarchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.QuestionUnarchive, cancellationToken);
        return await _questionService.UnarchiveAsync(id, cancellationToken);
    }
}

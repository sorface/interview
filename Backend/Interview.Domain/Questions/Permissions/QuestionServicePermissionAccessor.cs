using Interview.Domain.Permissions;
using Interview.Domain.Questions.QuestionTreeById;
using Interview.Domain.Questions.QuestionTreePage;
using Interview.Domain.Questions.Records.FindPage;
using Interview.Domain.Questions.Services;
using Interview.Domain.Questions.UpsertQuestionTree;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Questions.Permissions;

public class QuestionServicePermissionAccessor(IQuestionService questionService, ISecurityService securityService, IEntityAccessControl entityAccessControl) : IQuestionService, IServiceDecorator
{
    public async Task<IPagedList<QuestionItem>> FindPageAsync(FindPageRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionFindPage, cancellationToken);
        return await questionService.FindPageAsync(request, cancellationToken);
    }

    public async Task<IPagedList<QuestionTreePageResponse>> FindQuestionTreePageAsync(QuestionTreePageRequest request, CancellationToken cancellationToken)
    {
        var permission = request.Filter?.Archived == true
            ? SEPermission.QuestionTreeFindArchivedPage
            : SEPermission.QuestionTreeFindPage;
        await securityService.EnsurePermissionAsync(permission, cancellationToken);
        return await questionService.FindQuestionTreePageAsync(request, cancellationToken);
    }

    public async Task<QuestionTreeByIdResponse> GetQuestionTreeByIdAsync(Guid questionTreeId, bool archive, CancellationToken cancellationToken)
    {
        var permission = archive ? SEPermission.GetArchiveQuestionTreeById : SEPermission.GetQuestionTreeById;
        await securityService.EnsurePermissionAsync(permission, cancellationToken);
        return await questionService.GetQuestionTreeByIdAsync(questionTreeId, archive, cancellationToken);
    }

    public async Task<ServiceResult<Guid>> UpsertQuestionTreeAsync(UpsertQuestionTreeRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.UpsertQuestionTree, cancellationToken);
        return await questionService.UpsertQuestionTreeAsync(request, cancellationToken);
    }

    public async Task<IPagedList<QuestionItem>> FindPageArchiveAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionFindPageArchive, cancellationToken);
        return await questionService.FindPageArchiveAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<QuestionItem> CreateAsync(
        QuestionCreateRequest request, Guid? roomId, CancellationToken cancellationToken = default)
    {
        await securityService.EnsureRoomPermissionAsync(roomId, SEPermission.QuestionCreate, cancellationToken);
        return await questionService.CreateAsync(request, roomId, cancellationToken);
    }

    public async Task<QuestionItem> UpdateAsync(
        Guid id, QuestionEditRequest request, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionUpdate, cancellationToken);
        await entityAccessControl.EnsureEditPermissionAsync<Question>(id, cancellationToken);
        return await questionService.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<QuestionItem> FindByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionFindById, cancellationToken);
        return await questionService.FindByIdAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> DeletePermanentlyAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionDeletePermanently, cancellationToken);
        return await questionService.DeletePermanentlyAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> ArchiveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionArchive, cancellationToken);
        return await questionService.ArchiveAsync(id, cancellationToken);
    }

    public async Task<QuestionItem> UnarchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.QuestionUnarchive, cancellationToken);
        return await questionService.UnarchiveAsync(id, cancellationToken);
    }
}

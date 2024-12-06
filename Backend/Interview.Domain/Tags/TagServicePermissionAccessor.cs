using CSharpFunctionalExtensions;
using Interview.Domain.Permissions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags.Records.Response;
using X.PagedList;

namespace Interview.Domain.Tags;

public class TagServicePermissionAccessor(ITagService service, ISecurityService securityService) : ITagService, IServiceDecorator
{
    public async Task<IPagedList<TagItem>> FindTagsPageAsync(string? value, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.TagFindPage, cancellationToken);
        return await service.FindTagsPageAsync(value, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> CreateTagAsync(TagEditRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.TagCreate, cancellationToken);
        return await service.CreateTagAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> UpdateTagAsync(Guid id, TagEditRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.TagUpdate, cancellationToken);
        return await service.UpdateTagAsync(id, request, cancellationToken);
    }
}

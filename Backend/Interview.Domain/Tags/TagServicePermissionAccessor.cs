using CSharpFunctionalExtensions;
using Interview.Domain.Permissions;
using Interview.Domain.Rooms.Service;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags.Records.Response;
using X.PagedList;

namespace Interview.Domain.Tags;

public class TagServicePermissionAccessor : ITagService, IServiceDecorator
{
    private readonly ITagService _service;
    private readonly ISecurityService _securityService;

    public TagServicePermissionAccessor(ITagService service, ISecurityService securityService)
    {
        _service = service;
        _securityService = securityService;
    }

    public async Task<IPagedList<TagItem>> FindTagsPageAsync(string? value, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.TagFindPage, cancellationToken);
        return await _service.FindTagsPageAsync(value, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> CreateTagAsync(TagEditRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.TagCreate, cancellationToken);
        return await _service.CreateTagAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> UpdateTagAsync(Guid id, TagEditRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.TagUpdate, cancellationToken);
        return await _service.UpdateTagAsync(id, request, cancellationToken);
    }
}

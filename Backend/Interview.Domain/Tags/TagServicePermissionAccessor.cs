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

    public Task<IPagedList<TagItem>> FindTagsPageAsync(string? value, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.TagFindPage);
        return _service.FindTagsPageAsync(value, pageNumber, pageSize, cancellationToken);
    }

    public Task<Result<ServiceResult<TagItem>, ServiceError>> CreateTagAsync(TagEditRequest request, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.TagCreate);
        return _service.CreateTagAsync(request, cancellationToken);
    }

    public Task<Result<ServiceResult<TagItem>, ServiceError>> UpdateTagAsync(Guid id, TagEditRequest request, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.TagUpdate);
        return _service.UpdateTagAsync(id, request, cancellationToken);
    }
}

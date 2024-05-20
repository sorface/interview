using CSharpFunctionalExtensions;
using Interview.Domain.Events.Service.Create;
using Interview.Domain.Events.Service.FindPage;
using Interview.Domain.Events.Service.Update;
using Interview.Domain.Permissions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Events.Service;

public class AppEventServicePermissionAccessor : IAppEventService, IServiceDecorator
{
    private readonly IAppEventService _service;
    private readonly ISecurityService _securityService;

    public AppEventServicePermissionAccessor(IAppEventService service, ISecurityService securityService)
    {
        _service = service;
        _securityService = securityService;
    }

    public async Task<IPagedList<AppEventItem>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.AppEventPage, cancellationToken);
        return await _service.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<AppEventItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.AppEventById, cancellationToken);
        return await _service.FindByIdAsync(id, cancellationToken);
    }

    public async Task<AppEventItem?> FindByTypeAsync(string type, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.AppEventByType, cancellationToken);
        return await _service.FindByTypeAsync(type, cancellationToken);
    }

    public async Task<Guid> CreateAsync(AppEventCreateRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.AppEventCreate, cancellationToken);
        return await _service.CreateAsync(request, cancellationToken);
    }

    public async Task<AppEventItem> UpdateAsync(Guid id, AppEventUpdateRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.AppEventUpdate, cancellationToken);
        return await _service.UpdateAsync(id, request, cancellationToken);
    }
}

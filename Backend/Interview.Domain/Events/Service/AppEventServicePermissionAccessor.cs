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

    public Task<IPagedList<AppEventItem>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.AppEventPage);
        return _service.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<AppEventItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.AppEventById);
        return _service.FindByIdAsync(id, cancellationToken);
    }

    public Task<AppEventItem?> FindByTypeAsync(string type, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.AppEventByType);
        return _service.FindByTypeAsync(type, cancellationToken);
    }

    public Task<Guid> CreateAsync(AppEventCreateRequest request, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.AppEventCreate);
        return _service.CreateAsync(request, cancellationToken);
    }

    public Task<AppEventItem> UpdateAsync(Guid id, AppEventUpdateRequest request, CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.AppEventUpdate);
        return _service.UpdateAsync(id, request, cancellationToken);
    }
}

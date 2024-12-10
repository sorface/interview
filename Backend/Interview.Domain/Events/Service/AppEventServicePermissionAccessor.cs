using Interview.Domain.Events.Service.Create;
using Interview.Domain.Events.Service.FindPage;
using Interview.Domain.Events.Service.Update;
using Interview.Domain.Permissions;
using X.PagedList;

namespace Interview.Domain.Events.Service;

public class AppEventServicePermissionAccessor(IAppEventService service, ISecurityService securityService) : IAppEventService, IServiceDecorator
{
    public async Task<IPagedList<AppEventItem>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.AppEventPage, cancellationToken);
        return await service.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<AppEventItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.AppEventById, cancellationToken);
        return await service.FindByIdAsync(id, cancellationToken);
    }

    public async Task<AppEventItem?> FindByTypeAsync(string type, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.AppEventByType, cancellationToken);
        return await service.FindByTypeAsync(type, cancellationToken);
    }

    public async Task<Guid> CreateAsync(AppEventCreateRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.AppEventCreate, cancellationToken);
        return await service.CreateAsync(request, cancellationToken);
    }

    public async Task<AppEventItem> UpdateAsync(Guid id, AppEventUpdateRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.AppEventUpdate, cancellationToken);
        return await service.UpdateAsync(id, request, cancellationToken);
    }
}

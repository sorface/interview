using Interview.Domain.Permissions;
using Interview.Domain.Roadmaps.RoadmapById;
using Interview.Domain.Roadmaps.RoadmapPage;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Roadmaps;

public class RoadmapServicePermissionAccessor(IRoadmapService service, ISecurityService securityService) : IRoadmapService, IServiceDecorator
{
    public async Task<ServiceResult<Guid>> UpsertAsync(UpsertRoadmapRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.RoadmapUpsert, cancellationToken);
        return await service.UpsertAsync(request, cancellationToken);
    }

    public async Task<RoadmapResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.RoadmapGetById, cancellationToken);
        return await service.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IPagedList<RoadmapPageResponse>> FindPageAsync(FilteredRequest<RoadmapPageRequestFilter> request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.RoadmapFindPage, cancellationToken);
        return await service.FindPageAsync(request, cancellationToken);
    }
}

using Interview.Domain.Roadmaps.RoadmapById;
using Interview.Domain.Roadmaps.RoadmapPage;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Roadmaps;

public interface IRoadmapService : IService
{
    Task<ServiceResult<Guid>> UpsertAsync(UpsertRoadmapRequest request, CancellationToken cancellationToken);

    Task<RoadmapResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IPagedList<RoadmapPageResponse>> FindPageAsync(FilteredRequest<RoadmapPageRequestFilter> request, CancellationToken cancellationToken);

    Task<IPagedList<RoadmapPageResponse>> FindArchivedPageAsync(FilteredRequest<RoadmapPageRequestFilter> request, CancellationToken cancellationToken);

    Task<RoadmapPageResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken);

    Task<RoadmapPageResponse> UnarchiveAsync(Guid id, CancellationToken cancellationToken);
}

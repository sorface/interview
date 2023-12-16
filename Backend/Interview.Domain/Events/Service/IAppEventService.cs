using CSharpFunctionalExtensions;
using Interview.Domain.Events.Service.Create;
using Interview.Domain.Events.Service.FindPage;
using Interview.Domain.Events.Service.Update;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Events.Service;

public interface IAppEventService : IService
{
    Task<IPagedList<AppEventItem>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<AppEventItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AppEventItem?> FindByTypeAsync(string type, CancellationToken cancellationToken);

    Task<Guid> CreateAsync(AppEventCreateRequest request, CancellationToken cancellationToken);

    Task<AppEventItem> UpdateAsync(Guid id, AppEventUpdateRequest request, CancellationToken cancellationToken);
}

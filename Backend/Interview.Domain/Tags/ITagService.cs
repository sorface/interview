using CSharpFunctionalExtensions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags.Records.Response;
using X.PagedList;

namespace Interview.Domain.Tags;

public interface ITagService : IService
{
    Task<IPagedList<TagItem>> FindTagsPageAsync(
        string? value, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<Result<ServiceResult<TagItem>, ServiceError>> CreateTagAsync(TagEditRequest request, CancellationToken cancellationToken);

    Task<Result<ServiceResult<TagItem>, ServiceError>> UpdateTagAsync(Guid id, TagEditRequest request, CancellationToken cancellationToken);
}

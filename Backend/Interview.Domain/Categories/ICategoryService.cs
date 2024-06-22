using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Categories;

public interface ICategoryService : IService
{
    Task<CategoryResponse> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken);

    Task<IPagedList<CategoryResponse>> FindArchivePageAsync(CategoryPageRequest request, CancellationToken cancellationToken);

    Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateAsync(CategoryEditRequest request, CancellationToken cancellationToken);

    Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken);

    Task<CategoryResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken);

    Task<CategoryResponse> UnarchiveAsync(Guid id, CancellationToken cancellationToken);
}

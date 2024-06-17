using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Categories;

public interface ICategoryService : IService
{
    Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken);

    Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateTagAsync(CategoryEditRequest request, CancellationToken cancellationToken);

    Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateTagAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken);
}

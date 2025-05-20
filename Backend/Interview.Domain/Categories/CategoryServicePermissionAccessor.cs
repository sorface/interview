using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.Permissions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Categories;

public class CategoryServicePermissionAccessor(ICategoryService service, ISecurityService securityService) : ICategoryService, IServiceDecorator
{
    public async Task<CategoryResponse> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.GetCategoryById, cancellationToken);
        return await service.FindByIdAsync(id, cancellationToken);
    }

    public async Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.FindCategoryPage, cancellationToken);
        return await service.FindPageAsync(request, cancellationToken);
    }

    public async Task<IPagedList<CategoryResponse>> FindArchivePageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.FindCategoryPageArchive, cancellationToken);
        return await service.FindArchivePageAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.EditCategory, cancellationToken);
        return await service.CreateAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.EditCategory, cancellationToken);
        return await service.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<CategoryResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.CategoryArchive, cancellationToken);
        return await service.ArchiveAsync(id, cancellationToken);
    }

    public async Task<CategoryResponse> UnarchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        await securityService.EnsurePermissionAsync(SEPermission.CategoryUnarchive, cancellationToken);
        return await service.UnarchiveAsync(id, cancellationToken);
    }
}

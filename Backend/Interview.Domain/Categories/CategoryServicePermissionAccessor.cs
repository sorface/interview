using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.Permissions;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using X.PagedList;

namespace Interview.Domain.Categories;

public class CategoryServicePermissionAccessor : ICategoryService, IServiceDecorator
{
    private readonly ICategoryService _service;
    private readonly ISecurityService _securityService;

    public CategoryServicePermissionAccessor(ICategoryService service, ISecurityService securityService)
    {
        _service = service;
        _securityService = securityService;
    }

    public async Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.FindCategoryPage, cancellationToken);
        return await _service.FindPageAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.EditCategory, cancellationToken);
        return await _service.CreateAsync(request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.EditCategory, cancellationToken);
        return await _service.UpdateAsync(id, request, cancellationToken);
    }
}

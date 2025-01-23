using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.Database;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Categories;

public class CategoryService(AppDbContext db, ArchiveService<Category> archiveService) : ICategoryService
{
    public async Task<CategoryResponse> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await db.Categories.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(CategoryResponse.Mapper.Expression)
            .FirstOrDefaultAsync(cancellationToken);
        if (response is null)
        {
            throw NotFoundException.Create<Category>(id);
        }

        return response;
    }

    public Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        return FindPageCoreAsync(false, request, cancellationToken);
    }

    public Task<IPagedList<CategoryResponse>> FindArchivePageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        return FindPageCoreAsync(true, request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        var error = await EnsureValidAsync(request, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        request.Name = request.Name!.Trim();
        var category = new Category
        {
            Name = request.Name,
            ParentId = request.ParentId,
        };
        await db.Categories.AddAsync(category, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var response = CategoryResponse.Mapper.Map(category);
        return ServiceResult.Ok(response);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken)
    {
        var error = await EnsureValidAsync(request, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        request.Name = request.Name!.Trim();
        var category = await db.Categories.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceError.NotFound($"Not found category by id '{id}'");
        }

        if (request.ParentId is not null)
        {
            if (request.ParentId == id)
            {
                return ServiceError.Error("It is impossible to make a parent of a category of its own.");
            }

            var children = await GetAllChildrenAsync(id, cancellationToken);
            if (children.Contains(request.ParentId.Value))
            {
                return ServiceError.Error("A child category cannot be specified as a parent category.");
            }
        }

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        await db.SaveChangesAsync(cancellationToken);
        var response = CategoryResponse.Mapper.Map(category);
        return ServiceResult.Ok(response);
    }

    public async Task<CategoryResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await archiveService.ArchiveAsync(id, cancellationToken);
        return CategoryResponse.Mapper.Map(result);
    }

    public async Task<CategoryResponse> UnarchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await archiveService.UnarchiveAsync(id, cancellationToken);
        return CategoryResponse.Mapper.Map(result);
    }

    private async Task<IPagedList<CategoryResponse>> FindPageCoreAsync(bool archive, CategoryPageRequest request, CancellationToken cancellationToken)
    {
        var filter = await BuildSpecificationAsync(archive, request, cancellationToken);
        return await db.Categories
            .AsNoTracking()
            .Where(filter)
            .OrderBy(e => e.Order)
            .Select(CategoryResponse.Mapper.Expression)
            .ToPagedListAsync(request.Page.PageNumber, request.Page.PageSize, cancellationToken);

        async Task<ASpec<Category>> BuildSpecificationAsync(bool archive, CategoryPageRequest request, CancellationToken cancellationToken)
        {
            ASpec<Category> res = new Spec<Category>(e => e.IsArchived == archive);
            if (request.Filter is null)
            {
                return res;
            }

            if (request.Filter.ParentId is not null)
            {
                res &= new Spec<Category>(e => e.ParentId == request.Filter.ParentId);
            }
            else if (request.Filter.ShowOnlyWithoutParent)
            {
                res &= new Spec<Category>(e => e.ParentId == null);
            }

            var name = request.Filter.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                res &= new Spec<Category>(e => e.Name.Contains(name));
            }

            if (request.Filter.EditingCategoryId is not null)
            {
                var children = await GetAllChildrenAsync(request.Filter.EditingCategoryId.Value, cancellationToken);
                res &= new Spec<Category>(e => !children.Contains(e.Id));
            }

            return res;
        }
    }

    private async Task<HashSet<Guid>> GetAllChildrenAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var childrenList = await db.Categories
            .AsNoTracking()
            .Where(e => e.ParentId == categoryId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);
        var children = childrenList.ToHashSet();
        var actualParent = children;
        while (actualParent.Count > 0)
        {
            var childrenNLevel = await db.Categories
                .AsNoTracking()
                .Where(e => e.ParentId != null && actualParent.Contains(e.ParentId.Value))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
            actualParent = [];
            foreach (var id in childrenNLevel)
            {
                if (children.Add(id))
                {
                    actualParent.Add(id);
                }
            }
        }

        return children;
    }

    private Task<ServiceError?> EnsureValidAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult<ServiceError?>(ServiceError.Error("Category should not be empty"));
        }

        return Category.ValidateParentAsync(db, request.ParentId, cancellationToken);
    }
}

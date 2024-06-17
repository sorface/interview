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

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        var filter = BuildSpecification(request);
        return _db.Categories
            .AsNoTracking()
            .Where(filter)
            .OrderBy(e => e.Name)
            .Select(e => new CategoryResponse { Id = e.Id, Name = e.Name, ParentId = e.ParentId })
            .ToPagedListAsync(request.Page.PageNumber, request.Page.PageSize, cancellationToken);

        static ASpec<Category> BuildSpecification(CategoryPageRequest request)
        {
            if (request.Filter is null)
            {
                return Spec<Category>.Any;
            }

            var res = Spec<Category>.Any;
            if (request.Filter.ParentId is not null)
            {
                res &= new Spec<Category>(e => e.ParentId == request.Filter.ParentId);
            }

            var name = request.Filter.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                res &= new Spec<Category>(e => e.Name.Contains(name));
            }

            return res;
        }
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateTagAsync(CategoryEditRequest request, CancellationToken cancellationToken)
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
        await _db.Categories.AddAsync(category, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Created(new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
        });
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateTagAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken)
    {
        var error = await EnsureValidAsync(request, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        request.Name = request.Name!.Trim();
        var category = await _db.Categories.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceError.NotFound($"Not found category by id '{id}'");
        }

        category.Name = request.Name.Trim();
        category.ParentId = request.ParentId;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok(new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
        });
    }

    private Task<ServiceError?> EnsureValidAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult<ServiceError?>(ServiceError.Error("Category should not be empty"));
        }

        return Category.ValidateParentAsync(_db, request.ParentId, cancellationToken);
    }
}
